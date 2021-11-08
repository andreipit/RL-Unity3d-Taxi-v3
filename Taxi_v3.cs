using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GymLibrary;
using Stat;

public class Taxi_v3 : MonoBehaviour
{
    public bool DoTrain = true;
    public bool UseSavedPolicy = false;
    public bool SavedPolicyEachTime = true;
    public TrainedPolicySO SavedPolicySO;

    // EXPLORE_EXPLOIT_COUNT = 5; GAMES_COUNT = 100; ONE_GAME_TIME_LIMIT = 100; == 5 min
    // EXPLORE_EXPLOIT_COUNT = 20; GAMES_COUNT = 50; ONE_GAME_TIME_LIMIT = 2000; == 60 min

    const int EXPLORE_EXPLOIT_COUNT = 100; //20; //UpdatePolicy calls count, 10, webinar = 100
    const int GAMES_COUNT = 250; //50; //10, webinar = 250
    const int ONE_GAME_TIME_LIMIT = 10000; //2000; //10, webinar = 10000, or 10**4 commented
    const int ELITE_PERCENTILE = 50; // use 90% of the best

    //const int EXPLORE_EXPLOIT_COUNT = 2; //20; //UpdatePolicy calls count, 10, webinar = 100
    //const int GAMES_COUNT = 10; //50; //10, webinar = 250
    //const int ONE_GAME_TIME_LIMIT = 100; //2000; //10, webinar = 10000, or 10**4 commented
    //const int ELITE_PERCENTILE = 50; // use 90% of the best


    Matplotlib3d m_Plt;
    Envir env;

    private void Start()
    {
        m_Plt = GameObject.Find("Histogram3d_prefab").GetComponent<Matplotlib3d>();
        StartCoroutine(Train());
    }

    IEnumerator Train()
    {
        env = Gym.Make("Taxi-v3");
        var policy = UseSavedPolicy ? Policy.LoadPolicy(SavedPolicySO) : Policy.CreateStochasticPolicy();

        if (DoTrain)
        {
            for (int i = 0; i < EXPLORE_EXPLOIT_COUNT; i++)
            {
                bool stepDone = false;
                StartCoroutine(SessionsLoop(policy, Callback)); // we use best policy on current moment
                while (stepDone == false) yield return new WaitForEndOfFrame();
                void Callback(List<Tuple<BoxObservation, int>> _Elites) // from several games we receive best actions
                {
                    policy = Policy.UpdatePolicy(policy, _Elites); // we improve our probabilities
                    if (SavedPolicyEachTime) Policy.SavePolicy(policy, SavedPolicySO);
                    stepDone = true;
                }
                Debug.Log("ONE EXPLORATION END");
            }
        }

        // test, ie watch result slowly
        StartCoroutine(GenerateSession(policy, ONE_GAME_TIME_LIMIT, _WatchSlowly:true));
        Debug.Log("WATCHING SLOWLY END");

    }

    IEnumerator SessionsLoop(
        Dictionary<BoxObservation, float[]> _Policy, 
        System.Action<List<Tuple<BoxObservation, int>>> _Callback) // session == one game (ends when time=time_limit, or when passenger is delivered)
    {

        List<List<BoxObservation>> statesBatches = new List<List<BoxObservation>>(); // batch - is one game or session
        List<List<int>> actionsBatches = new List<List<int>>(); // batch - is one game or session
        List<float> rewardsBatches = new List<float>(); // batch - is one game or session

        for (int i = 0; i < GAMES_COUNT; i++)
        {
            bool stepDone = false;
            StartCoroutine(GenerateSession(_Policy, ONE_GAME_TIME_LIMIT, Callback));

            while (stepDone == false)
            {
                yield return new WaitForEndOfFrame();
            }
            void Callback(List<BoxObservation> batchStates, List<int> batchActions, float batchReward)
            {
                statesBatches.Add(batchStates);
                actionsBatches.Add(batchActions);
                rewardsBatches.Add(batchReward);
                stepDone = true;
            }
            Debug.Log("ONE SESSION END");
        }
        m_Plt.Hist(rewardsBatches, 10);
        var elites = Policy.SelectElites(statesBatches, actionsBatches, rewardsBatches, ELITE_PERCENTILE);
        _Callback(elites); //foreach(var el in elites) Debug.Log("in taxiLoc:" + el.Item1.DebugTaxiLoc + " action was:" + el.Item2);
    }


    //    Play game until end or for t_max ticks.
    //    :param policy: an array of shape [n_states, n_actions] with action probabilities
    //    :returns: list of states, list of actions and sum of rewards
    IEnumerator GenerateSession(
        Dictionary<BoxObservation, float[]> _Policy, 
        int t_max= 10*10*10*10, 
        Action<List<BoxObservation>, List<int>, float> _Callback = null,
        bool _WatchSlowly = false)
    {
        var states = new List<BoxObservation>();
        var actions = new List<int>();
        float total_reward = 0f;

        var s = env.Reset(); // observation 0

        for (int t = 0; t < t_max; t++)
        {
            //var a = new System.Random().Next(0, Envir.action_space + 1); // 0 1 .. 5
            var a = env.m_AgentAction.GetRandomAllowedAction(_Policy, env.GetObserv());

            //env.stepStart(a);
            //yield return new WaitForFixedUpdate();
            //if (_WatchSlowly) yield return new WaitForSeconds(0.1f);
            if (_WatchSlowly) yield return new WaitForFixedUpdate();
            else yield return new WaitForEndOfFrame();
            var newobs_Rew_Isdone_Info = env.stepEnd(a);

            // parse tuple
            var new_s = newobs_Rew_Isdone_Info.Item1;
            var r = newobs_Rew_Isdone_Info.Item2;
            var done = newobs_Rew_Isdone_Info.Item3;
            var info = newobs_Rew_Isdone_Info.Item4;

            states.Add(s);
            actions.Add(a);
            total_reward += r;

            s = new_s;
            if (done)
            {
                break;
            }

        }
        _Callback?.Invoke(states, actions, total_reward);
    }

    

}


//// Policy example with 2 states and 6 actions
////[1.6, 1.6, 1.6, 1.6, 1.6, 1.6] => in 1st state all 6 actions have same proba, ie 1.6*6 = 1
////[1.6, 1.6, 1.6, 1.6, 1.6, 1.6] => in 2nd state all 6 actions have same proba, ie 1.6*6 = 1
//float[][] GenerateUniformPolicy(int _StatesCount, int _ActionsCount)
//{
//    var result = new float[_StatesCount][];
//    for (int i = 0; i < _StatesCount; i++) // states count
//    {
//        float[] probas = new float[_ActionsCount];
//        for (int j = 0; j < _ActionsCount; j++)
//        {
//            probas[j] = 1 / _ActionsCount; // uniform distribution
//        }
//        result[i] = probas;
//    }
//    return result;
//}