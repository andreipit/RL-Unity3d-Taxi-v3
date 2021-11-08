using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GymLibrary;

public class TestReward : MonoBehaviour
{
    public Action A;
    public Debugger DebugStruct;
    public BoxObservation observation;

    public struct Debugger
    {
        public Envir env;
    }

    private void Start()
    {
        A = GameObject.Find("Taxi").GetComponent<Action>();
        DebugStruct = new Debugger();
        DebugStruct.env = new Envir();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) DebugStruct.env.Reset();

        A.UpdateRays();
        if (A.South && Input.GetKeyDown(KeyCode.DownArrow)) { A.Apply(0); PrintReward(0); }
        if (A.North && Input.GetKeyDown(KeyCode.UpArrow)) { A.Apply(1); PrintReward(1); }
        if (A.East && Input.GetKeyDown(KeyCode.RightArrow)) { A.Apply(2); PrintReward(2); }
        if (A.West && Input.GetKeyDown(KeyCode.LeftArrow)) { A.Apply(3); PrintReward(3); }
        if (Input.GetKeyDown(KeyCode.Space)) { A.Apply(4); PrintReward(4); }
        if (Input.GetKeyDown(KeyCode.Escape)) { A.Apply(5); PrintReward(5); }
        //There are 6 discrete deterministic actions:
        //-0: move south
        //-1: move north
        //-2: move east
        //-3: move west
        //-4: pickup passenger
        //-5: drop off passenger

        void PrintReward(int _ActionIndex)
        {
            observation = DebugStruct.env.GetObserv();
            float reward = DebugStruct.env.GetPerStepReward(observation, _ActionIndex);
            bool is_done = (reward == 20);
            if (is_done) 
                DebugStruct.env.Reset();
            //Debug.Log("reward=" + reward);
        }
    }
}
