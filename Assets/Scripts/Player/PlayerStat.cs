using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player : MonoBehaviour
{
    [SerializeField] private SkillScriptable _protectStaff;

    [Header("===== Player Stat Info =====")]
    private int _playerLevel;                           // 플레이어 레벨
    private float _originMaxHP;                         // 처음 게임 시작했을시 최대 체력
    [HideInInspector] public float _addMaxHP;           // 최대 체력 + 추가 최대 체력
    private float _addAllMaxHP;                         // 현재 까지 추가된 최대 체력(최대 100% 제한 걸기 위함)
    [HideInInspector] public float _maxExp;             // 해당 레벨 최대 경험치
    [HideInInspector] public bool _death;               // 플레이어가 죽었는가
    private bool _godMode;                              // 현재 무적인가
    private float _currentHP;                           // 현재 체력
    private float _currentExp;                          // 현재 경험치
    private float _beforeMaxExp;                        // 레벨업 포기를 위한 이전 최대 경험치
    private int _gold;                                  // 플레이어 골드
    [HideInInspector] public bool _reBorn;              // 부활 소지 여부
    [HideInInspector] public float addExp;              // 추가경험치(물약)
    [HideInInspector] public float monsterExp;          // 추가경험치(몬스터)
    [HideInInspector] public float addHP;               // 체력 회복했을시 추가 체력
    [HideInInspector] public float addMoveSpeed;        // 추가 이동속도
    private int _defensive;                             // 피격 데미지 감소
    [HideInInspector] public int killMonster;           // 현재까지 죽인 몬스터 수
    [HideInInspector] public int desertCubeKill;        // 사막에서 큐브죽인 횟수
    [HideInInspector] public int iceVirusKill;          // 설원에서 균집체 죽인 횟수

    // 기본 공격력과 속성별 공격력
    private float _originMagic, _fire, _electricity, _wind, _earth, _water, _clear;

    // 속성별 증폭 공격력
    private float _fireAmp, _electricityAmp, _windAmp, _earthAmp, _waterAmp;

    // 속성별 쿨타임
    private float _fireCoolTime, _electricityCoolTime, _windCoolTime, _earthCoolTime, _waterCoolTime;

    /// <summary>
    /// 현재 체력
    /// </summary>
    public float CurrentHP
    {
        get => _currentHP;
        set
        {
            if (value < 0) value = 0;

            // 무적
            if (_godMode)
            {
                if (_currentHP > value) return;

                _currentHP = value;
                IngameUI.GetInstance().UpdateHpBar(_addMaxHP, value);
                return;
            }

            // 체력이 0 이하(부활 있으면 다시 회복)            
            if (value <= 0 && !_death)
            {
                _currentHP = 0;
                IngameUI.GetInstance().UpdateHpBar(_addMaxHP, value);
                _death = true;
                ChangePlayerAnimation("Die_2", false);

                if (_reBorn) _playerRoot.AnimationState.Complete += RebornEvent;
                // 애니메이션 끝나는 콜백 받으면 UI 띄우기
                else _playerRoot.AnimationState.Complete += DeadEvent;

                return;
            }
           
            // 최대 체력을 넘으면
            if (value > _addMaxHP) value = _addMaxHP;
            if (_currentHP > _addMaxHP) _currentHP = _addMaxHP;

            // 방어력이 있으면
            if (_defensive > 0 && _currentHP - value > 0)
            {
                var damage = _currentHP - value;

                damage -= damage * _defensive * 0.01f;

                value = _currentHP - damage;
            }

            if (_currentHP > value) IngameUI.GetInstance().PlayerHit();

            if (value <= _addMaxHP * 30 * 0.01f) IngameUI.GetInstance().LessHP(false);
            else IngameUI.GetInstance().LessHP(true);
            
            _currentHP = value;

            IngameUI.GetInstance().UpdateHpBar(_addMaxHP, value);
        }
    }

    /// <summary>
    /// 플레이어 레벨
    /// </summary>
    public int PlayerLevel
    {
        get => _playerLevel;
        set
        {
            _playerLevel = value;
            IngameUI.GetInstance().UpdateExpBar(_currentExp, _maxExp);
            IngameUI.GetInstance().LevelUpUI();
            IngameUI.GetInstance().UpdateExpText(_playerLevel);
        }
    }

    /// <summary>
    /// 플레이어 골드
    /// </summary>
    public int Gold
    {
        get => _gold;
        set
        {
            _gold = value;
            if (value < 0) value = 0;
            IngameUI.GetInstance().UpdateGoldText(value);
        }
    }

    /// <summary>
    /// 플레이어가 죽었을때 애니메이션이 끝나고 호출할 이벤트
    /// </summary>
    /// <param name="trackEntry"></param>
    private void DeadEvent(TrackEntry trackEntry)
    {
        _playerRoot.AnimationState.Complete -= DeadEvent;

        Time.timeScale = 0;
        //GameManager.GetInstance().PlayerEffectSound(_dieClip, 0.3f);
        GameManager.GetInstance().PlayerEffectSound(GameManager.GetInstance()._fileID_Die, 0.3f);
        IngameUI.GetInstance().PlayerDeadUI();

        // 이거 지워야함?
        //  EndGameoberAds();
    }

    /// <summary>
    /// 광고가 끝나고 게임종료 UI 출력
    /// </summary>
    public void EndGameoberAds()
    {
        Destroy(GameManager.GetInstance().adsManager.gameObject);
        LoadingManager.LoadScene(SceneName.Ingame, SceneName.Lobby);
    }

    /// <summary>
    /// 캐릭터가 부활할때 실행할 이벤트
    /// </summary>
    /// <param name="trackEntry"></param>
    private void RebornEvent(TrackEntry trackEntry)
    {
        CurrentHP = _addMaxHP * 50 * 0.01f;
        _death = false;
        _reBorn = false;
        OnGodMode(3);
        IngameUI.GetInstance().LessHP(true);
        _playerRoot.AnimationState.Complete -= RebornEvent;
    }

    /// <summary>
    /// 플레이어의 모든 스텟을 초기화 및 업데이트
    /// </summary>
    private void StatInit()
    {
        _maxExp = _startRequireExp;
        _playerLevel = 0;
        recoverHpTime = 1;
        Gold = 0;
        _defensive = 0;

        _fireAmp = _electricityAmp = _windAmp = _earthAmp = _waterAmp = 0;
        _fireCoolTime = _electricityCoolTime = _windCoolTime = _earthCoolTime = _waterCoolTime = 0;

        _originMagic = PlayerInfo.originAttack == 0 ? 10 : PlayerInfo.originAttack;
        moveSpeed = PlayerInfo.originSpeed == 0 ? 5 : PlayerInfo.originSpeed;
        _originMaxHP = PlayerInfo.originHP == 0 ? 100 : PlayerInfo.originHP + (PlayerInfo.originHP * PlayerInfo.maxHP * 0.01f);
        CurrentHP = _addMaxHP = _originMaxHP;

        _fire += PlayerInfo.firePropertyAttack == 0 ? 100 : PlayerInfo.firePropertyAttack;
        _electricity += PlayerInfo.electricPropertyAttack == 0 ? 100 : PlayerInfo.electricPropertyAttack;
        _wind += PlayerInfo.windPropertyAttack == 0 ? 100 : PlayerInfo.windPropertyAttack;
        _earth += PlayerInfo.earthPropertyAttack == 0 ? 100 : PlayerInfo.earthPropertyAttack;
        _water += PlayerInfo.waterPropertyAttack == 0 ? 100 : PlayerInfo.waterPropertyAttack;

        _fireCoolTime += PlayerInfo.firePropertyCool;
        _electricityCoolTime += PlayerInfo.electricPropertyCool;
        _windCoolTime += PlayerInfo.windPropertyCool;
        _earthCoolTime += PlayerInfo.earthPropertyCool;
        _waterCoolTime += PlayerInfo.waterPropertyCool;

        addMoveSpeed += PlayerInfo.moveSpeed;
        _defensive += (int)PlayerInfo.passiveDamage;
        addHP += PlayerInfo.portionHP;
        addExp += PlayerInfo.portionEXP;
        monsterExp += PlayerInfo.monsterEXP;
        selectSkillCount += (int)PlayerInfo.addSelectWeapon;
        _reBorn = PlayerInfo.reborn;

        SkillInventory firstSkill = new SkillInventory
        {
            skillInfo = PlayerInfo.startSkill,
            skillLevel = 1
        };
        CheckSkill(firstSkill);

        if (PlayerInfo.protectSkill)
        {
            SkillInventory newSkill = new SkillInventory
            {
                skillInfo = _protectStaff,
                skillLevel = 1
            };
            CheckSkill(newSkill);
        }

    }

    // 테스트용
    // 레벨업 시킴
    public void TestLevelUp() => CalculateEXP(_maxExp - _currentExp, true);

    /// <summary>
    /// 획득하는 경험치를 계산함
    /// </summary>
    /// <param name="value">얻을 경험치</param>
    /// <param name="item">아이템으로 얻는 경험치인가</param>
    public void CalculateEXP(float value, bool item)
    {
        _currentExp += value;

        if (item)
        {
            var add = value * addExp * 0.01f;
            _currentExp += add;
        }
        else
        {
            var add = value * monsterExp * 0.01f;
            _currentExp += add;
        }

        if (_currentExp >= _maxExp)
        {
           
            var temp = _currentExp - _maxExp;
            if (_playerLevel == endLevel) return;

            _beforeMaxExp = _maxExp;
            PlayerLevel++;

            _maxExp += 50;
            _currentExp = temp;
        }

        IngameUI.GetInstance().UpdateExpBar(_currentExp, _maxExp);
    }

    /// <summary>
    /// 따로 계산하지 않고 해당 벨류로 경험치를 바꿈
    /// </summary>
    /// <param name="value">경험치 값</param>
    public void CalculateEXP(float value)
    {
        _currentExp = value;
        IngameUI.GetInstance().UpdateExpBar(_currentExp, _maxExp);
    }

    /// <summary>
    /// 플레이어가 골드를 사용할때 구매가 가능한지 체크
    /// </summary>
    /// <param name="gold">아이템 가격</param>
    /// <returns></returns>
    public bool CheckGold(int gold)
    {
        // 돈 부족
        if (Gold - gold < 0) return false;

        Gold -= gold;
        return true;
    }

    /// <summary>
    /// 플레이어가 레벨업을 포기함
    /// </summary>
    /// <param name="expReturn">경험치를 30% 되돌려 줄것인가</param>
    public void ReturnLevel(bool expReturn = false)
    {
        CalculateEXP(0);
        _maxExp = _beforeMaxExp;
        _playerLevel--;

        if (!expReturn) return;
        CalculateEXP(_beforeMaxExp * 30f * 0.01f);
    }


    /// <summary>
    /// 스킬 능력치에 따른 값 적용
    /// </summary>
    /// <param name="type">속성 타입</param>
    /// <param name="ability">능력 타입</param>
    /// <param name="value">능력치</param>
    public void SetPropertyValue(PropertyType type, SkillAbility ability, float value)
    {
        switch (type)
        {
            case PropertyType.Fire:
                if (ability == SkillAbility.PropertyDamage) _fire += value;
                else if (ability == SkillAbility.PropertyAmplified) _fireAmp += value;
                else if (ability == SkillAbility.PropertyCoolTime) _fireCoolTime += value;
                print("화속성 데미지 : " + _fire + "//증폭 데미지 : " + _fireAmp + "//쿨타임 : " + _fireCoolTime);
                break;
            case PropertyType.Electricity:
                if (ability == SkillAbility.PropertyDamage) _electricity += value;
                else if (ability == SkillAbility.PropertyAmplified) _electricityAmp += value;
                else if (ability == SkillAbility.PropertyCoolTime) _electricityCoolTime += value;
                break;
            case PropertyType.Wind:
                if (ability == SkillAbility.PropertyDamage) _wind += value;
                else if (ability == SkillAbility.PropertyAmplified) _windAmp += value;
                else if (ability == SkillAbility.PropertyCoolTime) _windCoolTime += value;
                break;
            case PropertyType.Earth:
                if (ability == SkillAbility.PropertyDamage) _earth += value;
                else if (ability == SkillAbility.PropertyAmplified) _earthAmp += value;
                else if (ability == SkillAbility.PropertyCoolTime) _earthCoolTime += value;
                break;
            case PropertyType.Water:
                if (ability == SkillAbility.PropertyDamage) _water += value;
                else if (ability == SkillAbility.PropertyAmplified) _waterAmp += value;
                else if (ability == SkillAbility.PropertyCoolTime) _waterCoolTime += value;
                break;
            case PropertyType.Clear:
                _clear += value;
                break;
            case PropertyType.Null:
                ApplyPlayerSkillAbility(ability, value);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 패시브 스킬 능력 적용
    /// </summary>
    /// <param name="ability">능력 타입</param>
    /// <param name="value">능력치</param>
    private void ApplyPlayerSkillAbility(SkillAbility ability, float value)
    {
        print(ability.ToString() + "능력 증가! " + value);
        switch (ability)
        {
            case SkillAbility.MaxHP:
                if (_addAllMaxHP == 100) break;
                _addMaxHP += _originMaxHP * 0.01f * value;
                _addAllMaxHP += value;
                CurrentHP += _originMaxHP * 0.01f * value;
                break;
            case SkillAbility.HPPotionAbility:
                addHP += value;
                break;
            case SkillAbility.EXPPotionAbility:
                addExp += value;
                break;
            case SkillAbility.ReceivedDamage:
                _defensive += (int)value;
                break;
            case SkillAbility.MoveSpeed:
                addMoveSpeed += value;
                break;
            case SkillAbility.SecRecoverHP:
                recoverHpValue += value;
                break;
            case SkillAbility.Camp:
                ReturnLevel();
                CurrentHP += _addMaxHP * value * 0.01f;
                break;
            case SkillAbility.GiveUpLevelUp:
                ReturnLevel(true);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 현재 속성 공격력을 반환함
    /// </summary>
    /// <param name="type">속성 타입</param>
    /// <returns></returns>
    public float GetPropertyValue(PropertyType type)
    {        
        switch (type)
        {
            case PropertyType.Fire:
                return _fire;
            case PropertyType.Electricity:
                return _electricity;
            case PropertyType.Wind:
                return _wind;
            case PropertyType.Earth:
                return _earth;
            case PropertyType.Water:
                return _water;
            case PropertyType.Clear:
                return _clear;
            case PropertyType.Null:
                return 0;
            default:
                break;
        }

        return -1;
    }

    /// <summary>
    /// 현재 속성 증폭 공격력을 반환함
    /// </summary>
    /// <param name="type">속성 타입</param>
    /// <returns></returns>
    public float GetAmpStat(PropertyType type = PropertyType.Null)
    {
        switch (type)
        {
            case PropertyType.Fire:
                return _fireAmp;
            case PropertyType.Electricity:
                return _electricityAmp;
            case PropertyType.Wind:
                return _windAmp;
            case PropertyType.Earth:
                return _earthAmp;
            case PropertyType.Water:
                return _waterAmp;
            default:
                break;
        }
        return 0;
    }

    /// <summary>
    /// 현재 속성 쿨타임을 반환함
    /// </summary>
    /// <param name="property">속성 타입</param>
    /// <returns></returns>
    public float GetPropertyCoolTime(PropertyType property)
    {
        var value = 0f;

        switch (property)
        {
            case PropertyType.Fire:
                value = _fireCoolTime;
                break;
            case PropertyType.Electricity:
                value = _electricityCoolTime;
                break;
            case PropertyType.Wind:
                value = _windCoolTime;
                break;
            case PropertyType.Earth:
                value = _earthCoolTime;
                break;
            case PropertyType.Water:
                value = _waterCoolTime;
                break;
            default:
                break;
        }

        return value;
    }
}



