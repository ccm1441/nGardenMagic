using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAroundSkill : SkillUtility
{
    public override void Init(WeaponSlot weapon) => base.Init(weapon);

    private void OnEnable()
    {
        StartCoroutine(StartWait());
    }

    IEnumerator StartWait()
    {
        print("asdfasdf");
        yield return new WaitUntil(() => _skillInfo != null);
        SettingSkillData();
    }

    private void SettingSkillData()
    {
        // 던지기, 날리기
        if (_skillInfo.attackType == 0) StartCoroutine(AroundShotAttack());
        else if (_skillInfo.attackType == 1) StartCoroutine(AroundDropAttack());
    }

    // 주변에 날리거나 발사
    IEnumerator AroundShotAttack()
    {
        while (!_coroutineStop)
        {
            if (IngameUI.GetInstance().pause)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }
            // 카메라 범위 가져오기
            base.GetCameraRange();

            // 타겟팅 스킬이면 범위 내 탐색
            if (_weaponSlot.addStat.isTargeting)
            {               
                Collider2D[] detectObj = Physics2D.OverlapBoxAll(_player.transform.position, new Vector2(_width + _weaponSlot.addStat.addAttackRange,_height + +_weaponSlot.addStat.addAttackRange),0, 1 << LayerMask.NameToLayer("HitBox"));

                if (detectObj.Length == 0)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }
               
                for (int i = 0; i < detectObj.Length; i++)
                {
                    // 탐지된 적을 저장하는 변수가 비었으면 i 번째 적으로 채움
                    if (_detectedEmeny == null) _detectedEmeny = detectObj[i].transform.parent.gameObject;

                    // 기존 탐지 된 타겟보다 새로운 타겟의 거리가 가까우면 새로갱신
                    if (Vector3.Distance(_player.transform.position, detectObj[i].transform.parent.position) < Vector3.Distance(transform.position, _detectedEmeny.transform.position))
                        _detectedEmeny = detectObj[i].transform.parent.gameObject;
                }

                // 방향 계산
                _direction = (_detectedEmeny.transform.position - _player.transform.position).normalized;
            }

            if (_weaponSlot.addStat.isBouncing)
            {
                Bouncing(_detectedEmeny);
                yield return new WaitForSeconds(base.CalculateCoolTime());
                continue;
            }

            if (_skillInfo.shotMonsterCount  > 1)
            {
                Collider2D[] hit = Physics2D.OverlapBoxAll(_player.transform.position, new Vector2(_width, _height), 0, 1 << LayerMask.NameToLayer("HitBox"));
             
                if (hit.Length > 0)
                {
                    // 감지된 몬스터 수가 설정한 몬스터 수보다 작거나 같을 때
                    if (hit.Length <= _skillInfo.shotMonsterCount)
                    {
                        for (int i = 0; i < hit.Length; i++)
                            CreateBullet((hit[i].transform.position - _player.transform.position).normalized, _player.transform.position, hit[i].transform);
                    }
                    else // 클때(랜덤으로 뽑아서)
                    {
                        var list = new List<Collider2D>();

                        // 중복 제거를 위한 리스트로 옮김
                        for (int i = 0; i < hit.Length; i++)
                            list.Add(hit[i]);

                        for (int i = 0; i < _skillInfo.shotMonsterCount; i++)
                        {
                            var index = Random.Range(0, list.Count);

                            CreateBullet((hit[i].transform.position - _player.transform.position).normalized, _player.transform.position, list[index].transform);
                            list.RemoveAt(index);
                        }
                    }
                }
            }
            else
            {
                var shotCount = _skillInfo.shotCount + _weaponSlot.addStat.addShotCount;
                // 한번 공격에 여러번 쏘는 경우도 있기때문에 반복문으로 돌림
                for (int i = 0; i < shotCount; i++)
                {
                    // 타겟팅은 이미 계산했기 때문에 타겟 이외는 계산함
                    if (!_weaponSlot.addStat.isTargeting) _direction = (transform.position + _player.GetMoveDirection() - transform.position).normalized;

                    // 길이만 조정하는 이팩트가 있기때문에 이와같이 설정
                    // View 스킬에 설명 되어있음
                    if (_skillInfo.BulletSpeed == 0)
                    {
                        var angle = Mathf.Atan2(_detectedEmeny.transform.position.y - _player.transform.position.y, _detectedEmeny.transform.position.x - _player.transform.position.x) * Mathf.Rad2Deg;
                        CreateBullet(_direction, _player.transform.position, new Vector3(angle, -90, 90));
                    }
                    else CreateBullet(_direction, _player.transform.position);

                    // 여러번 쏘는 경우 약간의 딜레이를 두어 시각적인 효과를 줌
                    if (shotCount > 1) yield return new WaitForSeconds(0.2f);
                }
            }

            yield return new WaitForSeconds(base.CalculateCoolTime());
        }

        // 테스트용
        Destroy(this);
    }

    // 주변에 떨구는것
    IEnumerator AroundDropAttack()
    {
        while (!_coroutineStop)
        {
            if (IngameUI.GetInstance().pause)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }
            // 카메라 범위 가져오기
            base.GetCameraRange();

            var shotCount = _skillInfo.shotMonsterCount + _weaponSlot.addStat.addShotCount;

            // 공격해야할 몬스터 수가 2마리 이상이면
            if (shotCount > 1)
            {
                if (_weaponSlot.addStat.randomSkill)
                {
                    print("adfasdf");
                    for (int i = 0; i < shotCount; i++)
                    {
                        CreateBullet(Vector3.down, _player.transform.position + new Vector3(Random.Range(-5, 5), Random.Range(-7, 7)));
                    }
                }
                else
                {
                    Collider2D[] hit = Physics2D.OverlapBoxAll(_player.transform.position, new Vector2(_width + _weaponSlot.addStat.addAttackRange, _height + +_weaponSlot.addStat.addAttackRange), 0, 1 << LayerMask.NameToLayer("HitBox"));

                    if (hit.Length > 0)
                    {
                        // 감지된 몬스터 수가 설정한 몬스터 수보다 작거나 같을 때
                        if (hit.Length <= shotCount)
                        {
                            for (int i = 0; i < hit.Length; i++)
                                CreateBullet(Vector3.down, hit[i].transform.position, hit[i].transform);
                        }
                        else // 클때(랜덤으로 뽑아서)
                        {
                            var list = new List<Collider2D>();

                            // 중복 제거를 위한 리스트로 옮김
                            for (int i = 0; i < hit.Length; i++)
                                list.Add(hit[i]);

                            for (int i = 0; i < shotCount; i++)
                            {
                                var index = Random.Range(0, list.Count);

                                CreateBullet(Vector3.down, hit[i].transform.position, list[index].transform);
                                list.RemoveAt(index);
                            }
                        }
                    }
                }
            }
            else // 1마리 일때
            {
                // 움직이지 않는 이팩트
                // x,y 랜덤 좌표 생성 후 떨어뜨림

                var xRamge = _weaponSlot.addStat.changeSpawn.x == 0 ? 5 : _weaponSlot.addStat.changeSpawn.x;
                var yRamge = _weaponSlot.addStat.changeSpawn.y == 0 ? 7 : _weaponSlot.addStat.changeSpawn.y;
                
                if (_skillInfo.BulletSpeed == 0)
                    CreateBullet(Vector3.down, _player.transform.position + new Vector3(Random.Range(-xRamge, xRamge), Random.Range(-yRamge, yRamge)));
                else
                    CreateBullet(Vector3.down, _player.transform.position + new Vector3(Random.Range(-5, 5),Random.Range(20,30)));
            }

            yield return new WaitForSeconds(base.CalculateCoolTime());
        }

        // 테스트용
        Destroy(this);
    }

    private void Bouncing(GameObject first)
    {
        List<Collider2D> detectObj;

        var targetList = new List<GameObject>();
        targetList.Add(first);        

        for (int i = 0; i < _weaponSlot.addStat.bouncingCount; i++)
        {
             detectObj = Physics2D.OverlapCircleAll(targetList[i].transform.position, 5, 1 << LayerMask.NameToLayer("HitBox")).ToList();

            for (int j = 0; j < targetList.Count; j++) detectObj.Remove(targetList[j].transform.GetChild(1).GetComponent<Collider2D>());

                       if (detectObj.Count == 0) break;

            var nextObject = detectObj
                  .OrderBy(obj =>
                  {                      
                      return Vector3.Distance(transform.position, obj.transform.position);
                  })
              .FirstOrDefault();

            targetList.Add(nextObject.transform.parent.gameObject);
        }

        // 플레이어와 첫번째 타깃
        var angle = Mathf.Atan2(targetList[0].transform.position.y - _player.transform.position.y, targetList[0].transform.position.x - _player.transform.position.x) * Mathf.Rad2Deg;
        CreateBullet(_direction, _player.transform.position, targetList[0].transform, new Vector3(angle, -90, 90), 1);

        // 첫번재 타깃과 그 이후 타깃
        for (int i = 0; i < targetList.Count; i++)
        {
            if (i == targetList.Count - 1)
            {
                print("마지막 몬스터 : " + targetList[i].name);
                  CreateBullet(_direction, targetList[i].transform.position, targetList[i].transform, new Vector3(angle, -90, 90), 2);
                break;

            }
            angle = Mathf.Atan2(targetList[i+1].transform.position.y - targetList[i].transform.position.y, targetList[i+1].transform.position.x - targetList[i].transform.position.x) * Mathf.Rad2Deg;
           
            CreateBullet(_direction, targetList[i].transform.position ,targetList[i+1].transform, new Vector3(angle, -90, 90),3);
                   
        }
    }
  
    // 총알 생성\
    private void CreateBullet(Vector3 direction, Vector3 position)
    {
        var obj = GameManager.GetInstance().spawn.GetSkill(_skillInfo.BulletPrefab);
        obj.transform.position = new Vector3(position.x, position.y , obj.transform.position.z);
        obj.SetActive(true);

        obj.GetComponent<Bullet>().BulletInit(direction, _weaponSlot);
        PlaySound(_weaponSlot.skillInfo.soundName);
    }

    private void CreateBullet(Vector3 direction, Vector3 position, Vector3 rotation)
    {
        var obj = GameManager.GetInstance().spawn.GetSkill(_skillInfo.BulletPrefab);
        obj.transform.position = new Vector3(position.x, position.y, obj.transform.position.z);
        obj.transform.rotation = Quaternion.Euler(rotation);
        obj.SetActive(true);

        obj.GetComponent<Bullet>().BulletInit(direction, _weaponSlot);
        PlaySound(_weaponSlot.skillInfo.soundName);
    }

    private void CreateBullet(Vector3 direction, Vector3 position,Transform target, Vector3 rotation, float order)
    {
        var obj = GameManager.GetInstance().spawn.GetSkill(_skillInfo.BulletPrefab);
        obj.transform.position = new Vector3(position.x, position.y, obj.transform.position.z);
        obj.transform.rotation = Quaternion.Euler(rotation);
        obj.SetActive(true);

        obj.GetComponent<Bullet>().BulletInit(direction, _weaponSlot,target, true);
        PlaySound(_weaponSlot.skillInfo.soundName);
    }

    private void CreateBullet(Vector3 direction, Vector3 position, Transform target)
    {
        var obj = GameManager.GetInstance().spawn.GetSkill(_skillInfo.BulletPrefab);

        obj.transform.position = new Vector3(position.x, position.y, obj.transform.position.z);

        obj.SetActive(true);
        obj.GetComponent<Bullet>().BulletInit(direction, _weaponSlot, target);
        PlaySound(_weaponSlot.skillInfo.soundName);
    }
}
