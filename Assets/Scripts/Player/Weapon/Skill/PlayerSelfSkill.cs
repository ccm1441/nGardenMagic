using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelfSkill : SkillUtility
{
    WaitForSeconds time = new WaitForSeconds(0.1f);

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
        StartCoroutine(OnOffAttack());

        for (int i = 0; i < _skillInfo.selfSkill.Count; i++)
            _player.SetPropertyValue(_skillInfo.Type, _skillInfo.selfSkill[i].ability, _skillInfo.selfSkill[i].value);
    }

    IEnumerator OnOffAttack()
    {
       var time = 0f;
        var stepTimer = 0f;

        while (!_coroutineStop)
        {
            if (IngameUI.GetInstance().pause)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            if (_weaponSlot.addStat.skillOff)  yield return new WaitUntil(() => !_weaponSlot.addStat.skillOff);
            

            if (_skillInfo.BulletPrefab != null)
            {
              
                var obj = GameManager.GetInstance().spawn.GetSkill(_skillInfo.BulletPrefab);
                obj.transform.position = _player.transform.position;

                obj.SetActive(true);

                if (_skillInfo.Name == "파이어 스탭" && stepTimer <= _skillInfo.DurationTime + _weaponSlot.addStat.addDurationTime)
                {
                    stepTimer += Time.deltaTime + 0.2f;
                    obj.GetComponent<Bullet>().BulletInit(Vector3.zero, _weaponSlot);

                    yield return new WaitForSeconds(0.2f);
                    continue;
                }
                else if(_skillInfo.Name.Contains("쉴드"))
                {
                    obj.transform.parent = _player.transform;
                    obj.name = "shield";
                    _player.isShield = true;
                    if (_weaponSlot.addStat.shieldStart)                    
                        _player.addMoveSpeed = _weaponSlot.addStat.value;

                    _player.SetInstanceWeapon(this);
                    yield return new WaitUntil(() => !_player.isShield);
                }

            }
            else if(_skillInfo.BulletPrefab == null)
            {
                _player.addMoveSpeed += 20;
                _player.addExp += _weaponSlot.addStat.value;
                _player.OnGodMode(_skillInfo.DurationTime +(_skillInfo.DurationTime*0.01f * _weaponSlot.addStat.addDurationTime));

                time = _skillInfo.DurationTime + (_weaponSlot.addStat.addDurationTime * _skillInfo.DurationTime * 0.01f);
                yield return new WaitForSeconds(time);
                      _player.addMoveSpeed -= 20;
            }
            
            stepTimer = 0;
            yield return new WaitForSeconds(CalculateCoolTime() - time);
        }

    }

    public void ActiveSkill(SkillAbility ability)
    {
        if(ability == SkillAbility.ShieldEnd)
        {
            if (_weaponSlot.addStat.ablity == SkillAbility.GodMode)
            {
                _player.OnGodMode(_weaponSlot.addStat.value);
            }
            else if(_weaponSlot.addStat.ablity == SkillAbility.MoveSpeed)
            {
                print(_weaponSlot.addStat.ablity.ToString() + "====== " + _weaponSlot.addStat.value);
                _player.addMoveSpeed += 50;
                Invoke("ReturnSpeed", 2f);
            }
        }
        else if(ability == SkillAbility.GodModeEnd)
        {
            if (_skillInfo.Name != "인비저블") return;
            _player.addMoveSpeed -= 20;

            if (_weaponSlot.addStat.ablity == SkillAbility.RecoverHP)
            {
                _player.CurrentHP += _player._addMaxHP * _weaponSlot.addStat.value * 0.01f;
            }
            else if(_weaponSlot.addStat.ablity == SkillAbility.RewardExp)
            {
                _player.addExp -= _weaponSlot.addStat.value;
            }
        }
    }

    private void ReturnSpeed()
    {
        _player.addMoveSpeed -= 50;
    }
}
