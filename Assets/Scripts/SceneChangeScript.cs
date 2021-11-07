// Jeremy Fischer 11/7/2021
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeScript : MonoBehaviour
{
    [SerializeField] string exactSceneName;
    public void ChangeScene()
    {
        SceneManager.LoadScene(exactSceneName);
    }
}
