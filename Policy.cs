using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GymLibrary
{
    public static class Policy
    {

        //     Observations:
        // There are 500 discrete states since there are 25 taxi positions, 5 possible locations of the passenger(including the case when the passenger is in the taxi), and 4 destination locations.
        //Note that there are 400 states that can actually be reached during an episode.The missing states correspond to situations in which the passenger is at the same location as their destination, as this typically signals the end of an episode.
        //Four additional states can be observed right after a successful episodes, when both the passenger and the taxi are at the destination.
        //This gives a total of 404 reachable discrete states.
        public static Dictionary<BoxObservation, float[]> CreateStochasticPolicy()
        {
            var result = new Dictionary<BoxObservation, float[]>();
            foreach (var row in Enum.GetValues(typeof(Rows)))
                foreach (var col in Enum.GetValues(typeof(Cols)))
                    foreach (var passLoc in Enum.GetValues(typeof(PassengerLocations)))
                        foreach (var destLoc in Enum.GetValues(typeof(DestinationLocations)))
                        {
                            BoxObservation state = new BoxObservation((Rows)row, (Cols)col, (PassengerLocations)passLoc, (DestinationLocations)destLoc);
                            float[] probas = new float[6] {1f/6f, 1f/6f, 1f/6f, 1f/6f, 1f/6f, 1f/6f };
                            result[state] = probas;
                        }
            //foreach (var pair in result)
            //{
            //    Debug.Log(pair.Value[0]);
            //    Debug.Log(pair.Value[1]);
            //    Debug.Log(pair.Value[2]);
            //    Debug.Log(pair.Value[3]);
            //    Debug.Log(pair.Value[4]);
            //    Debug.Log(pair.Value[5]);
            //    break;
            //}
            return result;
        }


        //public List<Dictionary<BoxObservation, float[]>> select_elites(states_batch, actions_batch, rewards_batch, percentile= 50):

        /// <summary>
        /// Get actions of state from policy
        /// </summary>
        /// <param name="_Policy"></param>
        /// <param name="_State"></param>
        /// <returns></returns>
        public static float[] GetActProbasOfState(Dictionary<BoxObservation, float[]> _Policy, BoxObservation _State)
        {
            foreach(var pair in _Policy)
            {
                if (StatesAreEqual(_State, pair.Key)) 
                    return pair.Value;
            }
            Debug.Log("Error: state is not found");
            return null;
        }

        static BoxObservation GetSameStateFromPolicy(Dictionary<BoxObservation, float[]> _Policy, BoxObservation _State)
        {
            foreach (var pair in _Policy)
            {
                if (StatesAreEqual(_State, pair.Key))
                    return pair.Key;
            }
            Debug.Log("Error: state is not found");
            return null;
        }

        static bool StatesAreEqual(BoxObservation _Obs1, BoxObservation _Obs2)
        {
            return (_Obs1.TaxiRow == _Obs2.TaxiRow
                && _Obs1.TaxiCol == _Obs2.TaxiCol
                && _Obs1.PassLoc == _Obs2.PassLoc
                && _Obs1.DestLoc == _Obs2.DestLoc);
        }

        //def select_elites(states_batch, actions_batch, rewards_batch, percentile= 50):
        /// <summary>
        /// Select states and actions from games that have rewards >= reward_threshold
        /// Batch means one "game" (or session).
        /// </summary>
        /// <param name="_StatesBatches"> _StatesBatch[0] = all states reached in game1. Index==Index in _ActionsBatch </param>
        /// <param name="_ActionsBatches"> _ActionsBatch[0] = all actions made in game1. Index==Index in _StatesBatch </param>
        /// <param name="_RewardsBatches"> _RewardsBatch[0] = total reward of game1. Index==Index in _StatesBatch </param>
        /// <param name="percentile"> We select only batches with reward >= percentile </param>
        public static List<Tuple<BoxObservation, int>> SelectElites(
            List<List<BoxObservation>> _StatesBatches, 
            List<List<int>> _ActionsBatches, 
            List<float> _RewardsBatches, 
            float _Percentile = 50)
        {
            //    Select states and actions from games that have rewards >= percentile
            //    :param states_batch: list of lists of states, states_batch[session_i][t]
            //    :param actions_batch: list of lists of actions, actions_batch[session_i][t]
            //    :param rewards_batch: list of rewards, rewards_batch[session_i]
            //    :returns: elite_states,elite_actions, both 1D lists of states and respective actions from elite sessions

            var result = new List<Tuple<BoxObservation, int>>();

            // find threshold
            float reward_threshold = _RewardsBatches.Min() + Mathf.Abs(_RewardsBatches.Max() - _RewardsBatches.Min()) * _Percentile/100;
            //Debug.Log("reward_threshold=" + reward_threshold + " min=" + _RewardsBatches.Min() + " max=" + _RewardsBatches.Max() + " diff=" + Mathf.Abs(_RewardsBatches.Max() - _RewardsBatches.Min()));
            for (int i = 0; i < _RewardsBatches.Count; i++) // for each game
            {
                if (_RewardsBatches[i] >= reward_threshold)
                {
                    for (int j = 0; j < _StatesBatches[i].Count; j++) // for each state in game
                    {
                        BoxObservation state = _StatesBatches[i][j];
                        int madeAction = _ActionsBatches[i][j];
                        result.Add(Tuple.Create(state, madeAction));
                    }
                }
            }
            return result;
        }

        //    Given old policy and a list of elite states/actions from select_elites,
        //    return new updated policy where each action probability is proportional to
        //    policy[s_i, a_i] ~ #[occurences of si and ai in elite states/actions]
        //    Don't forget to normalize policy to get valid probabilities and handle 0/0 case.
        //    In case you never visited a state, set probabilities for all actions to 1./n_actions
        //    :param elite_states: 1D list of states from elite sessions
        //    :param elite_actions: 1D list of actions from elite sessions
        public static Dictionary<BoxObservation, float[]> UpdatePolicy(
            Dictionary<BoxObservation, float[]> _Policy,
            List<Tuple<BoxObservation, int>> _EliteStateAction)
        {
            // 1) make all probas == 0
            var new_policy = new Dictionary<BoxObservation, float[]>();
            foreach (var pair in _Policy) new_policy[pair.Key] = new float[6] { 0, 0, 0, 0, 0, 0 };

            // 2) add 1 to proba, each time it's action appears among elites
            foreach (var StateAction in _EliteStateAction)
            {
                BoxObservation state = GetSameStateFromPolicy(new_policy, StateAction.Item1);
                int action = StateAction.Item2;
                new_policy[state][action] += 1;
            }

            // 3) convert absolute value to fraction, also replace zero by 1f/6f
            foreach (var pair in _Policy)
            {
                BoxObservation state = GetSameStateFromPolicy(new_policy, pair.Key);
                float total_occ = new_policy[state].Sum(); // [5,0,1,4,6,0] => 16
                if (total_occ != 0)
                {
                    for (int i = 0; i < 6; i++) new_policy[state][i] /= total_occ;
                }
                else
                {
                    new_policy[state] = new float[6] { 1f / 6f, 1f / 6f, 1f / 6f, 1f / 6f, 1f / 6f, 1f / 6f };
                }
            }

            return new_policy;
        }

        public static void SavePolicy(Dictionary<BoxObservation, float[]> _Policy, TrainedPolicySO _SO)
        {
            _SO.Observations = new List<BoxObservation>();
            _SO.Actions = new List<TrainedPolicySO.Probas>();
            foreach (var pair in _Policy)
            {
                var s = pair.Key;
                float[] a = pair.Value;
                TrainedPolicySO.Probas probas = new TrainedPolicySO.Probas();
                probas.One = a[0];
                probas.Two = a[1];
                probas.Three = a[2];
                probas.Four = a[3];
                probas.Five = a[4];
                probas.Six = a[5];

                _SO.Observations.Add(new BoxObservation(s.TaxiRow, s.TaxiCol, s.PassLoc, s.DestLoc));
                _SO.Actions.Add(probas);
            }
        }

        public static Dictionary<BoxObservation, float[]> LoadPolicy(TrainedPolicySO _SO)
        {

            var result = new Dictionary<BoxObservation, float[]>();
            for (int i = 0; i < _SO.Observations.Count; i++)
            {
                var a = _SO.Actions[i];
                result[_SO.Observations[i]] = new float[6]{ a.One, a.Two, a.Three, a.Four, a.Five, a.Six };
            }
            return result;
        }

    }
}






