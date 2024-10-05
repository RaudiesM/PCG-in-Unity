using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static Action OnGenerate;

    public static Action OnIterate;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            OnGenerate?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            OnIterate?.Invoke();
        }
    }
}
