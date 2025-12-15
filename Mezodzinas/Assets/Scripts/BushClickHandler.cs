using UnityEngine;

public class BushClickHandler : MonoBehaviour
{
    private RandomNumberGenerator rng;

    void Awake()
    {
        if (rng == null)
            rng = FindObjectOfType<RandomNumberGenerator>();
    }

    void OnMouseUpAsButton()
    {
        TutorialManager.Instance?.ShowTutorial();

        if (rng != null)
        {
            rng.Generate();
        }
        else
            Debug.LogWarning("No RandomNumberGenerator found in the scene!");
    }
}