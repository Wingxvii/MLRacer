using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    //spawn
    private Vector3 startPosition;
    private Vector3 startRotation;

    //physics
    [Range(-1f, 1f)]
    public float acceleration;
    [Range(-1f, 1f)]
    public float turn;

    //idle timer
    public float lifetime = 0f;

    [Header("Fitness")]
    public float overallFitness;
    private Vector3 lastPosition;

    //weight of distance to fitness
    public float distanceWeight = 1.4f;
    private float totalDist;

    //weight of speed to fitness
    public float speedWeight = 0.2f;
    private float avgSpeed;

    //sensors
    private float sensor1;
    private float sensor2;
    private float sensor3;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
