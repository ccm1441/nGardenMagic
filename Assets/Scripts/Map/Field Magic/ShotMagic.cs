using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotMagic : MonoBehaviour
{
    // 타이머, 이펙트, 범위
    private MagicScriptable _magicInfo;
    private MagicSpawn _magicSpawn;

    private float _timer;
    [SerializeField] private float _castingTime;
    private Transform _attackEffect;

    private int _range;
    [SerializeField] private Transform _redZone;

    private bool _effectInit;
    private bool _successCasting;
    private bool _finish;

    private Vector3 _tempPos;
    ParticleSystem rain;

    public void Init(MagicScriptable magicinfo, MagicSpawn magicSpawn)
    {
        print("초기화");
         rain = GameManager.GetInstance().player.transform.GetChild(3).GetComponent<ParticleSystem>();
        _magicInfo = magicinfo;
        _magicSpawn = magicSpawn;

        _range = magicinfo.Range;
        _castingTime = magicinfo.CastingTime;
        _attackEffect = magicinfo.AttackEffect.transform;
        _redZone = transform.GetChild(0);
        _redZone.localScale = Vector3.zero;
        transform.localScale = Vector3.one * _range;

        _timer = 0;
        _finish = false;
        _effectInit = false;
        _successCasting = false;

        StartCoroutine(MagicUpdate());
    }

    private void Update()
    {
        _timer += Time.deltaTime;
    }

    IEnumerator MagicUpdate()
    {
        _timer = 0;
        while (!_finish)
        {
            yield return null;

            if (!_effectInit && _timer < _castingTime)
            {
                _redZone.localScale = new Vector3(_timer / _castingTime, _timer / _castingTime, _timer / _castingTime);              
                continue;
            }
           
            _successCasting = true;
            if (_magicInfo.Type == FieldMagicType.Drop) DropMagic();
            else SpawnMagic();       
        }
    }


    private void DropMagic()
    {
        // 캐스팅 완료시
        if (!_effectInit)
        {
            _effectInit = true;
            _attackEffect = _magicSpawn.GetMagic(_magicInfo.Name).transform;
            _attackEffect.gameObject.SetActive(true);
            _attackEffect.position = transform.position + new Vector3(0, 15, 0);
            _tempPos = _attackEffect.position;
            _timer = 0;
        }

        _attackEffect.position = Vector3.Lerp(_tempPos, transform.position, _timer / _magicInfo.AttackTime);

        if (_timer > 2)
        {
            _magicSpawn.ReturnMagic(_attackEffect.gameObject);
            _magicSpawn.ReturnMagic(gameObject);
            _finish = true;
        }
    }

    private void SpawnMagic()
    {
        // 캐스팅 완료시
        if (!_effectInit)
        {
            _effectInit = true;
            _attackEffect = _magicSpawn.GetMagic(_magicInfo.Name).transform;
            _attackEffect.gameObject.SetActive(true);
            _attackEffect.position = transform.position;
            _tempPos = _attackEffect.position;
            _timer = 0;
        }

        if (_magicInfo.Name != "Thunder" && _timer > 2.8f)
        {
            _magicSpawn.ReturnMagic(_attackEffect.gameObject);
            _magicSpawn.ReturnMagic(gameObject);
            _finish = true;
        }
        else if (_magicInfo.Name == "Thunder" && _timer > 0.5f)
        {
            _magicSpawn.ReturnMagic(_attackEffect.gameObject);
            _magicSpawn.ReturnMagic(gameObject);
            _finish = true;
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (_magicInfo.Name == "Thunder" && !rain.isPlaying) rain.Play();

            if (_magicInfo.Type == FieldMagicType.Drop && Vector3.Distance(_attackEffect.position, transform.position) < 0.1f)
            {
                print("필드 마법 맞음");
                collision.GetComponent<Player>().CurrentHP -= _magicInfo.Damage;
                gameObject.GetComponent<CircleCollider2D>().enabled = false;
            }
            else if (_successCasting && _magicInfo.Type == FieldMagicType.Spawn)
            {
                if (_magicInfo.Name == "Thunder")
                {
                    collision.GetComponent<Player>().CurrentHP -= _magicInfo.Damage;
                    gameObject.GetComponent<CircleCollider2D>().enabled = false;
                    return;
                }

                GameManager.GetInstance().player.HalfMoveSpeed(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && _magicInfo.Name == "Thunder")
           rain.Stop();

        if (_successCasting && collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (_magicInfo.Type == FieldMagicType.Spawn)
                GameManager.GetInstance().player.HalfMoveSpeed(false);
        }
    }
}
