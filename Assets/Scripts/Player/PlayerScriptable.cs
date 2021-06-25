using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "character", menuName = "Create Data/character", order = int.MaxValue)]
public class PlayerScriptable : ScriptableObject
{
    public string playerName_KR;
    public string playerName_EN;
    public int price;
    [TextArea(1,3)] public string explain_KR;
    [TextArea(1,3)] public string explain_EN;
    public GameObject playerObject;
    public int hpValue;
    public float moveSpeed;
    public float attackValue;

    public float fireValue;
    public float electricityValue;
    public float windValue;
    public float earthValue;
    public float waterValue;
    public float clearValue;

}
