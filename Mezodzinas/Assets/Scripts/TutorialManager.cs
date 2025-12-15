using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI")]
    public Image tutorialImage;      // assign the UI Image (disabled by default)
    public Sprite frameA;           // first PNG
    public Sprite frameB;           // second PNG

    [Header("Animation")]
    public float frameDuration = 0.25f; // time each frame is shown
    public int cycles = 3;              // how many times to alternate (1 = A->B once)
    public bool hideAfter = true;       // hide image when animation completes

    bool isPlaying = false;
    bool shownOnce = false; // if you only want to show once ever

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        if (tutorialImage != null) tutorialImage.enabled = false;
    }

    // Call this from your bush click handler
    public void ShowTutorial()
    {
        if (isPlaying) return;
        if (shownOnce) return; // remove this line if you want it repeatable
        if (tutorialImage == null || frameA == null || frameB == null) return;

        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        isPlaying = true;
        shownOnce = true;
        tutorialImage.enabled = true;

        // start with frameA
        tutorialImage.sprite = frameA;

        for (int i = 0; i < cycles; i++)
        {
            // A
            tutorialImage.sprite = frameA;
            yield return new WaitForSeconds(frameDuration);

            // B
            tutorialImage.sprite = frameB;
            yield return new WaitForSeconds(frameDuration);
        }

        if (hideAfter) tutorialImage.enabled = false;

        isPlaying = false;
    }
}