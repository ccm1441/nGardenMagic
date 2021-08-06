using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Security.Cryptography;

/// <summary>
///  데이터를 관리하는 스크립트
/// </summary>
/// 


public class DataManager : MonoBehaviour
{
    // 대충 선언한 싱글톤
    private static DataManager instance;

    // 저장 경로
    string goldPath, skillPath;

    // 데이터 셋
    [HideInInspector] public SkillData skillData;
    [HideInInspector] public Data data;
    [HideInInspector] public LobbyUI lobbyUI;

    private void Awake()
    {
        if (instance != null) Destroy(instance);

        goldPath = Application.persistentDataPath + "\\Data.json";
        skillPath = Application.persistentDataPath + "\\Data2.json";
        print(goldPath);
        instance = this;       
    }

    public static DataManager GetInstance()
    {
        if (instance == null)
        {
            print("데이터매니저 인스턴스 없음");
            return null;
        }

        return instance;
    }

    #region 데이터 암,복호화
    // 암호키 정의
    private static readonly string PASSWORD = "djbkdje3yslvnd2asn4vn3jkv9dj0f";

    // 인증키 정의
    private static readonly string KEY = PASSWORD.Substring(0, 128 / 8);

    // 암호화
    public static string AESEncrypt128(string plain)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plain);

        RijndaelManaged myRijndael = new RijndaelManaged();
        myRijndael.Mode = CipherMode.CBC;
        myRijndael.Padding = PaddingMode.PKCS7;
        myRijndael.KeySize = 128;

        MemoryStream memoryStream = new MemoryStream();

        ICryptoTransform encryptor = myRijndael.CreateEncryptor(Encoding.UTF8.GetBytes(KEY), Encoding.UTF8.GetBytes(KEY));

        CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
        cryptoStream.FlushFinalBlock();

        byte[] encryptBytes = memoryStream.ToArray();

        string encryptString = Convert.ToBase64String(encryptBytes);

        cryptoStream.Close();
        memoryStream.Close();

        return encryptString;
    }

    // 복호화
    public static string AESDecrypt128(string encrypt)
    {
        byte[] encryptBytes = Convert.FromBase64String(encrypt);

        RijndaelManaged myRijndael = new RijndaelManaged();
        myRijndael.Mode = CipherMode.CBC;
        myRijndael.Padding = PaddingMode.PKCS7;
        myRijndael.KeySize = 128;

        MemoryStream memoryStream = new MemoryStream(encryptBytes);

        ICryptoTransform decryptor = myRijndael.CreateDecryptor(Encoding.UTF8.GetBytes(KEY), Encoding.UTF8.GetBytes(KEY));

        CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

        byte[] plainBytes = new byte[encryptBytes.Length];

        int plainCount = cryptoStream.Read(plainBytes, 0, plainBytes.Length);

        string plainString = Encoding.UTF8.GetString(plainBytes, 0, plainCount);

        cryptoStream.Close();
        memoryStream.Close();

        return plainString;
    }
    #endregion

    // 골드, 무기 오픈정보 세이브
    public bool SaveGold(int gold)
    {
        Data data = new Data();
        data.gold = gold;
        data.weaponOpenArray = PlayerInfo.weaponOpenArray;
        data.removeADs = PlayerInfo.removeADs;
        data.doubleReward = PlayerInfo.doubleReward;

        string jsonData = JsonUtility.ToJson(data);
        jsonData = AESEncrypt128(jsonData);
        File.WriteAllText(goldPath, jsonData);

        return true;
    }

    // 위에꺼 외 모든거 세이브
    public void SaveSkill(UpgradSkillInfo[] skillArray, int[] characterBuyList, int applyCharacter, int[] mapOpenArray)
    {
        SkillData data = new SkillData();
        data.currentLevel = new int[15];
        data.maxLevel = new int[15];
        data.characterBuy = new int[3];
        data.mapOpenArray = new int[3];
        data.applyCharacter = applyCharacter;
        data.controllerFixed = PlayerInfo.controllerFixed;
        data.sound = PlayerInfo.sound;

        for (int i = 0; i < 15; i++)
        {
            data.currentLevel[i] = skillArray[i].currentLevel;
            data.maxLevel[i] = skillArray[i].maxLevel;
        }

        for (int i = 0; i < 3; i++)
            data.characterBuy[i] = characterBuyList[i];

        for (int i = 0; i < 3; i++)
            data.mapOpenArray[i] = mapOpenArray[i];


        string jsonData = JsonUtility.ToJson(data);
        jsonData = AESEncrypt128(jsonData);
        File.WriteAllText(skillPath, jsonData);
    }

    // 데이터 로드
    public SkillData Load()
    {
        string jsonData;

        try
        {
            if (File.Exists(goldPath))
            {
                jsonData = File.ReadAllText(goldPath);
                jsonData = AESDecrypt128(jsonData);
                data = JsonUtility.FromJson<Data>(jsonData);
                PlayerInfo.gold = data.gold;
                PlayerInfo.removeADs = data.removeADs;
                PlayerInfo.doubleReward = data.doubleReward;
            }

            if (File.Exists(skillPath))
            {
                jsonData = File.ReadAllText(skillPath);
                jsonData = AESDecrypt128(jsonData);
                skillData = JsonUtility.FromJson<SkillData>(jsonData);
                return skillData;
            }
        }
        catch (Exception e)
        {
            print("ERROR Data Load : " + e.Message);
        }

        return null;
    }
}

// 저장할 데이터
// 캐릭터 오픈 여부 및 착용 인덱스, 플레이어 재화, 스킬 강화 및 가격, 상점 아이템 구매여부, 옵션
[System.Serializable]
public class Data
{
    public int gold;
    public int[] weaponOpenArray;
    public bool removeADs;
    public bool doubleReward;
}

[System.Serializable]
public class SkillData
{
    public int[] currentLevel;
    public int[] maxLevel;
    public int[] characterBuy;
    public int[] mapOpenArray;
    public int applyCharacter;
    public bool controllerFixed;
    public bool sound;
}
