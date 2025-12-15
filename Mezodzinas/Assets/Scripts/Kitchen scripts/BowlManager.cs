using UnityEngine;

public class BowlManager : MonoBehaviour
{
    [Header("Bowl Stages (empty to final)")]
    public GameObject[] bowlStages;  // 0 = empty, 1 = sugar added, etc.
    public string[] ingredientOrder = { "Sugar", "Butter", "Flour", "Milk", "Berries", "Spoon" };

    [HideInInspector]
    public int currentStage = 0; // Start with empty bowl

    [Header("Audio (optional)")]
    [Tooltip("Optional persistent AudioSource to play SFX from. DO NOT assign an AudioSource that lives on the object that will be destroyed.")]
    public AudioSource persistentAudioSource;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ingredient"))
        {
            IngredientDrag ingredient = collision.GetComponent<IngredientDrag>();

            if (ingredient != null)
            {
                string ingredientName = ingredient.ingredientName;

                // Check if it's the correct ingredient in sequence
                if (currentStage < ingredientOrder.Length && ingredientName == ingredientOrder[currentStage])
                {
                    // Let the ingredient handle playing its own assigned clip (safe if the ingredient will be destroyed).
                    ingredient.PlayOnUseClip(persistentAudioSource);

                    AddIngredient();

                    // remove the used ingredient (destroy after playing SFX)
                    Destroy(collision.gameObject);
                }
                else
                {
                    Debug.Log("Wrong ingredient! Expected: " + ingredientOrder[currentStage]);
                }
            }
        }
    }

    void AddIngredient()
    {
        currentStage++;

        // Ensure there's a prefab for the next stage
        if (currentStage < bowlStages.Length)
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            // Spawn the next stage bowl
            GameObject newBowl = Instantiate(bowlStages[currentStage], pos, rot);

            // Transfer progress data so it continues working
            BowlManager newBowlManager = newBowl.GetComponent<BowlManager>();
            if (newBowlManager != null)
            {
                newBowlManager.currentStage = currentStage;
                newBowlManager.bowlStages = bowlStages;
                newBowlManager.ingredientOrder = ingredientOrder;

                // copy persistent audio source so next bowl keeps playing SFX via the same AudioSource
                newBowlManager.persistentAudioSource = persistentAudioSource;
            }

            // Remove the old bowl object
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("All ingredients added!");
        }
    }
}