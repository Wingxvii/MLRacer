using UnityEngine;
using System;
using System.Collections.Generic;

/*
* Name: John Wang
* Date: 11/18/20
* Desc: Car controller class with NN AI support
*
*/

[RequireComponent(typeof(NeuralNet))]
public class CarMovement : MonoBehaviour
{
    //spawn position
    private Vector3 startPosition;
    private Vector3 startRotation;
    
    //manual controls from editor
    [Header("Controls")]
    public bool saveButton = false;     //budget save button
    [Range(0f, 1f)]
    public float acceleration;      // output of nn
    [Range(-1f, 1f)]
    public float turn;              // output of nn

    //physics variables
    [Header("Physics")]
    public float accelRate = 0.02f;
    public float forwardSpeed = 11.4f;
    private Vector3 moveVec;
    public float turnRate = 0.02f;

    //neural network
    private NeuralNet nnet;
    [Header("Network")]
    public int n_layers = 1;
    public int n_neurons = 10;

    //idle timer
    public float lifetime = 0f;

    //fitness variables
    [Header("Fitness")]
    public float overallFitness;
    private Vector3 lastPosition;

    //weight of distance to fitness
    public float distanceWeight = 1.4f;
    private float totalDist;

    //weight of speed to fitness
    public float speedWeight = 0.2f;
    private float avgSpeed;

    //weight of a completed lap
    private float lapsCompleted = 0;
    public float lapCompletedWeight = 300.0f;

    //weight of the sensors
    public float sensorWeight = 0.1f;

    //sensors
    public float[] sensors = { 0, 0, 0 };
    public float sensorDist = 5f;
    public LayerMask wallMask;

    //fitness kill gates (time, fitness)
    public List<Tuple<float, float>> gates;
    public float successGate = 1000f;
    public bool useGate = true;

    //track agent statistics
    public static int alphaAgents = 0;
    public bool inTraining = false;
    
    //non-singleton manager references
    public RLManager learningManager;
    public MapManager mapManager;
    
    private bool started = false;
        
    private void Awake()
    {
        //setup environment
        nnet = GetComponent<NeuralNet>();
        wallMask = LayerMask.GetMask("Wall");
        gates = new List<Tuple<float, float>>
        {
            //add gates here
            new Tuple<float, float>(20, 40)
        };
    }
    
    private void FixedUpdate()
    {   
        if (started)
        {
            //update sensors first
            Sensors();
            
            //then run network
            (acceleration, turn) = nnet.RunNetwork(sensors[0], sensors[1], sensors[2]);                     //TODO: adapt network to accomadate variable size input and remove hardcoded sensor arguments
            
            //move according to network output
            Move(acceleration, turn);
            
            //iterate lift
            lifetime += Time.deltaTime;
            
            //evaluate fitness last
            EvalFitness();
        }

        //update save button
        if (saveButton)
        {
            OnSaveButton();
            saveButton = false;
        }
    }

    //set starting position data
    public void SetStart(Transform start) {
        startPosition = start.position;
        startRotation = start.rotation.eulerAngles;

        this.transform.position = start.position;
        this.transform.rotation = start.rotation;
        started = true;
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
    
    //calculate sensor input
    private void Sensors() {
        //init sensor directions
        Vector3[] dirs = { transform.forward + transform.right, transform.forward, transform.forward - transform.right };
        Ray raycast;

        //raycast each sensor
        for (int i = 0; i < dirs.Length; i++) {
            raycast = new Ray(transform.position, dirs[i]);
            RaycastHit hit;
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
        foreach (float sensor in sensors)
        {
            sensorTotalAvg += sensor;
        }
        sensorTotalAvg /= sensors.Length;

        //add up fitness weights
        overallFitness = (totalDist * distanceWeight) + (avgSpeed * speedWeight) + (sensorTotalAvg * sensorWeight) + (lapsCompleted * lapCompletedWeight);

        //check kill gates
        if (useGate)
        {
            foreach (Tuple<float, float> gate in gates)
            {
                if (lifetime > gate.Item1 && overallFitness < gate.Item2)
                {
                    Death();
                }
            }
        }
        //success gate
        if (overallFitness >= successGate) {
            //nnet.Save();
            alphaAgents++;
            Death();
        }
    }

    //button for manually saving button
    public void OnSaveButton()
    {
        nnet.Save();
    }

    //when a lap has been completed
    public void CompletedLap() {
        lapsCompleted++;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //hitting a wall = death
        if (collision.gameObject.tag == "Wall" && inTraining)
        {
            Death();
        }
    }

    //check for end gate logic
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Gate1")
        {
            mapManager.HitGate(1, false);
        }
        else if (other.gameObject.tag == "Gate2")
        {
            mapManager.HitGate(2, false);
        }
    }

    //check for end gate logic
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Gate1")
        {
            mapManager.HitGate(1, true);
        }
        else if (other.gameObject.tag == "Gate2")
        {
            mapManager.HitGate(2, true);
        }

    }

    //agent death
    private void Death() {
        if (inTraining && learningManager)
        {
            learningManager.Death(overallFitness, nnet);
        }
    }

    //reset network
    public void ResetWithNetwork(NeuralNet net) {
        nnet = net;
        Reset();
    }

    //reset everthing
    public void Reset()
    {
        lifetime = 0f;
        totalDist = 0f;
        avgSpeed = 0f;
        lapsCompleted = 0.0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }
}
