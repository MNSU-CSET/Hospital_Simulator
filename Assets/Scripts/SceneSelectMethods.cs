using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelectMethods : MonoBehaviour
{
    //all things you will have to add onto the script
    public AudioSource audioSource;
    //click noise
    public AudioClip clickSound;
    public AudioClip hoverSound;

    public void MainMenu()
    {
        audioSource.PlayOneShot(clickSound);
        //change this number depending on the build settings layout
        SceneManager.LoadScene(0);
    }

    public void Scene1()
    {
        audioSource.PlayOneShot(clickSound);
        //change this number depending on the build settings layout
        SceneManager.LoadScene(1);
    }
    public void Scene2()
    {
        audioSource.PlayOneShot(clickSound);
        //change this number depending on the build settings layout
        SceneManager.LoadScene(2);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

        public void HoverSound()
    {
        //attach to button when being hovored, to play this noise.
        audioSource.PlayOneShot(hoverSound);
    }
}
