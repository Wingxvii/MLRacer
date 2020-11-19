using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
/*
* Name: John Wang, Boris Au, Alex Siciak
* Date: 11/18/20
* Desc: Manager for map, track control and perspetive
*
*/

public class MapManager : MonoBehaviour
{
    //ref to car
    public CarMovement car;
    
    //lists of all tracks
    public GameObject[] trackPrefabs;
    private List<GameObject> tracks = new List<GameObject>();

    //track variables
    public GameObject currentTrack;
    public int currentTrackNum = 0;
    public bool iterateTracks = false;
    
    //camera references
    public Camera fps;
    public Camera tp;

    //start gate logic
    private bool firstHit = false;
    private bool secondHit = false;

    private void Awake()
    {
        //init perspective on awake
        if (AsyncSceneManager.Instance.perspectiveBool)
        {
            fps.gameObject.SetActive(true);
        }
        else
        {
            tp.gameObject.SetActive(true);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //init all 4 tracks
        foreach (GameObject track in trackPrefabs) {
            GameObject trackInst = Instantiate(track, new Vector3(0, -0.45f, 0), Quaternion.identity);
            trackInst.SetActive(false);

            tracks.Add(trackInst);
        }
        if (AsyncSceneManager.Instance.trainingTrack != -1)
        {
            //load user selected scene
            OpenTrack(AsyncSceneManager.Instance.trainingTrack);
        }
        else
        {
            //open track 1 by default
            OpenTrack(0);
        }

        //load training model through menus
        if (AsyncSceneManager.Instance.modelPath != "") {
            try{
                StreamReader sr = new StreamReader(AsyncSceneManager.Instance.modelPath);
                car.GetComponent<NeuralNet>().Load(sr.ReadToEnd());
            }
            catch (Exception e)
            {
                Debug.LogError("Model Invalid!");
            }
        }

    }

    private void Update() {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    //open first track
    void OpenTrack(int trackNum)
    {
        if (currentTrack)
        {
            currentTrack.SetActive(false);
        }
        currentTrack = tracks[trackNum];
        currentTrack.SetActive(true);
        car.SetStart(currentTrack.transform.Find("StartPosition"));
    }

    //look at gate logic
    public void HitGate(int gateNum, bool exited) {
        //check to see if user entered gate 1
        if (gateNum == 1 && !exited)
        {
            if (secondHit && !firstHit)
            {
                //if gate 2 is already entered, and first wasn't already hit, we know the car passed through the right way
                LapCompleted();
            }
            else {
                //if ai re-enters first gate, after leaving the wrong way, dont end the lap
                firstHit = true;
            }
        }
        else if (gateNum == 1 && exited) {
            //if player exits the gate without hitting second, reset firstHit, and let them count the lap next time they pass
            if (!secondHit)
            {
                firstHit = false;
            }
        }
        //check second hit
        else if (gateNum == 2 && !exited)
        {
            secondHit = true;
        }else if (gateNum == 2 && exited)
        {
            secondHit = false;
        }
    }

    //reset course
    public void LapCompleted() {
        if (iterateTracks)
        {
            currentTrackNum++;
            if (currentTrackNum == 4)
            {
                currentTrackNum = 0;
            }
            //go to next track
            OpenTrack(currentTrackNum);
        }

        //iterate laps
        car.CompletedLap();

        //reset hit logic
        firstHit = false;
        secondHit = false;

        Debug.Log("Lap Completed");
    }
}
