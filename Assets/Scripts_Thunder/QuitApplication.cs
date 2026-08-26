using UnityEngine;

public class QuitApplication : MonoBehaviour
{
    public void QuitApp()
    {
        Debug.Log("Application Quit");

        Application.Quit();
    }
}