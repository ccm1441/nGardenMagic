using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicPortal : MonoBehaviour
{
   [SerializeField] private MagicScriptable _magicInfo;
    private MagicSpawn _magicSpawn;

   [SerializeField] private float _reCastingTime;

   public void Init(MagicScriptable magicInfo, MagicSpawn magicSpawn)
    {
        _magicInfo = magicInfo;
        _magicSpawn =magicSpawn;
        _reCastingTime = magicInfo.ReCastingTime;
    }

    public void Casting()
    {
        for (int i = 0; i < _magicInfo.Count; i++)
        {
            var obj = _magicSpawn.GetMagic("Casting");
            obj.transform.parent = GameManager.GetInstance().magicParent;
            obj.transform.position = new Vector3(transform.position.x + Random.Range(-_magicSpawn.magicSpawnRange, _magicSpawn.magicSpawnRange),
                transform.position.y + Random.Range(-_magicSpawn.magicSpawnRange, _magicSpawn.magicSpawnRange), 0);
           
            obj.GetComponent<ShotMagic>().Init(_magicInfo, _magicSpawn);
        }
    }

    private void FixedUpdate()
    {
        _reCastingTime -= Time.deltaTime;

        if (_reCastingTime > 0) return;

        _reCastingTime = _magicInfo.ReCastingTime;

        Casting();
    }
}
