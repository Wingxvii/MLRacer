using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
/*
* Name: John Wang, Boris Au, Alex Siciak 
* Date: 11/18/20
* Desc: Singleton scene manager script
*
*/
public class AsyncSceneManager : MonoBehaviour
{
    //singleton stuff
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
    
    //track current scene(other from the async scene)
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
    public bool perspectiveBool = true;

}
