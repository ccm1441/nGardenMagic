using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public enum SceneName
{
    None,
    Loading,
    Lobby,
    Ingame,
}

public class LoadingManager : MonoBehaviour
{
    static SceneName nextScene, beforeScene;

    [SerializeField] private Text percentageText;
    [SerializeField] private Transform[] _loadingObject;
    private int _percent;

    public static void LoadScene(SceneName beforeSceneName, SceneName nextSceneName)
    {
        nextScene = nextSceneName;
        beforeScene = beforeSceneName;

        SceneManager.LoadSceneAsync(SceneName.Loading.ToString(), LoadSceneMode.Additive);
    }

    private void Start()
    {
        Time.timeScale = 1;

        try
        {
            print($"{beforeScene} > {nextScene} 로딩 시작");

            // 로딩바 활성화
            var loadingBar = _loadingObject[Random.Range(0, _loadingObject.Length)];
            loadingBar.gameObject.SetActive(true);

            if (beforeScene != SceneName.None)
            {
                if (beforeScene == SceneName.Ingame)
                {
                    SceneManager.UnloadSceneAsync(SceneName.Ingame.ToString());
                }
                else if (beforeScene == SceneName.Lobby)
                {
                    SceneManager.UnloadSceneAsync(SceneName.Lobby.ToString());
                }
            }

            beforeScene = SceneName.None;
            StartCoroutine(LoadSceneProcess());
        }
        catch (System.Exception e)
        {
            print(e);
        }

    }

    private void Update()
    {
        percentageText.text = _percent + " %";
    }

    IEnumerator LoadSceneProcess()
    {
        yield return new WaitForSeconds(1f);

        AsyncOperation scene1 = null;

        if (nextScene == SceneName.Ingame)
        {
            scene1 = SceneManager.LoadSceneAsync(SceneName.Ingame.ToString(), LoadSceneMode.Additive);
        }
        else if (nextScene == SceneName.Lobby)
        {
            scene1 = SceneManager.LoadSceneAsync(SceneName.Lobby.ToString(), LoadSceneMode.Additive);
        }

        _percent = 10;

        scene1.allowSceneActivation = false;

        float timer = 0f;

        _percent = 30;

        while (true)
        {
            timer += Time.deltaTime;


            if (scene1.progress < 0.9f)
                _percent = 90;
            else break;

            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        _percent = 100;

        yield return new WaitForSeconds(2.0f);

        scene1.allowSceneActivation = true;
        yield return new WaitForSeconds(0.1f);
      
        SceneManager.UnloadSceneAsync(SceneName.Loading.ToString());
    }
}
