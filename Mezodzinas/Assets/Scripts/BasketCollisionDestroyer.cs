using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Attach this to the Basket GameObject. When a GameObject with tag "Projectile"
/// collides with the basket it will play an optional sound and then be removed
/// from the scene after a short delay. Additionally, this script tracks which
/// prefab (by name) collided and keeps counts per prefab. You can assign up to
/// five prefabs in the inspector (trackedPrefabs array) and the script will
/// track how many of each of those prefabs have hit the basket. All counts are
/// written to the debug log on each hit.
public class BasketCollisionDestroyer : MonoBehaviour
{
    [Tooltip("Tag to check for and destroy on contact.")]
    public string projectileTag = "Projectile";

    [Tooltip("Delay in seconds between collision and actual Destroy() (gives time for sound/animation).")]
    public float destroyDelay = 0.3f;

    [Header("Sound")]
    [Tooltip("Optional audio clip to play when projectile hits the basket.")]
    public AudioClip destroyClip;
    [Tooltip("If provided, this AudioSource will be used to play the clip via PlayOneShot. If null, PlayClipAtPoint is used.")]
    public AudioSource audioSource;
    [Tooltip("Playback volume (0..1).")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("If true, logs each destroy for debugging.")]
    public bool debugLog = true;

    [Header("Tracked Prefabs (assign exactly 5 if you like)")]
    [Tooltip("Assign up to 5 prefab assets here to track how many of each collided with the basket. Leave unused slots null.")]
    public GameObject[] trackedPrefabs = new GameObject[5];

    // Internal counts keyed by prefab-name (stripped of '(Clone)' if present) or other runtime names.
    private readonly Dictionary<string, int> collisionCounts = new Dictionary<string, int>();

    // Map from tracked prefab stripped name -> original index in trackedPrefabs (for quick lookup)
    private readonly Dictionary<string, int> trackedNameToIndex = new Dictionary<string, int>();

    void OnValidate()
    {
        // Ensure the array has exactly length 5 so inspector shows 5 slots consistently.
        if (trackedPrefabs == null || trackedPrefabs.Length != 5)
        {
            var newArr = new GameObject[5];
            if (trackedPrefabs != null)
            {
                for (int i = 0; i < Mathf.Min(5, trackedPrefabs.Length); i++)
                    newArr[i] = trackedPrefabs[i];
            }
            trackedPrefabs = newArr;
        }
    }

    void Awake()
    {
        BuildTrackedNameMap();
        InitializeCounts();
    }

    // Build lookup map from assigned prefab names to their index
    private void BuildTrackedNameMap()
    {
        trackedNameToIndex.Clear();
        for (int i = 0; i < trackedPrefabs.Length; i++)
        {
            var prefab = trackedPrefabs[i];
            if (prefab == null) continue;
            string nameKey = StripCloneSuffix(prefab.name);
            if (!trackedNameToIndex.ContainsKey(nameKey))
                trackedNameToIndex.Add(nameKey, i);
        }
    }

    // Initialize collision counts for tracked prefabs (and clear any previous)
    private void InitializeCounts()
    {
        collisionCounts.Clear();
        // seed counts for tracked prefabs with 0
        foreach (var kv in trackedNameToIndex)
        {
            if (!collisionCounts.ContainsKey(kv.Key))
                collisionCounts.Add(kv.Key, 0);
        }
    }

    // Called when using trigger colliders (Collider2D.isTrigger = true)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        TryHandleProjectile(other.gameObject);
    }

    // Called when using non-trigger physics collisions
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null || collision.collider == null) return;
        TryHandleProjectile(collision.collider.gameObject);
    }

    // Common check/entry point
    private void TryHandleProjectile(GameObject other)
    {
        if (other == null) return;
        if (!other.CompareTag(projectileTag)) return;

        // Determine the root object in case collider is on a child
        GameObject rootObj = other.transform.root.gameObject;

        // Determine a prefab name to track. Try to match against assigned trackedPrefabs by name.
        string rawName = rootObj.name;
        string prefabName = StripCloneSuffix(rawName);

        // If tracked, increment its counter. If not, add a generic entry for this name.
        if (trackedNameToIndex.TryGetValue(prefabName, out int index))
        {
            // Ensure key exists in dictionary
            if (!collisionCounts.ContainsKey(prefabName))
                collisionCounts[prefabName] = 0;

            collisionCounts[prefabName] += 1;
        }
        else
        {
            // Not in the assigned trackedPrefabs: still track it under its stripped name
            if (!collisionCounts.ContainsKey(prefabName))
                collisionCounts[prefabName] = 0;

            collisionCounts[prefabName] += 1;
        }

        // Log current counts
        if (debugLog)
            Debug.Log($"BasketCollisionDestroyer: Hit by '{prefabName}'. Counts: {GetCountsLogString()}");

        // Start coroutine to play sound and destroy after delay (preserves previous behavior)
        StartCoroutine(PlaySoundAndDestroy(rootObj));
    }

    private IEnumerator PlaySoundAndDestroy(GameObject target)
    {
        if (debugLog) Debug.Log($"BasketCollisionDestroyer: Scheduling destroy for '{target.name}' in {destroyDelay:F2}s.");

        // Play sound immediately
        if (destroyClip != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(destroyClip, volume);
            }
            else
            {
                AudioSource.PlayClipAtPoint(destroyClip, target.transform.position, volume);
            }
        }

        // Optionally disable collider and visuals to prevent repeated collisions while waiting to be destroyed
        var col = target.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        var sr = target.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        if (destroyDelay > 0f)
            yield return new WaitForSeconds(destroyDelay);

        if (target != null)
        {
            Destroy(target);
            if (debugLog) Debug.Log($"BasketCollisionDestroyer: Destroyed '{target.name}'. Current counts: {GetCountsLogString()}");
        }
    }

    // Helper: make a single-line string listing counts for all known keys
    private string GetCountsLogString()
    {
        // Build "name:count, name2:count, ..." sorted by tracked prefabs first (if present)
        var parts = new List<string>();

        // Add tracked prefab entries in inspector order if present
        for (int i = 0; i < trackedPrefabs.Length; i++)
        {
            var prefab = trackedPrefabs[i];
            if (prefab == null) continue;
            string key = StripCloneSuffix(prefab.name);
            int count = collisionCounts.ContainsKey(key) ? collisionCounts[key] : 0;
            parts.Add($"{key}:{count}");
        }

        // Add any other keys that are not in trackedPrefabs
        foreach (var kv in collisionCounts)
        {
            if (trackedNameToIndex.ContainsKey(kv.Key)) continue;
            parts.Add($"{kv.Key}:{kv.Value}");
        }

        return string.Join(", ", parts);
    }

    // Remove "(Clone)" suffix if present to better match prefab asset names
    private string StripCloneSuffix(string name)
    {
        if (name == null) return "";
        if (name.EndsWith("(Clone)"))
            return name.Substring(0, name.Length - "(Clone)".Length).Trim();
        return name;
    }

    /// Public helper to reset counts (callable from other scripts or via UnityEvents)
    public void ResetCounts()
    {
        InitializeCounts();
        if (debugLog) Debug.Log("BasketCollisionDestroyer: Counts reset.");
    }
}