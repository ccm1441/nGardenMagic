using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewSkill : SkillUtility
{
    // 현재 무기에 추가된 옵션을 모두 캐싱
    public override void Init(WeaponSlot weapon) => base.Init(weapon);

    private void OnEnable()
    {
        StartCoroutine(StartWait());
    }

    IEnumerator StartWait()
    {
        yield return new WaitUntil(() => _skillInfo != null);
        SettingSkillData();
    }

    private void SettingSkillData()
    {
        if (_skillInfo.attackType == 0) StartCoroutine(ViewThrowAttack());
    }

    // 보는 방향으로 날아가는거, 날리지않고 제한 거리까지 이팩트만 출력하는 것
    IEnumerator ViewThrowAttack()
    {
        while (!_coroutineStop)
        {
            if (IngameUI.GetInstance().pause)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            base.GetCameraRange();

            _direction = _player.GetMoveDirection().normalized;

            // 타겟팅 스킬이면 범위 내 탐색
            if (_weaponSlot.addStat.isTargeting)
            {               
                Collider2D[] detectObj = Physics2D.OverlapBoxAll(_player.transform.position, new Vector2(_width + _weaponSlot.addStat.addAttackRange, _height + +_weaponSlot.addStat.addAttackRange), 0, 1 << LayerMask.NameToLayer("HitBox"));

                if (detectObj.Length == 0)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                print("targeting");

                for (int i = 0; i < detectObj.Length; i++)
                {
                    // 탐지된 적을 저장하는 변수가 비었으면 i 번째 적으로 채움
                    if (_detectedEmeny == null) _detectedEmeny = detectObj[i].gameObject;

                    // 기존 탐지 된 타겟보다 새로운 타겟의 거리가 가까우면 새로갱신
                    if (Vector3.Distance(_player.transform.position, detectObj[i].transform.position) < Vector3.Distance(transform.position, _detectedEmeny.transform.position))
                        _detectedEmeny = detectObj[i].gameObject;
                }

                // 방향 계산
                _direction = (_detectedEmeny.transform.position - _player.transform.position).normalized;
            }

            // 넣은 이유는 날리는 이팩트도 있지만
            // 플레이어를 기준으로 어떠한 지점 또는 거리까지 출력하는 이팩트로 있기때문에 구분하기 위함
            // 이것은 회전값만 주면 됨
            if (_skillInfo.BulletSpeed == 0)
            {
                var angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
                CreateBullet(_direction, _player.transform.position, new Vector3(angle, -90, 90));
            }
            else
            {
                var shotCount = _skillInfo.shotCount + _weaponSlot.addStat.addShotCount;
                
                // 한번 공격에 여러번 쏘는 경우도 있기때문에 반복문으로 돌림
                for (int i = 0; i < shotCount; i++)
                {
                    // 총알 발사
                    CreateBullet(_direction, _player.transform.position);
                    if (shotCount > 1) yield return new WaitForSeconds(0.2f);
                }
            }
            yield return new WaitForSeconds(base.CalculateCoolTime());
        }

        // 무기 삭제 테스트 용
        Destroy(this);
    }

    private void CreateBullet(Vector3 direction, Vector3 position)
    {
        var obj = GameManager.GetInstance().spawn.GetSkill(_skillInfo.BulletPrefab);
        obj.transform.position = position;
        obj.SetActive(true);

        obj.GetComponent<Bullet>().BulletInit(direction, _weaponSlot);

        PlaySound(_weaponSlot.skillInfo.soundName);
    }
    
        private void CreateBullet(Vector3 direction, Vector3 position, Vector3 rotation)
    {
        var obj = GameManager.GetInstance().spawn.GetSkill(_skillInfo.BulletPrefab);
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.Euler(rotation);
        obj.SetActive(true);

        obj.GetComponent<Bullet>().BulletInit(direction, _weaponSlot);
        PlaySound(_weaponSlot.skillInfo.soundName);
    }
}
