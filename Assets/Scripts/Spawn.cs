using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

[System.Serializable]
public class FieldItem
{
    public int createCount;
    public List<ItemScritable> propList;
}

public class Spawn : MonoBehaviour
{
    // 공통
    private Camera camera;
    private float _height, _width;
    private Vector3 _up, _down, _left, _right;
    private Vector3 _mapUp, _mapDown, _mapLeft, _mapRight;

    [Header("● Controler")]
    [SerializeField] private bool _offMonster;
    [SerializeField] private bool _offProp;

    [Header("● Monster")]
    [SerializeField] private Transform _monsterParent;
    [SerializeField] private List<MonsterScriptable> _monsterDataList;
    [SerializeField] private GameObject _monsterPrefab;
    [SerializeField] private int _monsterPoolLimit;
    [SerializeField] private GameObject _monsterDieEffect;
    private Queue<GameObject> _monsterEffectPool;
    private List<GameObject> _portalArray;
    [SerializeField] private List<float> _portalActiveTime;
    [SerializeField] private float _monsterSpawnTimer;

    [Header("● Prop")]
    [SerializeField] private List<FieldItem> _fieldItemList;
    [SerializeField] private GameObject _propPrefab;
    [SerializeField] private Transform _propParent;
    [SerializeField] private float _propSpawnTime;
    [SerializeField] private int _propSpawnRange;
    [SerializeField] private int _propPoolLimit;
    [SerializeField] private SpriteAtlas _atlas;
    private float _propTimer;
    private Queue<GameObject> _propPool;

    [Header("● Map")]
    [SerializeField] private Transform _mapParent;
    [SerializeField] private GameObject _mapPrefab;
    [SerializeField] private GameObject _shopPrefab;
    public List<MapScriptable> mapData;
    private int _x, _y;

    private Queue<Transform> _mapPool;
    private bool _cantSpawnShop;
    private int _mapCount;

    [Header("● Skill Effect")]
    [SerializeField] private List<GameObject> _skillPrefabList;
    [SerializeField] private Transform _skillParent;
    [SerializeField] private int _skillPoolLimit;
    private List<GameObject> _skillPool;

    private void Awake()
    {
        MonsterInit();
        CreatePool();
        FirstCreateMap();
    }

    private void Start()
    {
        camera = GameManager.GetInstance().camera;
    }

    private void Update()
    {
        UpdateMonsterSpawn();
    }

    #region 공통 함수
    /// <summary>
    /// 카메라 범위 밖에서 무작위 좌표에 생성
    /// </summary>
    /// <returns> 좌표 설정후 좌표 리턴</returns>
    public Vector3 GetSpawnPos(int addRange = 1)
    {
        var index = Random.Range(0, 4);

        // 카메라 정보 처리 후 소환
        _height = camera.orthographicSize + addRange;
        _width = camera.orthographicSize * camera.aspect + addRange;

        switch (index)
        {
            case 0:
                _up = new Vector3(camera.transform.position.x + Random.Range(-_width, _width), camera.transform.position.y + _height, 0);
                return _up;
            case 1:
                _down = new Vector3(camera.transform.position.x + Random.Range(-_width, _width), camera.transform.position.y - _height, 0);
                return _down;
            case 2:
                _left = new Vector3(camera.transform.position.x - _width, camera.transform.position.y + Random.Range(-_height, _height), 0);
                return _left;
            case 3:
                _right = new Vector3(camera.transform.position.x + _width, camera.transform.position.y + Random.Range(-_height, _height), 0);
                return _right;
            default:
                break;
        }

        return Vector3.zero;
    }

    #endregion

