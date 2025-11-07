using UnityEngine;

public class ReadRngInspector : MonoBehaviour
{
    [SerializeField] private RandomNumberGenerator rng; // assign in inspector

    public void LogLast()
    {
        if (rng == null) Debug.Log("ReadRngInspector: rng not assigned.");
        else Debug.Log($"Last value = {rng.LastValue}");
    }
}