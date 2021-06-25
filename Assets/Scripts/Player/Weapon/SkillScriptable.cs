using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_", menuName = "Create Data/Skill", order = int.MaxValue)]
public class SkillScriptable : ScriptableObject
{
    public bool UsePassive;

    // 공통(패시브, 지팡이)
    [Header("공통")]
    public string Name;
    public Sprite Image;
    public Sprite skillImage;
    public string Explain;
    public PropertyType Type;
    public int MaxLevel;
    public List<SpecialSkill> LevelUpRewardValue;
    public bool isUniqueWeapon;
    public string openExplain;
    public RequireMapOpen requireMapOpen;
    public MapType map;
    public int openValue;
    public string soundName;

    // 지팡이
    [Header("지팡이 옵션")]
    public string StaffName;
    public SkillDataName ScriptName;
    public List<SpecialSkill> staffOption;
    public GameObject BulletPrefab;
    public int BulletSpeed;
    public int BulletMaxDistance;
    public int AttackValue;
    public float AttackCoolTime;

    // 지팡이 - 지속시간
    public bool UseDuration;
    public float DurationTime;
    public float DurationAttackCool;

    // 지팡이 - 스킬 범위
    // 0 = 캐릭터 주변 몇칸, 1 = 원형(반지름), 2 = 백터
    public bool UseRange;
    public int RangeDataType;
    public float RangeCharValue;
    public float RangeRadius;
    public Vector2 RangeVector;

    // 지팡이가 어떤 스킬인가
    public bool targetingTarget;
    public bool bouncingTarget;
    public bool penetrationTarget;
    public int bouncingTargetCount;
    public int penetrationTargetCount;
    public bool pullTarget;
    public bool slowTarget;

    public List<SkillScriptable> StaffAddSkill; //특성스킬 리스트

    // 이 스킬이 특성 스킬인가
    public bool isSpecialSkill;
    public List<SpecialSkill> specialSkill = new List<SpecialSkill>();
    public string specialExplain;

    // 지팡이 - 공격범위
    // 0 = 캐릭터 주변, 1 = 보는 방향, 2 = 자기자신
    public int attackRnage;
    // 0 = 발사 또는 날리기, 1 = 떨어뜨리기, 2 = 빨아당기기
    public int attackType;
    public int shotCount;               // 한번에 몇발 발사할지 
    public int shotMonsterCount;        // 한번에 몇 마리 공격할지
    public bool RandomAttack;           // 무작위 공격
    public MonsterState monsterControl; // 상태이상 분류
    public float monsterControlTime;    // 상태 제어시간이 따로 필요하다면

    // 장착시 옵션( 쉴드 등)
    public List<SpecialSkill> selfSkill;
}

[System.Serializable]
public class SpecialSkill
{
    public SkillAbility ability;
    public float value;
    public Unit unit;
    public bool SetAbility;
}