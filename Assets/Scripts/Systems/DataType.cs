#region 몬스터
public enum MonsterAttackType
{
    Collision = 0x00000001,
    LongAttack = 0x00000002,
    Straight = 0x00000004,
}

public enum MonsterHitSpecial
{
ChangeColor,
ChangeSpeed,
}

public enum MonsterState
{
    None,
    Stun,
    Burn,
    Electro,
    Slow,
}
#endregion

public enum FieldMagicType
{    
    Drop,
    Spawn,
}

#region 맵

public enum Map
{
    AddEXP,                 // 추가 경험치
    MonsterSpawnTime,       // 몬스터 스폰 시간 조정
    MonsterSpawnCount,      // 몬스터 스폰 마리수 조정
    FinalReward,            // 최종 보상 업그레이드
}

// 맵 이름
public enum MapType
{
    Grass = 0x00000001,
    Desert = 0x00000002,
    Ice  = 0x00000004,
}

// 맵 오픈 조건
public enum RequireMapOpen 
{
    CatchMonster,
    GetCoin,
    GetLevel,
    CubeMonster,
    OverTime,
    Virus,
}


#endregion

#region 필드,상점 아이템
public enum ItemType
{    
    EXP_Value,
    EXP_Percentage,
    LevelUP,
    HP_Value,
    HP_Percentage,
    Reborn,
    RandomRune,
    Gold
}

public enum PropType
{    
    EXP,
    HP,
    Gold,
    Rune,
    Shop
}
#endregion

#region 룬, 스킬
public enum PropertyType
{
    Fire,
    Electricity,
    Wind,
    Earth,
    Water,
    Clear,
    Null
}

public enum RuneClass
{
    C,
    B,
    A,
    S,
    SPlus,
    All
}

public enum RuneType
{
    Class,
    Normal
}
public enum Unit
{
    Percent,
    Value,
    Second,
    None,
}

public enum SkillAbility
{    
    PropertyDamage,         // 속성 데미지 증가
    PropertyAmplified,      // 속성 증폭 데미지 증가
    MaxHP,                  // 플레이어 최대 체력 증가
    HPPotionAbility,        // 플레이어 포션 회복력 증가
    EXPPotionAbility,       // 플레이어 경험치 포션 증가량 증가
    ReceivedDamage,         // 피격 피해량 감소
    PropertyCoolTime,       // 속성 스킬 쿨타임 감소
    MoveSpeed,              // 이동 속도 증가
    SecRecoverHP,           // 초당 회복률 증가
    RecoverHP,              // 체력 회복
    RewardExp,              // 경험치 추가 지급
    Camp,                   // 레벨업을 하지 않고 체력 회복
    GiveUpLevelUp,          // 레벨업 포기
    SkillRange,             // 스킬 범위
    AttackRange,            // 공격 범위
    DurationTime,           // 지속 시간
    StrunTime,              // 스턴 시간(-1 하면 능력 발동 안함)
    ShotObjectCount,        // 발사 오브젝트 관리
    ChainCount,             // 체인 횟수
    Shiend,                 // 쉴드
    GodMode,                // 무적
    PullPower,              // 당기는 힘
    Penetrate,              // 관통(1 On, 0 Off)
    DamageDelay,            // 피해 간격
    SkillScale,             // 스킬 크기
    AllPropertyDamage,      // 모든 속성 데미지
    RemoveMonsterState,     // 몬스터 상태제어 삭제
    ProvokeHP,              // 도발 오브젝트체력
    WarmholePower,          // 웜홀 파워
    ShieldStart,            // 쉴드 가지고있으면
    ShieldEnd,              // 쉴드 끝난 직후
    GodModeStart,           // 무적 시작 후
    GodModeEnd,             // 무적 끝난 후
    AddLevelReward,         // 레벨업시 선택 무기 하나 더 추가
    Reborn,                 // 부활
    ProtectSkill,           // 보호 지팡이 가지고들어감      
    SkillDamage,            // 스킬 데미지
    SkillAmp,               // 스킬 증폭 데미지 
    SkillCoolTime,          // 스킬 쿨타임
}

public enum SkillDataName
{
    PlayerAroundSkill,        // 플레이어 주변
    PlayerViewSkill,          // 플레이어가 보는 방향
    PlayerSelfSkill,          // 플레이어 자기 자신
}

public enum SkillRange
{
    ViewDirection,          // 보는 방향
    Around,                 // 주변
    PlayerPos,              // 플레이어 위치
}

public enum TargetSkill
{
    Fireball,               // 파이어볼
    Meteor,                 // 메테오
    FireStep,               // 파이어스텝
    EarthWall,              // 어스월
    Earthquake,             // 어스퀘이크
    WindMissile,            // 윈드 미사일
    WindCutter,             // 윈드 커터
    WaterBall,              // 워터볼
    IceField,               // 아이스 필드
    LightingChain,          // 라이트닝 체인
    LightingStorm,          // 라이트닝 스톰
}

public enum RuneSkill
{
    ChangeTargeting,        // 타겟팅으로 바꿈
    PropertyDamage,         // 속성 데미지
    SkillDamage,            // 스킬 데미지
    SkillCoolTime,          // 스킬 쿨 타임
    SkillRange,             // 스킬 범위
    SkillOff,               // 스킬 시전안함
    SkillScale,             // 스킬 크기
    SkillCount,             // 해당 스킬의 개수 증가(파이어볼 3개, 등)
    DurationTime,           // 지속 시간
    RemoveDuration,         // 지속 피해 삭제
    ChainCount,             // 체인 횟수
    EXP,                    // 경험치
    SpawnVector,            // 스킬 소환 범위
    AddSkillRangeVector,    // 백터형 스킬 범위 증가
}

#endregion


