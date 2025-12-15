using UnityEngine;

public class ClickToSpawnTwo : MonoBehaviour
{
    [Header("First Object Settings")]
    public GameObject firstObjectToSpawn;
    public Transform firstSpawnPoint;
    [Tooltip("Sound played when the first object spawns (optional).")]
    public AudioClip firstSpawnClip;
    [Range(0f, 1f)] public float firstSpawnVolume = 1f;

    [Header("Second Object Settings")]
    public GameObject secondObjectToSpawn;
    public Transform secondSpawnPoint;
    [Tooltip("Sound played when the second object spawns (optional).")]
    public AudioClip secondSpawnClip;
    [Range(0f, 1f)] public float secondSpawnVolume = 1f;

    [Header("Audio")]
    [Tooltip("Optional: assign an AudioSource to use. If null, a temporary AudioSource will be created on this GameObject.")]
    public AudioSource audioSource;
    [Tooltip("When true, use 2D (non-spatial) sound. When false, allow 3D spatial sound on the AudioSource.")]
    public bool force2D = true;

    private int clickCount = 0;

    void Awake()
    {
        // If no AudioSource assigned, create one to play clips
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Configure 2D/3D behavior. If you want spatial sound set force2D = false and adjust spatialBlend.
        audioSource.spatialBlend = force2D ? 0f : 1f;
    }

    private void OnMouseDown()
    {
        clickCount++;

        if (clickCount == 1)
        {
            if (firstObjectToSpawn != null && firstSpawnPoint != null)
            {
                Instantiate(firstObjectToSpawn, firstSpawnPoint.position, firstSpawnPoint.rotation);
            }

            if (firstSpawnClip != null)
                PlayClip(firstSpawnClip, firstSpawnVolume);
        }
        else if (clickCount == 2)
        {
            if (secondObjectToSpawn != null && secondSpawnPoint != null)
            {
                Instantiate(secondObjectToSpawn, secondSpawnPoint.position, secondSpawnPoint.rotation);
            }

            if (secondSpawnClip != null)
                PlayClip(secondSpawnClip, secondSpawnVolume);
        }

        // Optionally reset clickCount after two clicks so it can be used again:
        // if (clickCount >= 2) clickCount = 0;
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}