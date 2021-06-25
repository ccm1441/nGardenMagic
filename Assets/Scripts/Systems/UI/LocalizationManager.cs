using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class LocalizationManager : MonoBehaviour
{
    private static LocalizationManager instance;
    Dictionary<string, string> lan_en;

    public SystemLanguage _language;
    private bool _change;

    private void Awake()
    {
        if (instance != null) Destroy(instance);

        instance = this;

        // _language = Application.systemLanguage;
        _language = SystemLanguage.Korean;
        //// 한국어 제외 모든언어 영어로 변환
        //if (_language != SystemLanguage.Korean)
        //{
        //    lan_en = new Dictionary<string, string>();
        //    Load();
        //    _change = true;
        //}
    }

    public static LocalizationManager GetInstance()
    {
        if (instance == null)
        {
            print("LocalizationManager 인스턴스 없음");
            return null;
        }

        return instance;
    }

    private void Load()
    {
        TextAsset _txt = (TextAsset)Resources.Load("Language") as TextAsset;

        string dataStaring = _txt.text;

        var dataValue = dataStaring.Split('\n');

        print(dataValue.Length);
        for (int i = 0; i < dataValue.Length; i++)
        {
            var result = dataValue[i].Split(',');
            lan_en.Add(result[0], result[2]);
        }
    }

    public string ChangeLanguage(string key)
    {
        if (!_change) return string.Empty;

        return lan_en[key];
    }
}

