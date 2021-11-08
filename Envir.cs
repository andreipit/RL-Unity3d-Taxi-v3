using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GymLibrary
{

    public class Envir
    {
       
        public const int action_space = 6;
        public const int observation_space = 500;

        Agent m_Agent; // taxi
        public Action m_AgentAction; // taxi
        Passenger m_Passenger;
        Location m_R; // house or location
        Location m_G; // house or location
        Location m_B; // house or location
        Location m_Y; // house or location
        DestinationLocations m_DestLoc; // location where passenger wants to go

        public int Timer;
        public int CurrentReward;


        #region Public methods

        public Envir()
        {
            m_Agent = GameObject.Find("Taxi").GetComponent<Agent>();
            m_AgentAction = m_Agent.GetComponent<Action>();
            m_Passenger = GameObject.Find("Passenger").GetComponent<Passenger>();
            m_R = GameObject.Find("R").GetComponent<Location>();
            m_G = GameObject.Find("G").GetComponent<Location>();
            m_B = GameObject.Find("B").GetComponent<Location>();
            m_Y = GameObject.Find("Y").GetComponent<Location>();
        }

        /// <summary> reset environment to initial state, return first observation </summary>
        public BoxObservation Reset()
        {
            var ran = new System.Random(); // otherwise all will be equal!
            int taxiRow = ran.Next(1, 6); // 1 or 2 or 3 or 4 or 5 (the range includes minValue, maxValue-1, and all numbers in between)
            int taxiCol = ran.Next(1, 6); // 1 or 2 or 3 or 4 or 5
            int passLoc = ran.Next(1, 5); // R or G or B or Y 

            // get random destination, that is not the same as pass place
            int destLocRan = ran.Next(1, 4); //1 or 2 or 3
            List<int> locations = new List<int>() { 1, 2, 3, 4 };
            locations.Remove(passLoc); // removes 1 entry, ie result = 1,3,4
            int destLoc = locations[destLocRan - 1];

            float posX = -1;
            float posZ = -1;
            switch(taxiCol)
            {
                case 1: posX = -20; break;
                case 2: posX = -10; break;
                case 3: posX = 0; break;
                case 4: posX = 10; break;
                case 5: posX = 20; break;
            }
            switch (taxiRow)
            {
                case 1: posZ = -20; break;
                case 2: posZ = -10; break;
                case 3: posZ = 0; break;
                case 4: posZ = 10; break;
                case 5: posZ = 20; break;
            }
            
            Vector3 PassPos = Vector3.zero;
            switch (passLoc)
            {
                case 1: PassPos = GameObject.Find("Locations/R").transform.localPosition; break;
                case 2: PassPos = GameObject.Find("Locations/G").transform.localPosition; break;
                case 3: PassPos = GameObject.Find("Locations/B").transform.localPosition; break;
                case 4: PassPos = GameObject.Find("Locations/Y").transform.localPosition; break;
            }

            switch (destLoc)
            {
                case 1: m_DestLoc = DestinationLocations.R;  break;
                case 2: m_DestLoc = DestinationLocations.G; break;
                case 3: m_DestLoc = DestinationLocations.B; break;
                case 4: m_DestLoc = DestinationLocations.Y; break;
            }

            m_Agent.transform.localPosition = new Vector3(posX, m_Agent.transform.position.y, posZ);
            m_Agent.transform.localEulerAngles = Vector3.zero;
            m_Passenger.transform.localPosition = PassPos;
            m_Passenger.InTaxi = false;
            GameObject.Find("Destination").transform.position = GameObject.Find("Locations/"+ m_DestLoc.ToString()).transform.position;
            return GetObserv();
        }

        /// <summary> show current environment state(a more colorful version :) ) </summary>
        public void render()
        {

        }

        /// <summary> 
        /// commit action a and return (new observation, reward, is done, info) 
        /// new observation - an observation right after commiting the action a
        /// reward - a number representing your reward for commiting action a
        /// is_done - True if the MDP has just finished, False if still in progress
        /// info - some auxilary stuff about what just happened.Ignore it for now.
        /// </summary>
        /// <param name="a"> Action index</param>
        /// <returns></returns>
        //public void stepStart(int a)
        //{
        //    //m_AgentAction.Apply(a);
        //}

        public Tuple<BoxObservation, float, bool, float> stepEnd(int _Action)
        {
            BoxObservation observation = GetObserv();
            float reward = GetPerStepReward(observation, _Action);

            bool is_done = (reward == 20);
            float info = 0;
            //if (reward == 20)

            m_AgentAction.Apply(_Action);


            return Tuple.Create(observation, reward, is_done, info);
        }

        #endregion


        #region Private methods

        public BoxObservation GetObserv()
        {
            Rows Row = Rows.one;
            switch (m_Agent.transform.position.z)
            {
                case 10: Row = Rows.two; break;
                case 0: Row = Rows.three; break;
                case -10: Row = Rows.four; break;
                case -20: Row = Rows.five; break;
            }

            Cols Col = Cols.one;
            switch (m_Agent.transform.position.x)
            {
                case -10: Col = Cols.two; break;
                case 0: Col = Cols.three; break;
                case 10: Col = Cols.four; break;
                case 20: Col = Cols.five; break;
            }

            PassengerLocations PassLoc = PassengerLocations.Taxi;
            if (!m_Passenger.InTaxi)
            {
                if (m_Passenger.transform.position == m_R.transform.position) PassLoc = PassengerLocations.R;
                else if (m_Passenger.transform.position == m_G.transform.position) PassLoc = PassengerLocations.G;
                else if (m_Passenger.transform.position == m_B.transform.position) PassLoc = PassengerLocations.B;
                else if (m_Passenger.transform.position == m_Y.transform.position) PassLoc = PassengerLocations.Y;
            }
            return new BoxObservation(Row, Col, PassLoc, m_DestLoc);
        }

        //Rewards:
        //There is a default per - step reward of -1,
        //except for delivering the passenger, which is +20,
        //or executing "pickup" and "drop-off" actions illegally, which is -10.
        public float GetPerStepReward(BoxObservation _Observ, int _LastAction)
        {
            if (_LastAction == 4)
            {
                if (!_Observ.IsPickupLegal()) // executing "pickup" illegally
                {
                    return -10;
                }
                else
                {
                    m_Passenger.InTaxi = true;
                    return 0;
                }
            }

            if (_LastAction == 5)
            {
                if (_Observ.PassLoc != PassengerLocations.Taxi) return -10;
                //if (m_Agent.transform.position != GameObject.Find("Destination").transform.position) // executing "drop-off" illegally
                if (_Observ.GetTaxiPos().ToString() != _Observ.DestLoc.ToString()) // executing "drop-off" illegally
                {
                    return -10;
                }
                else // delivering the passenger
                {
                    m_Passenger.InTaxi = false;
                    return 20;
                }
            }
            return -1; // (0,3)=> 0 1 2
        }

        #endregion
    }




}
