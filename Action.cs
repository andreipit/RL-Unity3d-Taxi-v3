using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GymLibrary
{
    public class Action : MonoBehaviour
    {
        [Header("Allowed actions")]
        public bool South;
        public bool North;
        public bool East;
        public bool West;
        public bool PickupPassenger;
        public bool DropOffPassenger;
 

        private void Start()
        {

        }


        private void Update()
        {

        }

        

        public int GetRandomAllowedAction(Dictionary<BoxObservation, float[]> _Policy, BoxObservation _State)
        {
            // refresh allowed actions
            UpdateRays();

            // put them to list
            List<int> AllowedActions = new List<int>();
            if (South) AllowedActions.Add(0);
            if (North) AllowedActions.Add(1);
            if (East) AllowedActions.Add(2);
            if (West) AllowedActions.Add(3);
            AllowedActions.Add(4);
            AllowedActions.Add(5);

            // put their probas to list
            float[] probas = Policy.GetActProbasOfState(_Policy, _State);

            // combine
            var AllowedActionProba = new Dictionary<int, float>(); // {3, 0.166}, {4, 0.12}, // ie 1/6=0.166
            foreach(var a in AllowedActions)
            {
                AllowedActionProba[a] = probas[a];
            }

            // get random value selected using probability
            int action = RandomChoiceByProbas(AllowedActionProba);
            return action;


            //int randomIndex = new System.Random().Next(0, AllowedActions.Count); // (0,3)=> 0 1 2
            //return AllowedActions[randomIndex];
        }

        /// <summary>
        ///  {3, 0.166}, {4, 0.12} -> 3
        /// </summary>
        /// <param name="_ValueWithProba"></param>
        /// <returns></returns>
        public int RandomChoiceByProbas(Dictionary<int, float> _ValueWithProba) // {3, 0.166}, {4, 0.12} -> 4
        {

            // 1) get sum
            float probasSum = 0;
            foreach (var pair in _ValueWithProba)
                probasSum += pair.Value;



            // generate 100 numbers
            List<int> choices = new List<int>();

            // fill by values: sum=10, proba=3, value=15, add three "15" to list
            foreach (var pair in _ValueWithProba)
            {
                //int count = (int)((pair.Value / probasSum) * 100); // round downwards 1.1->1
                int count = (int)System.Math.Ceiling((pair.Value / probasSum) * 100); // round upwards 1.1->2
                for (int i = 0; i < count + 1; i++)
                {
                    choices.Add(pair.Key); // if value = 15, we add 15,15,15,15..15
                }
            }

            // select random from lis {15,15,15,15,15, 10,10,10,10, 4,4,4, 16,16,16,16,16}
            int randomIndex = new System.Random().Next(0, choices.Count); // (0,3)=> 0 1 2
            //Debug.Log(randomIndex + "  cpount=" + choices.Count);
            if (randomIndex < 0 || randomIndex >= choices.Count) return 4; // fixes bug, just call pickup
            return choices[randomIndex];
        }


        public void Apply(int _ActionIndex)
        {
            //There are 6 discrete deterministic actions:
            //-0: move south
            //-1: move north
            //-2: move east
            //-3: move west
            //-4: pickup passenger
            //-5: drop off passenger
            var p = transform.position;
            switch (_ActionIndex)
            {
                case 0:
                    transform.position = new Vector3(p.x, p.y, p.z-10);
                    break;
                case 1:
                    transform.position = new Vector3(p.x, p.y, p.z+10);
                    break;
                case 2:
                    transform.position = new Vector3(p.x+10, p.y, p.z);
                    break;
                case 3: 
                    transform.position = new Vector3(p.x-10, p.y, p.z);
                    break;
                case 4: 
                    break;
                case 5: 
                    break;
            }



        }

        public void UpdateRays()
        {
            North = HasFreeWay(transform.forward);
            South = HasFreeWay(-transform.forward);
            West = HasFreeWay(-transform.right);
            East = HasFreeWay(transform.right);
        }

        bool HasFreeWay(Vector3 _Direction)
        {
            const float RAYLEN = 5f;
            Vector3 start = transform.position;
            Vector3 stop = _Direction * RAYLEN;
            Debug.DrawRay(start, stop, Color.yellow);
            if (Physics.Raycast(start, stop, RAYLEN))
                return false;
            return true;
        }
    }
}
