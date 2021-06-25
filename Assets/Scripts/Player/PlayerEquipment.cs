using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player : MonoBehaviour
{
    [Header("===== Weapon Info =====")]
    public WeaponSlot[] weaponSlot;                                 // 무기 슬롯
    public List<SkillInventory> skillInventory;                     // 가지고 있는 패시브, 지팡이 스킬
    public RuneInventory[] runeInventory;                           // 가지고 있는 룬
    [HideInInspector] public int selectSkillCount = 3;              // 레벨업시 고를 수 있는 스킬 수

    /// <summary>
    /// 3레벨 스킬선택시 이전 스킬을 삭제 후 선택한 스킬을 적용함
    /// </summary>
    /// <param name="skillInfo">새로 적용할 스킬</param>
    public void ChangeSkill(SkillScriptable skillInfo)
    {
        for (int i = 0; i < skillInventory.Count; i++)
        {
            if (skillInfo.Name == skillInventory[i].skillInfo.Name)
            {
                skillInventory[i].skillInfo = skillInfo;
                print("무기 삭제 할 인벤토리 : " + i);
                if (skillInventory[i].threeLevelSkill.Count > 0)
                {
                    var index = skillInventory[i].threeLevelSkill.FindIndex(x => x.Name == skillInfo.Name);
                    skillInventory[i].threeLevelSkill.RemoveAt(index);
                }

                print("남은 스킬 개수" + skillInventory[i].threeLevelSkill.Count);

                weaponSlot[skillInventory[i].slotIndex].Init(skillInfo);
                ApplySkillStat(skillInventory[i], i);
            }
        }
    }

   /// <summary>
   /// 최종적으로 인벤토리에 스킬을 장착시키며, 스탯을 적용함
   /// </summary>
   /// <param name="inventory">장착할 인벤토리 정보</param>
   /// <param name="index">장착할 슬롯 정보</param>
    private void ApplySkillStat(SkillInventory inventory, int index)
    {
        // 최초 삽입
        if(index == -1)
        {
            for (int i = 0; i < weaponSlot.Length; i++)
            {
                if (!weaponSlot[i].CheckSlotNull()) continue;
                print(i);
                // 인덱스 저장
                skillInventory[i].slotIndex = i;
                weaponSlot[i].gameObject.SetActive(true);
                weaponSlot[i].Init(inventory.skillInfo);

                // 패시브
                if (inventory.skillInfo.UsePassive)
                {
                    SetPropertyValue(inventory.skillInfo.Type, inventory.skillInfo.LevelUpRewardValue[0].ability, inventory.skillInfo.LevelUpRewardValue[0].value);
                    break;
                }

                // 지팡이
                // 무기 슬롯 초기화               
                skillInventory[i].InventoryInit();

                // 특성 데이터 업데이트
                // 같은 부모스태프가 있으면 해당 특성 데이터를 가져와 삽입           
                for (int j = 0; j < skillInventory.Count; j++)
                {
                    if(skillInventory[j].endSlot && skillInventory[j].originSkill == inventory.skillInfo)
                    {
                        skillInventory[i].threeLevelSkill = skillInventory[j].threeLevelSkill;
                        break;
                    }    
                }            

                for (int j = 0; j < inventory.skillInfo.staffOption.Count; j++)
                    SetPropertyValue(inventory.skillInfo.Type, inventory.skillInfo.staffOption[j].ability, inventory.skillInfo.staffOption[j].value);

                for (int j = 0; j < inventory.skillInfo.LevelUpRewardValue.Count; j++)
                    weaponSlot[i].SetSkillStat(inventory.skillInfo.LevelUpRewardValue[j]);

                weaponSlot[i].transform.parent.GetComponent<WeaponRotate>().ResetCircle(i + 1);
                break;
            }

            // 룬 검사후 적용 안된 룬이 있으면 적용
            for (int i = 0; i < runeInventory.Length; i++)
            {
                if (runeInventory[i].runeInfo == null || runeInventory[i].apply) continue;

                ApplyRune(runeInventory[i]);
                print("룬 재검사");
            }

            return;
        }

        // 기존에 패시브
        if (inventory.skillInfo.UsePassive)
        {
            SetPropertyValue(inventory.skillInfo.Type, inventory.skillInfo.LevelUpRewardValue[0].ability, inventory.skillInfo.LevelUpRewardValue[0].value);
            return;
        }
        
        // 기존에 지팡이
            var levelList = inventory.skillInfo.LevelUpRewardValue;

            print("=============스킬 능력치 증가=============");
            print("슬롯 인덱스 : " + index);
            for (int i = 0; i < levelList.Count; i++)
                weaponSlot[index].SetSkillStat(levelList[i]);

        // 룬 검사후 적용 안된 룬이 있으면 적용
        for (int i = 0; i < runeInventory.Length; i++)
        {
            if (runeInventory[i].runeInfo == null || runeInventory[i].apply) continue;
            
            ApplyRune(runeInventory[i]);
            print("룬 재검사");
        }
    }

    /// <summary>
    /// 패시브 스킬 적용
    /// </summary>
    /// <param name="inventory">장착할 인벤토리 정보</param>
    /// <param name="index">장착할 슬롯 정보</param>
    private void ApplyPassiveSkill(SkillInventory inventory, int index)
    {
        print("[패시브] 정보 : " + inventory.skillInfo.Name);

        // 인벤토리에 없음, 새롭게 추가
        if (index == -1)
        {
            skillInventory.Add(inventory);
        }
        else // 인벤토리에 있음
        {
            // 레벨업
            skillInventory[index].skillLevel++;

            // 최대 레벨이라면 더이상 안나오도록 삭제
            if (inventory.skillInfo.MaxLevel == skillInventory[index].skillLevel)
            {
                GameManager.GetInstance().DeleteSkillIndex(inventory.skillInfo);
                print(inventory.skillInfo.Name + "스킬이 최대 레벨에 도달하여 리스트에서 삭제");
            }
        }

        ApplySkillStat(inventory, index);
    }

    /// <summary>
    /// 지팡이 스킬 적용
    /// </summary>
    /// <param name="inventory">장착할 인벤토리 정보</param>
    /// <param name="index">장착할 슬롯 정보</param>
    private void ApplyStaffSkill(SkillInventory inventory, int index)
    {
        print("[지팡이] 정보 : " + inventory.skillInfo.Name);

        // 인벤토리에 없음, 새롭게 추가
        if (index == -1)
        {
            skillInventory.Add(inventory);
        }
        else // 인벤토리에 있음
        {
            // 레벨업
            skillInventory[index].skillLevel++;

            // 스킬이 만랩이면
            if (inventory.skillInfo.MaxLevel == skillInventory[index].skillLevel)
            {
                // 최초 기본 스킬
                if (!inventory.skillInfo.isSpecialSkill)
                {
                    print("3레벨 특성 스킬 선택");
                    IngameUI.GetInstance().ActiveSpecialSkillUI(true);
                    IngameUI.GetInstance().SettingSpecialSkill(skillInventory[index]);
                }
                else // 지팡이 특성 스킬
                {
                    skillInventory[index].endSlot = true;

                    // 더이상 선택할 특성 스킬이 없으면 이후 뽑기 리스트에서 지팡이 삭제
                    if (skillInventory[index].threeLevelSkill.Count == 0)
                    {
                        GameManager.GetInstance().DeleteSkillIndex(skillInventory[index].skillInfo);
                        ApplySkillStat(inventory, index);
                    }
                    else
                    {
                        GameManager.GetInstance().ReturnOriginSkill(skillInventory[index]);
                    }                   

                    print("6레벨 특성 선택");
                    IngameUI.GetInstance().ActiveLastSkillUI(true);
                    IngameUI.GetInstance().SettingLastSkill(skillInventory[index]);
                }               
            }
        }

        ApplySkillStat(inventory, index);
    }

    // 해당 스킬 데이터가 있는지 확인하며 있으면 해당 레벨 반환
    public int CheckSkill(SkillScriptable skillData)
    {
        // 리스트에서 해당 스킬 검색
        var index = skillInventory.FindIndex(x => x.skillInfo == skillData);

        if (index != -1) return skillInventory[index].slotIndex;
        else return -1;
    }

    // 무기가 있는지, 레벨이 최대레벨인지 확인함
    public void CheckSkill(SkillInventory inventory)
    {
        // 리스트에서 해당 스킬 검색
        var index = skillInventory.FindIndex(x => x.skillInfo == inventory.skillInfo);

            // 패시브 스킬이면
            if (inventory.skillInfo.UsePassive) ApplyPassiveSkill(inventory, index);
            // 지팡이 스킬이면
            else ApplyStaffSkill(inventory, index);
    }

#region 룬
    // 룬 인벤토리에 룬 추가

    // 등급룬 추가
    public void AddRuneToInventory(int index, RuneScriptable runeInfo, RuneClass runeClass)
    {
        var rune = new RuneInventory();
        rune.runeInfo = runeInfo;
        rune.runeClass = runeClass;
        runeInventory[index] = rune;

        ApplyRune(rune);
    }

    // 일반룬 추가
    public void AddRuneToInventory(int index, RuneScriptable runeInfo)
    {
        var rune = new RuneInventory();
        rune.runeInfo = runeInfo;
        rune.runeClass = RuneClass.All;
        runeInventory[index] = rune;

        ApplyRune(rune);
    }

    // 룬 버리기 - 옵션 지정했던거 그대로 뺌
    public void RemoveRuneToInventory()
    {
        // 해당 슬롯에 룬이 있는지 검사
        var runeIndex = IngameUI.GetInstance().selectRuneSlot;
        if (runeIndex == -1 || runeInventory[runeIndex].runeInfo == null) return;

        runeInventory[runeIndex].apply = false;
      
        // 등급 룬이면
        if (runeInventory[runeIndex].runeInfo.RuneType == RuneType.Class)
        {
            var value = GetRuneValue(runeInventory[runeIndex]);

            SetPropertyValue(runeInventory[runeIndex].runeInfo.ProType, runeInventory[runeIndex].runeInfo.CommonAbility, -value);
        }
        else
        {
            // 일반룬이면
            var name = GetSkillName(runeInventory[runeIndex].runeInfo);
            var skillIndex = CheckingSkillSlot(name);

            if (skillIndex >= 0)
            {
                for (int i = 0; i < runeInventory[runeIndex].runeInfo.normalAbilityList.Count; i++)
                    SetSkillRuneAbility(skillIndex, runeInventory[runeIndex].runeInfo, runeInventory[runeIndex].runeInfo.normalAbilityList[i], true);
            }
        }

        runeInventory[IngameUI.GetInstance().selectRuneSlot].runeInfo = null;
        IngameUI.GetInstance().LoadRuneInventory();
    }

    // 룬 능력치 적용
    private void ApplyRune(RuneInventory inventory)
    {

        // 등급 룬이면 등급에 맞는 값 적용
        if (inventory.runeInfo.RuneType == RuneType.Class)
        {
            if (inventory.apply) return;

            var value = GetRuneValue(inventory);

            inventory.apply = true;
            SetPropertyValue(inventory.runeInfo.ProType, inventory.runeInfo.CommonAbility, value);
            return;
        }

        // 일반룬이면
        var name = GetSkillName(inventory.runeInfo);
        var index = CheckingSkillSlot(name);
        print(name + "|||" + index);

        // 해당 룬이 가지고 있는 스킬이 없으면 그냥 반환 - 착용만 하는 상태
        if (index == -1 || inventory.apply) return;

        for (int i = 0; i < inventory.runeInfo.normalAbilityList.Count; i++)
            SetSkillRuneAbility(index, inventory.runeInfo, inventory.runeInfo.normalAbilityList[i], false);

        inventory.apply = true;

    }

    // 등급 룬 값 가져오기
    private int GetRuneValue(RuneInventory inventory)
    {
        switch (inventory.runeClass)
        {
            case RuneClass.C:
                return inventory.runeInfo.CValue;
            case RuneClass.B:
                return inventory.runeInfo.BValue;
            case RuneClass.A:
                return inventory.runeInfo.AValue;
            case RuneClass.S:
                return inventory.runeInfo.SValue;
            case RuneClass.SPlus:
                return inventory.runeInfo.SPValue;
            default:
                break;
        }

        return -1;
    }

    // 일반룬 - 해당 스킬이 존재하는지 체크
    private int CheckingSkillSlot(string name)
    {
        for (int i = 0; i < weaponSlot.Length; i++)
        {
            if (weaponSlot[i].CheckSlotNull()) continue;

            print(weaponSlot[i].skillInfo.Name + "[룬검사]" + name);
            if (weaponSlot[i].skillInfo.Name.Contains(name)) return i;
        }

        return -1;
    }

    // 일반룬 - 스킬 이름 가져오기
    private string GetSkillName(RuneScriptable runeInfo)
    {
        switch (runeInfo.targetSkill)
        {
            case TargetSkill.Fireball:
                return "파이어 볼";
            case TargetSkill.Meteor:
                return "메테오";
            case TargetSkill.FireStep:
                return "파이어 스탭";
            case TargetSkill.EarthWall:
                return "어스 윌";
            case TargetSkill.Earthquake:
                return "어스퀘이크";
            case TargetSkill.WindMissile:
                return "윈드 미사일";
            case TargetSkill.WindCutter:
                return "윈드 커터";
            case TargetSkill.WaterBall:
                return "워터 볼";
            case TargetSkill.IceField:
                return "아이스 필드";
            case TargetSkill.LightingChain:
                return "라이트닝 체인";
            case TargetSkill.LightingStorm:
                return "라이트닝 스톰";
            default:
                break;
        }

        return string.Empty;
    }

    // 일반룬 - 룬 스킬 적용
    private void SetSkillRuneAbility(int index, RuneScriptable runeInfo, RuneAbility runeAbility, bool reverse)
    {
        switch (runeAbility.ability)
        {
            case RuneSkill.ChangeTargeting:
                print("[룬] 스킬 타겟팅으로 변경");
                if (reverse)
                {
                    weaponSlot[index].addStat.isTargeting = false;
                    return;
                }
                weaponSlot[index].addStat.isTargeting = true;
                break;
            case RuneSkill.PropertyDamage:
                print("[룬] 속성 데미지 변경");
                if (reverse)
                {
                    SetPropertyValue(runeInfo.ProType, runeInfo.CommonAbility, -runeAbility.value);
                    return;
                }
                SetPropertyValue(runeInfo.ProType, runeInfo.CommonAbility, runeAbility.value);
                break;
            case RuneSkill.SkillDamage:
                print("[룬] 스킬 데미지 변경");
                if (reverse)
                {
                    weaponSlot[index].addStat.addDamage -= runeAbility.value;
                    return;
                }
                weaponSlot[index].addStat.addDamage += runeAbility.value;
                break;
            case RuneSkill.SkillCoolTime: // 초단위만 적용
                print("[룬] 스킬 쿨타임 변경");
                if (reverse)
                {
                    weaponSlot[index].addStat.addCooltimeSec -= runeAbility.value;
                    return;
                }
                weaponSlot[index].addStat.addCooltimeSec += runeAbility.value;
                break;
            case RuneSkill.SkillRange:
                print("[룬] 스킬 피격 범위 변경");
                if (reverse)
                {
                    weaponSlot[index].addStat.addSkillRange -= runeAbility.value;
                    return;
                }
                weaponSlot[index].addStat.addSkillRange += runeAbility.value;
                break;
            case RuneSkill.SkillOff:
                print("[룬] 스킬 삭제");
                if (reverse)
                {
                    weaponSlot[index].addStat.skillOff = false;
                    return;
                }
                weaponSlot[index].addStat.skillOff = true;
                break;
            case RuneSkill.SkillScale:
                print("[룬] 스킬 크기 변경");
                if (reverse)
                {
                    weaponSlot[index].addStat.addSkillScale -= runeAbility.value;
                    return;
                }
                weaponSlot[index].addStat.addSkillScale += runeAbility.value;
                break;
            case RuneSkill.SkillCount:
                print("[룬] 타겟 수 변경");
                if (reverse)
                {
                    weaponSlot[index].addStat.randomSkill = false;
                    weaponSlot[index].addStat.addShotCount -= (int)runeAbility.value;
                    return;
                }
                weaponSlot[index].addStat.randomSkill = true;
                weaponSlot[index].addStat.addShotCount += (int)runeAbility.value;
                break;
            case RuneSkill.DurationTime:
                print("[룬] 지속시간 변경");
                if (reverse)
                {
                    weaponSlot[index].addStat.addDurationTime -= runeAbility.value;
                    return;
                }
                weaponSlot[index].addStat.addDurationTime += runeAbility.value;
                break;
            case RuneSkill.RemoveDuration:
                // 스킬 지속 시간 삭제
                weaponSlot[index].addStat.addDurationTime += -99;
                break;
            case RuneSkill.ChainCount:
                print("[룬] 체인 횟수 변경");
                if (reverse)
                {
                    weaponSlot[index].addStat.bouncingCount -= (int)runeAbility.value;
                    return;
                }
                weaponSlot[index].addStat.bouncingCount += (int)runeAbility.value;
                break;
            case RuneSkill.EXP:
                if (reverse)
                {
                    monsterExp -= runeAbility.value;
                    return;
                }
                monsterExp += runeAbility.value;
                break;
            case RuneSkill.SpawnVector:
                print("[룬] 스길 스폰 변경");
                if (reverse)
                {
                    weaponSlot[index].addStat.changeSpawn -= new Vector2(runeAbility.value, runeAbility.value);
                    return;
                }
                weaponSlot[index].addStat.changeSpawn += new Vector2(runeAbility.value, runeAbility.value);
                break;
            case RuneSkill.AddSkillRangeVector:
                print("[룬] 스킬 피격 범위 증가(백터)");
                if (reverse)
                {
                    weaponSlot[index].addStat.addRangeVector -= new Vector2(runeAbility.value, runeAbility.value);
                    return;
                }
                weaponSlot[index].addStat.addRangeVector += new Vector2(runeAbility.value, runeAbility.value);
                break;
            default:
                break;
        }
    }
#endregion
}
