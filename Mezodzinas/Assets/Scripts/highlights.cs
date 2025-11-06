using System.Collections.Generic;
using UnityEngine;

public class highlights : MonoBehaviour
{
    [Tooltip("Which layers should be considered for hover detection.")]
    public LayerMask hoverMask = ~0;

    [Tooltip("Color used for the outline copies.")]
    public Color outlineColor = new Color(0f, 0f, 0f, 0.9f);

    [Tooltip("Distance (in world units) from the sprite center for outline copies.")]
    public float outlineThickness = 0.02f;

    [Tooltip("Number of outline copies placed around the sprite. Higher = rounder outline, costlier.")]
    [Range(4, 16)]
    public int outlineSamples = 8;

    [Tooltip("Whether to outline all child SpriteRenderers of the hit object. If false, only the nearest parent renderer is outlined.")]
    public bool outlineChildren = false;

    [Tooltip("Optional camera to use. If null, Camera.main will be used.")]
    public Camera sourceCamera;

    // runtime state
    private Transform currentRoot;
    private Dictionary<SpriteRenderer, List<GameObject>> createdOutlines = new Dictionary<SpriteRenderer, List<GameObject>>();

    void Awake()
    {
        if (sourceCamera == null) sourceCamera = Camera.main;
    }

    void Update()
    {
        if (sourceCamera == null) return;

        Vector3 mousePos = Input.mousePosition;
        Vector2 worldPoint = sourceCamera.ScreenToWorldPoint(mousePos);

        var hits = Physics2D.OverlapPointAll(worldPoint, hoverMask);

        Collider2D bestHit = null;
        SpriteRenderer bestRenderer = null;
        int bestOrder = int.MinValue;
        float bestZ = float.MinValue;

        foreach (var c in hits)
        {
            if (c == null) continue;
            var sr = c.GetComponentInParent<SpriteRenderer>();
            if (sr != null)
            {
                int order = sr.sortingOrder;
                float z = sr.transform.position.z;
                if (bestRenderer == null || order > bestOrder || (order == bestOrder && z > bestZ))
                {
                    bestRenderer = sr;
                    bestOrder = order;
                    bestZ = z;
                    bestHit = c;
                }
            }
            else if (bestHit == null)
            {
                bestHit = c;
            }
        }

        if (bestHit != null)
        {
            if (bestRenderer != null)
                HandleHoverEnter(bestRenderer.gameObject);
            else
                HandleHoverEnter(bestHit.gameObject);
        }
        else
        {
            ClearCurrent();
        }
    }

    private void HandleHoverEnter(GameObject hitObject)
    {
        if (hitObject == null)
        {
            ClearCurrent();
            return;
        }

        // decide a sensible root for outlines
        Transform root = hitObject.transform;
        var parentRenderer = hitObject.GetComponentInParent<SpriteRenderer>();
        if (parentRenderer != null)
            root = parentRenderer.transform;

        if (currentRoot == root)
            return; // already highlighted

        ClearCurrent();
        currentRoot = root;

        if (outlineChildren)
        {
            var rlist = root.GetComponentsInChildren<SpriteRenderer>(includeInactive: false);
            foreach (var r in rlist)
                CreateOutlineForRenderer(r);
        }
        else
        {
            SpriteRenderer sr = hitObject.GetComponentInParent<SpriteRenderer>();
            if (sr == null)
                sr = hitObject.GetComponent<SpriteRenderer>();
            if (sr != null)
                CreateOutlineForRenderer(sr);
            else
            {
                // last resort: try to find child sprite
                var srChild = hitObject.GetComponentInChildren<SpriteRenderer>();
                if (srChild != null)
                    CreateOutlineForRenderer(srChild);
            }
        }
    }

    private void CreateOutlineForRenderer(SpriteRenderer sr)
    {
        if (sr == null) return;
        if (createdOutlines.ContainsKey(sr)) return; // already created

        var list = new List<GameObject>();

        // Parent container keeps outlines grouped under the same transform so they follow the sprite exactly.
        var parentName = $"HoverOutline_for_{sr.gameObject.name}";
        var parentGO = new GameObject(parentName);
        parentGO.transform.SetParent(sr.transform, false);
        parentGO.transform.localPosition = Vector3.zero;
        parentGO.transform.localRotation = Quaternion.identity;
        parentGO.transform.localScale = Vector3.one;

        // copy basic renderer settings and create several offset duplicates around the sprite
        float twoPI = Mathf.PI * 2f;
        for (int i = 0; i < outlineSamples; i++)
        {
            float angle = twoPI * i / outlineSamples;
            Vector3 offsetLocal = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * outlineThickness;

            var go = new GameObject("o");
            go.transform.SetParent(parentGO.transform, false);
            // place in local space so it follows scaling/rotation of the original
            go.transform.localPosition = offsetLocal;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var copy = go.AddComponent<SpriteRenderer>();
            copy.sprite = sr.sprite;
            copy.drawMode = sr.drawMode;
            copy.size = sr.size;
            copy.flipX = sr.flipX;
            copy.flipY = sr.flipY;
            copy.material = sr.sharedMaterial; // share material
            copy.maskInteraction = sr.maskInteraction;
            copy.color = outlineColor;

            // ensure outline is rendered behind the original: use same sorting layer, slightly lower order
            copy.sortingLayerID = sr.sortingLayerID;
            copy.sortingOrder = sr.sortingOrder - 1;

            // if original uses a custom tile/sprite settings, this tries to mimic them
            copy.sortingLayerName = sr.sortingLayerName;

            list.Add(go);
        }

        createdOutlines[sr] = list;
    }

    private void ClearCurrent()
    {
        // destroy all created outline GameObjects
        foreach (var kv in createdOutlines)
        {
            var list = kv.Value;
            if (list == null) continue;
            foreach (var go in list)
            {
                if (go != null)
                {
                    // children were created at runtime; use Destroy
                    Destroy(go);
                }
            }
        }
        // also destroy parent containers if left behind
        foreach (var kv in createdOutlines)
        {
            // try to remove parent container (they were the parent of created objects)
            // created objects were immediate children of the container; get one and destroy parent if empty
            if (kv.Value != null && kv.Value.Count > 0)
            {
                var any = kv.Value[0];
                if (any != null && any.transform.parent != null)
                {
                    var parent = any.transform.parent.gameObject;
                    // schedule destroy - parent should be empty after children destroyed above
                    Destroy(parent);
                }
            }
        }

        createdOutlines.Clear();
        currentRoot = null;
    }

    void OnDisable()
    {
        ClearCurrent();
    }
}
