using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public CarMovement car;

    public GameObject[] trackPrefabs;
    private List<GameObject> tracks = new List<GameObject>();

    public GameObject currentTrack;
    public int currentTrackNum = 0;
    public bool iterateTracks = false;

    //start gate logic
    public bool firstHit = false;
    public bool secondHit = false;

    // Start is called before the first frame update
    void Start()
    {
        //init all 4 tracks
        foreach (GameObject track in trackPrefabs) {
            GameObject trackInst = Instantiate(track, new Vector3(0, -0.45f, 0), Quaternion.identity);
            trackInst.SetActive(false);

            tracks.Add(trackInst);
        }

        //open track 1 by default
        OpenTrack(0);
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
        if (gateNum == 1 && !exited)
        {
            if (secondHit && !firstHit)
            {
                LapCompleted();
            }
            else {
                firstHit = true;
            }
        }
        else if (gateNum == 1 && exited) {
            if (!secondHit)
            {
                firstHit = false;
            }
        }
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
            OpenTrack(currentTrackNum);
        }

        //reset hit logic
        firstHit = false;
        secondHit = false;

        Debug.Log("Lap Completed");
    }
}
