using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenFadeManager : MonoBehaviour
{
    public FadeScreen fadeScreen;
    public string nextSceneName;

    private void Start()
    {
        GoToScene(nextSceneName);
    }

    public void GoToScene(string sceneName)
    {
        StartCoroutine(GoToSceneRoutine(sceneName));
    }

    IEnumerator GoToSceneRoutine(string sceneName)
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeScreen.fadeDuration);


        SceneManager.LoadScene(sceneName);
        
    }

}
