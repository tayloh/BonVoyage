using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [SerializeField]
    private Slider _LoadingBar;

    private void Start()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void LoadLevel (int sceneIndex)
    {
        StartCoroutine(LoadLevelAsync(sceneIndex));
    }

    private IEnumerator LoadLevelAsync(int sceneIndex)
    {
        var audioClip = GetComponent<AudioSource>();
        audioClip.Play();

        StartCoroutine(AudioManager.FadeDownSoundtrackCoroutine(audioClip.clip.length*1000, 0.05f));

        yield return new WaitForSeconds(audioClip.clip.length);

        _LoadingBar.gameObject.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        var loadText = _LoadingBar.GetComponentInChildren<TMPro.TextMeshProUGUI>();

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            _LoadingBar.value = progress;
            loadText.text = Mathf.RoundToInt(progress * 100) + "%";

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

    public void PlayLevel3()
    {
        LoadLevel(3);
    }

    public void PlayLevel4()
    {
        LoadLevel(4);
    }

    public void PlayLevel5()
    {
        LoadLevel(5);
    }

    public void PlayLevel6()
    {
        LoadLevel(6);
    }

    public void PlayLevel7()
    {
        LoadLevel(7);
    }

    public void PlayLevel8()
    {
        LoadLevel(8);
    }

    public void PlayLevel9()
    {
        LoadLevel(9);
    }

    public void PlayLevel10()
    {
        LoadLevel(10);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
