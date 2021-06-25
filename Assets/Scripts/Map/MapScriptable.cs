using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Map_", menuName = "Create Data/Map", order = int.MaxValue)]
public class MapScriptable : ScriptableObject
{
    public int mapIndex;
    public string mapName;
    public Sprite image;
    public string phase;
    public int openTime;
    public List<MapInfo> mapAbilityList;

    public List<TileBase> tilebase;
    public List<GameObject> mapEtcObj;
    public Sprite background;
    public int removeLimit;
}

[System.Serializable]
public class MapInfo
{
    public Map mapAbility;
    public float value;
}
