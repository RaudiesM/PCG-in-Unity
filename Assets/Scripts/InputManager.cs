using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static Action OnSetup;
    public static Action OnIterate;
    public static Action OnSpawnDungeon;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            OnSetup?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            OnIterate?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            OnSpawnDungeon?.Invoke();
        }
    }
}
