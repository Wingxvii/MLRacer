using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MathNet.Numerics.LinearAlgebra;
using Random = UnityEngine.Random;

public class NeuralNet : MonoBehaviour
{

    //Neural Network
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 3);      //[sensor[0], sensor[1], sensor[2]] input layer
    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();    //hidden layers
    public List<Matrix<float>> weights = new List<Matrix<float>>();         //weights
    public List<float> biases = new List<float>();                         //biases
    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2);     //[acceleration, turn] output layer

    public float fitness;

    //initalize neural net
    public void Init(int hiddenLayersCount, int hiddenNeuronsCount)
    {
        //clear it all
        inputLayer.Clear();
        hiddenLayers.Clear();
        weights.Clear();
        biases.Clear();
        outputLayer.Clear();

        //reinit input and output layers
        inputLayer = Matrix<float>.Build.Dense(1, 3);
        outputLayer = Matrix<float>.Build.Dense(1, 2);

        //add hidden layers + biases
        for (int i = 0; i < hiddenLayersCount-1; i++) {                                             //TODO: this should be -1??
            hiddenLayers.Add(Matrix<float>.Build.Dense(1, hiddenNeuronsCount));
            biases.Add(Random.Range(-1f,1f));

            //first hidden layer
            if (i == 0) {
                weights.Add(Matrix<float>.Build.Dense(3, hiddenNeuronsCount));                      //TODO: changed this from hiddenLayerCount
            }
            else
            {
                weights.Add(Matrix<float>.Build.Dense(hiddenNeuronsCount, hiddenNeuronsCount));
            }
        }
        //add final layer
        weights.Add(Matrix<float>.Build.Dense(hiddenNeuronsCount, 2));
        biases.Add(Random.Range(-1f, 1f));

        RandomizeWeights();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //add random weights to neural net
    public void RandomizeWeights() { 
        for(int x = 0; x < weights.Count; x++){
            for (int y = 0; y < weights[x].RowCount; y++) {
                for (int z = 0; z < weights[x].ColumnCount; z++)
                {
                    weights[x][y, z] = Random.Range(-1f, 1f);
                }
            }
        }
    }


    public (float, float) RunNetwork(float a, float b, float c) {
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;

        //activation function uses TanH
        inputLayer = inputLayer.PointwiseTanh();

        //first hidden layer
        hiddenLayers[0] = ((inputLayer * weights[0]) + biases[0]).PointwiseTanh();

        //remaining hidden layers
        for (int i = 1; i < hiddenLayers.Count; i++) {
            hiddenLayers[i] = ((hiddenLayers[i - 1] * weights[i]) + biases[i]).PointwiseTanh();
        }

        //output layer
        outputLayer = ((hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count - 1]) + biases[biases.Count - 1]).PointwiseTanh();

        //outputs acceleration and turn
        return (Sigmoid(outputLayer[0,0]), (float)Math.Tanh(outputLayer[0,1]));
    }

    //sigmoid function activation
    private float Sigmoid(float s) {
        return (1 / (1 + Mathf.Exp(-s)));
    }
}
