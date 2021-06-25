/** 
 * 자신이 들고있는 몬스터데이터를 참고하여 해당 몬스터만 주기적으로 소환
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private Spawn _spawn;
    [SerializeField] private float _spawnTimer;
    private float _upgradeTimer;
    private int _upgradeValue;

    [SerializeField] private MonsterScriptable _monsterInfo;

    private Queue<GameObject> _monsterPool;
    private Queue<GameObject> _monsterBulletPool;

    public void Init(MonsterScriptable monsterInfo, Spawn spawn)
    {
        _monsterPool = new Queue<GameObject>();
        _monsterInfo = monsterInfo;
        _spawn = spawn;
        CreatePool();
    }

    private void CreatePool()
    {
        for (int i = 0; i < 100; i++)
        {
            var obj = Instantiate(_monsterInfo.MonsterPrefab, transform);
            obj.GetComponent<Animator>().enabled = false;
            obj.SetActive(false);

            _monsterPool.Enqueue(obj);
        }

        if((int)_monsterInfo.AttackType == 3)
        {
            _monsterBulletPool = new Queue<GameObject>();

            for (int i = 0; i < 50; i++)
            {
                var obj = Instantiate(_monsterInfo.BulletPrefab, transform);
                obj.name = _monsterInfo.BulletPrefab.name;
                obj.SetActive(false);

                _monsterBulletPool.Enqueue(obj);
            }
        }
    }

    public void ReturnMonster(GameObject obj)
    {
       IngameUI.GetInstance().MonsterCount--;
        obj.GetComponent<Animator>().enabled = false;
        obj.SetActive(false);
        _monsterPool.Enqueue(obj);
    }

    public GameObject GetBullet()
    {
        var obj = _monsterBulletPool.Dequeue();
        obj.SetActive(true);

        return obj;
    }

    public void ReturnBullet(GameObject obj)
    {
        obj.SetActive(false);
        _monsterBulletPool.Enqueue(obj);
    }

    // Update is called once per frame
    void Update()
    {
        if (IngameUI.GetInstance().pause) return;

        _spawnTimer += Time.deltaTime;
        _upgradeTimer += Time.deltaTime;

        UpgradeMonster();
        SpawnMonster();
    }

    private void SpawnMonster()
    {
        // 여기 시간 빼주기
        if (_spawnTimer < (_monsterInfo.ReSpawnTime + PlayerInfo.addMonsterCool)) return;
        _spawnTimer = 0;

        for (int i = 0; i < PlayerInfo.addMonsterCount + _monsterInfo.RespawnCount; i++)
        {
            if (_monsterPool.Count == 0) break;

            var obj = _monsterPool.Dequeue();
            obj.SetActive(true);
            obj.GetComponent<Animator>().enabled = true;
            obj.transform.position = _spawn.GetSpawnPos();
            obj.GetComponent<MonsterBase>().Init(_monsterInfo, _spawn, _upgradeValue, this);

            IngameUI.GetInstance().MonsterCount++;
        }
    }

    private void UpgradeMonster()
    {
        if (_upgradeTimer <= 60.0f || _upgradeValue == 50) return;

        _upgradeTimer = 0;
        _upgradeValue += 5;
    }
}
