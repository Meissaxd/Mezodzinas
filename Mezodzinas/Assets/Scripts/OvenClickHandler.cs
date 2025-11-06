using UnityEngine;

public class OvenClickHandler : MonoBehaviour
{
    [Header("Spawn Settings (for debug)")]
    private GameObject bakedPiePrefab;
    private Vector3 tablePosition;
    private bool canSpawn = false;

    public void Setup(GameObject piePrefab, Vector3 position)
    {
        bakedPiePrefab = piePrefab;
        tablePosition = position;
        canSpawn = true;

        Debug.Log($"[OvenClickHandler] Setup complete. Will spawn '{bakedPiePrefab.name}' at {tablePosition}");
    }

    void OnMouseDown()
    {
        Debug.Log($"[OvenClickHandler] Oven clicked! canSpawn = {canSpawn}");

        if (!canSpawn)
        {
            Debug.LogWarning("[OvenClickHandler] Cannot spawn yet!");
            return;
        }

        if (bakedPiePrefab == null)
        {
            Debug.LogError("[OvenClickHandler] No baked pie prefab assigned!");
            return;
        }

        // ensure it's visible in 2D
        Vector3 spawnPos = new Vector3(tablePosition.x, tablePosition.y, 0f);

        GameObject pie = Instantiate(bakedPiePrefab, spawnPos, Quaternion.identity);
        Debug.Log($"[OvenClickHandler] Spawned baked pie '{pie.name}' at {spawnPos}");

        // make sure it's active and visible
        pie.SetActive(true);
        var sr = pie.GetComponent<SpriteRenderer>();
        if (sr == null)
            Debug.LogWarning("[OvenClickHandler] Spawned pie has no SpriteRenderer!");

        canSpawn = false;
    }
}