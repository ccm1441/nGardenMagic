
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBase : MonoBehaviour
{
    private Color hitColor = new Color(1, 0, 0, 0.5f);

    public MonsterScriptable _monsterInfo;
    private Spawn _spawn;
    private Portal _portal;
    private IngameUI _ingameUI;
    private GameManager _gameManager;

    private Animator _animator;
    private SpriteRenderer _sr;
    private Rigidbody2D _rd;
    private Transform _player;

    private bool _stateCoStop;
    private bool _moveStop;
    private float _attackTimer;
    private bool _onOverlap;
    private bool _isStraight;
    private Vector3 direction;
    private bool _alreadyCo;
    [SerializeField] private Transform _target;

    // 지속 공격
    public float repeatDamage;
    public float durationTime;
    public bool isFireSetp;

    // 몬스터 정보
    public float speed;
    [SerializeField] private float _hitSpeed;
    [SerializeField] private float _hp;
    private int _upgradeValue;
    private Color color;
    private Vector3 _beforeScale;
    private float _distance;
    private int _removeLimit = 30;
    private bool _targetChange;

    /// <summary>
    /// 해당 몬스터의 이미지와 각종 정보를 오브젝트에 업데이트 합니다.
    /// </summary>
    /// <param name="monsterInfo"> 몬스터 데이타 </param>
    public void Init(MonsterScriptable monsterInfo, Spawn spawn, int upgradeValue, Portal portal)
    {
        _upgradeValue = upgradeValue;

        color = monsterInfo.color;
        _sr.color = color;
        _monsterInfo = monsterInfo;
        _spawn = spawn;
        _portal = portal;
        speed = monsterInfo.Speed;

        //  _hitSpeed = monsterInfo.HitSpeed == 0 ? 0 : monsterInfo.HitSpeed;
        _hp = monsterInfo.HP + (monsterInfo.HP * upgradeValue * 0.01f);

      //  _removeLimit = _spawn.mapData[PlayerInfo.currentMap].removeLimit;

        // 충돌 & 원거리
        if ((int)monsterInfo.AttackType == 3) _onOverlap = true;
        // 직진
        else if ((int)monsterInfo.AttackType == 4)
        {
            direction = (GameManager.GetInstance().player.transform.position - transform.position).normalized;
            _isStraight = true;
        }

    }


    private void Update()
    {
        if (_ingameUI.pause)
        {
            _animator.enabled = false;
            return;
        }
        else if (!_ingameUI.pause && !_animator.enabled && !_moveStop) _animator.enabled = true;

        if (_onOverlap)
        {
            LongAttack();
        }
        else if (_isStraight)
        {
            StraightAttack();
            if(_distance >= 15) _portal.ReturnMonster(gameObject);
            return;
        }

        MonsterMove();
    }

    private void OnEnable()
    {
        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _ingameUI = IngameUI.GetInstance();
        _rd = GetComponent<Rigidbody2D>();
        _gameManager = GameManager.GetInstance();
        _player =_gameManager.player.transform;
        _sr.color = color;
        _alreadyCo = false;
        _beforeScale = transform.localScale;
    }

    private void OnDisable()
    {
        _moveStop = false;
        transform.localScale = _beforeScale;       
    }

    private void MonsterMove()
    {
        if (_moveStop) return;

        transform.Translate(GetTargetDirection() * speed * Time.deltaTime);
       _distance = Vector3.Distance(_player.transform.position, transform.position);
      
        if (_distance >= GameManager.GetInstance().testRemoveLimit) _portal.ReturnMonster(gameObject);
        else if (_distance >= 10) _rd.simulated = false;
        else _rd.simulated = true;
    }

    private Vector3 GetTargetDirection()
    {
        if (_target != null) return (_target.position - transform.position).normalized;

        //if (!_target.gameObject.activeSelf) _target = _player;
        return (_gameManager.playerBeforePosition - transform.position).normalized;
    }

    public void ChangeTarget(Transform target)
    {
        _target = target;
       GetTargetDirection();
    }

    public void HitFireStep(float time)
    {
        isFireSetp = true;
        Invoke("OffFireStep", time);
    }

    private void OffFireStep()
    {
        isFireSetp = false;
    }

    public bool MonsterHit(float damage, bool fireStep = false)
    {
        if (fireStep && isFireSetp) return false;

        _hp -= damage;

        SpecialAbility();
        if (!_alreadyCo)
        {
            _alreadyCo = true;
            StartCoroutine(Hit());
        }

        if (_hp > 0) return false;

       
       // GameManager.GetInstance().PlayerEffectSound(_monsterInfo.sound);
        GameManager.GetInstance().PlayerEffectSound(_gameManager._fileID_MonsterDie);
        Reward();

        // 무기 오픈 조건
        if (PlayerInfo.currentMap == 1 && _monsterInfo.Name == "큐브") GameManager.GetInstance().player.desertCubeKill++;
        else if (PlayerInfo.currentMap == 2)
        {
            if (_monsterInfo.name == "균집체") GameManager.GetInstance().player.iceVirusKill++;
        }

        // 죽을때 이팩트
        var effect = _spawn.GetDieEffect();
        effect.transform.localScale = new Vector3(3 / transform.localScale.x, 3 / transform.localScale.y, 0);
        effect.transform.position = transform.position;
        effect.SetActive(true);
        effect.GetComponent<DisableReturnObj>().Init(1.2f);
        _portal.ReturnMonster(gameObject);

        return true;
    }

    private void Reward()
    {
        var player = GameManager.GetInstance().player;
        player.CalculateEXP(_monsterInfo.EXP, false);
        player.Gold += Random.Range(1, 3);
        player.killMonster++;
    }

    // 여기서 기절이나 감전등 상호작용 컨트롤 해놓기
    public void SetState(MonsterState state, float time)
    {
        _stateCoStop = true;
        StartCoroutine(CoSetState(state, time));
    }

    // 지속 데미지 시간 설정
    public void SetDuretaionTime(float damage, float time)
    {
        repeatDamage = damage;
        durationTime = time;
    }

    // TODO:몬스터 기절,화상,감전, 이동속도 감소 효과 구현해야함!
    IEnumerator CoSetState(MonsterState state, float time)
    {
        _stateCoStop = false;

        while (!_stateCoStop)
        {
            if (state == MonsterState.Stun)
            {
                _moveStop = true;
                _animator.enabled = false;
                print("몬스터 스턴!");
            }
            else if (state == MonsterState.Burn || state == MonsterState.Electro)
            {
                yield return new WaitForSeconds(time);

                if (durationTime <= 0) break;
                durationTime -= time;
                MonsterHit(repeatDamage);
                continue;
            }

            yield return new WaitForSeconds(time);

            if (state == MonsterState.Stun)
            {
                _animator.enabled = true;
                _moveStop = false;
                break;
            }
        }
    }

    IEnumerator Hit()
    {
        transform.localScale = transform.localScale * 130 * 0.01f;
        _sr.color = hitColor;
        yield return new WaitForSeconds(0.15f);
        transform.localScale = _beforeScale;
        _sr.color = _monsterInfo.color;
        _alreadyCo = false;
    }

    private void SpecialAbility()
    {
        if (_monsterInfo == null || !_monsterInfo.useHitReAction) return;

        switch (_monsterInfo.reactionType)
        {
            case MonsterHitSpecial.ChangeColor:
                color = _monsterInfo.reactionColor;
                break;
            case MonsterHitSpecial.ChangeSpeed:
                speed = _monsterInfo.reactionSpeed;
                break;
            default:
                break;
        }
    }

    private void StraightAttack()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void LongAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, _monsterInfo.AttackRange, 1 << LayerMask.NameToLayer("Player"));

        if (hit)
        {
            _attackTimer += Time.deltaTime;

            if (_attackTimer > _monsterInfo.AttackTime)
            {
                var state = Random.Range(0, 101);

                if (state >= 50)
                {
                    _moveStop = false;
                    _attackTimer = 0;
                    return;
                }

                _moveStop = true;

                var obj = _portal.GetBullet().GetComponent<Rigidbody2D>();
                obj.GetComponent<MonsterBullet>().SetPortal(_portal, _monsterInfo.Damage);
                obj.position = transform.position;

                obj.AddForce((GameManager.GetInstance().player.transform.position - transform.position).normalized * 10, ForceMode2D.Impulse);
                _attackTimer = 0;
            }
        }
        else
        {
            _attackTimer = 0;
            _moveStop = false;
        }
    }


    public float GetDamage()
    {
        if (_monsterInfo == null) return 0;
        return _monsterInfo.Damage + (_monsterInfo.Damage * _upgradeValue * 0.01f);
    }
    public float GetExp() => _monsterInfo.EXP;
}
