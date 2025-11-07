using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Monitors the five category PlayerPrefs keys written by BasketCollisionDestroyer
/// (keys are "BasketCount_<category>" where <category> is lowercased) and switches
/// scenes to `nextSceneName` when every configured category has at least
/// `requiredPerCategory` points.
///
/// Usage:
/// - Attach this to an active GameObject in your main scene (e.g. an empty "GameManager").
/// - Verify the category names match the ones used in BasketCollisionDestroyer (defaults provided).
/// - Make sure the target scene "Virtuve" is added to Build Settings (or set nextSceneName to a valid scene).
public class CategorySceneManager : MonoBehaviour
{
    [Header("Categories to check (must match BasketCollisionDestroyer.categoryKeys)")]
    [Tooltip("Category names used by BasketCollisionDestroyer (matching is done lowercased).")]
    [SerializeField]
    private string[] categoryKeys = new string[5] { "dzervene", "avene", "zemene", "mellene", "lacene" };

    [Header("Transition settings")]
    [Tooltip("Name of the scene to load when condition is met.")]
    [SerializeField]
    private string nextSceneName = "Virtuve";

    [Tooltip("Minimum count required per category to trigger scene switch.")]
    [SerializeField]
    private int requiredPerCategory = 1;

    [Tooltip("How often (seconds) to poll PlayerPrefs for updated counts.")]
    [SerializeField]
    private float checkInterval = 0.2f;

    [Tooltip("Enable debug logging.")]
    [SerializeField]
    private bool debugLog = true;

    // PlayerPrefs key prefix used by BasketCollisionDestroyer
    private const string PlayerPrefsPrefix = "BasketCount_";

    // Prevents multiple loads
    private bool triggered = false;

    private Coroutine monitorCoroutine;

    private void OnValidate()
    {
        if (categoryKeys == null || categoryKeys.Length != 5)
            categoryKeys = new string[5] { "dzervene", "avene", "zemene", "mellene", "lacene" };

        if (checkInterval <= 0f)
            checkInterval = 0.2f;

        if (requiredPerCategory < 1)
            requiredPerCategory = 1;
    }

    private void OnEnable()
    {
        // Start monitoring counts
        monitorCoroutine = StartCoroutine(MonitorCountsRoutine());
    }

    private void OnDisable()
    {
        if (monitorCoroutine != null)
        {
            StopCoroutine(monitorCoroutine);
            monitorCoroutine = null;
        }
    }

    // Polling routine that checks counts periodically
    private IEnumerator MonitorCountsRoutine()
    {
        while (!triggered)
        {
            if (AllCategoriesMet())
            {
                triggered = true;
                if (debugLog) Debug.Log($"CategorySceneManager: All categories reached >= {requiredPerCategory}. Loading scene '{nextSceneName}'.");
                // Optionally you can use LoadSceneAsync if you want an asynchronous load
                // SceneManager.LoadSceneAsync(nextSceneName);
                SceneManager.LoadScene(nextSceneName);
                yield break;
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    // Checks PlayerPrefs for each category key
    private bool AllCategoriesMet()
    {
        for (int i = 0; i < categoryKeys.Length; i++)
        {
            string cat = (categoryKeys[i] ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(cat))
            {
                // Empty category name — treat as not met (or you could skip)
                if (debugLog) Debug.LogWarning($"CategorySceneManager: categoryKeys[{i}] is empty. Scene will not trigger until category names are set.");
                return false;
            }

            string key = PlayerPrefsPrefix + cat;
            int count = PlayerPrefs.GetInt(key, 0);
            if (debugLog)
            {
                Debug.Log($"CategorySceneManager: read PlayerPrefs '{key}' = {count}");
            }

            if (count < requiredPerCategory)
                return false;
        }

        return true;
    }

    /// Public method to immediately re-check counts (useful for testing)
    public void ForceCheckNow()
    {
        if (!triggered && AllCategoriesMet())
        {
            triggered = true;
            if (debugLog) Debug.Log($"CategorySceneManager: Force check confirmed counts. Loading scene '{nextSceneName}'.");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}