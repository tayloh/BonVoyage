using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [SerializeField]
    private Slider _LoadingBar;

    public void LoadLevel (int sceneIndex)
    {
        StartCoroutine(LoadLevelAsync(sceneIndex));
    }

    private IEnumerator LoadLevelAsync(int sceneIndex)
    {
        _LoadingBar.gameObject.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        var loadText = _LoadingBar.GetComponentInChildren<TMPro.TextMeshProUGUI>();

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            _LoadingBar.value = progress;
            loadText.text = progress * 100 + "%";

            yield return null;
        }
    } 

    public void PlayLevel1()
    {
        LoadLevel(1);
    }

    public void PlayLevel2()
    {
        LoadLevel(2);
    }

    public void PlayLevel10()
    {
        LoadLevel(3);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
