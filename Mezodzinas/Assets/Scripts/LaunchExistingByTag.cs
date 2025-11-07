using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Attach this to an empty GameObject. Hold Space to charge force, release Space to
/// launch already-instantiated GameObjects with the given tag in the chosen direction
/// (default: up-left). A UI Slider (and optional Text) can be assigned to visually
/// show the current charge. The slider will update while Space is held and stop
/// changing when Space is released (it will remain showing the final charge unless
/// you enable ResetSliderOnRelease).
///
/// This version also schedules launched objects for destruction after a configurable delay.
public class LaunchExistingByTag : MonoBehaviour
{
    [Header("Tag / Targeting")]
    [Tooltip("Tag of the already-instantiated GameObjects you want to launch.")]
    public string targetTag = "Projectile";

    [Header("Direction")]
    [Tooltip("Launch direction in degrees (0 = right, 90 = up). 135 = up-left).")]
    [Range(0f, 360f)]
    public float launchAngleDegrees = 135f;

    [Header("Charge / Force")]
    [Tooltip("Minimum impulse magnitude applied when Space is tapped.")]
    public float minLaunchForce = 1f;
    [Tooltip("Maximum impulse magnitude applied when Space is held to full charge.")]
    public float maxLaunchForce = 10f;
    [Tooltip("Time in seconds required to reach full charge (from min to max). If <= 0, charging is instantaneous).")]
    public float maxChargeTime = 1.5f;
    [Tooltip("Optional curve to shape charge (x: 0..1 normalized charge, y: multiplier 0..1).")]
    public AnimationCurve chargeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Launch Options")]
    [Tooltip("If true, launch every object found. If false, launch only the first found.")]
    public bool launchAll = true;
    [Tooltip("If true, each object will only be launched once (tracked by instance).")]
    public bool oneTimeLaunchPerObject = true;

    [Header("Auto-destroy")]
    [Tooltip("If > 0, scheduled destruction delay (seconds) after an object is launched.")]
    public float destroyDelay = 5f;

    [Header("UI (optional)")]
    [Tooltip("Optional UI Slider to visualize the current charge/force. The slider's min/max will be set automatically.")]
    public Slider forceSlider;
    [Tooltip("Optional UI Text to show numeric force value (legacy UI.Text). Leave null if not used.")]
    public Text forceValueText;
    [Tooltip("If true the slider will be reset to min value when Space is released; otherwise it will stay showing the final charge.")]
    public bool resetSliderOnRelease = false;
    [Tooltip("If true the slider GameObject will be hidden when not charging.")]
    public bool hideSliderWhenIdle = false;

    // Tracks which objects have already been launched (if oneTimeLaunchPerObject)
    private readonly HashSet<int> launchedInstanceIDs = new HashSet<int>();

    // Charging state
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private float currentForce = 0f;

    void Start()
    {
        // Initialize slider if present
        if (forceSlider != null)
        {
            forceSlider.minValue = minLaunchForce;
            forceSlider.maxValue = maxLaunchForce;
            forceSlider.value = minLaunchForce;
            forceSlider.wholeNumbers = false;
            forceSlider.gameObject.SetActive(!hideSliderWhenIdle); // show by default if not hiding
        }

        UpdateForceText(minLaunchForce);
        currentForce = minLaunchForce;
    }

    void Update()
    {
        // Start charging
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            chargeTimer = 0f;
            // Show slider when charging if hideSliderWhenIdle is enabled
            if (forceSlider != null && hideSliderWhenIdle)
                forceSlider.gameObject.SetActive(true);
        }

        // Continue charging while held
        if (isCharging && Input.GetKey(KeyCode.Space))
        {
            // increment timer
            if (maxChargeTime > 0f)
                chargeTimer += Time.deltaTime;
            else
                chargeTimer = maxChargeTime; // treat as instantly full

            if (maxChargeTime > 0f)
                chargeTimer = Mathf.Min(chargeTimer, maxChargeTime);

            float normalized = (maxChargeTime <= 0f) ? 1f : Mathf.Clamp01(chargeTimer / maxChargeTime);
            float shaped = (chargeCurve != null) ? chargeCurve.Evaluate(normalized) : normalized;
            currentForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, shaped);

            // Update UI
            if (forceSlider != null)
                forceSlider.value = currentForce;
            UpdateForceText(currentForce);
        }

        // Release: compute final force and launch
        if (isCharging && Input.GetKeyUp(KeyCode.Space))
        {
            float finalForce = currentForce;
            LaunchByTag(finalForce);

            // reset/stop charging state
            isCharging = false;
            chargeTimer = 0f;
            currentForce = (resetSliderOnRelease) ? minLaunchForce : finalForce;

            // Update UI after release
            if (forceSlider != null)
                forceSlider.value = currentForce;
            UpdateForceText(currentForce);

            // Optionally hide slider when not charging
            if (forceSlider != null && hideSliderWhenIdle && !isCharging)
                forceSlider.gameObject.SetActive(!hideSliderWhenIdle ? true : false);
        }
    }

    // Launch targets found by tag using provided impulse magnitude
    void LaunchByTag(float force)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning($"LaunchExistingByTag: No objects found with tag '{targetTag}'.");
            return;
        }

        // Precompute direction from angle (2D)
        float rad = launchAngleDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        int attempts = 0;
        int launched = 0;

        foreach (var go in targets)
        {
            attempts++;
            if (go == null) continue;

            if (oneTimeLaunchPerObject && launchedInstanceIDs.Contains(go.GetInstanceID()))
                continue;

            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning($"LaunchExistingByTag: '{go.name}' has no Rigidbody2D. Skipping.");
                if (!launchAll) break;
                continue;
            }

            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                Debug.LogWarning($"LaunchExistingByTag: '{go.name}' Rigidbody2D is not Dynamic (currently {rb.bodyType}). Skipping.");
                if (!launchAll) break;
                continue;
            }

            // Apply impulse in the chosen direction
            rb.AddForce(direction * force, ForceMode2D.Impulse);
            launched++;

            // Schedule destruction after configured delay (if > 0)
            if (destroyDelay > 0f)
            {
                // Use Destroy with delay so sound/particles can still play if scheduled elsewhere.
                Destroy(go, destroyDelay);
            }

            if (oneTimeLaunchPerObject)
                launchedInstanceIDs.Add(go.GetInstanceID());

            if (!launchAll) break;
        }

        Debug.Log($"LaunchExistingByTag: Attempted {attempts} targets, launched {launched} with force {force:F2}.");
    }

    // Helper to update optional UI text
    private void UpdateForceText(float value)
    {
        if (forceValueText != null)
            forceValueText.text = value.ToString("F1");
    }

    /// Optional: call to clear launched tracking so objects can be relaunched
    public void ResetLaunchedTracking()
    {
        launchedInstanceIDs.Clear();
    }

    // Draw direction gizmo in the editor for convenience
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        float rad = launchAngleDegrees * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f).normalized;
        Gizmos.DrawLine(transform.position, transform.position + dir * 1.5f);
        Vector3 right = Quaternion.Euler(0, 0, 150) * dir;
        Vector3 left = Quaternion.Euler(0, 0, -150) * dir;
        Gizmos.DrawLine(transform.position + dir * 1.5f, transform.position + dir * 1.2f + right * 0.2f);
        Gizmos.DrawLine(transform.position + dir * 1.5f, transform.position + dir * 1.2f + left * 0.2f);
    }
}