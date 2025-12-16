using UnityEngine;

/// <summary>
/// Attach to the clickable prefab (must have Collider2D or Collider). On click it calls VictoryManager.ShowVictory().
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class VictoryTrigger : MonoBehaviour
{
    [Tooltip("If true, this object will only trigger once.")]
    public bool triggerOnce = true;
    [Tooltip("If true, the object will be deactivated after triggering.")]
    public bool deactivateOnTrigger = true;

    bool triggered = false;

    void OnMouseDown()
    {
        if (triggerOnce && triggered) return;
        triggered = true;

        if (VictoryManager.Instance != null)
            VictoryManager.Instance.ShowVictory();
        else
            Debug.LogWarning("VictoryTrigger: No VictoryManager found in the scene!");

        if (deactivateOnTrigger)
            gameObject.SetActive(false);
    }
}