using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Canvas sceneMenuCanvas;

    private void Start()
    {
        if (sceneMenuCanvas != null)
        {
            sceneMenuCanvas.gameObject.SetActive(false);

        }
        

    }

    public void RandomScene()
    {
        int randomNumber = Random.Range(1,1);
        SceneManager.LoadScene(randomNumber);
    }

    public void SceneMenu()
    {
        if (sceneMenuCanvas.gameObject.activeInHierarchy)
        {
            sceneMenuCanvas.gameObject.SetActive(false);
        }
        else
        {
            sceneMenuCanvas.gameObject.SetActive(true);

        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
