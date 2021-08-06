using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using GooglePlayGames;

/// <summary>
/// 로비 UI를 관리
/// </summary>

public class LobbyUI : UIManager
{
    // 안드로이드에서 뒤로가기
    [Header("===== Android Func =====]")]
    [SerializeField] private AndroidSet _androidSet;                    // 안드롤이드에서 뒤로가기
    private int _doubleClick = 0;                                       // 더블클릭 여부

    [Header("===== UI =====")]
    [SerializeField] private List<GameObject> _lobbyUI;                 // 로비 UI 리스트
    public Text versionText;

    [Header("==== Weapon =====")]
    [SerializeField] private DataSet _staffData;                        // 고유 무기 데이터 셋
    [SerializeField] private Transform _weaponButtonParent;             // 고유 무기 부모 오브젝트
    [SerializeField] private Transform _weaponInfoParent;               // 고유 무기 정보 부모 오브젝트
    private SkillScriptable _firstselect;                               // 첫번째 무기로 들고갈 무기 정보 
    [SerializeField] private Transform _firstSelectUI;                  // 현재 어떤 무기를 선택하는가 나타낼수 있는 오브젝트

    [Header("===== Character =====")]
    [SerializeField] private GameObject _characterObj;                  // 캐릭터가 모여있는 오브젝트
    private List<GameObject> _characterList;                            // 캐릭터 리스트
    private int _characterListPage;                                     // 현재 보고있는 캐릭터 인덱스

    [Header("===== Upgrade =====")]
    private int _currentSkillPrice;                                     // 현재 업그레이드 비용
    [SerializeField] private int _startSkillIncreasePrice;              // 시작 업그레이드 비용
    [SerializeField] private List<SkillPriceTable> table;               // 업그레이드 할때마다 테이블 내 경험치 적용
    private int _currentSelect;                                         // 현재 보고 있는 캐릭터 인덱스
    private int _applyIndex;                                            // 현재 적용되어있는 캐릭터 인덱스
    [HideInInspector] public int skillAllLevel, currentChar;            // 현재 업그레이드 한 스킬 총 레벨

    [Header("===== Shop =====")]
    [SerializeField] private Button _removeAdButton;                    // 광고 제거 구매 버튼
    [SerializeField] private Button _doubleRewardButton;                // 리워드 2배 구매 버튼

    [Header("===== Optiion =====")]
    [SerializeField] private List<Toggle> _optionToggleList;            // 옵션 토글 리스트(사운드, 컨트롤러)
    [SerializeField] private AudioMixer _mixer;                         // 오디오 믹서

    [Header("===== Map =====")]
    [SerializeField] private List<MapScriptable> mapList;               // 맵 데이터 리스트
    private int _mapListPage;                                           // 현재 보고있는 맵 인덱스

    [HideInInspector] public int[] characterBuyArray;                   // 캐릭터 구매 정보 배열
    [HideInInspector] public int[] mapOpenArray;                        // 맵 오픈 정보 배열

    public bool login;
    // 업그레이드 스킬 정보 배열
  // [HideInInspector]
    public UpgradSkillInfo[] upgradeDataArray = new UpgradSkillInfo[15]
{
        new UpgradSkillInfo {type = PropertyType.Fire, ability = SkillAbility.PropertyDamage, name_KR = "화염 속성 증가", currentLevel = 0,maxLevel = 3, value = 10},
        new UpgradSkillInfo {type = PropertyType.Water,ability = SkillAbility.PropertyDamage, name_KR = "물 속성 증가", currentLevel = 0,maxLevel = 3, value = 10},
        new UpgradSkillInfo {type = PropertyType.Electricity,ability = SkillAbility.PropertyDamage, name_KR = "전기 속성 증가", currentLevel = 0,maxLevel = 3, value = 10},
        new UpgradSkillInfo {type = PropertyType.Earth,ability = SkillAbility.PropertyDamage, name_KR = "대지 속성 증가", currentLevel = 0,maxLevel = 3, value = 10},
        new UpgradSkillInfo {type = PropertyType.Wind,ability = SkillAbility.PropertyDamage, name_KR = "바람 속성 증가", currentLevel = 0,maxLevel = 3, value = 10},
        new UpgradSkillInfo {type = PropertyType.Fire,ability = SkillAbility.PropertyCoolTime, name_KR = "화염 속성 쿨타임 감소", currentLevel = 0,maxLevel = 4, value = -5},
        new UpgradSkillInfo {type = PropertyType.Water,ability = SkillAbility.PropertyCoolTime, name_KR = "물 속성 쿨타임 감소", currentLevel = 0,maxLevel = 4, value = -5},
        new UpgradSkillInfo {type = PropertyType.Electricity,ability = SkillAbility.PropertyCoolTime, name_KR = "전기 속성 쿨타임 감소", currentLevel = 0,maxLevel = 4, value = -5},
        new UpgradSkillInfo {type = PropertyType.Earth,ability = SkillAbility.PropertyCoolTime, name_KR = "대지 속성 쿨타임 감소", currentLevel = 0,maxLevel = 4, value = -5},
        new UpgradSkillInfo {type = PropertyType.Wind,ability = SkillAbility.PropertyCoolTime, name_KR = "바람 속성 쿨타임 감소", currentLevel = 0,maxLevel = 4, value = -5},
        new UpgradSkillInfo {type = PropertyType.Null,ability = SkillAbility.MaxHP, name_KR = "생명력 증가", currentLevel = 0,maxLevel = 2, value = 25},
        new UpgradSkillInfo {type = PropertyType.Null,ability = SkillAbility.HPPotionAbility, name_KR = "회복력 증가", currentLevel = 0,maxLevel = 2, value = 25},
        new UpgradSkillInfo {type = PropertyType.Null,ability = SkillAbility.ReceivedDamage, name_KR = "방어력 증가", currentLevel = 0,maxLevel = 2, value = 10},
        new UpgradSkillInfo {type = PropertyType.Null,ability = SkillAbility.MoveSpeed, name_KR = "이동 속도 증가", currentLevel = 0,maxLevel = 2, value = 5},
        new UpgradSkillInfo {type = PropertyType.Null,ability = SkillAbility.EXPPotionAbility, name_KR = "경험치 증가", currentLevel = 0,maxLevel = 2, value =20}
};

