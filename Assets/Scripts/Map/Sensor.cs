using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public Direction direction;

    private Vector3 _spawnPos;
    private Spawn _spawn;
    private bool _isCreate = false;

    private void Start()
    {
        _spawn = GameManager.GetInstance().spawn;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            Generator();
    }

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    print("map : " + _isCreate);
    //    if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
    //        Generator();        
    //}

    private void Generator()
    {
        _spawnPos = _spawn.GetMapSpawnPos(direction, transform.parent);

        if (_spawnPos.x == -1) return;

        Collider2D[] hit = Physics2D.OverlapCircleAll(_spawnPos, 10, 1 << LayerMask.NameToLayer("Ground"));
        if (hit.Length > 0)
        {
            _isCreate = true;
            return;
        }

        var map = _spawn.GetMap();
        map.name = "Map1";
        map.position = _spawnPos;
        _spawn.SpawnShop(map);

        Collider2D[] checkProp = Physics2D.OverlapBoxAll(_spawnPos, new Vector2(26, 26), 0, 1 << LayerMask.NameToLayer("Prop"));
        if (checkProp.Length == 0) _spawn.SpawnProp(map);
        else
        {
            Prop item;

            for (int i = 0; i < checkProp.Length; i++)
            {
                item = checkProp[i].GetComponent<Prop>();
                item.OnTimer = false;
                item.map = map.gameObject;
            }
        }
    }

    private void OnDisable()
    {
        _isCreate = false;
    }

    private void OnEnable()
    {
        _isCreate = false;
    }
}
