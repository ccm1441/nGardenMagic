using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private bool _changeLength;
    [SerializeField] private bool _bouncingCreate;

  [SerializeField]  private SkillScriptable _skillInfo;
    private Player _player;
    public WeaponSlot weaponSlot;

    private Vector3 _direction;

    public bool _isProvoke;                 // 도발하다
    public float provokeHp;                 // 도발 체력
    public bool useOnlyTime;                // 오로지 시간으로만 제거함
    public float directDamage;              // 바로 데미지 입히는것
    private float _currentProvokeHp;
    public bool onlyHitEffect;              // 때렷을때만 이펙트 출력
    public bool onlyCollider;               // 콜라이더 몬스터만 데미지
    public bool followPlayer;               // 플레이어 따라가기
    public bool dontDestroy;                // 삭제 안함
    private float targetOrder;              // 타겟 순서를 받아서 차등한 데미지 적용
    private Transform _beforeParent;
    private Vector3 _startPos;

    // 총알 추가 옵션 - 무기 슬롯에서 캐싱해놓음
    private int _penetrateTargetCount;      // 관통 타겟 카운트
    private int _bouncingTargetCount;       // 전이 타겟 카운트

    [SerializeField] private List<GameObject> _FXSpawnList;
    [SerializeField] private float _LifeTime;
    [SerializeField] private float _deleyEffect;
    private Transform _target;

    private bool _checkBullet;
    private bool _startTimer;
   [SerializeField] private float _timer;
    private GameObject fx;

    // 총알 초기화, 방향과 슬롯정보를 받음
    public void BulletInit(Vector3 direction, WeaponSlot weapon)
    {
        _player = GameManager.GetInstance().player;
        _direction = direction;
        this.weaponSlot = weapon;
        _skillInfo = weaponSlot.skillInfo;
  
        // 일정 시간을 지정하였으면 그 시간 뒤로 사라지도록
        if (_LifeTime > 0)
        {
            _timer = 0 + weaponSlot.addStat.addDurationTime;
            _startTimer = true;
        }

        SetWeaponSkill();
    }

    public void BulletInit(Vector3 direction, WeaponSlot weapon, Transform target, bool fit = false, float order = 0)
    {
        _player = GameManager.GetInstance().player;

        // 이팩트 길이 변경
        if (_changeLength) ChangeEffectLength(target, fit);

        _target = target;
       if(order != 0) targetOrder = order;

        _startPos = transform.position;
        _direction = direction;
        this.weaponSlot = weapon;
        _skillInfo = weaponSlot.skillInfo;

        // 일정 시간을 지정하였으면 그 시간 뒤로 사라지도록
        if (_LifeTime > 0)
        {
            _timer = 0 + weaponSlot.addStat.addDurationTime;
            _startTimer = true;
        }
       
        SetWeaponSkill();
        if (target != null) Invoke("RangeAttack", directDamage);
    }

    private void SetWeaponSkill()
    {
        if (followPlayer)
        {
            _beforeParent = transform.parent;
            transform.parent = _player.transform;
        }

        _penetrateTargetCount = weaponSlot.addStat.penetrateTargetCount;

        _currentProvokeHp = provokeHp + (provokeHp * weaponSlot.addStat.addProvokeHp * 0.01f);
               
        if(weaponSlot.addStat.addSkillScale != 0)
        {
            _checkBullet = transform.TryGetComponent(out ChangeSkillParticle _);
            if (_checkBullet) transform.GetComponent<ChangeSkillParticle>().ChangeScaleSize(weaponSlot.addStat.addSkillScale);
        }

    }

    // 타이머 업데이트 - 정해둔 타이머에 다다르면 이팩트 생성
    // 총알을 이동시킴
    private void FixedUpdate()
    {
        LifeTime();

        // 스킬 정보가 없거나, 총알 속도가 0이면 리턴
        if (_skillInfo == null || _skillInfo.BulletSpeed == 0) return;

        // 총알을 이동시킴
        transform.Translate(_direction * _skillInfo.BulletSpeed * Time.deltaTime);
    }

    // 지정된 시간이 지나면 이팩트 출력
    // 지정된 타겟이 있다면 지정된 시간뒤에 지정타겟에 데미지를 가함
    private void LifeTime()
    {
        if (!_startTimer) return;

        _timer += Time.deltaTime;

        // 생존 타이머에 도달하면 이팩트 출력
        if (_timer > _LifeTime + weaponSlot.addStat.addDurationTime)
        {
            if(!onlyHitEffect) SpawnHitEffect(false, Vector3.zero);
            else GameManager.GetInstance().spawn.ReturnSkill(gameObject);
            _startTimer = false;
        }

        // 도발 체력이 없으면 사라짐       
        if (_isProvoke && _currentProvokeHp <= 0)
        {
            SpawnHitEffect(false, Vector3.zero);
            _startTimer = false;
        }
    }


    // 몬스터와 충돌 하였을때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_skillInfo == null) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("HitBox"))
        {            
            // 충돌한 몬스터 정보를 가져옴
            var monsterBase = collision.transform.parent.GetComponent<MonsterBase>();
           
            if(_skillInfo.monsterControl != MonsterState.None)
                monsterBase.SetState(_skillInfo.monsterControl, _skillInfo.monsterControlTime);

            if (_skillInfo.Name == "파이어 스탭")
            {
                if (monsterBase.isFireSetp) return;

                monsterBase.MonsterHit(weaponSlot.GetDamage(), true);
                monsterBase.HitFireStep(_skillInfo.DurationAttackCool);
                return;
            }

            // 관통옵션을 가지고 있음
            // 관통 카운터 -1, 맞는 이팩트만 출력 > 데미지 가함
            if (weaponSlot.addStat.isPenetrate && _penetrateTargetCount > 0)
            {
                print("관통");
                _penetrateTargetCount--;
                monsterBase.MonsterHit(weaponSlot.GetDamage());
                SpawnHitEffect(true, Vector3.zero);
                return;
            }
            else if (_isProvoke)
            {
                _currentProvokeHp -= monsterBase._monsterInfo.Damage;
                monsterBase.ChangeTarget(transform);
                return;
            }
            else if (_skillInfo.pullTarget)
            {
                monsterBase.ChangeTarget(transform);
                monsterBase.speed += monsterBase.speed + (monsterBase.speed * weaponSlot.addStat.addWarmholePower * 0.01f);
                return;
            }
            else if (_skillInfo.slowTarget)
            {
                monsterBase.speed *= 0.5f;
                return;
            }
            else if (onlyCollider)
            {
                SpawnHitEffect(false, collision.transform.position);
                monsterBase.MonsterHit(weaponSlot.GetDamage());
                return;
            }
            else if (!useOnlyTime)
            {
                _target = collision.transform.parent;
                SpawnHitEffect(false, collision.transform.position);
                return;
            }



            monsterBase.MonsterHit(weaponSlot.GetDamage());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_skillInfo == null) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("HitBox"))
        {
            // 충돌한 몬스터 정보를 가져옴
            var monsterBase = collision.transform.parent.gameObject.GetComponent<MonsterBase>();

            if (_skillInfo.pullTarget)
            {
                monsterBase.ChangeTarget(null);
                monsterBase.speed = monsterBase._monsterInfo.Speed;
            }
            else if (_skillInfo.slowTarget)
            {
                monsterBase.speed *= 2;
            }
        }
    }


    // 타격 이팩트 출력 후 범위공격 실행(맞는 데미지만 출력할것인가?)
    private void SpawnHitEffect(bool onlyHitEffect, Vector3 targetPostion)
    {
        // 출력할 타격이팩트가 없으면 범위공격
        if (_FXSpawnList.Count == 0)
        {
            RangeAttack();
            return;
        }

        // 타격 이팩트가 있으면
        for (int i = 0; i < _FXSpawnList.Count; i++)
        {
            if (_FXSpawnList.Count <= 0) break;

            // 해당 이팩트를 가져 포지션 잡아줌
            fx = GameManager.GetInstance().spawn.GetSkill(_FXSpawnList[i]);
            fx.transform.position = targetPostion == Vector3.zero ? transform.localPosition : targetPostion;

            // 스킬 이팩트 관련 향상 옵션이 있으면 전용 스크립트를 넣고 해당 옵션을 향상시킴        
            // 현재는 범위만 해놓음
            // TODO: 스킬 지속시간이나 관련 향상옵션 있으면 여기 구현!
            if (weaponSlot.addStat.addSkillRange > 0)
            {
                _checkBullet = fx.TryGetComponent(out ChangeSkillParticle _);
                if (_checkBullet) fx.GetComponent<ChangeSkillParticle>().ChangeRange(weaponSlot.addStat.addSkillRange);
            }

            // 타격 이팩트에 전용 스크립트 삽입
            // 시간이 지나면 사라지도록 함
            if (!fx.TryGetComponent(out DisableReturnObj _)) fx.AddComponent<DisableReturnObj>().Init(_LifeTime);
            else fx.GetComponent<DisableReturnObj>().Init(_LifeTime);

            fx.SetActive(true);

            // 타격 이팩트만 출려하면 여기서 마무리
            if (onlyHitEffect) continue;

            // 삭제 딜레이가 있다면
            if (_deleyEffect > 0)
            {
                Invoke("RangeAttack", _deleyEffect);
                return;
            }

            // 범위 공격을 사용한다면 실행
            if (_skillInfo.UseRange) RangeAttack();
            else
            {
                if (followPlayer) transform.parent = _beforeParent;
                GameManager.GetInstance().spawn.ReturnSkill(gameObject);
            }
        }
    }

    // 범위 공격
    public void RangeAttack()
    {       
        // 스킬 정보가 없으면 그냥 반환
        if (_skillInfo == null)
        {
            GameManager.GetInstance().spawn.ReturnSkill(gameObject);
            return;
        }

        // 백터 범위
        if (_skillInfo.RangeDataType == 2)
        {
            // 범위를 가져오고, 추가 확장 범위가 있으면 적용
            var size = _skillInfo.RangeVector + weaponSlot.addStat.addRangeVector;
            size += size * weaponSlot.addStat.addSkillRange * 0.01f;

            // 해당 범위만큼 콜라이더 적용
            Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, size, 0, 1 << LayerMask.NameToLayer("HitBox"));

            // 감지된 몬스터만큼 데미지 적용
            for (int i = 0; i < hit.Length; i++)
            {
                // 몬스터 정보 가져옴
                var monster = hit[i].transform.parent.GetComponent<MonsterBase>();

                //// 데미지 적용 > 안죽었으면 추가 cc기 적용
                if (!weaponSlot.addStat.cantUseMonsterState && !monster.MonsterHit(weaponSlot.GetDamage()))
                {
                    if (_skillInfo.monsterControl == MonsterState.None) continue;

                    monster.SetDuretaionTime(weaponSlot.GetDamage(), _skillInfo.DurationTime + weaponSlot.addStat.addDurationTime);
                    monster.SetState(_skillInfo.monsterControl,(_skillInfo.monsterControlTime == 0 ? _skillInfo.DurationAttackCool : _skillInfo.monsterControlTime) + weaponSlot.addStat.addDamageDelay);
                }
            }
        }
        else if(_skillInfo.RangeDataType == 1)
        {
            // 해당 범위만큼 콜라이더 적용
            Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, _skillInfo.RangeRadius, 1 << LayerMask.NameToLayer("HitBox"));

            // 감지된 몬스터만큼 데미지 적용
            for (int i = 0; i < hit.Length; i++)
            {
                // 몬스터 정보 가져옴
                var monster = hit[i].transform.parent.GetComponent<MonsterBase>();

                //// 데미지 적용 > 안죽었으면 추가 cc기 적용
                if (!weaponSlot.addStat.cantUseMonsterState && !monster.MonsterHit(weaponSlot.GetDamage()))
                {
                    if (_skillInfo.monsterControl == MonsterState.None) continue;

                    monster.SetDuretaionTime(weaponSlot.GetDamage(), _skillInfo.DurationTime + weaponSlot.addStat.addDurationTime);
                    monster.SetState(_skillInfo.monsterControl, (_skillInfo.monsterControlTime == 0 ? _skillInfo.DurationAttackCool : _skillInfo.monsterControlTime) + weaponSlot.addStat.addDamageDelay);
                }
            }
        }
        else
        {
            if (_target != null)
            {
                var monster = _target.GetComponent<MonsterBase>();

                if (_skillInfo.Name == "파이어 스탭") return;           


                //// 데미지 적용 > 안죽었으면 추가 cc기 적용
                if (!weaponSlot.addStat.cantUseMonsterState && !monster.MonsterHit(weaponSlot.GetDamage()))
                {
                    print("call");
                    if (_skillInfo.monsterControl != MonsterState.None)
                    {
                       
                        monster.SetDuretaionTime(weaponSlot.GetDamage(), _skillInfo.DurationTime + weaponSlot.addStat.addDurationTime);
                        monster.SetState(_skillInfo.monsterControl, (_skillInfo.monsterControlTime == 0 ? _skillInfo.DurationAttackCool : _skillInfo.monsterControlTime) + weaponSlot.addStat.addDamageDelay);
                    }

                }
            }
        }

        // 해당 스킬 반납
        if (followPlayer) transform.parent = _beforeParent;
        _target = null;
      if(!dontDestroy) GameManager.GetInstance().spawn.ReturnSkill(gameObject);
    }

    private void ChangeEffectLength(Transform target, bool fit = false)
    {
        var main = GetComponent<ParticleSystemRenderer>();

        if (!fit)
        {
            main.lengthScale = Vector3.Distance(_player.transform.position + new Vector3(0, 20, 0), target.position);
            return;
        }
       
        main.lengthScale = Vector3.Distance(transform.position, target.position) *0.5f;
    }
}