    #region 몬스터
    private void MonsterInit()
    {
        _portalArray = new List<GameObject>();
        _portalActiveTime = new List<float>();

        // 몬스터 소환 시작시간을 기준으로 정렬
        _monsterDataList.Sort(delegate (MonsterScriptable A, MonsterScriptable B)
        {
            var aTime = (A.StartSpawnTime.x * 60) + A.StartSpawnTime.y;
            var bTime = (B.StartSpawnTime.x * 60) + B.StartSpawnTime.y;

            if (aTime > bTime) return 1;
            else if (aTime < bTime) return -1;
            else return 0;
        });

        // 해당 맵에 대한 몬스터가 아니면 리스트에서 제거
        for (int i = _monsterDataList.Count -1 ; i > 0; i--)
        {
            if (PlayerInfo.currentMap == 0 && (int)_monsterDataList[i].spawnMap != 1) _monsterDataList.RemoveAt(i);         // 초원, 초원빼고 제거
            else if(PlayerInfo.currentMap == 1 && (int)_monsterDataList[i].spawnMap == -1) _monsterDataList.RemoveAt(i);    // 설원, 제거 하지 않음.
        }

        // 몬스터마다 포탈을 생성
        for (int i = 0; i < _monsterDataList.Count; i++)
        {
            _portalActiveTime.Add((_monsterDataList[i].StartSpawnTime.x * 60) + _monsterDataList[i].StartSpawnTime.y);
            var obj = new GameObject().AddComponent<Portal>();

            obj.gameObject.SetActive(false);
            obj.name = _monsterDataList[i].name;
            obj.transform.parent = transform;
            obj.Init(_monsterDataList[i], this);

            _portalArray.Add(obj.gameObject);
        }
    }

    private void UpdateMonsterSpawn()
    {
        if (_offMonster || _portalActiveTime.Count == 0) return;

        _monsterSpawnTimer += Time.deltaTime;

        if (_portalActiveTime[0] <= _monsterSpawnTimer)
        {
            _portalArray[0].gameObject.SetActive(true);
            _portalActiveTime.RemoveAt(0);
            _portalArray.RemoveAt(0);
        }
    }
    #endregion

    #region 프롭(맵에 무작위로 생성되는 오브젝트)
    public void SpawnProp(Transform map)
    {
        if (_offProp || _propPool.Count == 0) return;

        for (int i = 0; i < _fieldItemList.Count; i++)
        {
            for (int j = 0; j < _fieldItemList[i].createCount; j++)
            {
                var item = GameManager.GetInstance().ProbabilityItem(_fieldItemList[i].propList);
                if (item == null) continue;

                var obj = GetProp();
                if (obj == null) break;

                var y = Random.Range(map.position.y - 9, map.position.y + 9);
                obj.transform.position = new Vector3(Random.Range(map.position.x - 9, map.position.x + 9), y, 0);
               
               // obj.GetComponent<Prop>().Init(this, item, _atlas.GetSprite(item.Image.name), map.gameObject);
                obj.GetComponent<Prop>().Init(this, item, item.Image, map.gameObject);
                if (item.Name == "HP 회복 포션") obj.transform.localScale = Vector3.one * 1.5f;
                else obj.transform.localScale = Vector3.one;
            }
        }
    }
    #endregion

    #region 맵
    public Vector3 GetMapSpawnPos(Direction directioln, Transform map)
    {
        var spawnPos = Vector3.zero;

        _mapLeft = new Vector3(map.position.x - 26, map.position.y, 1);
        _mapRight = new Vector3(map.position.x + 26, map.position.y, 1);
        _mapUp = new Vector3(map.position.x, map.position.y + 26, 1);
        _mapDown = new Vector3(map.position.x, map.position.y - 26, 1);

        switch (directioln)
        {
            case Direction.Left:
                return _mapLeft;
            case Direction.Right:
                return _mapRight;
            case Direction.Up:
                return _mapUp;
            case Direction.Down:
                return _mapDown;
            case Direction.LeftUp:
                return _mapLeft + _mapUp - map.position;
            case Direction.LeftDown:
                return _mapLeft + _mapDown - map.position;
            case Direction.RightUp:
                return _mapRight + _mapUp - map.position;
            case Direction.RightDown:
                return _mapRight + _mapDown - map.position;
            case Direction.Off:
                map.GetChild(0).gameObject.SetActive(false);
                ReturnMap(map);
                break;
            default:
                break;
        }

        return Vector3.one * -1;
    }

    private void FirstCreateMap()
    {
        GetMap().position = new Vector3(0,0,1);
        print("first map spawn");
    }

