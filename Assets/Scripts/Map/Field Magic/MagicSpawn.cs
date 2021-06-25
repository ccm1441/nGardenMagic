using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSpawn : MonoBehaviour
{
    private GameManager _gm;

    public bool off;
    [Range(5, 50)] public int magicSpawnRange = 5;
    [SerializeField] private GameObject _castingZone;
    [SerializeField] private List<MagicScriptable> _magicDataList;
    [SerializeField] private Transform _magicParent;

    private List<float> _magicActiveTime;
    private List<GameObject> _magicObject;
    [SerializeField] private float timer;
    private Transform _player;

    private List<GameObject> _magicPool;

    private void Start()
    {
        _player = GameManager.GetInstance().player.transform;
        _gm = GameManager.GetInstance();

        _magicActiveTime = new List<float>();
        _magicObject = new List<GameObject>();
        _magicPool = new List<GameObject>();

        // 풀링
        CreatePool();

        _magicDataList.Sort(delegate (MagicScriptable A, MagicScriptable B)
        {
            var aTime = (A.StartTime.x * 60) + A.StartTime.y;
            var bTime = (B.StartTime.x * 60) + B.StartTime.y;

            if (aTime > bTime) return 1;
            else if (aTime < bTime) return -1;
            else return 0;
        });

        for (int i = 0; i < _magicDataList.Count; i++)
        {
            _magicActiveTime.Add((_magicDataList[i].StartTime.x * 60) + _magicDataList[i].StartTime.y);

            var obj = new GameObject().AddComponent<MagicPortal>();
            obj.gameObject.SetActive(false);
            obj.name = _magicDataList[i].name;
            obj.transform.parent = transform;
            obj.Init(_magicDataList[i], this);

            _magicObject.Add(obj.gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (off || _magicActiveTime.Count == 0) return;

        transform.position = _player.position;

        //timer += Time.deltaTime;
        timer = _gm.GetGameSecond();

        for (int i = 0; i < _magicActiveTime.Count; i++)
        {
            if(_magicActiveTime[i] <= timer)
            {
                _magicObject[i].gameObject.SetActive(true);
                _magicObject[i].GetComponent<MagicPortal>().Casting();
                _magicObject.RemoveAt(i);
                _magicActiveTime.RemoveAt(i);
                break;
            }
        }
    }

    private void CreatePool()
    {
        // 마법 이펙트 풀링
        for (int i = 0; i < _magicDataList.Count; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                var obj = Instantiate(_magicDataList[i].AttackEffect, _magicParent);
                obj.SetActive(false);
                _magicPool.Add(obj);
            }
        }

        // 캐스팅 존 풀링
        for (int i = 0; i < 10; i++)
        {
            var obj = Instantiate(_castingZone, _magicParent);
            obj.SetActive(false);
            _magicPool.Add(obj);
        }
    }

    public GameObject GetMagic(string name)
    {
        var index = _magicPool.FindIndex(x => x.name.Contains(name));
        var obj = _magicPool[index];
        obj.SetActive(true);
        _magicPool.RemoveAt(index);
        return obj;
    }

    public void ReturnMagic(GameObject obj)
    {
        obj.SetActive(false);
        _magicPool.Add(obj);
    }
}
