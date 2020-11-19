using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
/*
 * Name: John Wang, Boris Au, Alex Siciak
 * Date: 11/17/2020
 * Desc: Manager for game start menu logic
 * 
 */
public class StartManager : MonoBehaviour
{
    //references to start menu ui assets
    public GameObject testing;
    public GameObject start;
    public Text trackDisplay;
    public InputField modelSelection;
    public Text perspective;

    //bool tracking which gamemode player requests
    private bool isTraining = false;

    public void Awake()
    {   
        //deactivate testing second UI canvas
        testing.SetActive(false);
    }

    //go to training scene
    public void OnHitTraining() {
        AsyncSceneManager.Instance.SwapScene(2);
    }

    //open options for testing
    public void OnHitTesting() {
        isTraining = false;
        start.SetActive(false);
        testing.SetActive(true);
    }

    //input checks for when begin is hit 
    public void OnBegin() {
        if (isTraining)
        {
            //if training is hit, go to training
            AsyncSceneManager.Instance.SwapScene(2);
        }
        else {
            //exception handling for model path
            if (AsyncSceneManager.Instance.modelPath == "")
            {
                Debug.LogWarning("Please Input Model Path");
            }
            else
            {
                //model path file exception handling
                bool validated = false;
                try
                {
                    StreamReader sr = new StreamReader(AsyncSceneManager.Instance.modelPath);
                    string temp = sr.ReadToEnd();
                    validated = true;
                }
                catch (Exception e)
                {
                    Debug.LogError("Model Invalid!");
                    modelSelection.text = "Model Invalid!";
                }

                if (validated)
                {
                    //move to testing once model is validated
                    AsyncSceneManager.Instance.SwapScene(3);
                }
            }
        }
    }

    //track picking logic
    public void PickTrack(int track) {
        AsyncSceneManager.Instance.trainingTrack = track;
        trackDisplay.text = (track + 1).ToString();
    }

    //model path input update
    public void UpdatePath() {
        AsyncSceneManager.Instance.modelPath = modelSelection.text;
    }

    //perspective change logic
    public void OnPerspective() {
        if (AsyncSceneManager.Instance.perspectiveBool) {
            //change to third person
            AsyncSceneManager.Instance.perspectiveBool = false;
            perspective.text = "Third Person";
        }
        else
        {
            //change to first person
            AsyncSceneManager.Instance.perspectiveBool = true;
            perspective.text = "First Person";
        }
    }

    //exit button
    public void exitButton() {
        Application.Quit();
    }
}
