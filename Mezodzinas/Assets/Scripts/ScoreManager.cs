using UnityEngine;
using UnityEngine.SceneManagement; // fallback if SceneTransitionManager isn't present

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Tooltip("scores[i] is the score for berry type (i+1)")]
    [SerializeField] private int[] scores = new int[5];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddScore(int berryType)
    {
        int idx = berryType - 1;
        if (idx < 0 || idx >= scores.Length)
        {
            Debug.LogWarning($"ScoreManager: invalid berryType {berryType}");
            return;
        }

        scores[idx]++;
        Debug.Log($"ScoreManager: +1 for berry {berryType}. Total now: {scores[idx]}");

        // Immediately proceed to the next scene ("Virtuve") when a berry is collected.
        // Prefer using a SceneTransitionManager if present (allows configurable delays/fades).
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadVirtuve();
        }
        else
        {
            // Fallback: load directly. Make sure "Virtuve" is added to Build Settings.
            SceneManager.LoadScene("Virtuve");
        }
    }

    public int GetScore(int berryType)
    {
        int idx = berryType - 1;
        if (idx < 0 || idx >= scores.Length) return 0;
        return scores[idx];
    }

    // Optional: expose all scores
    public int[] GetAllScores()
    {
        return (int[])scores.Clone();
    }
}