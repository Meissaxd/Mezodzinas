using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class IntEvent : UnityEvent<int> { }

public class basket : MonoBehaviour
{
    [Tooltip("Assign the ReadRngInspector component that holds the RandomNumberGenerator (private field 'rng').")]
    public ReadRngInspector readRngInspector;

    [Header("Per-number sprites (index 0 -> number 1, index 4 -> number 5)")]
    [Tooltip("Assign sprites for numbers 1..5")]
    public Sprite[] numberSprites = new Sprite[5];

    [Tooltip("Where spawned sprites appear. If null, this GameObject's position is used.")]
    public Transform spawnPoint;

    [Tooltip("Optional parent for spawned GameObjects.")]
    public Transform spawnParent;

    [Header("Lifecycle")]
    public bool autoDestroy = false;
    public float destroyAfterSeconds = 5f;

    [Header("Signals")]
    [Tooltip("Fired as soon as Space is pressed (no payload).")]
    public UnityEvent OnSpacePressed;
    [Tooltip("Fired after a sprite is spawned; provides the spawned number (1..5).")]
    public IntEvent OnNumberSpawned;

    // C# event for code subscribers
    public event Action<int> NumberSpawnedEvent;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        // Signal that space was pressed
        OnSpacePressed?.Invoke();

        if (readRngInspector == null)
        {
            Debug.LogWarning("basket: ReadRngInspector is not assigned.");
            return;
        }

        // Access the private serialized field 'rng' on ReadRngInspector to get the RandomNumberGenerator instance.
        var field = typeof(ReadRngInspector).GetField("rng", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field == null)
        {
            Debug.LogWarning("basket: ReadRngInspector does not contain a field named 'rng'.");
        }
    }
}