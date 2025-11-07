using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to a UI Button (or any GameObject) and call LoadCharacterSelect from the Button OnClick.
/// Make sure the "character select" scene is added to Build Settings (File > Build Settings > Scenes In Build).
/// </summary>
public class ButtonSceneManager : MonoBehaviour
{
    [Tooltip("Name of the scene to load. Must match the scene name in your Project and be included in Build Settings.")]
    public string targetSceneName = "character select";

    /// <summary>
    /// Call this method from the Button OnClick() in the Inspector.
    /// </summary>
    public void LoadCharacterSelect()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("ButtonSceneManager: targetSceneName is empty. Set it in the inspector or code.");
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }
}