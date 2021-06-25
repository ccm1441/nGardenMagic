using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Localization : MonoBehaviour
{
    public string keyValue;
    private Text _text;

    private void Start()
    {
        var text = LocalizationManager.GetInstance().ChangeLanguage(keyValue);

        if (text != string.Empty) GetComponent<Text>().text = text;
    }
}
