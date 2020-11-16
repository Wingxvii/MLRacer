﻿using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public class AsyncSceneManager : MonoBehaviour
{

    #region SingletonCode
    private static AsyncSceneManager _instance;
    public static AsyncSceneManager Instance { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        //loads start menu
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
    }
    //single pattern ends here
    #endregion

    public int loadedScene = 1;

    //use to swap scene
    public void SwapScene(int scene)
    {
        UnloadScene(loadedScene);
        LoadScene(scene);
    }

    //use to load scene
    public void LoadScene(int scene)
    {
        loadedScene = scene;
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
    }

    //use to unload scene
    public void UnloadScene(int scene)
    {
        SceneManager.UnloadSceneAsync(scene);
    }

    //start variables
    public int trainingTrack = -1;
    public string modelPath = "";

}