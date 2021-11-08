using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GymLibrary;

[System.Serializable]
[CreateAssetMenu(fileName = "TrainedPolicy", menuName = "RL/TrainedPolicy", order = 1)]
public class TrainedPolicySO : ScriptableObject
{
    public List<BoxObservation> Observations;
    public List<Probas> Actions;

    [Serializable]
    public class Probas
    {
        public float One;
        public float Two;
        public float Three;
        public float Four;
        public float Five;
        public float Six;
    }
}
