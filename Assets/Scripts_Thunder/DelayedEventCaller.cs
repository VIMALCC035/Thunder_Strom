using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class DelayedEventCaller : MonoBehaviour
{
    public float delay = 3f;
    public UnityEvent onDelayComplete;

    public void StartDelay()
    {
        StartCoroutine(CallEventAfterDelay());
    }

    IEnumerator CallEventAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        onDelayComplete?.Invoke();
    }
}