using UnityEngine;

/// <summary>
/// Simple helper to read which character was selected.
/// Put this on a GameObject in MainScene (or call PlayerPrefs directly elsewhere).
/// </summary>
public class CharacterSelectionReceiver : MonoBehaviour
{
    public int SelectedIndex { get; private set; }
    public string SelectedName { get; private set; }

    private void Awake()
    {
        SelectedIndex = PlayerPrefs.GetInt("SelectedCharacter", -1);
        SelectedName = PlayerPrefs.GetString("SelectedCharacterName", "");
        Debug.Log($"CharacterSelectionReceiver: SelectedIndex={SelectedIndex}, SelectedName='{SelectedName}'");
        // Use SelectedIndex / SelectedName to initialize player spawn, visuals, etc.
    }
}