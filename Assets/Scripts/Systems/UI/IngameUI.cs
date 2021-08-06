using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IngameUI : UIManager
{
    #region 싱글톤
    private static IngameUI instance;

    private void Awake()
    {
        if (instance != null) Destroy(instance);

        instance = this;
    }

    public static IngameUI GetInstance()
    {
        if (instance == null)
        {
            print("UIManager 인스턴스 없음");
            return null;
        }

        return instance;
    }
    #endregion

    [Header("===== Test")]
    [SerializeField] private Text _hpText;
    [SerializeField] private Text monsterCountText;
    public InputField removeLimit;
    public Text removeText;
    private int monsterCount;
    public InputField frameCountInput;
    public Text frameText;

    [Header("===== System")]
    [SerializeField] private Text _timeText;
    [SerializeField] private Sprite _crossSprite;
    [SerializeField] private VariableJoystick _joystick;
    [SerializeField] private List<GameObject> _ingameUI;
    [SerializeField] private GameObject _adGuard;
    public bool pause;

    [Header("===== Player")]
    [SerializeField] private Image _playerHpBar;
    [SerializeField] private Image _playerExpBar;
    [SerializeField] private Image _hitImage;
    [SerializeField] private Image _lessImage;
    [SerializeField] private Text _playerExpText;
    [SerializeField] private Text _playerGoldText;
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private GameObject _selectSkillUI;
    [SerializeField] private GameObject _specialSkillUI;
    [SerializeField] private Transform _specialSkillButtonParent;
    [SerializeField] private GameObject _lastSkillUI;
    [SerializeField] private Transform _lastSkillButtonParent;
    private bool _activeHitEffect;
    private bool _activeLessEffect;
    private float _getGold;

    [Header("===== Weapons")]
    private GameObject[] _weaponArray;
    [SerializeField] private Transform _selectWeaponParent;
    [SerializeField] private Transform _myStaffParent;
    [SerializeField] private List<SkillScriptable> _uniqueWeaponList;
    public int selectRuneSlot;
    private bool isSpecialSelect;

    [Header("===== Runes")]
    private GameObject[] _runesArray;

    [Header("===== Shop")]
    [SerializeField] private GameObject _shopUI;
    [SerializeField] private Transform[] _shopItemSlot;
    private bool _isOpenShop;


   
    public int MonsterCount
    {
        get => monsterCount; set
        {
            monsterCount = value;
            monsterCountText.text = monsterCount.ToString();
        }
    }

 
    // 테스트
    public void ChangeRemoveLImit()
    {
        GameManager.GetInstance().testRemoveLimit = int.Parse(removeLimit.text);
        removeText.text = removeLimit.text + " 적용됨";
        EventSystem.current.firstSelectedGameObject = null;
    }
    public void ChangeFrame()
    {
        GameManager.GetInstance().settingFrameCount = int.Parse(frameCountInput.text);
        frameText.text = frameCountInput.text + " 적용됨";
        EventSystem.current.firstSelectedGameObject = null;
    }


    private void Start()
    {
        // if (!PlayerInfo.sound) _mixer.SetFloat("Main", -80f);

        _audioSource = GetComponent<AudioSource>();

        if (PlayerInfo.controllerFixed)
        {
            _joystick.GetComponent<RectTransform>().sizeDelta = new Vector2(490, 490);
            _joystick.SetMode(JoystickType.Fixed);
        }
    }

    public void SelfPassiveUI(GameObject obj) => obj.SetActive(false);

    // 게임 시간 업데이트
    public void UpdateGameTime(string timeText) => _timeText.text = timeText;

    // UI 활성화 또는 비활성화
    public void SetActiveUI(string value) => base.SetActiveUI(_ingameUI, value, true);

    // 사운드 플레이
    public void PlaySound(bool isOn)
    {
        if (isOn) AndroidNativeAudio.play(GameManager.GetInstance()._fileID_Yes);
        else AndroidNativeAudio.play(GameManager.GetInstance()._fileID_No);
    }

    #region 광고 UI
    // 광고 재생시 뒷판 활성화 ㅂ및 비활성화
    public void ActiveAdGuard(bool value)
    {
        _adGuard.SetActive(value);
    }

    // 표시할 광고가 없거나 로드 실패일때
    public void FailLoadADs(int type)
    {
        base.SetPopUpUIActive("광고가 없습니다.", false, true);

        // 배너
        if (type == 1) base.SetPopUpButtonAction("확인", GameOverAdsFail);
        // 보상형
        else base.SetPopUpButtonAction("확인", RebornAdsFail);
    }

    // 배너 광고 실패
    private void GameOverAdsFail(Button button)
    {
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            Time.timeScale = 1;
            ActiveAdGuard(false);
            GameManager.GetInstance().player.EndGameoberAds();
            base.PassivePopUpUI();
        });
    }

    private void RebornAdsFail(Button button)
    {
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            ActiveAdGuard(false);
            base.PassivePopUpUI();
           // Reborn();
           
        });

    }

    public void RewardAdClosed()
    {
        var deadUI = _ingameUI[base.GetUIIndex(_ingameUI, "DeadUI")].transform;
        deadUI.GetChild(7).GetComponent<Button>().interactable = false;
    }
    #endregion

    #region 플레이어 상태
    public void UpdateHpBar(float maxHP, float hp)
    {
        //  _hpText.text = hp.ToString("N2") + "/" + maxHP;
        _playerHpBar.fillAmount = Mathf.Lerp(0, 1, hp / maxHP);
    }

    public void UpdateExpBar(float exp, float maxExp)
    {
        _playerExpBar.fillAmount = Mathf.InverseLerp(0, maxExp, exp);
    }

    public void UpdateExpText(int level)
    {
        _playerExpText.text = "LV." + level.ToString();
        base.SetActiveUI(_ingameUI, "LevelUp|on", true);
    }

    public void UpdateGoldText(int gold) => _playerGoldText.text = gold == 0 ? "0" : gold.ToString("#,###");

    // 플레이어 피격
    public void PlayerHit()
    {
        if (_activeHitEffect) return;

        _activeHitEffect = true;
        StartCoroutine(HitEffect());
    }

    IEnumerator HitEffect()
    {
        _hitImage.color = new Color(1, 0, 0, 1);

        while (_hitImage.color.a >= 0.3)
        {
            _hitImage.color = new Color(1, 0, 0, Mathf.Lerp(_hitImage.color.a, 0, Time.deltaTime));
            yield return null;
        }

        _hitImage.color = new Color(1, 0, 0, 0);
        _activeHitEffect = false;
    }

    // 30퍼 이하일때 어둡게
    public void LessHP(bool reverse)
    {
        if (reverse && _activeLessEffect)
        {
            _activeLessEffect = false;
            StartCoroutine(LessHpEffect(true));
        }

        if (_activeLessEffect || reverse) return;

        _activeLessEffect = true;
        StartCoroutine(LessHpEffect(false));
    }

    IEnumerator LessHpEffect(bool reverse)
    {
        if (reverse)
        {
            while (_lessImage.color.a > 0)
            {
                if (_activeLessEffect) break;

                _lessImage.color = new Color(0, 0, 0, Mathf.Lerp(_lessImage.color.a, 0, Time.deltaTime));
                yield return null;
            }
            yield break;
        }

        _lessImage.color = new Color(0, 0, 0, 0);

        while (_lessImage.color.a <= 0.4f)
        {
            if (!_activeLessEffect) break;

            _lessImage.color = new Color(0, 0, 0, Mathf.Lerp(_lessImage.color.a, 0.42f, Time.deltaTime));
            yield return null;
        }
    }

    // 
    public void UIReset() => AllActiveUI(_ingameUI, false);

    // 플레이어가 죽었을때
    public void PlayerDeadUI()
    {
        base.SetActiveUI(_ingameUI, "DeadUI|on", true);

        var deadUI = _ingameUI[base.GetUIIndex(_ingameUI, "DeadUI")].transform;
        deadUI.GetChild(2).GetComponent<Text>().text = GameManager.GetInstance().currentTime;

        var kill = GameManager.GetInstance().player.killMonster;
        var time = GameManager.GetInstance().GetGameSecond() * 0.01f;
        var level = GameManager.GetInstance().player.PlayerLevel * 0.01f;
        _getGold = kill * (time + level);

        if (time < 0.02f) _getGold = 0;

       if (time * 100 >= PlayerInfo.nextOpenTime )
            PlayerInfo.nextMapOpen = true;
        
        deadUI.GetChild(5).GetComponent<Text>().text = _getGold == 0 ? "0" : PlayerInfo.doubleReward ? ((int)_getGold * 2).ToString("#,###") : ((int)_getGold).ToString("#,###");

        deadUI.GetChild(6).GetComponent<Button>().onClick.RemoveAllListeners();
        deadUI.GetChild(6).GetComponent<Button>().onClick.AddListener(() =>
        {
            CheckWeaponOpen();

        });

        deadUI.GetChild(7).GetComponent<Button>().onClick.RemoveAllListeners();
        deadUI.GetChild(7).GetComponent<Button>().onClick.AddListener(() =>
        {
            // 광고
            if (PlayerInfo.removeADs)
            {
                Reborn();
                return;
            }

            ActiveAdGuard(true);
            GameManager.GetInstance().adsManager.RebornAds();

            // 이거 지워야함
            // Reborn();
        });

    }

    public void Reborn()
    {
        Time.timeScale = 1;

        // 플레이어 상태 초기화
        var player = GameManager.GetInstance().player;
        player.CurrentHP = player._addMaxHP;
        player._death = false;
        player.ChangePlayerAnimation("Idle_1", true);
        player.OnGodMode(3);

        // 몬스터 1초 정지
        GameManager.GetInstance().MonsterPause();
        StartCoroutine(MonsterPause());

        // 광고 1회만 사용 가능하도록
        var deadUI = _ingameUI[base.GetUIIndex(_ingameUI, "DeadUI")].transform;
        deadUI.GetChild(7).GetComponent<Button>().interactable = false;

        // UI 닫기
        AllActiveUI(_ingameUI, false);
        Time.timeScale = 1;
        GameManager.GetInstance().player.StartDeathUpdate();
        GameManager.GetInstance().player.weaponObj.SetActive(true);
      //  base.SetActiveUI(_ingameUI, "DeadUI|off", true);
    }

    IEnumerator MonsterPause()
    {
        yield return new WaitForSeconds(1.2f);
        GameManager.GetInstance().MonsterPause();
    }

    // 무기 오픈
    private void CheckWeaponOpen()
    {
        var comment = string.Empty;

        for (int i = 1; i < _uniqueWeaponList.Count; i++)
        {
            if ((PlayerInfo.currentMap == 0 ? 1 : PlayerInfo.currentMap * 2) != (int)_uniqueWeaponList[i].map || PlayerInfo.weaponOpenArray[i] == 1) continue;

            comment = string.Empty;

            switch (_uniqueWeaponList[i].requireMapOpen)
            {
                case RequireMapOpen.CatchMonster:
                    if (GameManager.GetInstance().player.killMonster >= _uniqueWeaponList[i].openValue)
                    {
                        PlayerInfo.weaponOpenArray[i] = 1;

                        if (PlayerInfo.currentMap == 0) comment += "초원에서 몬스터";
                        else if (PlayerInfo.currentMap == 1) comment += "사막에서 몬스터";
                        else if (PlayerInfo.currentMap == 2) comment += "설원에서 몬스터";

                        base.SetPopUpUIActive(_uniqueWeaponList[i].openValue + "마리 이상을 잡아 \"" + _uniqueWeaponList[i].Name + "\"가(이) 해금 되었습니다."
                            , false, true);
                        base.SetPopUpButtonAction("확인", ReCheck);
                        return;
                    }
                    break;
                case RequireMapOpen.GetCoin:
                    if (GameManager.GetInstance().player.Gold >= _uniqueWeaponList[i].openValue)
                    {
                        PlayerInfo.weaponOpenArray[i] = 1;
                        base.SetPopUpUIActive("초원에서 인게임 코인 " + _uniqueWeaponList[i].openValue + "이상 획득하여 \"" + _uniqueWeaponList[i].Name + "\"가(이) 해금 되었습니다."
                            , false, true);
                        base.SetPopUpButtonAction("확인", ReCheck);
                        return;
                    }
                    break;
                case RequireMapOpen.GetLevel:
                    if (GameManager.GetInstance().player.PlayerLevel >= _uniqueWeaponList[i].openValue)
                    {
                        PlayerInfo.weaponOpenArray[i] = 1;
                        base.SetPopUpUIActive("초원에서 캐릭터 레벨 " + _uniqueWeaponList[i].openValue + "이상 올려 \"" + _uniqueWeaponList[i].Name + "\"가(이) 해금 되었습니다."
                            , false, true);
                        base.SetPopUpButtonAction("확인", ReCheck);
                        return;
                    }
                    break;
                case RequireMapOpen.OverTime:
                    if (GameManager.GetInstance().GetGameSecond() >= _uniqueWeaponList[i].openValue)
                    {
                        PlayerInfo.weaponOpenArray[i] = 1;

                        if (PlayerInfo.currentMap == 0) comment += "초원에서 ";
                        else if (PlayerInfo.currentMap == 1) comment += "사막에서 ";
                        else if (PlayerInfo.currentMap == 2) comment += "설원에서 ";

                        base.SetPopUpUIActive(_uniqueWeaponList[i].openValue + "분 이상 플레이 하여 \"" + _uniqueWeaponList[i].Name + "\"가(이) 해금 되었습니다."
                                  , false, true);
                        base.SetPopUpButtonAction("확인", ReCheck);
                        return;
                    }
                    break;
                case RequireMapOpen.Virus:
                    if (GameManager.GetInstance().player.iceVirusKill >= _uniqueWeaponList[i].openValue)
                    {
                        PlayerInfo.weaponOpenArray[i] = 1;

                        base.SetPopUpUIActive("설원에서 대형슬라임 & 균집체 몬스터를 각각" + _uniqueWeaponList[i].openValue + "마리 이상 잡아 \"" + _uniqueWeaponList[i].Name + "\"가(이) 해금 되었습니다."
                                  , false, true);
                        base.SetPopUpButtonAction("확인", ReCheck);
                        return;
                    }
                    break;
                case RequireMapOpen.CubeMonster:
                    if (GameManager.GetInstance().player.desertCubeKill >= _uniqueWeaponList[i].openValue)
                    {
                        PlayerInfo.weaponOpenArray[i] = 1;

                        base.SetPopUpUIActive("사막에서 큐브 몬스터를" + _uniqueWeaponList[i].openValue + "마리 이상 잡아 \"" + _uniqueWeaponList[i].Name + "\"가(이) 해금 되었습니다."
                                  , false, true);
                        base.SetPopUpButtonAction("확인", ReCheck);
                        return;
                    }
                    break;
                default:
                    break;
            }
        }

        Time.timeScale = 1;
        if (DataManager.GetInstance().SaveGold(((PlayerInfo.doubleReward ? 2 : 1) * (int)_getGold) + PlayerInfo.gold))
        {

            if (!PlayerInfo.removeADs)
            {
                // 광고 호출
                //IngameUI.GetInstance().ActiveAdGuard(true);
                GameManager.GetInstance().adsManager.GameOverAds();
                return;
            }

            Destroy(GameManager.GetInstance().adsManager.gameObject);
            GameManager.GetInstance().UnloadSound();
            LoadingManager.LoadScene(SceneName.Ingame, SceneName.Lobby);
        }
    }

    // 로비로 이동
    private void ReCheck(Button button)
    {
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            CheckWeaponOpen();
        });
    }

    #endregion

    #region 게임 상태 컨트롤

    // 게임 중단시 나타나는 정보 셋팅
    public void SetPauseInfo()
    {
        var pauseUI = _ingameUI[base.GetUIIndex(_ingameUI, "Pause")].transform;

        pauseUI.GetChild(2).GetChild(0).GetComponent<Text>().text = GameManager.GetInstance().player.PlayerLevel == GameManager.GetInstance().player.endLevel ? "MAX" : GameManager.GetInstance().player.PlayerLevel.ToString();
        pauseUI.GetChild(3).GetChild(0).GetComponent<Text>().text = GameManager.GetInstance().currentTime;
        pauseUI.GetChild(4).GetChild(0).GetComponent<Text>().text = GameManager.GetInstance().player.killMonster.ToString();
        pauseUI.GetChild(5).GetChild(0).GetComponent<Text>().text = GameManager.GetInstance().player.Gold == 0 ? "0" : GameManager.GetInstance().player.Gold.ToString("#,###");

        var resumeButton = pauseUI.GetChild(6).GetComponent<Button>();
        resumeButton.onClick.RemoveAllListeners();
        resumeButton.onClick.AddListener(() =>
        {

            base.SetActiveUI(_ingameUI, "PauseUI|off", true, false);
            StartCoroutine(ResumeGame());
        });
    }

    // 게임 나가기
    public void ExitGame()
    {
        base.SetPopUpUIActive("게임을 종료 하시겠습니까?", true, true);

        base.SetPopUpButtonAction("예", AddButtonActionExitGame, true);
        base.SetPopUpButtonAction("아니요", base.AddButtonActionPassiveUI, false);
    }

    private void AddButtonActionExitGame(Button button)
    {
        button.onClick.AddListener(() =>
        {
            base.LoadNextScene("Lobby");
        });
    }

    IEnumerator ResumeGame()
    {
        pause = true;

        print("Call");
        GameManager.GetInstance().player.OnGodMode(1.2f);
        yield return new WaitForSeconds(1f);
        print("Resume");
        pause = false;
    }


    #endregion

    #region 레벨업 UI

    // 레벨업 하면 호출됨
    public void LevelUpUI()
    {
        base.SetActiveUI(_ingameUI, "LevelUpU|on", true, false);
        SettingSkillSlot();
    }

    // 레벨업 UI에 출력할 무기 정보 셋팅
    private void SettingSkillSlot()
    {
        var list = GameManager.GetInstance().GetSelectRandomWeapon();
        var ui = _ingameUI[base.GetUIIndex(_ingameUI, "LevelUp")].transform;
        ui.GetChild(1).GetChild(0).GetComponent<Text>().text = GameManager.GetInstance().player.PlayerLevel == GameManager.GetInstance().player.endLevel ? "MAX" : GameManager.GetInstance().player.PlayerLevel.ToString();

        if (list.Count == 3) ui.GetChild(2).gameObject.SetActive(true);
        else if (list.Count == 4) ui.GetChild(3).gameObject.SetActive(true);

        for (int i = 0; i < list.Count; i++)
        {
            var weapon = list[i].skillInfo;

            var obj = ui.GetChild(GameManager.GetInstance().player.selectSkillCount - 1).GetChild(i);

            obj.GetChild(0).GetComponent<Image>().sprite = weapon.UsePassive ? weapon.Image : weapon.skillImage;
            obj.GetChild(0).GetComponent<Image>().SetNativeSize();
            obj.GetChild(1).GetComponent<Text>().text = weapon.Name;
            var explainText = obj.GetChild(2).GetComponent<Text>();
            explainText.text = list[i].skillInfo.MaxLevel == 0 ? string.Empty : ("LV " + list[i].skillLevel + "\n");

            if (!list[i].skillInfo.UsePassive && list[i].skillLevel == 6) explainText.text += "특성 부여 가능";
            else explainText.text += weapon.Explain;

            explainText.text += ConvertSpecialSkill(weapon.LevelUpRewardValue[0]) == string.Empty ? string.Empty : ("\n" + ConvertSpecialSkill(weapon.LevelUpRewardValue[0]));
            if (weapon.LevelUpRewardValue.Count > 1) explainText.text += ", " + ConvertSpecialSkill(weapon.LevelUpRewardValue[1]);

            obj.gameObject.SetActive(true);

            AddFuncSkillButton(obj.GetComponent<Button>(), list[i]);
        }

        // 레벨업 포기 버튼에 기능 삽입
        var returnButton = ui.GetChild(GameManager.GetInstance().player.selectSkillCount - 1).GetChild(list.Count).GetComponent<Button>();

        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(() =>
        {
            GameManager.GetInstance().player.ReturnLevel(true);

            base.SetActiveUI(_ingameUI, "LevelU|off", _isOpenShop ? false : true, false);
            GameManager.GetInstance().player.OnGodMode(1.5f);
        });
    }

    // 레벨업 UI에서 해당 무기를 눌렀을떄
    private void AddFuncSkillButton(Button button, SkillInventory weaponInfo)
    {
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            base.SetActiveUI(_ingameUI, "LevelUpU|off", _isOpenShop ? false : true, false);
            GameManager.GetInstance().player.CheckSkill(weaponInfo);
            GameManager.GetInstance().player.OnGodMode(1.5f);
        });
    }

    private string GetTranslationProperty(PropertyType type)
    {
        switch (type)
        {
            case PropertyType.Fire:
                return "불";
            case PropertyType.Electricity:
                return "전";
            case PropertyType.Wind:
                return "바";
            case PropertyType.Earth:
                return "대";
            case PropertyType.Water:
                return "물";
            case PropertyType.Clear:
                return "무";
            default:
                break;
        }

        return string.Empty;
    }

    // 특성 스킬 선택창 온 오프
    public void ActiveSpecialSkillUI(bool value)
    {
        if (value)
        {
            base.SetActiveUI(_ingameUI, "SpecialSkill|on", true, true);
            return;
        }
        base.SetActiveUI(_ingameUI, "SpecialSkill|off", _isOpenShop ? false : true, false);
    }

    // 6레벨 특성 선택창
    public void ActiveLastSkillUI(bool value)
    {
        if (value)
        {
            base.SetActiveUI(_ingameUI, "LastSkill|on", true, true);
            return;
        }
        base.SetActiveUI(_ingameUI, "LastSkill|off", _isOpenShop ? false : true, false);
    }

    // 3레벨 특성 스킬 선택창에있는 버튼에 특성 스킬 데이터 삽입
    public void SettingSpecialSkill(SkillInventory skillInventory)
    {
        _specialSkillButtonParent.GetChild(0).gameObject.SetActive(false);
        _specialSkillButtonParent.GetChild(1).gameObject.SetActive(false);

        for (int i = 0; i < skillInventory.threeLevelSkill.Count; i++)
        {
            _specialSkillButtonParent.GetChild(i).gameObject.SetActive(true);
            var button = _specialSkillButtonParent.GetChild(i).GetComponent<Button>();

            var skillInfo = skillInventory.threeLevelSkill[i];

            button.transform.GetChild(0).GetComponent<Image>().sprite = skillInfo.skillImage;
            button.transform.GetChild(1).GetComponent<Text>().text = skillInfo.Name;
            button.transform.GetChild(2).GetComponent<Text>().text = skillInfo.specialExplain;
            var explainText = button.transform.GetChild(3).GetComponent<Text>();
            explainText.text = ConvertSpecialSkill(skillInfo.LevelUpRewardValue[0]);
            if (skillInfo.LevelUpRewardValue.Count > 1) explainText.text += "\n" + ConvertSpecialSkill(skillInfo.LevelUpRewardValue[1]);

            AddFuncSpecialButton(button, skillInventory, skillInfo);
        }
    }

    // 6레벨 특성 - 각 버튼별로 전용 특성이 존재함
    public void SettingLastSkill(SkillInventory skillInventory)
    {
        int j = 0;

        for (int i = 0; i < _lastSkillButtonParent.childCount; i++)
        {
            var button = _lastSkillButtonParent.GetChild(i).GetComponent<Button>();


            for (; j < skillInventory.skillInfo.specialSkill.Count;)
            {
                var info = skillInventory.skillInfo.specialSkill[j];
                var specialList = new List<SpecialSkill>();
                specialList.Clear();

                specialList.Add(info);
                button.transform.GetChild(0).GetComponent<Image>().sprite = skillInventory.skillInfo.skillImage;
                button.transform.GetChild(1).GetComponent<Text>().text = ConvertSpecialSkill(info) + "\n";

                if (info.SetAbility)
                {
                    ++j;
                    info = skillInventory.skillInfo.specialSkill[j];
                    specialList.Add(info);
                    button.transform.GetChild(1).GetComponent<Text>().text += ConvertSpecialSkill(info);

                }
                ++j;
                AddFuncLastButton(button, specialList, skillInventory.slotIndex);
                break;
            }

        }
    }

    // 특성 스킬 버튼에 기능 삽입 - 3레벨
    private void AddFuncSpecialButton(Button button, SkillInventory inventory, SkillScriptable skillInfo)
    {
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            print("[지팡이] " + skillInfo.Name + "특성 스킬 선택");
            ActiveSpecialSkillUI(false);
            GameManager.GetInstance().ChangeSkillIndex(inventory, skillInfo);
            GameManager.GetInstance().player.ChangeSkill(skillInfo);

        });
    }

    private void AddFuncLastButton(Button button, List<SpecialSkill> skillList, int slotIndex)
    {
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            ActiveLastSkillUI(false);

            for (int i = 0; i < skillList.Count; i++)
            {
                print("slot index " + slotIndex);
                GameManager.GetInstance().player.weaponSlot[slotIndex].SetSkillStat(skillList[i]);
            }

        });
    }

    // 특성 별 옵션 컨버터
    private string ConvertSpecialSkill(SpecialSkill info)
    {
        var skillName = string.Empty;

        switch (info.ability)
        {
            case SkillAbility.PropertyDamage:
                return "속성 데미지 " + info.value + "% 증가";
            case SkillAbility.PropertyAmplified:
                skillName = "속성 증폭 데미지 ";
                break;
            case SkillAbility.MaxHP:
                return "최대 체력 " + info.value + "% 증가";
            case SkillAbility.HPPotionAbility:
                return "포션 회복률 " + info.value + "% 증가";
            case SkillAbility.EXPPotionAbility:
                return "경험치 포션 획득 시 경험치 " + info.value + "% 증가";
            case SkillAbility.ReceivedDamage:
                return "받는 피해량 " + info.value + "% 감소";
            case SkillAbility.PropertyCoolTime:
                return info.value + "% 감소";
            case SkillAbility.MoveSpeed:
                return "이동속도 " + info.value + "% 증가";
            case SkillAbility.SecRecoverHP:
                return "초당 체력 회복률 " + info.value + "% 증가";
            case SkillAbility.RecoverHP:
                skillName = "체력 회복";
                break;
            case SkillAbility.Camp:
                return string.Empty;
            case SkillAbility.GiveUpLevelUp:
                break;
            case SkillAbility.SkillRange:
                skillName = "스킬 범위 ";
                break;
            case SkillAbility.AttackRange:
                skillName = "사정거리 증가 ";
                break;
            case SkillAbility.DurationTime:
                skillName = "지속시간 ";
                break;
            case SkillAbility.StrunTime:
                skillName = "스턴 시간 증가 ";
                break;
            case SkillAbility.ShotObjectCount:
                skillName = "발사 갯수 ";
                break;
            case SkillAbility.ChainCount:
                skillName = "체인 횟수 ";
                break;
            case SkillAbility.Shiend:
                break;
            case SkillAbility.GodMode:
                skillName = "무적 ";
                break;
            case SkillAbility.PullPower:
                break;
            case SkillAbility.Penetrate:
                skillName = "관통 횟수 ";
                break;
            case SkillAbility.DamageDelay:
                skillName = "지속 데미지 딜레이 ";
                break;
            case SkillAbility.SkillScale:
                skillName = "스킬 크기 ";
                break;
            case SkillAbility.ProvokeHP:
                skillName = "도발 HP ";
                break;
            case SkillAbility.WarmholePower:
                skillName = "당기는 힘 ";
                break;
            case SkillAbility.ShieldStart:
                return "쉴드 존속 시";
            case SkillAbility.ShieldEnd:
                return "쉴드 소진 시";
            case SkillAbility.GodModeStart:
                return "무적 시간 동안";
            case SkillAbility.GodModeEnd:
                return "무적 끝나면";
            case SkillAbility.SkillDamage:
                skillName = "스킬 데미지 ";
                break;
            case SkillAbility.SkillCoolTime:
                skillName = "스킬 쿨타임 ";
                break;
            case SkillAbility.SkillAmp:
                skillName = "스킬 데미지 증폭 ";
                break;
            case SkillAbility.RewardExp:
                skillName = "경험치 획득 ";
                break;
            default:
                break;
        }

        var unit = string.Empty;

        switch (info.unit)
        {
            case Unit.Percent:
                unit = " %";
                break;
            case Unit.Second:
                unit = " 초";
                break;
            default:
                break;
        }

        return skillName + (Mathf.Sign(info.value) == 1 ? "+" : string.Empty) + (info.unit == Unit.None ? string.Empty : info.value.ToString()) + unit;
    }
    #endregion

    #region 장비

    // 룬 획득시 선택할수 있는 UI에 룬 셋팅
    public void SetSelectRuneUI(RuneScriptable runeData, RuneClass runeClass = RuneClass.All)
    {
        base.SetActiveUI(_ingameUI, "GetRuneUI|on", true, false);

        var runeUI = _ingameUI[base.GetUIIndex(_ingameUI, "GetRune")].transform;

        // 획득한 룬 정보 셋팅
        var getRuneInfo = runeUI.GetChild(2).transform;
        getRuneInfo.GetChild(0).GetComponent<Image>().sprite = runeData.Image;
        getRuneInfo.GetChild(1).GetComponent<Text>().text = runeData.Name + (runeClass != RuneClass.All ? " - " + runeClass.ToString() : string.Empty);
        getRuneInfo.GetChild(2).GetComponent<Text>().text = runeData.Explain;

        // 정보 UI 초기화
        runeUI.GetChild(4).GetChild(0).GetComponent<Text>().text = string.Empty;
        SetRuneInfo(runeUI.GetChild(5));

        var equip = runeUI.GetChild(runeUI.childCount - 1).GetComponent<Button>();
        equip.onClick.RemoveAllListeners();

        // 장착 버튼 기능 추가
        equip.onClick.AddListener(() =>
        {
            if (selectRuneSlot == -1) return;

            // 일반룬이냐 등급룬이냐
            if (runeClass == RuneClass.All) GameManager.GetInstance().player.AddRuneToInventory(selectRuneSlot, runeData);
            else GameManager.GetInstance().player.AddRuneToInventory(selectRuneSlot, runeData, runeClass);

            GameManager.GetInstance().player.OnGodMode(1.5f);
            base.SetActiveUI(_ingameUI, "GetRune|off", _isOpenShop ? false : true, false);
        });

        // 버리기 버튼
        equip = runeUI.GetChild(3).GetComponent<Button>();
        equip.onClick.RemoveAllListeners();
        equip.onClick.AddListener(() =>
        {
            GameManager.GetInstance().player.OnGodMode(1.5f);
            base.SetActiveUI(_ingameUI, "GetRune|off", _isOpenShop ? false : true, false);
        });

    }

    // 룬 인벤토리 로드 - 버튼
    public void LoadRuneInventory()
    {
        var runeUI = _ingameUI[base.GetUIIndex(_ingameUI, "RuneInven")].transform;
        var text = runeUI.GetChild(4).GetChild(0).GetComponent<Text>().text = string.Empty;
        SetRuneInfo(runeUI.GetChild(5));
    }

    // 룬 로드
    private void SetRuneInfo(Transform buttonParent)
    {
        var inventory = GameManager.GetInstance().player.runeInventory;

        selectRuneSlot = -1;

        try
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                // 버튼별 기능 및 슬롯 번호 입력
                var index = i;
                AddFuncButtonRune(buttonParent.GetChild(i).GetComponent<Button>(), buttonParent.parent.GetChild(4), inventory, index);

                // 비었으면 비었는 이미지로 교체
                if (inventory[i].runeInfo == null)
                {
                    buttonParent.GetChild(i).GetComponent<Image>().sprite = _crossSprite;
                    continue;
                }

                // 있으면 룬 이미지 출력
                buttonParent.GetChild(i).GetComponent<Image>().sprite = inventory[i].runeInfo.Image;
            }
        }
        catch (System.Exception e)
        {
            print(e.Message);

        }

    }

    // 룬 클릭하면 정보 출력
    private void AddFuncButtonRune(Button button, Transform runeInfoParent, RuneInventory[] runeInfo, int index)
    {
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            selectRuneSlot = index;
            var text = runeInfoParent.GetChild(0).GetComponent<Text>();
            text.text = $"[{index + 1}번칸]";

            if (runeInfo[index].runeInfo == null) return;

            text.text = $"[{index + 1}번칸] {runeInfo[index].runeInfo.Name + (runeInfo[index].runeClass != RuneClass.All ? " - " + runeInfo[index].runeClass.ToString() : string.Empty)} \n{runeInfo[index].runeInfo.Explain}";
        });
    }

    // 무기 인벤토리 오픈
    public void LoadStaffInventory()
    {
        var player = GameManager.GetInstance().player;
        var inventory = player.skillInventory;

        // 인벤토리 무기 가져와서 정보 처리하기
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].skillInfo.UsePassive) continue;

            var slot = _myStaffParent.GetChild(i);
            slot.gameObject.SetActive(true);

            var image = slot.GetChild(0).GetComponent<Image>();
            image.sprite = inventory[i].skillInfo.Image;
            image.SetNativeSize();
            if (inventory[i].skillInfo.isUniqueWeapon || inventory[i].skillInfo.Name == "언랭크") image.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 45));

            slot.GetChild(1).GetComponent<Image>().sprite = inventory[i].skillInfo.skillImage == null ? _crossSprite : inventory[i].skillInfo.skillImage;
            slot.GetChild(2).GetComponent<Text>().text = inventory[i].skillInfo.Name;
            slot.GetChild(3).GetComponent<Text>().text = inventory[i].skillInfo.Explain;

            if (inventory[i].skillInfo.UsePassive)
                slot.GetChild(4).GetComponent<Text>().text = inventory[i].skillLevel == inventory[i].skillInfo.MaxLevel ? "Lv.MAX" : "Lv." + inventory[i].skillLevel;
            else slot.GetChild(4).GetComponent<Text>().text = inventory[i].skillLevel == 6 ? "Lv.MAX" : "Lv." + inventory[i].skillLevel;

        }
    }
    #endregion

    #region 상점
    public void ActiveShop(bool value)
    {
        _isOpenShop = true;
        base.SetActiveUI(_ingameUI, "ShopU|on", true);
    }

    // 상점 아이템 셋팅 
    public void SettingShopItem(Shop shop)
    {
        _shopUI.transform.GetChild(2).GetComponent<Text>().text = GameManager.GetInstance().player.Gold == 0 ? "0" : GameManager.GetInstance().player.Gold.ToString("#,###");

        var shopItem = shop.GetItem();

        for (int i = 0; i < _shopItemSlot.Length; i++)
        {
            _shopItemSlot[i].GetChild(0).GetComponent<Image>().sprite = shopItem[i].Image;
            _shopItemSlot[i].GetChild(0).GetComponent<Image>().SetNativeSize();
            if (shopItem[i].Type == ItemType.HP_Value) _shopItemSlot[i].GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(51, 69);

            _shopItemSlot[i].GetChild(1).GetComponent<Text>().text = shopItem[i].Name;
            _shopItemSlot[i].GetChild(2).GetComponent<Text>().text = shopItem[i].Explain;
            _shopItemSlot[i].GetChild(3).GetComponent<Text>().text = shopItem[i].Price.ToString("#,###");

            _shopItemSlot[i].gameObject.SetActive(true);
        }

        AddFuncButton(shop);
    }

    // 상점 구매 버튼에 기능추가
    private void AddFuncButton(Shop shop)
    {
        // 초기화
        _shopItemSlot[0].GetChild(4).GetComponent<Button>().onClick.RemoveAllListeners();
        _shopItemSlot[1].GetChild(4).GetComponent<Button>().onClick.RemoveAllListeners();

        // 부모에 유아이 가져오는거 있으니 그걸 교체해라~
        _shopUI.transform.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();

        // 아이템 리스트
        var list = shop.GetItem(true);

        // 등록 - 아이템 구매 버튼
        _shopItemSlot[0].GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
        {
            if (GameManager.GetInstance().player.CheckGold(list[0].Price))
            {
                _shopUI.transform.GetChild(2).GetComponent<Text>().text = GameManager.GetInstance().player.Gold == 0 ? "0" : GameManager.GetInstance().player.Gold.ToString("#,###");
                _shopItemSlot[0].gameObject.SetActive(false);

                EachShopItemFunc(list[0]);
            }
            else
            {
                base.SetPopUpUIActive("재화가 부족하여 구매를 할 수 없습니다.", false, true);
                base.SetPopUpButtonAction("확인", base.AddButtonActionPassiveUI);
            }
        });

        _shopItemSlot[1].GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
        {
            if (GameManager.GetInstance().player.CheckGold(list[1].Price))
            {
                _shopUI.transform.GetChild(2).GetComponent<Text>().text = GameManager.GetInstance().player.Gold == 0 ? "0" : GameManager.GetInstance().player.Gold.ToString("#,###");
                _shopItemSlot[1].gameObject.SetActive(false);

                EachShopItemFunc(list[1]);
            }
            else
            {
                base.SetPopUpUIActive("재화가 부족하여 구매를 할 수 없습니다.", false, true);
                base.SetPopUpButtonAction("확인", base.AddButtonActionPassiveUI);
            }
        });

        // 닫기 버튼 기능 추가
        var exitButton = _shopUI.transform.GetChild(1).GetComponent<Button>();
        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(() =>
        {
            _isOpenShop = false;
            GameManager.GetInstance().player.OnGodMode(1.5f);
            base.SetActiveUI(_ingameUI, "Shop|off", true, false);
        });
    }

    // 상점에서 아이템별로 팝업창을 다르게 뜨게 함
    private bool EachShopItemFunc(ItemScritable itemInfo)
    {
        switch (itemInfo.Type)
        {
            case ItemType.EXP_Percentage:
            case ItemType.LevelUP:
            case ItemType.RandomRune:
                base.SetPopUpUIActive(itemInfo.Name + "구매 성공", false, true);
                base.SetPopUpButtonAction("확인", AddFuncButtonShopBuy, itemInfo);
                break;
            case ItemType.HP_Value:
            case ItemType.HP_Percentage:
                GameManager.GetInstance().RewardItemValue(itemInfo);
                return false;
            case ItemType.Reborn:
                base.SetPopUpUIActive("부활의 구슬 획득", false, true);
                base.SetPopUpButtonAction("확인", AddButtonActionPassiveUI);
                break;
            default:
                break;
        }

        return true;
    }

    // 상점에서 룬, 경험치포션 을 구매하였을때 확인 버튼 기능
    private void AddFuncButtonShopBuy(Button button, ItemScritable itemInfo)
    {
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            base.PassivePopUpUI();
            GameManager.GetInstance().RewardItemValue(itemInfo);
        });
    }
    #endregion

}
