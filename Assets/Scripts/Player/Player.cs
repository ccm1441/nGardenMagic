/**
 * 플레이어 움직임, 상태, 버프 등을 관리하는 스크립트
 * 플레이어 본체에 들어가있음
 * [ PlayerStat.cs ] - 플레이어의 모든 스탯을 관리함
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public partial class Player : MonoBehaviour
{
    [Header("===== Character Info =====")]
    [SerializeField] private List<GameObject> _playerList;                  // 플레이어 캐릭터 프리팹 리스트
    [SerializeField] private SkeletonAnimation _playerRoot;                 // 게임을 진행할 캐릭터 스파인 정보
    private string _currentAnimation;                                       // 현재 진행중인 애니메이션

    [Header("===== Movement =====")]
    [SerializeField] private VariableJoystick joyStick;                     // 조이스틱 오브젝트
    public float moveSpeed;                                                 // 이동 속도
    private float _inputX;                                                  // x축 방향
    private float _inputY;                                                  // y축 방향
    [HideInInspector] public Vector2 _moveVelocity, _viewDirection;         // 현재 이동하는 벨로시티, 보고있는 방향
    private bool _playerHitCool, _halfSpeed;                                // 플레이어 데미지 입는 플래그, 이동속도 절반

    [Header("===== Buff Info =====")]
    public float recoverHpTime;                                             // 몇초마다 체력 회복
    public float recoverHpValue;                                            // 몇초마다 체력 회복 정도
    private float _recoverHpTimer;                                          // 체력 회복 전용 타이머
    public bool isShield;                                                   // 현재 쉴드가 있는가
    private PlayerSelfSkill _selfSkill;                                     // 쉴드 관련 함수실행을 위함

    [Header("===== EXP Table =====")]
    [SerializeField] private int _startRequireExp;                          // 최초 시작 경험치
    public int endLevel;                                                    // 마지막 레벨

    [Header("===== Sound =====")]
    [SerializeField] private AudioClip _dieClip;                            // 플레이어가 죽을때 나는 소리

    private void Awake()
    {
        // 플레이어를 생성 하고 정보 업데이트
        var character = Instantiate(_playerList[PlayerInfo.character], transform);
        character.transform.SetAsFirstSibling();
        _playerRoot = character.GetComponent<SkeletonAnimation>();
    }

    private void Start()
    {
        // 룬 인벤토리 생성 및 초기화
        runeInventory = new RuneInventory[3]
   {
        new RuneInventory {runeClass = RuneClass.All, runeInfo = null , apply = false},
        new RuneInventory {runeClass = RuneClass.All, runeInfo = null , apply = false},
        new RuneInventory {runeClass = RuneClass.All, runeInfo = null , apply = false}
   };

        StatInit();
    }

    void Update()
    {
        if (_death || IngameUI.GetInstance().pause) return;

        UpdateBuff();
        UpdateMove();
    }

    /// <summary>
    /// 플레이어의 n초마다 체력회복 버프등을 관리
    /// </summary>
    private void UpdateBuff()
    {
        _recoverHpTimer += Time.deltaTime;

        if (recoverHpValue > 0 && recoverHpTime < _recoverHpTimer)
        {
            _recoverHpTimer = 0;

            CurrentHP += _addMaxHP * recoverHpValue * 0.01f;
        }
    }

    /// <summary>
    /// 플레이어의 움직임을 담당
    /// </summary>
    private void UpdateMove()
    {
#if UNITY_EDITOR
        _inputX = Input.GetAxisRaw("Horizontal");
        _inputY = Input.GetAxisRaw("Vertical");
#else
        _inputX = joyStick.Horizontal;
        _inputY = joyStick.Vertical;
#endif

        _moveVelocity.x = _inputX;
        _moveVelocity.y = _inputY;

        if (_moveVelocity == Vector2.zero) ChangePlayerAnimation("Idle_1", true);
        else ChangePlayerAnimation("Walk_NoHand", true, 1.5f);

        if (_inputX < 0) _playerRoot.skeleton.ScaleX = -1;
        else if (_inputX > 0) _playerRoot.skeleton.ScaleX = 1;

        if (_inputX != 0 || _inputY != 0)
            _viewDirection = new Vector2(_inputX, _inputY);

        var speed = moveSpeed + (addMoveSpeed == 0 ? 0 : moveSpeed * addMoveSpeed * 0.01f);
        transform.Translate(_moveVelocity.normalized * speed * Time.deltaTime);
    }

   /// <summary>
   /// 플레이어 이동속도를 절반으로 낮추거나 복구함
   /// </summary>
   /// <param name="value"></param>
    public void HalfMoveSpeed(bool value)
    {
        if (value == _halfSpeed) return;
        _halfSpeed = value;

        if (value) addMoveSpeed -= 50;
        else addMoveSpeed += 50;
    }

    /// <summary>
    /// 플레이어 캐릭터의 애니메이션을 관리함
    /// </summary>
    /// <param name="animation">애니메이션 이름</param>
    /// <param name="loop">무한루프 여부</param>
    /// <param name="animationSpeed">애니메이션 속도</param>
    public void ChangePlayerAnimation(string animation, bool loop, float animationSpeed = 1f)
    {
        if (_currentAnimation == animation) return;

        _playerRoot.AnimationState.SetAnimation(0, animation, loop);
        _playerRoot.AnimationState.TimeScale = animationSpeed;
        _currentAnimation = animation;
    }

    /// <summary>
    /// 현재 캐릭터가 보고 있는 방향을 반환함
    /// </summary>
    /// <returns></returns>
    public Vector3 GetMoveDirection() => _viewDirection;

    /// <summary>
    /// 캐릭터 무적
    /// </summary>
    /// <param name="time">무적을 진행할 시간</param>
    public void OnGodMode(float time)
    {
        _godMode = true;
        StartCoroutine(GodModeCoroutine(time));
    }

    /// <summary>
    /// 캐릭터 무적 코루틴, 30% 투명화
    /// </summary>
    /// <param name="time">무적을 진행할 시간</param>
    /// <returns></returns>
    IEnumerator GodModeCoroutine(float time)
    {
        _playerRoot.skeleton.SetColor(new Color(1, 1, 1, 0.3f));
        yield return new WaitForSeconds(time);
        _playerRoot.skeleton.SetColor(new Color(1, 1, 1, 1));
        _godMode = false;
    }

    /// <summary>
    /// 쉴드를 위해 공격컴포넌트를 담아둠
    /// </summary>
    /// <param name="skill"></param>
    public void SetInstanceWeapon(PlayerSelfSkill skill) => _selfSkill = skill;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            // 쉴드가 활성화 상태이면 쉴드만 삭제후 리턴
            if (isShield && !_godMode)
            {
                isShield = false;
                _selfSkill.ActiveSkill(SkillAbility.ShieldEnd);

                for (int i = transform.childCount - 1; i > 0; i--)
                {
                    if (transform.GetChild(i).name == "shield")
                        GameManager.GetInstance().spawn.ReturnSkill(transform.GetChild(i).gameObject);
                }
                return;
            }

            // 맞고 0.1초동안 데미지 안입음
            if (!_playerHitCool)
            {
                _playerHitCool = true;

                CurrentHP -= collision.gameObject.GetComponent<MonsterBase>().GetDamage();
                Invoke("SetReturnDamage", 0.5f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster") && collision.CompareTag("Monster") && collision.gameObject.name == "MonsterBullet")
        {
            collision.gameObject.SetActive(false);

            // 쉴드가 활성화 상태이면 쉴드만 삭제후 리턴
            if (isShield && !_godMode)
            {
                isShield = false;
                _selfSkill.ActiveSkill(SkillAbility.ShieldEnd);

                for (int i = transform.childCount - 1; i > 0; i--)
                {
                    if (transform.GetChild(i).name == "shield")
                        GameManager.GetInstance().spawn.ReturnSkill(transform.GetChild(i).gameObject);
                }
                return;
            }

            // 맞고 0.1초동안 데미지 안입음
            if (!_playerHitCool)
            {
                _playerHitCool = true;

                CurrentHP -= collision.GetComponent<MonsterBullet>().damage;
                Invoke("SetReturnDamage", 0.5f);
            }

            
        }
    }

    private void SetReturnDamage() => _playerHitCool = false;
}


[System.Serializable]
public class EXPTable
{
    public int changeLevel;
    public int increaseValue;
}

[System.Serializable]
public class SkillInventory
{
    public SkillScriptable skillInfo;                // 장착 무기
    public SkillScriptable originSkill;              // 원래 무기 - 3레벨 특성 선택시 원래 무기 저장됨
    public int skillLevel = 1;                       // 스킬 레벨
    public int slotIndex = -1;                       // 몇번 무기슬롯에 있는가
    public List<SkillScriptable> threeLevelSkill;    // 3레벨 스킬 2종류
    public bool endSlot;                             // 더이상 해당 슬롯에 무기는 검사안함

    public void InventoryInit()
    {
        threeLevelSkill = new List<SkillScriptable>();

        Debug.Log("[지팡이] 특성 스킬 목록");
        if (skillInfo.StaffAddSkill.Count == 0)
        {
            Debug.Log("[지팡이] 특성 스킬 없음");
            return;
        }

        for (int i = 0; i < skillInfo.StaffAddSkill.Count; i++)
        {
            Debug.Log(i + 1 + "번 " + skillInfo.StaffAddSkill[i].Name);
            threeLevelSkill.Add(skillInfo.StaffAddSkill[i]);
        }
        Debug.Log("=========================");

    }
}

[System.Serializable]
public class RuneInventory
{
    public RuneClass runeClass;
    public RuneScriptable runeInfo;
    public bool apply;
}