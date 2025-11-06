using UnityEngine;

public class IngredientDrag : MonoBehaviour
{
    [Header("Ingredient Settings")]
    public string ingredientName; 

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
}