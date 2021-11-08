using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zipper : MonoBehaviour
{
    void Start()
    {
        // way1
        Debug.Log("Old way (dict):");
        var dict1 = new Dictionary<int, string>() { { 1, "one" }, { 2, "two" }, { 3, "three" } };
        foreach (var item in dict1) Debug.Log(item.Key + " " + item.Value);

        // way2
        Debug.Log("New way (zip):");
        int[] numbers = { 1, 2, 3 };
        string[] words = { "one", "two", "three" };
        var dict2 = numbers.Zip(words, (x, y) =>
            {
                Debug.Log("key=" + x + "value=" + y);
                return x + " " + y;
            });
        foreach (var item in dict2) Debug.Log(item);

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
