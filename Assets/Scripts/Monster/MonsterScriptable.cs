using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster_", menuName = "Create Data/Monster", order = int.MaxValue)]
public class MonsterScriptable : ScriptableObject
{
    public string Name;

    public Vector2 StartSpawnTime;
    
    [EnumFlags]
    public MonsterAttackType AttackType;

    public Color color;                     // 몬스터 색
    public GameObject MonsterPrefab;        // 몬스터 프리팹
    public GameObject BulletPrefab;         // 원거리 공격시 총알
    public float AttackRange;               // 원거리 공격시 공격거리
    public float AttackTime;                // 공격 속도
    public float Damage;                    // 공격력
    public float Speed;                     // 이동 속도
    public float HP;                        // 체력
    public float EXP;                       // 경험치
    public float ReSpawnTime;               // 리젠 속도(초)
    public float RespawnCount;              // 한번 리젠당 몇마리
    public AudioClip sound;                 // 몬스터 사운드

    public bool useHitReAction;             // 맞았을때 뭘 바꿀것인가
    public MonsterHitSpecial reactionType;  // 맞았을때 리액션 타입
    public Color reactionColor;             // 맞았을때 색상
    public float reactionSpeed;             // 맞았을때 이동속도

    public MapType spawnMap;                // 스폰 맵
}
