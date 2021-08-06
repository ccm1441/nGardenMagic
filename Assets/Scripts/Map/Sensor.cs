using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sensor : MonoBehaviour
{
    public Direction direction;

    private Vector3 _spawnPos;
    private Spawn _spawn;
    private bool _isCreate;
    private bool _startCoroutine;

    public Text text;
    private Transform _nextMap;

    private WaitForSeconds time = new WaitForSeconds(0.3f);

    public bool IsCreate
    {
        get => _isCreate; 
        set 
        {
            _isCreate = value;
            if(text != null) text.text = value.ToString();
        } 
    }


    private void Start()
    {
        _spawn = GameManager.GetInstance().spawn;
       if(text != null) text.text = IsCreate.ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && !_startCoroutine)
            StartCoroutine(MapGenerator());
                //  Generator();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && !_startCoroutine)
            StartCoroutine(MapGenerator());
        //Generator();
    }

    IEnumerator MapGenerator()
    {
        _startCoroutine = true;

        while (true)
        {
            _spawnPos = _spawn.GetMapSpawnPos(direction, transform.parent);

            if (_spawnPos.x == -1) yield break;

            Collider2D hit = Physics2D.OverlapCircle(_spawnPos, 10, 1 << LayerMask.NameToLayer("Ground"));

            if (hit)
            {
                yield return time;
                _startCoroutine = false;
              //  IsCreate = true;
                yield break;
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

            yield return time;
        }
    }

    private void Generator()
    {
        _spawnPos = _spawn.GetMapSpawnPos(direction, transform.parent);

        if (_spawnPos.x == -1) return;

        Collider2D hit = Physics2D.OverlapCircle(_spawnPos, 10, 1 << LayerMask.NameToLayer("Ground"));

        if (hit)
        {
            IsCreate = true;           
            return;
        }

        var map = _spawn.GetMap();
        map.name = "Map1";
        map.position = _spawnPos;
        IsCreate = true;
        _nextMap = map;
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

    private void ResetSensor()
    {
        if (_nextMap == null) return;

        switch (direction)
        {
            case Direction.Left:
                _nextMap.Find("RightTrigger").GetComponent<Sensor>().IsCreate = false;
                break;
            case Direction.Right:
                _nextMap.Find("LeftTrigger").GetComponent<Sensor>().IsCreate = false;
                break;
            case Direction.Up:
                _nextMap.Find("DownTrigger").GetComponent<Sensor>().IsCreate = false;
                break;
            case Direction.Down:
                _nextMap.Find("UpTrigger").GetComponent<Sensor>().IsCreate = false;
                break;
            case Direction.LeftUp:
                _nextMap.Find("RightUpTrigger").GetComponent<Sensor>().IsCreate = false;
                break;
            case Direction.LeftDown:
                _nextMap.Find("RightDownTrigger").GetComponent<Sensor>().IsCreate = false;
                break;
            case Direction.RightUp:
                _nextMap.Find("LeftUpTrigger").GetComponent<Sensor>().IsCreate = false;
                break;
            case Direction.RightDown:
                _nextMap.Find("LeftDownTrigger").GetComponent<Sensor>().IsCreate = false;
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {       
     //   IsCreate = false;
        _startCoroutine = false;
        //   ResetSensor();
    }
}
