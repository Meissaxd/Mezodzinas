using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// Attach this to the Basket GameObject. When a GameObject with tag "Projectile"
/// collides with the basket it will:
/// - play an optional sound,
/// - be removed from the scene after a short delay,
/// - and update on-screen counts (TextMeshPro) for five categories based on whether
///   the prefab's name contains one of these words (case-insensitive):
///   dzervene, avene, zemene, mellene, lacene
///
/// Matching is case-insensitive and uses the root object's name (strips "(Clone)").
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

    [Header("Categories (fixed)")]
    [Tooltip("Category names (fixed order). Matching is done by checking whether the prefab name contains one of these words (case-insensitive).")]
    public string[] categoryKeys = new string[5] { "dzervene", "avene", "zemene", "mellene", "lacene" };

    [Header("UI - TextMeshPro fields (one per category in same order as categoryKeys)")]
    [Tooltip("Assign a TMP_Text for each category to display its saved count (same order as categoryKeys).")]
    public TMP_Text[] categoryTexts = new TMP_Text[5];

    // Internal counts per category (index matches categoryKeys)
    private int[] categoryCounts = new int[5];

    // Lowercased category keys for fast case-insensitive matching
    private string[] categoryKeysLower = new string[5];

    // PlayerPrefs key prefix
    private const string PlayerPrefsPrefix = "BasketCount_";

    void OnValidate()
    {
        // Ensure arrays are length 5 to keep inspector consistent
        if (categoryKeys == null || categoryKeys.Length != 5)
            categoryKeys = new string[5] { "dzervene", "avene", "zemene", "mellene", "lacene" };

        if (categoryTexts == null || categoryTexts.Length != 5)
            categoryTexts = new TMP_Text[5];

        if (categoryCounts == null || categoryCounts.Length != 5)
            categoryCounts = new int[5];

        if (categoryKeysLower == null || categoryKeysLower.Length != 5)
            categoryKeysLower = new string[5];

        // normalize lowercase cache for editor feedback
        for (int i = 0; i < 5; i++)
        {
            categoryKeys[i] = categoryKeys[i] ?? "";
            categoryKeysLower[i] = categoryKeys[i].Trim().ToLowerInvariant();
        }
    }

    void Awake()
    {
        // Prepare lowercase keys
        for (int i = 0; i < categoryKeys.Length; i++)
            categoryKeysLower[i] = (categoryKeys[i] ?? "").Trim().ToLowerInvariant();

        // ensure arrays initialized
        if (categoryCounts == null || categoryCounts.Length != 5)
            categoryCounts = new int[5];

        LoadCountsFromPrefs();
        UpdateAllCategoryUI();
    }

    // Load persisted counts for the five categories
    private void LoadCountsFromPrefs()
    {
        for (int i = 0; i < categoryKeys.Length; i++)
        {
            string key = PlayerPrefsPrefix + categoryKeys[i].ToLowerInvariant();
            categoryCounts[i] = PlayerPrefs.GetInt(key, 0);
            if (debugLog) Debug.Log($"BasketCollisionDestroyer: Loaded {categoryKeys[i]} = {categoryCounts[i]} (PlayerPrefs key: {key})");
        }
    }

    // Save one category count to PlayerPrefs
    private void SaveCountToPrefs(int index)
    {
        if (index < 0 || index >= categoryKeys.Length) return;
        string key = PlayerPrefsPrefix + categoryKeys[index].ToLowerInvariant();
        PlayerPrefs.SetInt(key, categoryCounts[index]);
        PlayerPrefs.Save();
        if (debugLog) Debug.Log($"BasketCollisionDestroyer: Saved {categoryKeys[index]} = {categoryCounts[index]} (PlayerPrefs key: {key})");
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

        // Determine a prefab name to match. Strip "(Clone)" and lower-case for matching.
        string rawName = rootObj.name;
        string prefabName = StripCloneSuffix(rawName).Trim();
        string prefabNameLower = prefabName.ToLowerInvariant();

        // Look for any category key that is contained in prefabNameLower (case-insensitive)
        int matchedIndex = -1;
        for (int i = 0; i < categoryKeysLower.Length; i++)
        {
            var key = categoryKeysLower[i];
            if (string.IsNullOrEmpty(key)) continue;
            if (prefabNameLower.Contains(key))
            {
                matchedIndex = i;
                break; // first match in order wins
            }
        }

        if (matchedIndex >= 0)
        {
            categoryCounts[matchedIndex]++;
            SaveCountToPrefs(matchedIndex);
            UpdateCategoryUI(matchedIndex);

            if (debugLog)
                Debug.Log($"BasketCollisionDestroyer: '{prefabName}' matched category '{categoryKeys[matchedIndex]}'. New count = {categoryCounts[matchedIndex]}");
        }
        else
        {
            if (debugLog)
                Debug.Log($"BasketCollisionDestroyer: Hit by untracked prefab '{prefabName}'. No category matched.");
        }

        // Start coroutine to play sound and destroy after delay
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

        // Disable all Collider2D components and SpriteRenderers immediately to avoid repeated collisions and visual overlap
        var cols = target.GetComponentsInChildren<Collider2D>();
        foreach (var c in cols) if (c != null) c.enabled = false;
        var srs = target.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs) if (sr != null) sr.enabled = false;

        if (destroyDelay > 0f)
            yield return new WaitForSeconds(destroyDelay);

        if (target != null)
        {
            Destroy(target);
            if (debugLog) Debug.Log($"BasketCollisionDestroyer: Destroyed '{target.name}'.");
        }
    }

    // Update single category UI text
    private void UpdateCategoryUI(int index)
    {
        if (categoryTexts == null || index < 0 || index >= categoryTexts.Length) return;
        var text = categoryTexts[index];
        if (text != null)
        {
            text.text = $"{categoryKeys[index]}: {categoryCounts[index]}";
        }
    }

    // Update all UI fields
    private void UpdateAllCategoryUI()
    {
        for (int i = 0; i < categoryKeys.Length; i++)
            UpdateCategoryUI(i);
    }

    // Public helper to reset counts both in-memory and in PlayerPrefs and update UI
    public void ResetCounts()
    {
        for (int i = 0; i < categoryCounts.Length; i++)
        {
            categoryCounts[i] = 0;
            string key = PlayerPrefsPrefix + categoryKeys[i].ToLowerInvariant();
            PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save();
        UpdateAllCategoryUI();
        if (debugLog) Debug.Log("BasketCollisionDestroyer: Counts reset and PlayerPrefs cleared for all categories.");
    }

    // Remove "(Clone)" suffix if present to better match prefab asset names
    private string StripCloneSuffix(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        if (name.EndsWith("(Clone)"))
            return name.Substring(0, name.Length - "(Clone)".Length).Trim();
        return name;
    }
}