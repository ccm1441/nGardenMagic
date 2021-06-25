using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class Prop : MonoBehaviour
{
    private Spawn _spawn;
    public GameObject map;
    [SerializeField] private float timer;
    [SerializeField] private bool onTimer;
    [SerializeField] private ItemScritable _itemInfo;
    [SerializeField] private bool _isShop;
    [SerializeField] private AudioClip _shopSound;

    public bool OnTimer
    {
        get => onTimer;
        set
        {
            onTimer = value;
            timer = 0;
        }
    }

    public void Init(Spawn spawn, ItemScritable itemInfo, Sprite image, GameObject map)
    {
        _spawn = spawn;
        _itemInfo = itemInfo;
        this.map = map;
        GetComponent<SpriteRenderer>().sprite = image;
        Invoke("ReturnTrigger", 1f);
    }

    private void ReturnTrigger() => transform.GetComponent<BoxCollider2D>().isTrigger = true;

    private void Update()
    {
        // 맵이 사라지면 아이템 사라지기까지의 카운트 작동
        if (map != null && !map.activeSelf)
        {
            map = null;
            OnTimer = true;           
        }

        // 10초가 지나면 사라짐
        // 그 전에 맵이 생성된다면 플래그 꺼짐 - MapGenerator 에서 관리중
        if (onTimer)
        {
            timer += Time.deltaTime;

            if (timer >= 10)
            {
                OnTimer = false;
                _spawn.ReturnProp(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (_isShop)
            {
                OpenShop();
                GameManager.GetInstance().PlayerEffectSound(GameManager.GetInstance()._fileID_Shop);

                return;
            }
            var sound = 0;

            if (_itemInfo.itemSound == "10Gold") sound = GameManager.GetInstance()._fileID_Coin10;
            else if (_itemInfo.itemSound == "30Gold") sound = GameManager.GetInstance()._fileID_Coin30;
            else if (_itemInfo.itemSound == "100Gold") sound = GameManager.GetInstance()._fileID_Coin100;
            else if (_itemInfo.itemSound == "Box") sound = GameManager.GetInstance()._fileID_Box;
            else if (_itemInfo.itemSound == "HP") sound = GameManager.GetInstance()._fileID_HP;
            else if (_itemInfo.itemSound == "EXP") sound = GameManager.GetInstance()._fileID_EXP;
          
            GameManager.GetInstance().PlayerEffectSound(sound);
            GameManager.GetInstance().RewardItemValue(_itemInfo);
            transform.GetComponent<BoxCollider2D>().isTrigger = false;
            _spawn.ReturnProp(gameObject);
        }
    }


    private void OpenShop()
    {
        IngameUI.GetInstance().ActiveShop(true);

        var shop = GetComponent<Shop>();
        IngameUI.GetInstance().SettingShopItem(shop);
        gameObject.SetActive(false);
    }
}
