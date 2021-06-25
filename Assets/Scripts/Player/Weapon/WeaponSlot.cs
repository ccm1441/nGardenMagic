using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddStat
{
    // 무기 추가 스텟
    public float addDamage;             // 추가 데미지
    public float addAmpDamage;          // 추가 증폭 데미지
    public float addCooltimePer;        // 추가 쿨타임(퍼센트)
    public float addCooltimeSec;        // 추가 쿨타임(초)      
    public float addSkillScale;         // 스킬 크기
    public float addDurationTime;       // 지속시간
    public float addSkillRange;         // 스킬 범위
    public float addAttackRange;        // 공격 범위
    public int addShotCount;            // 발사 수 
    public float addProvokeHp;          // 도발 체력
    public float addDamageDelay;        // 피해 간격
    public float addWarmholePower;      // 웜홀 파워
    public Vector2 changeSpawn;         // 공격범위 바꿈(벡터)
    public Vector2 addRangeVector;      // 피격 범위

    // 특수 옵션
    public bool isPenetrate;            // 관통 여부
    public int penetrateTargetCount;    // 몇번 관통할것이냐
    public bool isBouncing;             // 전이 여부
    public int bouncingCount;           // 전이 횟수
    public bool isTargeting;            // 타겟팅 여부
    public bool cantUseMonsterState;    // 몬스터 상태제어를 더이상 못씀
    public bool skillOff;               // 더이상 스킬을 발동하지 않음
    public bool randomSkill;            // 랜덤ㅇ 스킬

    // 쉴드, 무적 관련
    public bool shieldStart;            // 쉴드 존속시
    public bool shieldEnd;              // 쉴드 소진시
    public SkillAbility ablity;         // 스킬 종류
    public float value;                 // 능력치
}

public class WeaponSlot : MonoBehaviour
{
    private SpriteRenderer _sr;
    public SkillScriptable skillInfo;

    // 필요한 컴포넌트 및 룬데이타
    Player _player;
    [SerializeField] [Range(0, 20)] private float _detectRange;

    // 무기 추가 정보
    public AddStat addStat;

    // 데미지 계산
    float ampDamage, originDamage, propertyDamage, normalResult, ampResult;

    // ========================================
    // ============테스트용 > 무기 삭제
    // ========================================
    public void RemoveSlot() => skillInfo = null;


    // 이 슬롯이 사용중인지 체킹
    public bool CheckSlotNull()
    {
        if (skillInfo == null) return true;
        return false;
    }

    private void OnEnable()
    {
        _sr = GetComponent<SpriteRenderer>();
        _player = GameManager.GetInstance().player;
    }

    // 슬롯 초기화
    public void Init(SkillScriptable skillInfo)
    {
        this.skillInfo = skillInfo;
        if (skillInfo.UsePassive || skillInfo == null)  return;       

        // 추가 옵션클래스 인스턴스 생성
        if (addStat == null)
        {
        print("인스턴스 생성");
            addStat = new AddStat();
        }
        // 무기 기본 정보 등록       
        _sr.sprite = skillInfo.Image;

        if (skillInfo.penetrationTarget)
        {
            addStat.isPenetrate = true;
            addStat.penetrateTargetCount = skillInfo.penetrationTargetCount;
        }
        else addStat.isPenetrate = false;

        if (skillInfo.targetingTarget) addStat.isTargeting = true;
        else addStat.isTargeting = false;

        if (skillInfo.bouncingTarget)
        {
            addStat.isBouncing = true;
            addStat.bouncingCount = skillInfo.bouncingTargetCount;
        }
        else addStat.isBouncing = false;

        // 무기 성향에 따른 스크릅티 삽입
        AddSkillComponent(skillInfo.ScriptName, skillInfo);
    }

    // 무기별 공격 스크립트 삽입
    public void AddSkillComponent(SkillDataName data, SkillScriptable skillInfo)
    {
        // 찌꺼기 제거
        DestroySkillComponent();

        switch (data)
        {
            case SkillDataName.PlayerAroundSkill:
                var playerAround = gameObject.AddComponent<PlayerAroundSkill>();
                playerAround.Init(this);
                break;
            case SkillDataName.PlayerViewSkill:
                var playerView = gameObject.AddComponent<PlayerViewSkill>();
                playerView.Init(this);
                break;
            case SkillDataName.PlayerSelfSkill:
                var playerSelf = gameObject.AddComponent<PlayerSelfSkill>();
                playerSelf.Init(this);
                break;
            default:
                break;
        }
    }

    // 기존에 스크립트가 있으면 삭제
    private void DestroySkillComponent()
    {
        Destroy(GetComponent<PlayerAroundSkill>());
        Destroy(GetComponent<PlayerViewSkill>());
        Destroy(GetComponent<PlayerSelfSkill>());
    }

