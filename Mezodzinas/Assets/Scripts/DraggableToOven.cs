using UnityEngine;
using System.Collections;

public class DraggableToOven : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private float originalZ;

    [Header("References")]
    public string ovenTag = "Oven";
    public AudioClip placedInOvenSound;
    public AudioClip bakingDoneSound;
    public GameObject bakedPiePrefab;

    [Header("Positions")]
    public Vector3 tablePosition = new Vector3(-0.9f, -3.61f, -1.05f);

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

        // instantly move into oven & hide raw pie
        transform.position = ovenObject.transform.position;
        if (placedInOvenSound)
            audioSource.PlayOneShot(placedInOvenSound);

        // hide raw pie immediately
        GetComponent<SpriteRenderer>().enabled = false;

        // wait baking time
        yield return new WaitForSeconds(3f);

        // --- Play "ding" sound reliably after baking ---
        if (bakingDoneSound != null)
        {
            AudioSource.PlayClipAtPoint(bakingDoneSound, ovenObject.transform.position);
            Debug.Log("[Oven] Ding! Baking finished.");
        }
        else
        {
            Debug.LogWarning("[Oven] No bakingDoneSound assigned!");
        }

        // set up oven to spawn baked pie on click
        if (bakedPiePrefab == null)
        {
            Debug.LogError("BakedPiePrefab not assigned! Assign a prefab from Project view.");
        }
        else
        {
            OvenClickHandler clicker = ovenObject.GetComponent<OvenClickHandler>();
            if (clicker == null)
                clicker = ovenObject.AddComponent<OvenClickHandler>();

            clicker.Setup(bakedPiePrefab, tablePosition);
        }

        // destroy raw pie completely
        Destroy(gameObject);
    }
}