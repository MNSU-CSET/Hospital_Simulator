//Nate Bursch
//2/8/2022
//Overall Scenario Controller

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioController : ScenarioElement
{
    //this is where the scene is managed as a whole

    //for example, when objectives are complete
    public void StartEndGame()
    {
        //when the objective controller is completed, it will call this function.
        StartCoroutine(EndGameStart());
    }
    IEnumerator EndGameStart()
    {
        //this function waits for 5 seconds before switching scenes
        yield return new WaitForSeconds(5);
        //calls this game
        EndGameEnd();
    }
    void EndGameEnd()
    {
        //change scenes
        SceneManager.LoadScene("MainMenu");
    }
}
