using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Monitors the five category PlayerPrefs keys written by BasketCollisionDestroyer
/// (keys are "BasketCount_<category>" where <category> is lowercased) and switches
/// scenes to `nextSceneName` when every configured category has at least
/// `requiredPerCategory` points.
///
/// This version adds an opt-in reset of those PlayerPrefs on start (useful for testing).
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

    [Header("Testing helpers")]
    [Tooltip("If true, this script will delete the saved BasketCount_<category> keys on start.")]
    [SerializeField]
    private bool resetCountsOnStart = true;

    [Tooltip("If true, the reset will run only when running in the Unity Editor (default). Disable to also reset in builds.")]
    [SerializeField]
    private bool resetOnlyInEditor = true;

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

    private void Awake()
    {
        // Optionally clear saved counts on start (useful for testing)
        if (resetCountsOnStart)
        {
            if (!resetOnlyInEditor || Application.isEditor)
            {
                if (debugLog) Debug.Log("CategorySceneManager: resetCountsOnStart is true; clearing BasketCount_* keys.");
                ResetCountsNow();
            }
            else
            {
                if (debugLog) Debug.Log("CategorySceneManager: resetCountsOnStart is true but resetOnlyInEditor is enabled and this is not the Editor — skipping reset.");
            }
        }
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

    /// Public method to delete the BasketCount_<category> PlayerPrefs keys now.
    /// Can be called from inspector buttons, debug UI, or tests.
    public void ResetCountsNow()
    {
        if (categoryKeys == null) return;

        for (int i = 0; i < categoryKeys.Length; i++)
        {
            string cat = (categoryKeys[i] ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(cat)) continue;
            string key = PlayerPrefsPrefix + cat;
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                if (debugLog) Debug.Log($"CategorySceneManager: Deleted PlayerPrefs key '{key}'.");
            }
        }
        PlayerPrefs.Save();
        // Also ensure we haven't already triggered
        triggered = false;
    }
}