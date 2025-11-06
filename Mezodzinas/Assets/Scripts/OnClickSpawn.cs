using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickSpawn : MonoBehaviour
{
    [Header("Object to Spawn")]
    public GameObject objectToSpawn;  // The prefab you want to spawn

    [Header("Spawn Position")]
    public Transform spawnPoint;      // The position where it should appear

    private void OnMouseDown()
    {
       
        if (objectToSpawn != null && spawnPoint != null)
        {
            Instantiate(objectToSpawn, spawnPoint.position, spawnPoint.rotation);
        }

       
        Destroy(gameObject);
    }
}