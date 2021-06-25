using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
///  UI를 관리하는데 공통적인 함수들을 모아둠
/// </summary>

public class UIManager : MonoBehaviour
{
    [Header("===== Button Sound =====")]
    [SerializeField] protected List<AudioClip> _sound;
    protected AudioSource _audioSource;

    [SerializeField] protected List<GameObject> _popUpUI;
    protected bool _isActiveUI;                                 // 현재 UI가 활성화 된 상태인가
    protected int _activeUIIndex;                               // 배열에서 몇번째 인덱스가 활성화 상태인가

    protected delegate void ButtonAction(Button button);
    protected delegate void ButtonAction<T>(Button button, T info);

    /// <summary>
    /// 버튼 사운드 재생
    /// </summary>
    /// <param name="yes"></param>
   protected void PlayButtonSound(bool yes)
    {
        if (yes)
        {
            // _audioSource.clip = _sound[0];
            //  _audioSource.Play();
            return;
        }

        _audioSource.clip = _sound[1];
        _audioSource.Play();
    }

    /// <summary>
    /// 리스트에 있는 UI 인덱스를 찾아서 반환
    /// </summary>
    /// <param name="UI"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    protected int GetUIIndex(List<GameObject> UI, string name)
    {
        return UI.FindIndex(x => x.name.Contains(name));
    }

    /// <summary>
    /// UI리스트에 있는 어떤것을 찾아서 켜고 끔, 시간 조절
    /// </summary>
    /// <param name="UI"></param>
    /// <param name="value"></param>
    /// <param name="timeStop">시간을 멈출것인가</param>
    /// <param name="passiveBefore">이전 UI를 끌것인가</param>
    protected void SetActiveUI(List<GameObject> UI, string value, bool timeStop = false, bool passiveBefore = true)
    {
        var splitIndex = value.IndexOf('|');
        var popUpName = value.Substring(0, splitIndex);
        var flag = value.Substring(splitIndex + 1) == "off" ? false : true;

        if (timeStop) Time.timeScale = flag ? 0 : 1;

        for (int i = 0; i < UI.Count; i++)
        {
            if (UI[i].name.Contains(popUpName))
            {
               if(passiveBefore) UI[_activeUIIndex].SetActive(!flag);

                UI[i].SetActive(flag);

                _isActiveUI = flag;
                _activeUIIndex = i;

                break;
            }
        }       
    }

    /// <summary>
    /// 팝업 켜고 끄기
    /// </summary>
    /// <param name="text"></param>
    /// <param name="isSelectUI"></param>
    /// <param name="flag"></param>
    protected void SetPopUpUIActive(string text, bool isSelectUI, bool flag)
    {
        if(isSelectUI)
        {
            _popUpUI[0].transform.GetChild(2).GetComponent<Text>().text = text;
            _popUpUI[0].SetActive(flag);
            return;
        }

        _popUpUI[1].transform.GetChild(2).GetComponent<Text>().text = text;
        _popUpUI[1].SetActive(flag);
    }

    /// <summary>
    /// 매개변수로 받은 버튼에 기능을 넣음(버튼이 2개인 팝업)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="action"></param>
    /// <param name="isRight"></param>
    protected void SetPopUpButtonAction(string text, ButtonAction action, bool isRight)
    {
        Button button;
        if(isRight) button = _popUpUI[0].transform.GetChild(3).GetComponent<Button>();
        else button = _popUpUI[0].transform.GetChild(4).GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.transform.GetChild(0).GetComponent<Text>().text = text;
        
        action(button);
    }

    /// <summary>
    ///  매개변수로 받은 버튼에 기능을 넣음(버튼이 1개인 팝업)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="action"></param>
    protected void SetPopUpButtonAction(string text, ButtonAction action)
    {
        Button button = _popUpUI[1].transform.GetChild(3).GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.transform.GetChild(0).GetComponent<Text>().text = text;

        action(button);
    }

    /// <summary>
    /// 항목별로 다른 팝업을 구성할때(인게임 상점 아이템 구매시)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="text"></param>
    /// <param name="action"></param>
    /// <param name="info"></param>
    protected void SetPopUpButtonAction<T>(string text, ButtonAction<T> action , T info)
    {
        Button button = _popUpUI[1].transform.GetChild(3).GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.transform.GetChild(0).GetComponent<Text>().text = text;

        action(button, info);
    }

    /// <summary>
    /// 매개변수로 받은 씬으로 넘어감
    /// </summary>
    /// <param name="value"></param>
    protected void LoadNextScene(string value) => SceneManager.LoadScene(value);

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    /// <param name="button"></param>
    protected void AddButtonActionPassiveUI(Button button)
    {
        button.onClick.AddListener(() =>
        {
            EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.SetActive(false);
        });
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    protected void PassivePopUpUI() => EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.SetActive(false);
}
