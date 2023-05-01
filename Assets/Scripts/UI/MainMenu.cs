using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void TransitionToGame() {
        SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Single).completed += _ => 
        { //When the in-game scene is loaded, set it to the currently active scene
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("SampleScene"));
        };
    }
}
