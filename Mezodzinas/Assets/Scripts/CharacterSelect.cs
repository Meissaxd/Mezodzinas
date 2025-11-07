using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to each character GameObject (sprite) that has a Collider2D.
/// Set a unique characterIndex for each option (0..3). When the sprite is clicked/tapped,
/// the selection is saved to PlayerPrefs and the MainScene is loaded.
/// </summary>
public class CharacterSelector : MonoBehaviour
{
    [Tooltip("Unique index for this character (e.g. 0..3)")]
    public int characterIndex = 0;

    [Tooltip("Optional name for debugging / display")]
    public string characterName = "";

    // Called when the object with a Collider2D is clicked (or tapped).
    private void OnMouseDown()
    {
        // Save selection so the next scene can read it
        PlayerPrefs.SetInt("SelectedCharacter", characterIndex);
        PlayerPrefs.SetString("SelectedCharacterName", characterName);
        PlayerPrefs.Save();

        // Load the main game scene (make sure "MainScene" is added to Build Settings)
        SceneManager.LoadScene("MainScene");
    }
}