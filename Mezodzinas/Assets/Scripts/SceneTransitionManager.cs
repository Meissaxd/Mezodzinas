using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple scene transition manager. Keeps itself alive across scenes and provides a safe API
/// to load a scene (optionally with a small delay). Attach to a GameObject in your first scene
/// (for example an empty "GameManagers" object).
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Tooltip("Optional delay (seconds) before actually loading the requested scene.")]
    [SerializeField] private float transitionDelay = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Immediately (or after transitionDelay) loads the given scene name.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    /// <summary>
    /// Convenience helper to load the "Virtuve" scene.
    /// </summary>
    public void LoadVirtuve()
    {
        LoadScene("Virtuve");
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        if (transitionDelay > 0f)
            yield return new WaitForSeconds(transitionDelay);

        // Ensure the scene exists in Build Settings, otherwise Unity will throw an error.
        SceneManager.LoadScene(sceneName);
    }
}