using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class RandomnessManager : MonoBehaviour
{
    [SerializeField] private bool useSeed = false;
    [SerializeField] private int currentSeed = 0;

    public void SetSeed()
    {
        if (useSeed == false)
        {
            currentSeed = (int) System.DateTime.Now.Ticks;
        }

        Debug.Log("CurrentSeed: "+ currentSeed);
        Random.InitState(currentSeed);
    }


}
