using System;
using System.Collections.Generic;

namespace Homework2
{
	public class NeuralNet
	{
		private const double response = 1.0d;
		private	int numInputs;
		private int numOutputs;
		private int numHiddenLayers;
		private int neuronsPerHiddenLayer;
		private List<NeuronLayer> Layers;

		public int NumOutputs { get { return numOutputs; } }

		struct Neuron{
			private int numInputs;
			private List<double> weights;

			public int NumInputs { get { return numInputs; } set { numInputs = value; } }
			public List<double> Weights { get { return weights; } set { weights = value; } }

			public Neuron(int numInputs){
				this.numInputs = numInputs + 1;
				Random r = new Random();
				weights = new List<double>();
				for(int i = 0; i < this.numInputs; i++)
				{
					weights.Add(r.NextDouble() * 2 - 1);
				}
			}
		}
		struct NeuronLayer{
			private List<Neuron> neurons;

			public List<Neuron> Neurons { get { return neurons; } }
			public NeuronLayer(int numNeurons, int numInputsPerNeuron){
				neurons = new List<Neuron>();
				for(int i = 0; i < numNeurons; i++){
					neurons.Add(new Neuron(numInputsPerNeuron));
				}
			}
		}
		public NeuralNet (int inputs, int outputs)
		{
			numInputs = inputs;
			numOutputs = outputs;
			numHiddenLayers = 1;
			neuronsPerHiddenLayer = 8;
			Layers = new List<NeuronLayer> ();
			CreateNet ();
		}

		void CreateNet()
		{
			if (numHiddenLayers > 0) {
				Layers.Add (new NeuronLayer (neuronsPerHiddenLayer, numInputs));

				for (int i = 0; i < numHiddenLayers - 1; i++) {
					Layers.Add (new NeuronLayer (neuronsPerHiddenLayer, neuronsPerHiddenLayer));
				}
				Layers.Add (new NeuronLayer (numOutputs, neuronsPerHiddenLayer));
			} else {
				Layers.Add (new NeuronLayer (numOutputs, numInputs));
			}
		}

		List<double> GetAllWeights(){
			List<double> allWeights = new List<double> ();
			foreach (NeuronLayer l in Layers) {
				foreach (Neuron n in l.Neurons) {
					foreach (double d in n.Weights) {
						allWeights.Add (d);
					}
				}
			}
			return allWeights;
		}

		public int GetNumWeights(){
			int total = 0;
			foreach (NeuronLayer l in Layers) {
				foreach (Neuron n in l.Neurons) {
					total += n.Weights.Count;
				}
			}
			return total;
		}

		public void SetWeights(List<double> weights){
			int i = 0;
			foreach (NeuronLayer l in Layers) {
				foreach (Neuron n in l.Neurons) {
					for (int j = 0; j < n.Weights.Count; j++) {
						n.Weights [j] = weights [i++];
					}
				}
			}
		}

		double Sigmoid(double activation){
			return 1 / (1 + Math.Exp (-activation / response));
		}

		public List<double> Update(List<double> inputs){
			List<double> outputs = new List<double>();
			int cWeight = 0;

			if (inputs.Count != numInputs) {
				return outputs;
			}

			for (int i = 0; i < numHiddenLayers + 1; i++) {
				if (i > 0) {
					inputs = new List<double> (outputs);
				}
				outputs.Clear ();

				cWeight = 0;

				for (int j = 0; j < Layers [i].Neurons.Count; j++) {
					double netInput = 0;
					int numInputs = Layers [i].Neurons [j].NumInputs;
					for (int k = 0; k < numInputs - 1; k++) {
						netInput += Layers [i].Neurons [j].Weights [k] * inputs [cWeight++];
					}

					netInput += Layers [i].Neurons [j].Weights [numInputs - 1] * -1;

					outputs.Add (Sigmoid (netInput));
					cWeight = 0;
				}
			}
			return outputs;
		}
	}
}

