using UnityEngine;
using System.Collections;

public class DraggableToOven : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private float originalZ;

    [Header("References")]
    public string ovenTag = "Oven";
    public AudioClip placedInOvenSound;   // sound when putting pie in oven
    public AudioClip bakingDoneSound;     // ding when ready
    public GameObject bakedPiePrefab;     // baked pie prefab

    [Header("Positions")]
    public Vector3 tablePosition = new Vector3(-0.9f, -3.61f, -9f); // baked pie location

    private AudioSource audioSource;
    private bool isBaking = false;
    private GameObject ovenObject;

    void Start()
    {
        originalZ = transform.position.z;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnMouseDown()
    {
        if (isBaking) return;
        isDragging = true;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mousePos.x, mousePos.y, transform.position.z);
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newPos = new Vector3(mousePos.x, mousePos.y, originalZ) + offset;
        transform.position = newPos;
    }

    void OnMouseUp()
    {
        isDragging = false;

        // detect oven via 2D collider
        Collider2D hit = Physics2D.OverlapPoint(transform.position);
        if (hit != null && hit.CompareTag(ovenTag))
        {
            ovenObject = hit.gameObject;
            StartCoroutine(StartBaking());
        }
    }

    private IEnumerator StartBaking()
    {
        isBaking = true;

        // Move into oven and play "placed" sound
        transform.position = ovenObject.transform.position;
        if (placedInOvenSound)
            audioSource.PlayOneShot(placedInOvenSound);

        // Hide raw pie
        GetComponent<SpriteRenderer>().enabled = false;

        // Wait baking time
        yield return new WaitForSeconds(3f);

        // Play ding
        if (bakingDoneSound)
            AudioSource.PlayClipAtPoint(bakingDoneSound, ovenObject.transform.position);

        // Spawn baked pie automatically
        if (bakedPiePrefab != null)
        {
            Vector3 spawnPos = new Vector3(tablePosition.x, tablePosition.y, 0f);
            GameObject bakedPie = Instantiate(bakedPiePrefab, spawnPos, Quaternion.identity);
            bakedPie.SetActive(true);

            Debug.Log($"[Oven] Baked pie spawned automatically at {spawnPos}");
        }
        else
        {
            Debug.LogError("[Oven] Baked pie prefab not assigned!");
        }

        // Remove raw pie
        Destroy(gameObject);
    }
}