using UnityEngine;

public class ClickToSpawnTwo : MonoBehaviour
{
    [Header("First Object Settings")]
    public GameObject firstObjectToSpawn;   
    public Transform firstSpawnPoint;       

    [Header("Second Object Settings")]
    public GameObject secondObjectToSpawn;  
    public Transform secondSpawnPoint;      

    private int clickCount = 0; 

    private void OnMouseDown()
    {
        clickCount++;

        if (clickCount == 1)
        {
            
            if (firstObjectToSpawn != null && firstSpawnPoint != null)
            {
                Instantiate(firstObjectToSpawn, firstSpawnPoint.position, firstSpawnPoint.rotation);
            }
        }
        else if (clickCount == 2)
        {
            
            if (secondObjectToSpawn != null && secondSpawnPoint != null)
            {
                Instantiate(secondObjectToSpawn, secondSpawnPoint.position, secondSpawnPoint.rotation);
            }

           
        }
    }
}