using UnityEngine;

public class DontDestroyOnLoadManager : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsToPersist;

    void Awake()
    {
        // Apply DontDestroyOnLoad to this manager
        DontDestroyOnLoad(gameObject);

        // Apply DontDestroyOnLoad to all specified objects
        foreach (GameObject obj in objectsToPersist)
        {
            if (obj != null)
            {
                if (obj.transform.parent == null)
                {
                    DontDestroyOnLoad(obj);
                }
                else
                {
                    Debug.LogWarning(obj.name + " is not a root GameObject and will not persist across scenes.");
                }
            }
        }
    }
}