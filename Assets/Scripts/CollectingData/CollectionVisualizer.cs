using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionVisualizer : MonoBehaviour
{
    [SerializeField] private AudioSource pingSound;

    private void Awake()
    {
        DataCollector.OnFinished += playPing;
    }
    private void playPing()
    {
        //play spound to inform that the collecting process is done
        pingSound.Play();
        Debug.Log("<color=red> Finished collecting!</color>");
    }
}