//def update_policy(elite_states, elite_actions):
//    """
//    Given old policy and a list of elite states/actions from select_elites,
//    return new updated policy where each action probability is proportional to

//    policy[s_i, a_i] ~ #[occurences of si and ai in elite states/actions]

//    Don't forget to normalize policy to get valid probabilities and handle 0/0 case.
//    In case you never visited a state, set probabilities for all actions to 1./n_actions

//    :param elite_states: 1D list of states from elite sessions
//    :param elite_actions: 1D list of actions from elite sessions

//    """

//    new_policy = np.zeros([n_states, n_actions])

//#     <Your code here: update probabilities for actions given elite states & actions >
//    # Don't forget to set 1/n_actions for all actions in unvisited states.

//    for s, a in zip(elite_states, elite_actions):
//      new_policy[s][a] += 1

//    # Counter
//    for s in range(n_states):
//      total_occ = sum(new_policy[s])
//      if total_occ != 0:
//        new_policy[s] = new_policy[s] / total_occ
//      else:
//        new_policy[s] = np.ones(n_actions) * (1. / n_actions)

//    # Probabilities


//return new_policy




//def select_elites(states_batch, actions_batch, rewards_batch, percentile= 50):
//    """
//    Select states and actions from games that have rewards >= percentile
//    :param states_batch: list of lists of states, states_batch[session_i][t]
//    :param actions_batch: list of lists of actions, actions_batch[session_i][t]
//    :param rewards_batch: list of rewards, rewards_batch[session_i]

//    :returns: elite_states,elite_actions, both 1D lists of states and respective actions from elite sessions

//    Please return elite states and actions in their original order
//    [i.e.sorted by session number and timestep within session]

//If you are confused, see examples below.Please don't assume that states are integers
//    (they will become different later).
//    """

//    reward_threshold = np.percentile(rewards_batch, percentile)# <Compute minimum reward for elite sessions. Hint: use np.percentile >

//    elite_mask = np.where(rewards_batch >= reward_threshold)# <your code here >

//    elite_states = np.array(states_batch)[elite_mask]# <your code here >
//    elite_actions = np.array(actions_batch)[elite_mask]# <your code here >

//    return np.concatenate(elite_states), np.concatenate(elite_actions)


