using UnityEngine;
using System;
using System.Collections.Generic;
public class CarMovement : MonoBehaviour
{
    //spawn
    private Vector3 startPosition;
    private Vector3 startRotation;

    //physics
    [Range(-1f, 1f)]
    public float acceleration;      // output of nn
    public float accelRate = 0.02f;
    public float forwardSpeed = 11.4f;
    private Vector3 moveVec;

    [Range(-1f, 1f)]
    public float turn;              // output of nn
    public float turnRate = 0.02f;

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

    //weight of the sensors
    public float sensorWeight = 0.1f;

    //sensors
    public float[] sensors = {0,0,0};
    public float sensorDist = 5f;
    public LayerMask wallMask;

    //fitness kill gates (time, fitness)
    public List<Tuple<float, float>> gates;
    public float successGate = 1000f;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        wallMask = LayerMask.GetMask("Wall");
        gates = new List<Tuple<float, float>>
        {
            //add gates here
            new Tuple<float, float>(20, 40)
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Sensors();

        //neural network here
        Move(acceleration,turn);
        lifetime += Time.deltaTime;

        EvalFitness();

        //acceleration = 0f;
        //turn = 0f;
    }

    //moves racer
    public void Move(float accel, float rot)
    {
        //acceleration
        moveVec = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, accel * forwardSpeed), accelRate);
        moveVec = transform.TransformDirection(moveVec);
        transform.position += moveVec;

        //rotation
        transform.eulerAngles += new Vector3(0, rot * 90 * turnRate, 0);
    }

    private void Sensors() {
        //init sensor directions
        Vector3[] dirs = { transform.forward + transform.right, transform.forward, transform.forward - transform.right };
        Ray raycast;
        
        //raycast each sensor
        for(int i = 0; i < dirs.Length; i++) {
            raycast = new Ray(transform.position, dirs[i]);
            RaycastHit hit;
            //TODO: limit sensor dist
            if (Physics.Raycast(raycast, out hit, sensorDist, wallMask))
            {
                sensors[i] = hit.distance / sensorDist;
            }
            //draw sensors
            Debug.DrawLine(raycast.origin, raycast.origin + (dirs[i] * sensorDist), Color.red);
        }
    }

   
    //calculate fitness based on reward criteria
    private void EvalFitness() {
        //calculate distance moved
        totalDist += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        //calculate average speed
        avgSpeed = totalDist / lifetime;

        //calculate sensor averages
        float sensorTotalAvg = 0f;
        foreach(float sensor in sensors)
        {
            sensorTotalAvg += sensor;
        }
        sensorTotalAvg /= sensors.Length;

        //add up fitness weights
        overallFitness = (totalDist * distanceWeight) + (avgSpeed * speedWeight) + (sensorTotalAvg * sensorWeight);
        
        //check kill gates
        foreach(Tuple<float, float> gate in gates) {
            if (lifetime > gate.Item1 && overallFitness < gate.Item2) {
                Reset();
            }
        }
        //success gate
        if (overallFitness >= successGate) {
            //save to json
            Reset();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //hitting a wall = death
        if(collision.gameObject.tag == "Wall")
        {
            Reset();
        }
    }

    //reset everthing
    public void Reset()
    {
        lifetime = 0f;
        totalDist = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }
}
