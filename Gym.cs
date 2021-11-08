using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace GymLibrary
{
    public static class Gym
    {
        public static Envir Make(string _Environment)
        {
            switch(_Environment)
            {
                case "Taxi-v3":
                    return new Envir();
                default:
                    return new Envir();
            }
        }
    }
}

