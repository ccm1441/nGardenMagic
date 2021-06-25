using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    #region 싱글톤
    private static GameManager instance;

    private void Awake()
    {
        if (instance != null) Destroy(instance);

        instance = this;
    }

    public static GameManager GetInstance()
    {
        if (instance == null)
        {
            print("GameManager 인스턴스 없음!");
            return null;
        }

        return instance;
    }
    #endregion

    public int stageInfo;
    private IngameUI _ingameUI;

    [Header("● 데이터")]
    [SerializeField] private List<RuneScriptable> _runeClassData;
    [SerializeField] private List<RuneScriptable> _runeNormalData;
    [SerializeField] private List<SkillScriptable> _weaponData;
    [SerializeField] private List<SkillScriptable> _passiveData;
    [SerializeField] private List<SkillScriptable> _skillData;
    private float _gameTimer;
    [HideInInspector] public float hours, minute, second;
    public string currentTime;
    public Vector3 playerBeforePosition;
    public int settingFrameCount = 2;
    private int _frameCount;

    [Header("● Component")]
    public Camera camera;
    public Player player;
    public Spawn spawn;
    public AdsManager adsManager;

    [Header("● Map")]
    [SerializeField] private GameObject[] _mapArray;
    public Transform mapParent;
    public Transform magicParent;

    [Header("● Sound")]
    [SerializeField] private GameObject audioParent;
    [SerializeField] private List<AudioSource> audioList;
    [SerializeField] private AudioMixerGroup _mixer;
    private int _currentAudioIndex;
    public int _fileID_Die, _fileID_Yes, _fileID_No, _fileID_Box, _fileID_Coin10, _fileID_Coin30, _fileID_Coin100, _fileID_MonsterDie, _fileID_HP, _fileID_EXP, _fileID_Shop;
    public int _skill_FireBall, _skill_FireStrike, _skill_LightingBolt, _skill_LightingThunder, _skill_LightingStorm, _skill_Whomhole, _skill_WindStorm;
    public int _skill_EarthBoom, _skill_EarthEarthquake, _skill_EarthMissile, _skill_EarthWall, _skill_FireStep, _skill_WaterBlast, _skill_WaterField, _skill_WindCutter;
   
    // 확률적 뽑기
    private float _totalProbability;
    private float _weight;

    // 테스트
    public int testRemoveLimit = 30;

    private void Start()
    {
       adsManager = GameObject.Find("Ads").GetComponent<AdsManager>();

        Application.targetFrameRate = 60;
        Time.timeScale = 1;

        _skillData = new List<SkillScriptable>();
        _skillData.AddRange(_weaponData);
        _skillData.AddRange(_passiveData);

        _ingameUI = IngameUI.GetInstance();

        CreateAudioSource();
        StartCoroutine(UpdatePlayerPositionOfFrame());
    }

    private void Update()
    {
        if (_ingameUI.pause) return;

        CalculateTime();

        _frameCount++;
    }

    IEnumerator UpdatePlayerPositionOfFrame()
    {
        while (true)
        {
            yield return new WaitUntil(() => _frameCount >= settingFrameCount);
            playerBeforePosition = player.transform.position;
            _frameCount = 0;
        }
    }

    private void CreateAudioSource()
    {
        _fileID_Yes = AndroidNativeAudio.load("Click Sound/Yes.wav");
        _fileID_No = AndroidNativeAudio.load("Click Sound/No.wav");
        _fileID_Box = AndroidNativeAudio.load("Box Sound/Box.wav");
        _fileID_Coin10 = AndroidNativeAudio.load("Coin Sound/10 Gold.wav");
        _fileID_Coin30 = AndroidNativeAudio.load("Coin Sound/30 Gold.wav");
        _fileID_Coin100 = AndroidNativeAudio.load("Coin Sound/100 Gold.wav");
        _fileID_MonsterDie = AndroidNativeAudio.load("Die Sound/Monster_Die.wav");
        _fileID_HP = AndroidNativeAudio.load("Potion Sound/Hp Potion.wav");
        _fileID_EXP = AndroidNativeAudio.load("Potion Sound/Exp Potion.wav");
        _fileID_Shop = AndroidNativeAudio.load("Shop Sound/Shop.wav");
        _fileID_Die = AndroidNativeAudio.load("Die Sound/Die.wav");

        // 스킬사운드
        _skill_FireBall = AndroidNativeAudio.load("Skill Sound/Fire_ball.wav");
        _skill_FireStrike = AndroidNativeAudio.load("Skill Sound/Fire_Strike.wav");
        _skill_LightingBolt = AndroidNativeAudio.load("Skill Sound/Lightning_bolt.wav");
        _skill_LightingThunder = AndroidNativeAudio.load("Skill Sound/Lightning_Thunder.wav");
        _skill_LightingStorm = AndroidNativeAudio.load("Skill Sound/Lightning_storm.wav");
        _skill_Whomhole = AndroidNativeAudio.load("Skill Sound/Whom_hole.wav");
        _skill_WindStorm = AndroidNativeAudio.load("Skill Sound/Wind_storm.wav");
        _skill_EarthBoom = AndroidNativeAudio.load("Skill Sound/Earth_boom.wav");
        _skill_EarthEarthquake = AndroidNativeAudio.load("Skill Sound/Earth_earthquake.wav");
        _skill_EarthMissile = AndroidNativeAudio.load("Skill Sound/Earth_missile.wav");
        _skill_EarthWall = AndroidNativeAudio.load("Skill Sound/Earth_wall.wav");
        _skill_FireStep = AndroidNativeAudio.load("Skill Sound/Fire_step.wav");
        _skill_WaterBlast = AndroidNativeAudio.load("Skill Sound/Water_blast.wav");
        _skill_WaterField = AndroidNativeAudio.load("Skill Sound/Water_field.wav");
        _skill_WindCutter = AndroidNativeAudio.load("Skill Sound/Wind_cutter.wav");


        //for (int i = 0; i < 30; i++)
        //{
        //    var component = audioParent.AddComponent<AudioSource>();
        //    component.outputAudioMixerGroup = _mixer;
        //    audioList.Add(component);
        //}
    }

    public void UnloadSound()
    {
        AndroidNativeAudio.unload(_fileID_Box);
        AndroidNativeAudio.unload(_fileID_HP);
        AndroidNativeAudio.unload(_fileID_EXP);
        AndroidNativeAudio.unload(_fileID_Coin10);
        AndroidNativeAudio.unload(_fileID_Coin30);
        AndroidNativeAudio.unload(_fileID_Coin100);
        AndroidNativeAudio.unload(_fileID_Yes);
        AndroidNativeAudio.unload(_fileID_No);
        AndroidNativeAudio.unload(_fileID_MonsterDie);
        AndroidNativeAudio.unload(_fileID_Shop);
        AndroidNativeAudio.unload(_fileID_Die);

        AndroidNativeAudio.unload(_skill_FireBall);
        AndroidNativeAudio.unload(_skill_FireStrike);
        AndroidNativeAudio.unload(_skill_LightingBolt);
        AndroidNativeAudio.unload(_skill_LightingThunder);
        AndroidNativeAudio.unload(_skill_LightingStorm);
        AndroidNativeAudio.unload(_skill_Whomhole);
        AndroidNativeAudio.unload(_skill_WindStorm);
        AndroidNativeAudio.unload(_skill_EarthBoom);
        AndroidNativeAudio.unload(_skill_EarthEarthquake);
        AndroidNativeAudio.unload(_skill_EarthMissile);
        AndroidNativeAudio.unload(_skill_EarthWall);
        AndroidNativeAudio.unload(_skill_FireStep);
        AndroidNativeAudio.unload(_skill_WaterBlast);
        AndroidNativeAudio.unload(_skill_WaterField);
        AndroidNativeAudio.unload(_skill_WindCutter);
    }

    //public void PlayerEffectSound(AudioClip clip, float volume = 1f)
    public void PlayerEffectSound(int id, float volume = 1f)
    {
        AndroidNativeAudio.play(id,volume);
    }

    public void CalculateTime()
    {
        _gameTimer += Time.deltaTime;

        second = _gameTimer;

        if ((int)second >= 59)
        {
            _gameTimer = 0;
            minute += 1;
        }
        if ((int)minute >= 59)
        {
            minute = 0;
            hours += 1;
        }

        currentTime = hours.ToString("00") + ":" + minute.ToString("00") + ":" + second.ToString("00"); // $"{hours:00}:{minute:00}:{second:00}";
        _ingameUI.UpdateGameTime(currentTime);
    }

    public int GetGameSecond()
    {
        return ((int)hours * 3600) + ((int)minute * 60) + (int)second;
    }

    public void ResetSkillData()
    {
        _skillData.Clear();
        _skillData.AddRange(_weaponData);
    }

    public void GetRandomRunes(ItemScritable itemInfo)
    {
        // 일반인지 등급인지 결정
        var randomType = Random.Range(0, 100);

        // 일반 룬일시
        if (randomType <= itemInfo.DetailNormalProbability)
        {
            print("일반 룬 획득");
            var index = Random.Range(0, _runeNormalData.Count);

            if (CheckingRuneInventory(_runeNormalData[index]))
            {
                GetRandomRunes(itemInfo);
                print("다시 돌림");
                return;
            }

            print("유아이 온");
            IngameUI.GetInstance().SetSelectRuneUI(_runeNormalData[index]);
            return;
        }

        print("등급 룬 획득");
        // 등급 룬일시
        var randomRune = _runeClassData[Random.Range(0, _runeClassData.Count)];
        var randomClass = Random.Range(0, 100);
        var info = itemInfo.ProbabilityList;

        for (int i = 0; i < info.Count; i++)
        {
            if (randomClass >= info[i].probability || i == info.Count -1)
            {
                if (CheckingRuneInventory(randomRune, info[i].runeClass))
                {
                    GetRandomRunes(itemInfo);
                    print("다시 돌림");
                    return;
                }
                print("유아이 온");
                _ingameUI.SetSelectRuneUI(randomRune, info[i].runeClass);
                break;
            }
        }
    }

    //해당 룬을 가지고 있는지 검사 > 있으면 TRUE 다시 뽑음
    public bool CheckingRuneInventory(RuneScriptable rune, RuneClass runeClass = RuneClass.All)
    {
        for (int i = 0; i < 3; i++)
        {
            if (player.runeInventory[i].runeInfo == null) continue;

            if (player.runeInventory[i].runeInfo.Name == rune.Name)
            {
                // 등급룬 일시
                if (rune.UseClass)
                {
                    if (player.runeInventory[i].runeClass == runeClass) return true;
                    else return false;
                }

                // 일반룬일시
                return true;
            }
        }

        return false;
    }


    // 스킬 뽑기 리스트에서 삭제
    public void DeleteSkillIndex(SkillScriptable data)
    {
        if (data.StaffName == "기본 지팡이") return;

        var index = _skillData.FindIndex(x => x.Name == data.Name);
        print(index);
        _skillData.RemoveAt(index);
    }

    // 스킬 뽑기 리스트에서 교체
    public void ChangeSkillIndex(SkillInventory inventory, SkillScriptable skillInfo)
    {
        var index = _skillData.FindIndex(x => x.StaffName == skillInfo.StaffName);
        print("리스트 인덱스 " + index);
        player.skillInventory[inventory.slotIndex].originSkill = _skillData[index];
        player.skillInventory[inventory.slotIndex].skillInfo = skillInfo;
        _skillData[index] = skillInfo;
    }

    // 원래 지팡이로 되돌림
    public void ReturnOriginSkill(SkillInventory inventory)
    {
      //   var index = _skillData.FindIndex(x => x.StaffName == inventory.skillInfo.StaffName);
        var index = _skillData.FindIndex(x => x.Name == inventory.skillInfo.Name);
        _skillData[index] = inventory.originSkill;
    }

    // 무기, 패시브를 랜덤으로 뽑아 리스트화 한뒤 넘김
    public List<SkillInventory> GetSelectRandomWeapon()
    {
        var weaponList = new List<SkillInventory>();

        // 추후 뽑은 데이터를 지우기위해 리스트 복사
        var selectList = _skillData.ConvertAll(x => x);

        for (int i = 0; i < player.selectSkillCount; i++)
        {
            var randomIndex = Random.Range(0, selectList.Count);
            var skillInfo = selectList[randomIndex];

            var index = player.CheckSkill(skillInfo);

            var slot = new SkillInventory();
            // 새로운 스킬
            if (index == -1)
            {               
                slot.skillInfo = skillInfo;
                weaponList.Add(slot);
                selectList.RemoveAt(randomIndex);
            }
            else // 가지고 있는것
            {
                slot.skillInfo = player.skillInventory[index].skillInfo;
                slot.skillLevel += player.skillInventory[index].skillLevel;
                weaponList.Add(slot);
                selectList.RemoveAt(randomIndex);
            }
        }

        return weaponList;
    }
    public void RewardItemValue(ItemScritable itemInfo)
    {
        switch (itemInfo.Type)
        {
            case ItemType.EXP_Value:
                player.CalculateEXP(itemInfo.Value, true);
                print("경험치 " + itemInfo.Value + "상승 완료");
                break;
            case ItemType.EXP_Percentage:
                player.CalculateEXP(player._maxExp * itemInfo.Value * 0.01f, true);
                print("경험치 " + itemInfo.Value + "% 상승 완료");
                break;
            case ItemType.LevelUP:
                player.PlayerLevel += (int)itemInfo.Value;
                print("레벨 " + itemInfo.Value + "상승 완료");
                break;
            case ItemType.HP_Value:
                player.CurrentHP += itemInfo.Value + (itemInfo.Value * player.addHP * 0.01f);
                print("체력 " + itemInfo.Value + "회복 완료");
                break;
            case ItemType.HP_Percentage:
                player.CurrentHP += (player._addMaxHP * itemInfo.Value * 0.01f) + (itemInfo.Value * player.addHP * 0.01f);
                print("체력 " + itemInfo.Value + "% 회복 완료");
                break;
            case ItemType.Reborn:
                player._reBorn = true;
                print("부활 소지");
                break;
            case ItemType.RandomRune:
                GetRandomRunes(itemInfo);
                break;
            case ItemType.Gold:
                player.Gold += (int)itemInfo.Value;
                break;
            default:
                break;
        }
    }

    #region 확률

    // 아이템 리스트에 대하여 확률적으로 뽑아서 줌
    public ItemScritable ProbabilityItem(List<ItemScritable> itemList)
    {
        // 고정 확률 (1개)
        if (itemList.Count == 1)
        {
            var index = Random.Range(0, 101);
            if (index <= itemList[0].Probability) return itemList[0];
            else return null;
        }

        ProbabilityInit(itemList);
        ShuffleList(itemList);

        var selectIndex = _totalProbability * Random.Range(0.0f, 1.0f);

        for (int i = 0; i < itemList.Count; i++)
        {
            _weight += itemList[i].Probability;

            if (selectIndex <= _weight) return itemList[i];
        }

        return null;
    }

    private void ProbabilityInit(List<ItemScritable> itemList)
    {
        _totalProbability = 0;
        _weight = 0;

        for (int i = 0; i < itemList.Count; i++)
            _totalProbability += itemList[i].Probability;
    }

    // 순서섞기
    private void ShuffleList(List<ItemScritable> itemList)
    {
        var count = itemList.Count;

        for (int i = 0; i < count; i++)
        {
            var index = Random.Range(0, count);

            var temp = itemList[index];
            itemList[index] = itemList[i];
            itemList[i] = temp;
        }
    }
    #endregion
}

public enum Direction
{
    Left,
    Right,
    Up,
    Down,
    LeftUp,
    LeftDown,
    RightUp,
    RightDown,
    Off,
}
