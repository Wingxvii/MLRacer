using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class RLManager : MonoBehaviour
{
    public CarMovement car;

    //learning parameters
    public int initialPop = 85;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.055f;
    public int bestAgentSelection = 8;
    public int worseAgentSelection = 3;
    public int numberToCrossover;

    public List<int> genePool = new List<int>();
    private int naturalSelected;
    private NeuralNet[] population;

    [Header("Debug View")]
    public int currentGeneration;
    public int currentGenome;

    private void Start()
    {
        CreatePopulation();
    }

    //create the innitial population of agents
    private void CreatePopulation() {
        population = new NeuralNet[initialPop];
        //init by index 0 to spawn fully random population
        FillPopulationWithRandomValues(population, 0);
        ResetToCurrentGenome();
    }

    //reset population
    private void ResetToCurrentGenome() {
        car.ResetWithNetwork(population[currentGenome]);
    }

    //spawn new population
    private void FillPopulationWithRandomValues(NeuralNet[] newPop, int startingIndex) {
        while (startingIndex < initialPop) {
            newPop[startingIndex] = new NeuralNet();
            newPop[startingIndex].Init(car.n_layers, car.n_neurons);
            startingIndex++;
        }
    }

    //increment genome fitness
    public void Death(float fitness, NeuralNet network) {
        if (currentGenome < population.Length - 1)                                                                         //is this genome or generation?
        {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetToCurrentGenome();
        }
        else {
            Repopulate();
        }
    }

    //create new generation of agents
    private void Repopulate() {
        genePool.Clear();
        currentGeneration++;
        naturalSelected = 0;
        SortPopulation();

        //iterate generation
        NeuralNet[] newGeneration = NaturalSelection();
        Crossover(newGeneration);
        Mutate(newGeneration);
    }

    //pick the best and worst agents in the population
    private NeuralNet[] NaturalSelection() {
        //copy over
        NeuralNet[] pop = (NeuralNet[])population.Clone();

        //select best agents
        for (int x = 0; x < bestAgentSelection; x++) {
            pop[naturalSelected].fitness = 0;
            naturalSelected++;

            int selectionBias = Mathf.RoundToInt(population[x].fitness * 10);
            for (int y = 0; y < selectionBias; y++) {
                //add the index of the population
                genePool.Add(x);
            }
        }

        //select worst agents
        for (int x = 0; x < worseAgentSelection; x++)
        {
            int last = population.Length - 1;
            last -= x;

            int selectionBias = Mathf.RoundToInt(population[last].fitness * 10);
            for (int y = 0; y < selectionBias; y++)
            {
                //add the index of the population
                genePool.Add(last);
            }
        }

        return pop;
    }

    //crossover genetics from parent agents
    private void Crossover(NeuralNet[] newGeneration) {
        for (int i = 0; i < numberToCrossover; i += 2) { 
            
        }
    }

    //sort by fitness
    private void SortPopulation()
    {
        //bubble sort
        for (int x = 0; x < population.Length; x++)
        {
            for (int y = 0; y < population.Length; y++)
            {
                if (population[x].fitness < population[y].fitness) {
                    NeuralNet temp = population[x];
                    population[x] = population[y];
                    population[y] = temp;
                }
            }
        }
    } 
}
