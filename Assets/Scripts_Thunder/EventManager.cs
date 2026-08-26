using System;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public UnityEvent myEvent;

    public void CallEvent()
    {
        myEvent.Invoke();
    }
}