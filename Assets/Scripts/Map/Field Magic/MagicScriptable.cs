using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Magic_", menuName = "Create Data/FieldMagic", order = int.MaxValue)]
public class MagicScriptable : ScriptableObject
{
    public string Name;

    public Vector2 StartTime;
    public FieldMagicType Type;

    public float CastingTime;
    public float Damage;
    public float AttackTime;
    public int Range;
    public int Count;
    public float ReCastingTime;

    public GameObject AttackEffect;
    
}
