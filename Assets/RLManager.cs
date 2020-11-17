using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine.UI;

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

    private List<int> genePool = new List<int>();
    private int naturalSelected;
    private NeuralNet[] population;

    public Text generationText;
    public Text genomeText;
    public Text fitnessText;
    public Text bestText;
    public Text lifetime;
    
    public int currentGeneration;
    public int currentGenome;
    public float bestFitness;

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
        if (currentGenome < population.Length - 1)
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

        //mutate crossovers
        for (int x = 0; x < naturalSelected; x++)
        {
            for (int y = 0; y < newGeneration[x].weights.Count; y++)
            {
                if (Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    newGeneration[x].weights[y] = Mutate(newGeneration[x].weights[y]);
                }
            }
        }

        //fill the rest up with randos
        FillPopulationWithRandomValues(newGeneration, naturalSelected);

        population = newGeneration;
        currentGenome = 0;
        ResetToCurrentGenome();
    }

    //pick the best and worst agents in the population
    private NeuralNet[] NaturalSelection() {
        //copy over
        NeuralNet[] pop = new NeuralNet[initialPop];
            
        //select best agents
        for (int x = 0; x < bestAgentSelection; x++) {
            pop[naturalSelected] = NeuralNet.InitCopy(population[x],car.n_layers, car.n_neurons);
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
        for (int x = 0; x < numberToCrossover; x += 2) {
            int AIndex = worseAgentSelection;
            int BIndex = x + 1;

            if (genePool.Count >= 1) {
                for (int y = 0; y < 100; y++) {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    //make sure they're not the same
                    if(AIndex != BIndex) { break; }
                }
            }

            NeuralNet child1 = new NeuralNet();
            NeuralNet child2 = new NeuralNet();

            child1.Init(car.n_layers, car.n_neurons);
            child2.Init(car.n_layers, car.n_neurons);

            //randomly select weights from the parents
            for (int y = 0; y < child1.weights.Count; y++) {
                //crossover using coinflip
                if (Random.Range(0.0f, 1.0f) > 0.5f) {
                    child1.weights[y] = population[AIndex].weights[y];               //do matrixes instead of individual values to reduce complexity
                    child2.weights[y] = population[BIndex].weights[y];
                }
                else
                {
                    child1.weights[y] = population[BIndex].weights[y];
                    child2.weights[y] = population[AIndex].weights[y];
                }
            }

            //randomly select biases from the parents
            for (int y = 0; y < child1.biases.Count; y++)
            {
                if (Random.Range(0.0f, 1.0f) > 0.5f)
                {
                    child1.biases[y] = population[AIndex].biases[y];
                    child2.biases[y] = population[BIndex].biases[y];
                }
                else
                {
                    child1.biases[y] = population[BIndex].biases[y];
                    child2.biases[y] = population[AIndex].biases[y];
                }
            }
            newGeneration[naturalSelected] = child1;
            naturalSelected++;
            newGeneration[naturalSelected] = child2;
            naturalSelected++;

        }
    }

    //mutate to add diversity beyond innitial population
    private Matrix<float> Mutate(Matrix<float> weights)
    {

        int randomPoints = Random.Range(1, (weights.RowCount * weights.ColumnCount) / 7);           //reduce the amount of mutation done

        Matrix<float> mutatedWeight = Matrix<float>.Build.DenseOfMatrix(weights);                   //dont change the parent

        for (int x = 0; x < randomPoints; x++)
        {
            int randomCol = Random.Range(0, mutatedWeight.ColumnCount);
            int randomRow = Random.Range(0, mutatedWeight.RowCount);

            mutatedWeight[randomRow, randomCol] = Mathf.Clamp(mutatedWeight[randomRow, randomCol] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return mutatedWeight;
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

    //updates text display  
    private void FixedUpdate()
    {
        generationText.text = currentGeneration.ToString();
        genomeText.text = currentGenome.ToString();
        fitnessText.text = car.overallFitness.ToString();
        lifetime.text = car.lifetime.ToString();

        if (car.overallFitness > bestFitness) {
            bestFitness = car.overallFitness;
        }

        bestText.text = bestFitness.ToString();
    }
}
