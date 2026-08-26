//using UnityEngine;

//public class PortalTrigger : MonoBehaviour
//{
//    [Header("Portal Settings")]
//    public string sceneToLoad;
//    public float disableTime = 1f; // prevent instant re-trigger

//    private bool isTriggered = false;
//    private Collider col;

//    private void Awake()
//    {
//        col = GetComponent<Collider>();
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (isTriggered) return; // prevent multiple triggers
//        if (other.CompareTag("Player"))
//        {
//            isTriggered = true;
//            StartCoroutine(HandlePortal());
//        }
//    }

//    private System.Collections.IEnumerator HandlePortal()
//    {
//        // temporarily disable collider
//        if (col) col.enabled = false;

//        if (SceneTransitionManager.Instance != null)
//            SceneTransitionManager.Instance.LoadSceneSmooth(sceneToLoad);
//        else
//            Debug.LogError("No SceneTransitionManager found!");

//        yield return new WaitForSeconds(disableTime);
//        isTriggered = false;
//        if (col) col.enabled = true;
//    }
//}
