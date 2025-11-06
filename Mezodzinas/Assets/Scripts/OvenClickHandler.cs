using UnityEngine;

public class OvenClickHandler : MonoBehaviour
{
    private GameObject bakedPiePrefab;
    private Vector3 tablePosition;
    private bool canSpawn = false;

    public void Setup(GameObject piePrefab, Vector3 position)
    {
        bakedPiePrefab = piePrefab;
        tablePosition = position;
        canSpawn = true;
        Debug.Log("[OvenClickHandler] Setup complete. Pie prefab: " + bakedPiePrefab.name);
    }

    void OnMouseDown()
    {
        Debug.Log("[OvenClickHandler] Oven clicked. canSpawn = " + canSpawn);

        if (!canSpawn) return;

        Instantiate(bakedPiePrefab, tablePosition, Quaternion.identity);
        Debug.Log("[OvenClickHandler] Baked pie spawned at: " + tablePosition);

        canSpawn = false; // Prevent multiple spawns
    }
}
