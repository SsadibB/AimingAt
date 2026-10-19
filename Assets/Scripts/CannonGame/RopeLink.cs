using UnityEngine; // Positions and rotates the existing rope image.

public class RopeLink : MonoBehaviour // Stretches one scene image from the holder to the aim bar.
{ // Block for: public class RopeLink : MonoBehaviour // Stretches one scene image from the holder to the aim bar.
    [SerializeField] RectTransform playfield = null; // Rope math uses this space, same as the rest of the shot.
    [SerializeField] RectTransform holderAnchor = null; // Point on the holder the rope leaves from.
    [SerializeField] RectTransform aimAnchor = null; // Point on the aim bar the rope ties to.
    [SerializeField] RectTransform rope = null; // The image that is scaled to the current length.

    float restWidth; // Authored width, so length is a scale and not a sizeDelta change.

    void Awake() // Cache the width the scene gave the rope.
    { // Block for: void Awake() // Cache the width the scene gave the rope.
        restWidth = rope.rect.width; // Read the layout. Do not write it.
    } // End of that block.

    void LateUpdate() // After the target has moved this frame, pull the rope with it.
    { // Block for: void LateUpdate() // After the target has moved this frame, pull the rope with it.
        if (playfield == null || holderAnchor == null || aimAnchor == null || rope == null) return; // Stay quiet until the scene refs are wired.
        Vector2 from = playfield.InverseTransformPoint(holderAnchor.position); // Holder end in playfield space.
        Vector2 to = playfield.InverseTransformPoint(aimAnchor.position); // Aim-bar end in playfield space.
        Vector2 delta = to - from; // The rope's span.
        float length = delta.magnitude; // How far the two anchors are.
        if (length < 0.001f || restWidth < 0.001f) return; // A zero span has no direction to draw.
        Vector2 mid = (from + to) * 0.5f; // The image pivot sits halfway, so scaling stays centered.
        rope.position = playfield.TransformPoint(mid); // The rope is a child of the playfield, placed from that space.
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg; // Canvas angle of the span.
        rope.localEulerAngles = new Vector3(0f, 0f, angle); // Point the image along the rope.
        rope.localScale = new Vector3(length / restWidth, 1f, 1f); // Length only. Thickness stays the scene height.
    } // End of that block.
} // End of that block.
