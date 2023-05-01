using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }
    void Awake() 
    { 
        if (Instance != null && Instance != this) Destroy(this); 
        else Instance = this;
    }
    int PackagesInExistence = 0;
    public int deliveredPackages = 0;
    public int packagesInExistence {
        get{
            return PackagesInExistence;
        }
        set{
            int delta = value - packagesInExistence;
            if(delta > 0) {
                for (int i = 0; i < delta; i++) {
                    if(allDoors.Count == 0) {
                        allDoors = GameObject.FindGameObjectsWithTag("Door").Select(d => d.GetComponent<Door>()).ToList();
                    }

                    if(allDoors.Count > 0) {
                        Door newDoor = allDoors[Random.Range(0, allDoors.Count)];
                        // if(targetDoors.Contains(newDoor)) {
                        //     i--;
                        //     continue;
                        // }

                        newDoor.AddDelivery();
                        if(!targetDoors.Contains(newDoor)) 
                        targetDoors.Add(newDoor);
                    }
                }
            }


            PackagesInExistence = value;
        }
    }
    List<Door> allDoors = new List<Door>();
    public List<Door> targetDoors = new List<Door>();
    void Start() {
        Random.InitState((int)System.DateTime.Now.Ticks);
        allDoors = GameObject.FindGameObjectsWithTag("Door").Select(d => d.GetComponent<Door>()).ToList();
    }
    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }
    public void CompleteDoor(Door door) {
        if(targetDoors.Contains(door)) {
            targetDoors.Remove(door);
        }
    }
    public void TransitionToMenu() {
        SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single).completed += _ => 
        { //When menu is loaded, set it to the currently active scene
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Menu"));
          
        };
    }
}
