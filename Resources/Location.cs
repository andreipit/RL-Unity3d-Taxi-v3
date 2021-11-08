using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location : MonoBehaviour
{
    public enum States { WaitingPassenger, Empty, Destination }
    public States State;



    public void ChangeColor(Material _Mat)
    {
        GetComponentsInChildren<Renderer>().ToList().ForEach(x => x.sharedMaterial = _Mat);
    }

}