    void Update()
    {
        if (_sr.sprite == null && !skillInfo.UsePassive) _sr.sprite = skillInfo.Image;
        transform.LookAt(GameManager.GetInstance().camera.transform);

        if(skillInfo.isUniqueWeapon || skillInfo.Name == "언랭크")
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 45));
       
    //    if (transform.localRotation.y >= 0) _sr.sortingOrder = 6;
     //   else _sr.sortingOrder = -1;
    }

    // 데미지 계산
    public float GetDamage()
    {
        // 증폭 데미지
        ampDamage = _player.GetAmpStat(skillInfo.Type);
        // 속성 데미지
        propertyDamage = _player.GetPropertyValue(skillInfo.isUniqueWeapon ? PropertyType.Null : skillInfo.Type);
        //고유 마력
        originDamage = PlayerInfo.originAttack;
        // 기본 공식 = 고유 마력 * ((속성 + 스킬) * 0.01f)
        normalResult =  originDamage * ((propertyDamage + skillInfo.AttackValue + addStat.addDamage) * 0.01f);

        // 증폭 공식 = 기본공식 + (기본공식 * (증폭 * 0.01f))
        ampResult = normalResult + (normalResult * ((ampDamage + addStat.addAmpDamage) * 0.01f));
        
        return ampResult;
    }

    // 스킬 수치 적용
    public void SetSkillStat(SpecialSkill specialSkill)
    {
        switch (specialSkill.ability)
        {
            case SkillAbility.PropertyDamage:
                print("속성 피해량 증가");
                _player.SetPropertyValue(skillInfo.Type, SkillAbility.PropertyDamage, specialSkill.value);               
                break;
            case SkillAbility.SkillDamage:
                print("스킬 피해량 증가");
                addStat.addDamage += specialSkill.value;
                break;
            case SkillAbility.PropertyAmplified:
                print("속성 증폭 증가");
                _player.SetPropertyValue(skillInfo.Type, SkillAbility.PropertyAmplified, specialSkill.value);
                break;
            case SkillAbility.SkillAmp:
                print("스킬 증폭 증가");
                addStat.addAmpDamage += specialSkill.value;
                break;
            case SkillAbility.PropertyCoolTime:
                print("속성 쿨타임 감소");
                _player.SetPropertyValue(skillInfo.Type, SkillAbility.PropertyCoolTime, specialSkill.value);
                break;
            case SkillAbility.SkillCoolTime:
                print("스킬 쿨타임 증가");
                if (specialSkill.unit == Unit.Percent) addStat.addCooltimePer += specialSkill.value;
                else addStat.addCooltimeSec += specialSkill.value;
                break;
            case SkillAbility.MoveSpeed:
                print("이동 속도");
                addStat.ablity = SkillAbility.MoveSpeed;
                addStat.value = specialSkill.value;
                break;
            case SkillAbility.SkillRange:
                print("범위 증가");
                addStat.addSkillRange += specialSkill.value;
                break;
            case SkillAbility.AttackRange:
                print("사거리 증가");
                addStat.addAttackRange += specialSkill.value;
                break;
            case SkillAbility.DurationTime:
                print("지속시간 감소 혹은 증가");
                addStat.addDurationTime += specialSkill.value;
                break;
            case SkillAbility.ShotObjectCount:
                print("스킬 수 증가");
                addStat.addShotCount += (int)specialSkill.value;
                break;
            case SkillAbility.ChainCount:
                addStat.bouncingCount += (int)specialSkill.value;
                if (addStat.bouncingCount == 0) addStat.isBouncing = false;
                break;
            case SkillAbility.GodMode:
                addStat.ablity = SkillAbility.GodMode;
                addStat.value = specialSkill.value;
                break;
            case SkillAbility.Penetrate:
                print("관통 적용");
                addStat.isPenetrate = true;
                addStat.penetrateTargetCount = (int)specialSkill.value;
                break;
            case SkillAbility.DamageDelay:
                addStat.addDamageDelay += specialSkill.value;
                break;
            case SkillAbility.SkillScale:
                print("크기 증가");
                addStat.addSkillScale += specialSkill.value;
                break;
            case SkillAbility.RemoveMonsterState:
                addStat.cantUseMonsterState = true;
                break;
            case SkillAbility.ProvokeHP:
                addStat.addProvokeHp += specialSkill.value;
                break;
            case SkillAbility.WarmholePower:
                addStat.addWarmholePower += specialSkill.value;
                break;
            case SkillAbility.ShieldStart:
                addStat.shieldStart = true;                
                break;
            case SkillAbility.RewardExp:
                _player.monsterExp += specialSkill.value;
                break;
            case SkillAbility.RecoverHP:
                _player.CurrentHP += _player._addMaxHP * specialSkill.value * 0.01f;
                break;
            case SkillAbility.AddLevelReward:
                _player.selectSkillCount += 1;
                break;
            case SkillAbility.AllPropertyDamage:
                _player.SetPropertyValue(PropertyType.Earth, SkillAbility.PropertyDamage, specialSkill.value);
                _player.SetPropertyValue(PropertyType.Electricity, SkillAbility.PropertyDamage, specialSkill.value);
                _player.SetPropertyValue(PropertyType.Fire, SkillAbility.PropertyDamage, specialSkill.value);
                _player.SetPropertyValue(PropertyType.Water, SkillAbility.PropertyDamage, specialSkill.value);
                _player.SetPropertyValue(PropertyType.Wind, SkillAbility.PropertyDamage, specialSkill.value);
                break;
            case SkillAbility.Reborn:
                _player._reBorn = true;
                break;
            default:
                break;
        }
    }
}
