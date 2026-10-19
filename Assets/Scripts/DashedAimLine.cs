using UnityEngine;                                   // Core Unity types.
using DG.Tweening;                                   // DOTween for the moving dashes.

[RequireComponent(typeof(LineRenderer))]             // Auto-adds a LineRenderer; the script can't work without one.
public class DashedAimLine : MonoBehaviour           // Put on an empty object named "AimLine".
{
    public float dashLength = 0.25f;                // World length of one dash (and of one gap).
    public float lineWidth = 0.06f;                 // Thickness of the line.
    public Color color = Color.white;               // Colour of the dashes.
    public float marchTime = 0.4f;                  // Seconds for the dashes to travel one dash+gap; smaller = faster.

    private LineRenderer lr;                        // Cached reference to the line component.
    private Material dashMaterial;                  // Cached so we can change tiling and offset.

    private void Awake()                            // One-time setup.
    {
        lr = GetComponent<LineRenderer>();          // Grab the LineRenderer on this object.
        lr.positionCount = 2;                       // A straight line needs only a start and an end point.
        lr.useWorldSpace = true;                    // Our points are world coordinates.
        lr.startWidth = lineWidth;                  // Same width at the start...
        lr.endWidth = lineWidth;                    // ...and at the end.
        lr.startColor = color;                      // Colour at the start.
        lr.endColor = color;                        // Colour at the end.
        lr.textureMode = LineTextureMode.Stretch;   // Stretch the texture along the whole line so tiling controls repeats.
        lr.sortingOrder = 5;                        // Draw above background sprites.

        Texture2D tex = new Texture2D(2, 1, TextureFormat.RGBA32, false); // 2x1 image: one dash pixel, one gap pixel.
        tex.SetPixel(0, 0, Color.white);            // Visible pixel = dash.
        tex.SetPixel(1, 0, Color.clear);            // Transparent pixel = gap.
        tex.Apply();                                // Upload the pixels; without this nothing changes.
        tex.filterMode = FilterMode.Point;          // Crisp edges, no blur.
        tex.wrapMode = TextureWrapMode.Repeat;      // Repeat the texture so one image makes many dashes.

        dashMaterial = new Material(Shader.Find("Sprites/Default")); // Simple shader with transparency support.
        dashMaterial.mainTexture = tex;             // Apply our dash/gap texture.
        lr.material = dashMaterial;                 // Use this material on the line.

        dashMaterial.DOOffset(new Vector2(-1f, 0f), marchTime) // Slide the texture by exactly one full repeat...
            .SetEase(Ease.Linear)                   // ...at constant speed, so motion looks smooth...
            .SetLoops(-1, LoopType.Restart);        // ...and loop forever; one repeat shift looks identical, so there is no visible jump.

        lr.enabled = false;                         // Hidden until the player starts pulling.
    }

    public void Show(Vector2 start, Vector2 end)    // Called every frame while the player drags.
    {
        lr.enabled = true;                          // Make the line visible.
        lr.SetPosition(0, start);                   // First point = cannon muzzle.
        lr.SetPosition(1, end);                     // Second point = end of the aim line.
        float length = Vector2.Distance(start, end); // Current line length.
        dashMaterial.mainTextureScale = new Vector2(length / (dashLength * 2f), 1f); // Keep dash size constant at any length.
    }

    public void Hide()                              // Called on release or reset.
    {
        lr.enabled = false;                         // Hide the line.
    }

    private void OnDestroy()                        // Runs when this object is destroyed.
    {
        dashMaterial.DOKill();                      // Stop the looping tween so it doesn't run on a destroyed material.
    }
}