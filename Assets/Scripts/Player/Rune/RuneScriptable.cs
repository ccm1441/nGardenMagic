using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Rune_", menuName = "Create Data/Rune", order = int.MaxValue)]
public class RuneScriptable : ScriptableObject
{   
    public string Name;
    public Sprite Image;
    public RuneType RuneType;
    public PropertyType ProType;

    [TextArea(1,3)]
    public string Explain;

    public bool UseCustomAbility;
    public SkillAbility CommonAbility;
    
    // 등급 룬일시
    public bool UseClass;
    public int CValue;
    public int BValue;
    public int AValue;
    public int SValue;
    public int SPValue;
    public int AllValue;

    // 일반 룬일시
    public TargetSkill targetSkill;
    public List<RuneAbility> normalAbilityList;
}

[System.Serializable]
public class RuneAbility
{
    public RuneSkill ability;
    public float value;
    public Unit unit;
}
