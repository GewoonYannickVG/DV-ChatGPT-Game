using UnityEngine;

public class DontDestroyOnLoadManager : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsToPersist;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        foreach (GameObject obj in objectsToPersist)
        {
            if (obj != null)
            {
                DontDestroyOnLoad(obj);
            }
        }
    }
}