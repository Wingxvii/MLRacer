using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class StartManager : MonoBehaviour
{
    public GameObject testing;
    public GameObject start;
    public Text trackDisplay;
    public InputField modelSelection;

    private bool isTraining = false;

    public void Awake()
    {
        testing.SetActive(false);
    }

    public void OnHitTraining() {
        AsyncSceneManager.Instance.SwapScene(2);
    }
    public void OnHitTesting() {
        isTraining = false;
        start.SetActive(false);
        testing.SetActive(true);
    }
    public void OnBegin() {
        if (isTraining)
        {
            AsyncSceneManager.Instance.SwapScene(2);
        }
        else {
            if (AsyncSceneManager.Instance.modelPath == "")
            {
                Debug.LogWarning("Please Input Model Path");
            }
            else
            {
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
                    AsyncSceneManager.Instance.SwapScene(3);
                }
            }
        }
    }
    public void PickTrack(int track) {
        AsyncSceneManager.Instance.trainingTrack = track;
        trackDisplay.text = (track + 1).ToString();
    }

    public void UpdatePath() {
        AsyncSceneManager.Instance.modelPath = modelSelection.text;
    }
}
