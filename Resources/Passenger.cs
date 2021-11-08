using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GymLibrary
{
    public class Passenger : MonoBehaviour
    {
        public bool InTaxi;

        public Rows Row;
        public Cols Col;
        Transform m_Taxi;


        private void Start()
        {
            m_Taxi = GameObject.Find("Taxi").transform;
        }

        private void Update()
        {
            if (InTaxi) transform.position = m_Taxi.position;
        }

        public void JumpToLocation(PassengerLocations _Loc)
        {
            switch(_Loc)
            {
                case PassengerLocations.R: transform.position = GameObject.Find("R").transform.position; break;
                case PassengerLocations.G: transform.position = GameObject.Find("G").transform.position; break;
                case PassengerLocations.B: transform.position = GameObject.Find("B").transform.position; break;
                case PassengerLocations.Y: transform.position = GameObject.Find("Y").transform.position; break;
            }
        }

    }
}