    public void SpawnShop(Transform map)
    {
        // 상점 등장 후 5개의 맵 이동까지 소환 불가
        if (_cantSpawnShop)
        {
            _mapCount++;

            if (_mapCount == 5) _cantSpawnShop = false;
            return;
        }

        var pro = Random.Range(0, 101);

        if (pro > 0) return;

        var shop = map.GetChild(0);
        shop.gameObject.SetActive(true);
        shop.position = new Vector2(Random.Range(map.transform.position.x - 12, map.transform.position.x + 12)
            , Random.Range(map.transform.position.y - 12, map.transform.position.y + 12));

        _mapCount = 0;
        _cantSpawnShop = true;
    }
    #endregion

    #region 풀링
    private void CreatePool()
    {      
        _propPool = new Queue<GameObject>();
        _mapPool = new Queue<Transform>();
        _skillPool = new List<GameObject>();
        _monsterEffectPool = new Queue<GameObject>();

        var tileData = mapData[PlayerInfo.currentMap];

        // 프롭풀링(필드에 놓이는 아이템)
        for (int i = 0; i < _propPoolLimit; i++)
        {
            var obj = Instantiate(_propPrefab, _propParent);
            obj.SetActive(false);

            _propPool.Enqueue(obj);
        }

        // 몬스터 죽는 이팩트 풀링
        for (int i = 0; i < 100; i++)
        {
            var obj = Instantiate(_monsterDieEffect, _skillParent);
            obj.SetActive(false);

            _monsterEffectPool.Enqueue(obj);
        }

        // 맵 풀링
        for (int i = 0; i < 12; i++)
        {
            var obj = Instantiate(_mapPrefab, _mapParent).transform;
            obj.GetChild(2).GetComponent<SpriteRenderer>().sprite = mapData[PlayerInfo.currentMap].background;

            // 맵 타일 랜덤 생성
            _x = -13;
            _y = 12;

            for (int j = 0; j < 676; j++)
            {
                obj.GetComponent<Tilemap>().SetTile(new Vector3Int(_x, _y, 0), tileData.tilebase[Random.Range(0, tileData.tilebase.Count)]);
                _x++;

                if (_x > 12)
                {
                    _x = -13;
                    _y--;
                }
            }

            for (int k = 0; k < 5; k++)
            {
                var mapEtc = Instantiate(tileData.mapEtcObj[Random.Range(0, tileData.mapEtcObj.Count)], obj.GetChild(1).transform);
                mapEtc.transform.position = new Vector2(Random.Range(-13, 14), Random.Range(-13, 14));

                var index = Random.Range(1, 3);
                mapEtc.transform.localScale = new Vector3(index, index, 0);
                mapEtc.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 361)));
            }

            obj.gameObject.SetActive(false);
            _mapPool.Enqueue(obj);
        }

        // 스킬 풀링
        for (int j = 0; j < _skillPrefabList.Count; j++)
        {
            for (int i = 0; i < _skillPoolLimit; i++)
            {
                var obj = Instantiate(_skillPrefabList[j], _skillParent);
                obj.gameObject.SetActive(false);
                obj.name = _skillPrefabList[j].name;
                _skillPool.Add(obj);
            }
        }
    }

    public GameObject GetProp()
    {
        if (_propPool.Count == 0) return null;

        var obj = _propPool.Dequeue();
        obj.SetActive(true);

        return obj;
    }

    public void ReturnProp(GameObject obj)
    {
        obj.SetActive(false);
        _propPool.Enqueue(obj);
    }

    public Transform GetMap()
    {
      var obj = _mapPool.Dequeue();
      
        obj.gameObject.SetActive(true);

        return obj;
    }

    public void ReturnMap(Transform obj)
    {      
        obj.gameObject.SetActive(false);
        _mapPool.Enqueue(obj);
    }

    public GameObject GetSkill(GameObject effectPrefab)
    {
        var index = _skillPool.FindIndex(x => x.name == effectPrefab.name);
        var obj = _skillPool[index];
        _skillPool.RemoveAt(index);

        return obj;
    }

    public void ReturnSkill(GameObject obj)
    {
        obj.transform.position = Vector3.zero;
        obj.SetActive(false);
        _skillPool.Add(obj);
    }

    public GameObject GetDieEffect()
    {
        var obj = _monsterEffectPool.Dequeue();

        return obj;
    }

    public void ReturnDieEffect(GameObject obj)
    {
        obj.SetActive(false);
        _monsterEffectPool.Enqueue(obj);
    }
    #endregion

}
