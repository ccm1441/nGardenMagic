using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "Create Data/Item", order = int.MaxValue)]
public class ItemScritable : ScriptableObject
{
    public Sprite Image;
    public string Name;
    public ItemType Type;

    [TextArea(1,5)]
    public string Explain;

    public float Value;
    public int Price;
    public float Probability;
    public int DetailNormalProbability;
    public int DetailClassProbability;

    public List<RuneProbability> ProbabilityList;
    public string itemSound;
}

[System.Serializable]
public class RuneProbability
{
    public RuneClass runeClass;
    public int probability;
}