    // 사운드
    int FileID_Yes;
    int FileID_No;
    int SoundID;

    private void Start()
    {
        versionText.text = Application.version;
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        print("로그인 진입 직전");
        Login();
        ///////////////////////////////////////

        _audioSource = GetComponent<AudioSource>();

        // 상점에 표시할 캐릭터 오브젝트 저장
        _characterList = new List<GameObject>();
        for (int i = 0; i < _characterObj.transform.childCount; i++)
            _characterList.Add(_characterObj.transform.GetChild(i).gameObject);

        // 데이터 초기화
        DataInit();

        AndroidNativeAudio.makePool();
        FileID_Yes = AndroidNativeAudio.load("Click Sound/Yes.wav");
        FileID_No = AndroidNativeAudio.load("Click Sound/No.wav");
    }

#if UNITY_ANDROID
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _doubleClick++;
            _androidSet.ShowToast("한번 더 누르면 게임을 종료합니다.");
            if (!IsInvoking("DoubleClick"))
                Invoke("DoubleClick", 0.5f);

        }
        else if (_doubleClick == 2)
        {
            CancelInvoke("DoubleClick");
            Application.Quit();
        }
    }
#endif

    /// <summary>
    /// 유저 데이터 초기화 및 로드
    /// </summary>
    private void DataInit()
    {
        PlayerInfo.gold = 3000;
        PlayerInfo.character = PlayerInfo.nextOpenTime = 0;
        PlayerInfo.originAttack = PlayerInfo.originSpeed = PlayerInfo.originHP = PlayerInfo.addMonsterCool = PlayerInfo.addMonsterCount = 0;
        PlayerInfo.firePropertyAttack = PlayerInfo.waterPropertyAttack = PlayerInfo.electricPropertyAttack = PlayerInfo.earthPropertyAttack = PlayerInfo.windPropertyAttack = 0;
        PlayerInfo.firePropertyCool = PlayerInfo.waterPropertyCool = PlayerInfo.electricPropertyCool = PlayerInfo.earthPropertyCool = PlayerInfo.windPropertyCool = 0;
        PlayerInfo.maxHP = PlayerInfo.portionHP = PlayerInfo.passiveDamage = PlayerInfo.portionEXP = PlayerInfo.moveSpeed = PlayerInfo.monsterEXP = PlayerInfo.addSelectWeapon = 0;
        PlayerInfo.reborn = PlayerInfo.protectSkill = PlayerInfo.removeADs = PlayerInfo.doubleReward = false;
        PlayerInfo.sound = true;
        PlayerInfo.startSkill = null;
        PlayerInfo.weaponOpenArray = new int[10] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        skillAllLevel = currentChar = 0;
        characterBuyArray = new int[3] { 1, 0, 0 };
        mapOpenArray = new int[3] { 1, 0, 0 };


        // 데이터 로드 후 파일이 없다면 저장후 재로드
        if (DataManager.GetInstance().Load() == null)
        {
            DataManager.GetInstance().SaveSkill(upgradeDataArray, characterBuyArray, 0, mapOpenArray);
            DataManager.GetInstance().SaveGold(300);
            DataManager.GetInstance().Load();
            return;
        }

        #region 로드한 데이터 업데이트
        // 업그레이드 스킬 정보 업데이트
        for (int i = 0; i < 15; i++)
        {
            upgradeDataArray[i].currentLevel = DataManager.GetInstance().skillData.currentLevel[i];
            upgradeDataArray[i].maxLevel = DataManager.GetInstance().skillData.maxLevel[i];
            skillAllLevel += upgradeDataArray[i].currentLevel;
            SetAbility(upgradeDataArray[i]);
        }

        // 업그레이드 스킬 가격 재설정
        for (int i = 0; i < skillAllLevel; i++)
        {
            _currentSkillPrice += _startSkillIncreasePrice;

            if (i + 1 == table[_applyIndex].changeLevel)
            {
                _applyIndex++;
                _startSkillIncreasePrice += table[_applyIndex].increaseValue;
            }
        }

        // 고유 무기 오픈 정보 업데이트
        for (int i = 0; i < PlayerInfo.weaponOpenArray.Length; i++)
            PlayerInfo.weaponOpenArray[i] = DataManager.GetInstance().data.weaponOpenArray[i];

        // 캐릭터 구매 정보 업데이트
        for (int i = 0; i < 3; i++)
            characterBuyArray[i] = DataManager.GetInstance().skillData.characterBuy[i];

        currentChar = DataManager.GetInstance().skillData.applyCharacter;
        _characterListPage = currentChar;

        // 맵 오픈 정보 업데이트
        for (int i = 0; i < 3; i++)
        {
            mapOpenArray[i] = DataManager.GetInstance().skillData.mapOpenArray[i];
            if (mapOpenArray[i] == 1) _mapListPage = i;
        }

        // 플레이어 컨트롤 방식 업데이트
        PlayerInfo.controllerFixed = DataManager.GetInstance().skillData.controllerFixed;
        #endregion

        // 게임 종료 후 다음 맵 오픈 조건을 달성하면 오픈
        if (PlayerInfo.nextMapOpen)
        {
            _mapListPage++;
            if (_mapListPage == mapOpenArray.Length) _mapListPage = mapOpenArray.Length - 1;
            mapOpenArray[_mapListPage] = 1;
            PlayerInfo.nextMapOpen = false;
            DataManager.GetInstance().SaveSkill(upgradeDataArray, characterBuyArray, currentChar, mapOpenArray);
        }

        // 사운드 설정
        PlayerInfo.sound = DataManager.GetInstance().skillData.sound;
        if (!PlayerInfo.sound) _mixer.SetFloat("Main", -80f);
    }

    #region 공통
    public void SetActiveUI(string value)
    {
        base.SetActiveUI(_lobbyUI, value);
    }

    public void LoadNextSceneButton(string value) => base.LoadNextScene(value);

    // 사운드 출력
    public void PlaySound(bool isOn)
    {
        if(isOn) AndroidNativeAudio.play(FileID_Yes);
        else AndroidNativeAudio.play(FileID_No);
    }
    
    // toggle 전용
    public void TogglePlaySound(Toggle toggle)
    {
        if (toggle.isOn == false || EventSystem.current.currentSelectedGameObject.name.Contains("Button")) return;
        AndroidNativeAudio.play(FileID_Yes);
    }

    private void Login()
    {
        // 구글 로그인
        Social.localUser.Authenticate((bool success) =>
        {
            if (success)
            {
                print("로그인 성공");
                login = true;
            }
            else
            {
                print("로그인 실패");
                login = false;
                var shopButton = _lobbyUI[base.GetUIIndex(_lobbyUI, "Lobby")];
                shopButton.transform.GetChild(4).GetComponent<Button>().interactable = false;
            }
        });
    }
    #endregion

    #region 캐릭터 구매
    // 페이지에 따른 캐릭터 출력 변경
    public void ActiveCharacter()
    {
        // 캐릭터 셋팅
        _characterObj.SetActive(true);

        // 모든 캐릭터 끔
        for (int i = 0; i < _characterList.Count; i++)
            _characterList[i].SetActive(false);

        // 장착한 캐릭터 오픈
        _characterList[currentChar].SetActive(true);
        _characterListPage = currentChar;
        var charShop = _lobbyUI[base.GetUIIndex(_lobbyUI, "SelectChar")].transform;
        charShop.GetChild(charShop.childCount - 1).GetComponent<Text>().text = PlayerInfo.gold == 0 ? "0" : PlayerInfo.gold.ToString("#,###");

        // 캐릭터이름 및 설명 셋팅
        charShop.GetChild(3).GetChild(0).GetChild(0).GetComponent<Text>().text = _characterList[currentChar].GetComponent<CharacterInfo>().info.playerName_KR;
        charShop.GetChild(3).GetChild(1).GetChild(0).GetComponent<Text>().text = _characterList[currentChar].GetComponent<CharacterInfo>().info.explain_KR;


        var buyButton = charShop.GetChild(4);

        buyButton.GetChild(0).GetComponent<Text>().text = "착용중";
        buyButton.GetComponent<Button>().interactable = false;

        // 좌우 화살표버튼 기능
        // 왼쪽 버튼
        var button = charShop.GetChild(5).GetComponent<Button>();
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            _characterList[_characterListPage].SetActive(false);

            if (_characterListPage == 0) _characterListPage = _characterList.Count - 1;
            else _characterListPage--;

            _characterList[_characterListPage].SetActive(true);
            // 캐릭터이름 및 설명 셋팅
            charShop.GetChild(3).GetChild(0).GetChild(0).GetComponent<Text>().text = _characterList[_characterListPage].GetComponent<CharacterInfo>().info.playerName_KR;
            charShop.GetChild(3).GetChild(1).GetChild(0).GetComponent<Text>().text = _characterList[_characterListPage].GetComponent<CharacterInfo>().info.explain_KR;

            SetCharacterShopButton(buyButton);

        });

        // 오른족 버튼
        button = charShop.GetChild(6).GetComponent<Button>();
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            _characterList[_characterListPage].SetActive(false);

            if (_characterListPage == _characterList.Count - 1) _characterListPage = 0;
            else _characterListPage++;

            _characterList[_characterListPage].SetActive(true);

            // 캐릭터이름 및 설명 셋팅
            charShop.GetChild(3).GetChild(0).GetChild(0).GetComponent<Text>().text = _characterList[_characterListPage].GetComponent<CharacterInfo>().info.playerName_KR;
            charShop.GetChild(3).GetChild(1).GetChild(0).GetComponent<Text>().text = _characterList[_characterListPage].GetComponent<CharacterInfo>().info.explain_KR;
            SetCharacterShopButton(buyButton);
        });

        SetActiveUI("Character|on");
    }

    // 캐릭터 구매여부에 따른 버튼 데이터 변경
    private void SetCharacterShopButton(Transform buyButton)
    {
        // 구매 안한 상태
        if (characterBuyArray[_characterListPage] == 0)
        {
            buyButton.GetChild(0).GetComponent<Text>().text = _characterList[_characterListPage].GetComponent<CharacterInfo>().info.price.ToString("#,###");
            buyButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            // 구매 했는데 착용 안한 상태
            if (currentChar != _characterListPage)
            {
                buyButton.GetChild(0).GetComponent<Text>().text = "착용하기";
                buyButton.GetComponent<Button>().interactable = true;
            }
            else
            {
                buyButton.GetChild(0).GetComponent<Text>().text = "착용중";
                buyButton.GetComponent<Button>().interactable = false;
            }
        }
    }

    public void PassiveCharacter() => _characterObj.SetActive(false);

    // 상점에서 캐릭터 구매 눌렀을때
    public void SetShopPopUpActive()
    {
        var buttonText = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;

        if (characterBuyArray[_characterListPage] == 1)
        {
            // 캐릭터 교체
            currentChar = _characterListPage;
            ActiveCharacter();
            DataManager.GetInstance().SaveSkill(upgradeDataArray, characterBuyArray, currentChar, mapOpenArray);
            return;
        }

        base.SetPopUpUIActive(buttonText + "를 지불하여 해당 캐릭터를 구매 하시겠습니까?", true, true);

        // 선택 팝업에서 오른쪽 버튼
        base.SetPopUpButtonAction("예", AddButtonActionBuy, true);

        // 선택 팝업에서 왼쪽 버튼
        base.SetPopUpButtonAction("아니요", AddButtonActionPassiveUI, false);

    }

    // 상점에서 캐릭터 구매 확정지을거냐
    private void AddButtonActionBuy(Button button)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            var charPrice = _characterList[_characterListPage].GetComponent<CharacterInfo>().info.price;

            //  돈 부족
            if (PlayerInfo.gold - charPrice < 0)
            {

                SetPopUpUIActive("보유한 재화가 부족 합니다.", true, true);
                // 선택 팝업에서 오른쪽 버튼
                base.SetPopUpButtonAction("확인", AddButtonActionPassiveUI, true);

                // 선택 팝업에서 왼쪽 버튼
                base.SetPopUpButtonAction("상점", AddButtonActionOpenShop, false);
                return;
            }

            SetPopUpUIActive(string.Empty, true, false);
            SetPopUpUIActive("구매가 완료 되었습니다", false, true);
            PlayerInfo.gold -= charPrice;
            currentChar = _characterListPage;
            characterBuyArray[_characterListPage] = 1;
            ActiveCharacter();

            DataManager.GetInstance().SaveSkill(upgradeDataArray, characterBuyArray, currentChar, mapOpenArray);
            DataManager.GetInstance().SaveGold(PlayerInfo.gold);

            var Popbutton = _popUpUI[1].transform.GetChild(3).GetComponent<Button>();
            SetPopUpButtonAction("확인", AddButtonActionPassiveUI);
        });
    }

    // 상점 오픈
    private void AddButtonActionOpenShop(Button button)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            PassiveCharacter();
            _characterListPage = currentChar;
            SetActiveUI(_lobbyUI, "Character|off");
            SetActiveUI(_lobbyUI, "Shop|on");
            EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.SetActive(false);
        });
    }

    #endregion

    #region 상점
    public void SettingShop()
    {
        if (PlayerInfo.removeADs)
        {
            _removeAdButton.interactable = false;
            _removeAdButton.transform.GetChild(0).GetComponent<Text>().text = "구매 완료";
            _removeAdButton.transform.GetChild(1).GetComponent<Text>().text = "구매 완료";
        }

        if (PlayerInfo.doubleReward)
        {
            _doubleRewardButton.interactable = false;
            _doubleRewardButton.transform.GetChild(0).GetComponent<Text>().text = "구매 완료";
            _doubleRewardButton.transform.GetChild(1).GetComponent<Text>().text = "구매 완료";
        }
    }

    public void ShopDataSave() => DataManager.GetInstance().SaveSkill(upgradeDataArray, characterBuyArray, 0, mapOpenArray);

    public void ActiveShopBuyMessage(bool result)
    {
        //var text = EventSystem.current.currentSelectedGameObject.transform.parent.GetChild(1).GetComponent<Text>().text;
        //SetPopUpUIActive(text + " 상품을 구매 하시겠습니까?", true, true);

        //// 선택 팝업에서 오른쪽 버튼
        //var PopButton = _popUpUI[0].transform.GetChild(3).GetComponent<Button>();
        //base.SetPopUpButtonAction("예", AddButtonShopBuy, true);

        //// 선택 팝업에서 왼쪽 버튼
        //base.SetPopUpButtonAction("아니요", AddButtonActionPassiveUI, false);
        print("결제 성공");
        SetPopUpUIActive(result ? "결제 성공!" : "결제 실패", false, true);
        base.SetPopUpButtonAction("확인", AddButtonActionPassiveUI);
    }

    private void AddButtonShopBuy(Button button)
    {
        button.onClick.AddListener(() =>
        {
            // 성공 실패 판단후 팝업 띄우기
            SetPopUpUIActive(string.Empty, true, false);
            SetPopUpUIActive("결제 성공!", false, true);

            base.SetPopUpButtonAction("확인", AddButtonActionPassiveUI);
        });
    }

    #endregion

    #region 게임 시작전 준비
    // 들어오면 무기 정보 세팅, 1번 무기등록하기
    public void SetFirstWeapon()
    {
        for (int i = 0; i < _staffData.data.Count; i++)
        {
            _weaponButtonParent.GetChild(i).GetChild(0).GetComponent<Image>().sprite = _staffData.data[i].Image;
            _weaponButtonParent.GetChild(i).GetChild(0).GetComponent<Image>().SetNativeSize();

            _weaponButtonParent.GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().sprite = _staffData.data[i].Image;
            _weaponButtonParent.GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().SetNativeSize();

            AddFuncButtonStaff(_weaponButtonParent.GetChild(i).GetComponent<Toggle>(), _staffData.data[i], i);
        }

        _weaponButtonParent.GetChild(0).GetComponent<Toggle>().isOn = true;
    }

    // 스태프 누를때마다 옵션 바뀌도록
    private void AddFuncButtonStaff(Toggle toggle, SkillScriptable data, int index)
    {
        toggle.onValueChanged.RemoveAllListeners();

        toggle.onValueChanged.AddListener((bool isOn) =>
        {
            if (!isOn) return;
            _firstSelectUI.position = toggle.transform.position;

            _weaponInfoParent.GetChild(0).GetComponent<Text>().text = data.Name;
            var image = _weaponInfoParent.GetChild(3).GetComponent<Image>();
            image.sprite = data.Image;
            image.SetNativeSize();

            var explain = _weaponInfoParent.GetChild(2).GetComponent<Text>();
            explain.text = data.Explain;

            for (int i = 0; i < data.LevelUpRewardValue.Count; i++)
            {
                explain.text += ConvertAbility(data, data.LevelUpRewardValue[i]);
            }

            var selectWeapon = _lobbyUI[base.GetUIIndex(_lobbyUI, "WeaponSelect")].transform;

            // 오픈 된 무기면 
            if (PlayerInfo.weaponOpenArray[index] == 1)
            {
                _weaponInfoParent.GetChild(4).gameObject.SetActive(false);
                _firstselect = data;
                selectWeapon.GetChild(4).GetComponent<Button>().interactable = true;
                return;
            }

            // 오픈되지 않은 무기면
            _weaponInfoParent.GetChild(4).gameObject.SetActive(true);
            _weaponInfoParent.GetChild(4).GetChild(2).GetComponent<Text>().text = data.openExplain;

            selectWeapon.GetChild(4).GetComponent<Button>().interactable = false;
        });
    }

    // 고유 무기창에서 다음 누르면 맵 버튼 기능 삽입
    public void SetSelectMapButton()
    {
        // 클리어 여부 상관없이 맨 처음 맵 보여줌
        _mapListPage = 0;

        var selectMap = _lobbyUI[base.GetUIIndex(_lobbyUI, "MapSelect")].transform;

        // 맵 정보 기입
        var mapInfo = selectMap.GetChild(4);
        mapInfo.GetChild(0).GetChild(0).GetComponent<Text>().text = mapList[_mapListPage].mapName;
        mapInfo.GetChild(1).GetComponent<Image>().sprite = mapList[_mapListPage].image;
        mapInfo.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = mapList[_mapListPage].phase;

        // 좌우 버튼 기능 삽입
        // 왼쪽
        var button = selectMap.GetChild(selectMap.childCount - 2).GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (_mapListPage == 0) return;

            _mapListPage--;

            mapInfo.GetChild(0).GetChild(0).GetComponent<Text>().text = mapList[_mapListPage].mapName;
            mapInfo.GetChild(1).GetComponent<Image>().sprite = mapList[_mapListPage].image;
            mapInfo.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = mapList[_mapListPage].phase;

            if (mapOpenArray[_mapListPage] == 0)
            {
                var lockUI = mapInfo.GetChild(1).GetChild(0).gameObject;

                lockUI.SetActive(true);
                lockUI.transform.GetChild(1).GetComponent<Text>().text = mapList[_mapListPage - 1].mapName + " 맵 " + mapList[_mapListPage].openTime + "분 이상 생존";

                selectMap.GetChild(5).GetComponent<Button>().interactable = false;
            }
            else
            {
                mapInfo.GetChild(1).GetChild(0).gameObject.SetActive(false);
                selectMap.GetChild(5).GetComponent<Button>().interactable = true;
            }
        });

        // 오른쪽
        button = selectMap.GetChild(selectMap.childCount - 1).GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (_mapListPage == mapOpenArray.Length - 1) return;

            _mapListPage++;

            mapInfo.GetChild(0).GetChild(0).GetComponent<Text>().text = mapList[_mapListPage].mapName;
            mapInfo.GetChild(1).GetComponent<Image>().sprite = mapList[_mapListPage].image;
            mapInfo.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = mapList[_mapListPage].phase;

            if (mapOpenArray[_mapListPage] == 0)
            {
                var lockUI = mapInfo.GetChild(1).GetChild(0).gameObject;

                lockUI.SetActive(true);
                lockUI.transform.GetChild(1).GetComponent<Text>().text = mapList[_mapListPage - 1].mapName + " 맵 " + mapList[_mapListPage].openTime + "분 이상 생존";

                selectMap.GetChild(5).GetComponent<Button>().interactable = false;
            }
        });
    }

    // 스타트 누르면 캐릭터 옵션 저장
    private void SetCharacterInfo()
    {
        var info = _characterList[currentChar].GetComponent<CharacterInfo>().info;

        PlayerInfo.originAttack += info.attackValue;
        PlayerInfo.firePropertyAttack += info.fireValue;
        PlayerInfo.electricPropertyAttack += info.electricityValue;
        PlayerInfo.windPropertyAttack += info.windValue;
        PlayerInfo.earthPropertyAttack += info.earthValue;
        PlayerInfo.waterPropertyAttack += info.waterValue;

        PlayerInfo.originHP = info.hpValue;
        PlayerInfo.originSpeed = info.moveSpeed;

        PlayerInfo.character = currentChar;
        PlayerInfo.nextOpenTime = mapList[_mapListPage + 1 == mapList.Count ? _mapListPage : _mapListPage + 1].openTime;
        PlayerInfo.currentMap = _mapListPage;
    }

    // 스태프 옵션 적용 - 맵 선택버튼에 있음
    private void SetFirstWeaponAbility()
    {
        var ability = _firstselect.LevelUpRewardValue;

        for (int i = 0; i < ability.Count; i++)
            if (ability[i].ability == SkillAbility.ProtectSkill) PlayerInfo.protectSkill = true;


        PlayerInfo.startSkill = _firstselect;
    }

    private void SetMapOption()
    {
        var list = mapList[_mapListPage].mapAbilityList;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].mapAbility == Map.AddEXP) PlayerInfo.monsterEXP += list[i].value;
            else if (list[i].mapAbility == Map.FinalReward) PlayerInfo.finalReward = list[i].value;
            else if (list[i].mapAbility == Map.MonsterSpawnCount) PlayerInfo.addMonsterCount += list[i].value;
            else if (list[i].mapAbility == Map.MonsterSpawnTime) PlayerInfo.addMonsterCool += list[i].value;
        }
    }


    // 능력 한글화
    private string ConvertAbility(SkillScriptable skillData, SpecialSkill data)
    {
        var korean = "\n";

        switch (skillData.Type)
        {
            case PropertyType.Fire:
                korean += "불 ";
                break;
            case PropertyType.Electricity:
                korean += "번개 ";
                break;
            case PropertyType.Wind:
                korean += "바람 ";
                break;
            case PropertyType.Earth:
                korean += "대지 ";
                break;
            case PropertyType.Water:
                korean += "물 ";
                break;
            default:
                break;
        }

        switch (data.ability)
        {
            case SkillAbility.PropertyDamage:
                korean += "속성 데미지 +";
                break;
            case SkillAbility.PropertyCoolTime:
                korean += "속성 쿨타임 ";
                break;
            case SkillAbility.MoveSpeed:
                korean += "캐릭터 이동속도 +";
                break;
            case SkillAbility.RewardExp:
                korean += "획득 경험치 +";
                break;
            case SkillAbility.AllPropertyDamage:
                korean += "모든 속성 데미지 +";
                break;
            case SkillAbility.AddLevelReward:
                korean += "레벨업 시 선택 스킬 개수 +";
                break;
            case SkillAbility.Reborn:
                korean += "사망시 1회 부활(HP 50%, 무적 3초)";
                break;
            case SkillAbility.ProtectSkill:
                korean += "보호 지팡이 LV.1 을 가지고 시작한다.";
                break;
            default:
                break;
        }

        var unit = string.Empty;
        switch (data.unit)
        {
            case Unit.Percent:
                unit = "%";
                break;
            case Unit.Second:
                unit = "초";
                break;
            default:
                break;
        }

        return korean + (data.value == 0 ? "" : data.value.ToString()) + unit;
    }

    public void AllApplyOption()
    {
        SetCharacterInfo();
        SetFirstWeaponAbility();
        SetMapOption();
        AndroidNativeAudio.unload(FileID_Yes);
        AndroidNativeAudio.unload(FileID_No);
        LoadingManager.LoadScene(SceneName.Lobby, SceneName.Ingame);
    }
    #endregion

    #region 업그레이드

    private string GetSkillExplain(UpgradSkillInfo skillInfo)
    {
        if (skillInfo.ability == SkillAbility.PropertyDamage)
            return "공격력 " + skillInfo.value * (skillInfo.currentLevel + 1) + "% 증가";
        else if (skillInfo.ability == SkillAbility.PropertyCoolTime)
            return skillInfo.value * (skillInfo.currentLevel + 1) + "% 감소";
        else if (skillInfo.ability == SkillAbility.MaxHP)
            return "최대 체력 " + skillInfo.value * (skillInfo.currentLevel + 1) + "% 증가";
        else if (skillInfo.ability == SkillAbility.HPPotionAbility)
            return "포션 회복률 " + skillInfo.value * (skillInfo.currentLevel + 1) + "% 증가";
        else if (skillInfo.ability == SkillAbility.ReceivedDamage)
            return "피해량 " + skillInfo.value * (skillInfo.currentLevel + 1) + "% 감소";
        else if (skillInfo.ability == SkillAbility.EXPPotionAbility)
            return "경험치 포션 획득 시 경험치 " + skillInfo.value * (skillInfo.currentLevel + 1) + "% 증가";
        else if (skillInfo.ability == SkillAbility.MoveSpeed)
            return skillInfo.value * (skillInfo.currentLevel + 1) + "% 증가";

        return string.Empty;
    }
    public void SetSkillInfo(int index)
    {
        _currentSelect = index;
        var upgradeUI = _lobbyUI[base.GetUIIndex(_lobbyUI, "UpgradeSkil")].transform;

        upgradeUI.GetChild(3).GetChild(1).GetComponent<Text>().text = "LV " + skillAllLevel;
        upgradeUI.GetChild(4).GetChild(1).GetComponent<Text>().text = _currentSkillPrice == 0 ? "0" : _currentSkillPrice.ToString("#,###");
        upgradeUI.GetChild(6).GetChild(1).GetComponent<Text>().text = upgradeDataArray[index].name_KR;
        upgradeUI.GetChild(6).GetChild(2).GetComponent<Text>().text = upgradeDataArray[index].currentLevel == upgradeDataArray[index].maxLevel ? "최대 레벨" : GetSkillExplain(upgradeDataArray[index]);
        upgradeUI.GetChild(7).GetChild(1).GetComponent<Text>().text = upgradeDataArray[index].currentLevel == upgradeDataArray[index].maxLevel ? "MAX" : "LV " + upgradeDataArray[index].currentLevel;
        upgradeUI.GetChild(9).GetComponent<Text>().text = PlayerInfo.gold == 0 ? "0" : PlayerInfo.gold.ToString("#,###");

        if (upgradeDataArray[index].currentLevel == upgradeDataArray[index].maxLevel || PlayerInfo.gold - _currentSkillPrice < 0)
            upgradeUI.GetChild(8).GetComponent<Button>().interactable = false;
        else upgradeUI.GetChild(8).GetComponent<Button>().interactable = true;
    }

    public void SkillUpgradeBuy()
    {
        //  돈 부족
        if (PlayerInfo.gold - _currentSkillPrice < 0) return;

        var data = upgradeDataArray[_currentSelect];

        // 구매 버튼 비활성화 하기
        if (data.currentLevel == data.maxLevel) return;

        // 구매 후 정보 업데이트
        data.currentLevel++;
        skillAllLevel++;

        if (table[_applyIndex].changeLevel == skillAllLevel)
        {
            _applyIndex++;
            _startSkillIncreasePrice += table[_applyIndex].increaseValue;
        }
        PlayerInfo.gold -= _currentSkillPrice;
        _currentSkillPrice += _startSkillIncreasePrice;

        SetAbility(data);
        SetSkillInfo(_currentSelect);
        DataManager.GetInstance().SaveSkill(upgradeDataArray, characterBuyArray, currentChar, mapOpenArray);
        DataManager.GetInstance().SaveGold(PlayerInfo.gold);
    }

    // 스킬 적용
    private void SetAbility(UpgradSkillInfo data)
    {
        // 속성 데미지
        if (data.ability == SkillAbility.PropertyDamage || data.ability == SkillAbility.PropertyCoolTime)
        {
            switch (data.type)
            {
                case PropertyType.Fire:
                    if (data.ability == SkillAbility.PropertyDamage) PlayerInfo.firePropertyAttack = data.value * data.currentLevel;
                    else PlayerInfo.firePropertyCool = data.value * data.currentLevel;
                    break;
                case PropertyType.Electricity:
                    if (data.ability == SkillAbility.PropertyDamage) PlayerInfo.electricPropertyAttack = data.value * data.currentLevel;
                    else PlayerInfo.electricPropertyCool = data.value * data.currentLevel;
                    break;
                case PropertyType.Wind:
                    if (data.ability == SkillAbility.PropertyDamage) PlayerInfo.windPropertyAttack = data.value * data.currentLevel;
                    else PlayerInfo.windPropertyCool = data.value * data.currentLevel;
                    break;
                case PropertyType.Earth:
                    if (data.ability == SkillAbility.PropertyDamage) PlayerInfo.earthPropertyAttack = data.value * data.currentLevel;
                    else PlayerInfo.earthPropertyCool = data.value * data.currentLevel;
                    break;
                case PropertyType.Water:
                    if (data.ability == SkillAbility.PropertyDamage) PlayerInfo.waterPropertyAttack = data.value * data.currentLevel;
                    else PlayerInfo.waterPropertyCool = data.value * data.currentLevel;
                    break;
                default:
                    break;
            }
        }
        else if (data.ability == SkillAbility.MaxHP) PlayerInfo.maxHP = data.value * data.currentLevel;
        else if (data.ability == SkillAbility.HPPotionAbility) PlayerInfo.portionHP = data.value * data.currentLevel;
        else if (data.ability == SkillAbility.ReceivedDamage) PlayerInfo.passiveDamage = data.value * data.currentLevel;
        else if (data.ability == SkillAbility.EXPPotionAbility) PlayerInfo.portionEXP = data.value * data.currentLevel;
        else if (data.ability == SkillAbility.MoveSpeed) PlayerInfo.moveSpeed = data.value * data.currentLevel;
    }

    public void FirstUpgrad()
    {
        var upgradeUI = _lobbyUI[base.GetUIIndex(_lobbyUI, "UpgradeSkil")].transform;

        upgradeUI.GetChild(5).GetChild(0).GetComponent<Toggle>().isOn = true;
    }
    #endregion

    #region 옵션

    // 옵션 창 열때마다 셋팅
    public void SetOptionToggle()
    {
        if (PlayerInfo.controllerFixed) _optionToggleList[0].isOn = true;
        else _optionToggleList[1].isOn = true;

        if (!PlayerInfo.sound) _optionToggleList[2].isOn = true;
        else _optionToggleList[3].isOn = true;
    }

    // 온오프 할때마다 컨트롤러 타입 바뀜
    public void ChangeControllerType(int type)
    {
        // 고정식
        if (type == 0) PlayerInfo.controllerFixed = true;
        else PlayerInfo.controllerFixed = false;
    }

    // 사운드 조정
    public void ChangeSound(int type)
    {
        // 음소거
        if (type == 0)
        {
            PlayerInfo.sound = false;
            _mixer.SetFloat("Main", -80f);
            return;
        }

        PlayerInfo.sound = true;
        _mixer.SetFloat("Main", 0f);
    }

    // 옵션 저장
    public void OptionSave()
    {
        DataManager.GetInstance().SaveSkill(upgradeDataArray, characterBuyArray, currentChar, mapOpenArray);
    }

    #endregion
}

[System.Serializable]
public class UpgradSkillInfo
{
    public PropertyType type;
    public SkillAbility ability;
    public string name_KR;
    public int currentLevel;
    public int maxLevel;
    public int value;
}

[System.Serializable]
public class SkillPriceTable
{
    public int changeLevel;
    public int increaseValue;
}

public static class PlayerInfo
{
    public static int gold, character, nextOpenTime, currentMap, language;
    public static SkillScriptable startSkill;

    public static float originAttack, originSpeed, originHP, finalReward, addMonsterCool, addMonsterCount;
    public static float firePropertyAttack, waterPropertyAttack, electricPropertyAttack, earthPropertyAttack, windPropertyAttack;
    public static float firePropertyCool, waterPropertyCool, electricPropertyCool, earthPropertyCool, windPropertyCool;
    public static float maxHP, portionHP, passiveDamage, portionEXP, moveSpeed, addSelectWeapon, monsterEXP;
    public static bool reborn, protectSkill, nextMapOpen, controllerFixed, sound, removeADs, doubleReward;
    public static int[] weaponOpenArray;
}