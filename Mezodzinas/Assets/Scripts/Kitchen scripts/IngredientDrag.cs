using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class IngredientDrag : MonoBehaviour
{
    [Header("Ingredient Settings")]
    public string ingredientName;

    // --- Audio settings per-prefab ---
    [Header("Optional audio when this ingredient is used/removed")]
    public AudioClip onUseClip;
    [Range(0f, 1f)] public float onUseVolume = 1f;

    [Header("Optional pitch randomization")]
    public bool randomizePitch = false;
    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;
    // -----------------------------------

    private bool isDragging = false;
    private Vector3 offset;
    private float startZ;

    void Start()
    {
        startZ = transform.position.z;
    }

    void OnMouseDown()
    {
        isDragging = true;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mousePos.x, mousePos.y, transform.position.z);
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x, mousePos.y, startZ) + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    /// <summary>
    /// Play the configured on-use clip. If a persistentAudioSource is provided and it's not on this
    /// ingredient GameObject, PlayOneShot is used there. Otherwise PlayClipAtPoint is used (safe when
    /// the ingredient will be destroyed).
    /// </summary>
    public void PlayOnUseClip(AudioSource persistentAudioSource = null)
    {
        if (onUseClip == null) return;

        float vol = Mathf.Clamp01(onUseVolume);

        // Optionally randomize pitch (only possible when using a provided persistent AudioSource).
        if (persistentAudioSource != null && persistentAudioSource.gameObject != this.gameObject)
        {
            if (randomizePitch)
            {
                float oldPitch = persistentAudioSource.pitch;
                persistentAudioSource.pitch = Random.Range(pitchMin, pitchMax);
                persistentAudioSource.PlayOneShot(onUseClip, vol);
                persistentAudioSource.pitch = oldPitch;
            }
            else
            {
                persistentAudioSource.PlayOneShot(onUseClip, vol);
            }
        }
        else
        {
            // Fallback: create a short-lived 3D/non-3D audio at this ingredient position so sound continues after destroy
            AudioSource.PlayClipAtPoint(onUseClip, transform.position, vol);
        }
    }
}