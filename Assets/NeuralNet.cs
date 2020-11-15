using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using MathNet.Numerics.LinearAlgebra;
using Random = UnityEngine.Random;
using System.IO;

public class NeuralNet : MonoBehaviour
{

    //Neural Network
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 3);      //[sensor[0], sensor[1], sensor[2]] input layer
    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();    //hidden layers
    public List<Matrix<float>> weights = new List<Matrix<float>>();         //weights
    public List<float> biases = new List<float>();                         //biases
    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2);     //[acceleration, turn] output layer

    public int hiddenLayersCount = 0;
    public int hiddenNeuronsCount = 0;

    public float fitness;

    //save data
    private string path;
    public TextAsset networkFile;

    //start ONLY WITHOUT training manager
    public void Start()
    {
        //load saved network instead of from training
        if (!GetComponent<CarMovement>().inTraining)
        {
            if (networkFile)
            {
                Load(networkFile.ToString());
            }
            else {
                Debug.LogError("Network Load File Missing");
            }
        }
    }

    //init by copy instead of reference
    public static NeuralNet InitCopy(NeuralNet copy, int hiddenLayersCount, int hiddenNeuronsCount)
    {
        NeuralNet net = new NeuralNet();

        net.hiddenLayersCount = hiddenLayersCount;
        net.hiddenNeuronsCount = hiddenNeuronsCount;

        //copy weigths over
        net.weights = new List<Matrix<float>>();
        for (int x = 0; x < copy.weights.Count; x++)
        {
            net.weights.Add(Matrix<float>.Build.DenseOfMatrix(copy.weights[x]));
        }

        //copy biases over
        net.biases = new List<float>(copy.biases);

        //add hidden layers
        for (int x = 0; x < hiddenLayersCount + 1; x++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronsCount);
            net.hiddenLayers.Add(newHiddenLayer);
        }

        return net;
    }

    //initalize neural net
    public void Init(int hiddenLayersCount, int hiddenNeuronsCount)
    {
        //clear it all
        inputLayer.Clear();
        hiddenLayers.Clear();
        weights.Clear();
        biases.Clear();
        outputLayer.Clear();
        fitness = 0;

        //reinit input and output layers
        inputLayer = Matrix<float>.Build.Dense(1, 3);
        outputLayer = Matrix<float>.Build.Dense(1, 2);

        this.hiddenLayersCount = hiddenLayersCount;
        this.hiddenNeuronsCount = hiddenNeuronsCount;

        //add hidden layers + biases
        for (int i = 0; i < hiddenLayersCount + 1; i++) {
            hiddenLayers.Add(Matrix<float>.Build.Dense(1, hiddenNeuronsCount));
            biases.Add(Random.Range(-1f,1f));

            //first hidden layer
            if (i == 0) {
                weights.Add(Matrix<float>.Build.Dense(3, hiddenNeuronsCount));
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

    //object used for json seralize
    public class SerializableNetwork
    {
        public int hiddenLayersCount;
        public int hiddenNeuronsCount;

        public float[][,] weights;
        public float[] biases;
    }

    //load from json method
    public void Load(string json)
    {
        //clear it all
        inputLayer.Clear();
        hiddenLayers.Clear();
        weights.Clear();
        biases.Clear();
        outputLayer.Clear();
        fitness = 0;

        //reinit input and output layers
        inputLayer = Matrix<float>.Build.Dense(1, 3);
        outputLayer = Matrix<float>.Build.Dense(1, 2);

        Debug.Log(json);

        SerializableNetwork save = JsonConvert.DeserializeObject<SerializableNetwork>(json);

        this.hiddenLayersCount = save.hiddenLayersCount;
        this.hiddenNeuronsCount = save.hiddenNeuronsCount;

        for (int i = 0; i < save.hiddenLayersCount + 1; i++)
        {
            hiddenLayers.Add(Matrix<float>.Build.Dense(1, save.hiddenNeuronsCount));
        }

        weights = WeightArrayToMatrix(save.weights);
        biases = new List<float>(save.biases);
        Debug.Log("Network Loaded.");
    }

    //json save method
    public void Save() 
    {
        //init path and id
        string ID = DateTime.Now.Ticks.ToString();
        path = Application.dataPath + "/NetworkFiles";

        SerializableNetwork save = new SerializableNetwork();

        save.hiddenLayersCount = hiddenLayersCount;
        save.hiddenNeuronsCount = hiddenNeuronsCount;

        save.weights = WeightMatrixToArray(weights);
        save.biases = biases.ToArray();

        string json = JsonConvert.SerializeObject(save);
        File.WriteAllText(path + "/network" + ID + ".txt", json);
        Debug.Log("Network Saved.");
    }


    // converts list of matrix to list of float array to be serialised
    private static float[][,] WeightMatrixToArray(List<Matrix<float>> weights) 
    { 
        List<float[,]> newWeights = new List<float[,]>();
        foreach (Matrix<float> w in weights)
        {
            newWeights.Add(w.ToArray());
        }
        return newWeights.ToArray();
    }

    // converts de serialised float array to list of matrices
    private static List<Matrix<float>> WeightArrayToMatrix(float[][,] weights)
    {
        List<Matrix<float>> newWeights = new List<Matrix<float>>();
        foreach (float[,] w in weights)
        {
            newWeights.Add(Matrix<float>.Build.DenseOfArray(w));
        }
        return newWeights;
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

