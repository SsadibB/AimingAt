using UnityEngine; // Rect and transforms for playfield-local bounds.

public class ArenaSettings : MonoBehaviour // Lives on Playfield and owns the shared flight settings.
{ // Block for: public class ArenaSettings : MonoBehaviour // Lives on Playfield and ...
    [SerializeField] float gravity = 1800f; // Canvas units per second squared, shared by the ball and the target.
    [SerializeField] RectTransform playfield = null; // The space gameplay math uses; this object.
    [SerializeField] RectTransform groundLine = null; // Marker whose height is the landing line.
    [SerializeField] float outOfBoundsMargin = 300f; // Extra canvas units past the playfield before a shot is gone.

    public float Gravity => gravity; // Both flight scripts read this so their steps stay identical.

    public float GroundY => ToPlayfieldLocal(groundLine.position).y; // Landing height in playfield space, taken from the marker.

    public Vector2 ToPlayfieldLocal(Vector3 worldPosition) // Converts a marker or muzzle into the space the ball uses.
    { // Block for: public Vector2 ToPlayfieldLocal(Vector3 worldPosition) // Converts a ...
        return playfield.InverseTransformPoint(worldPosition); // Playfield local, independent of canvas scale.
    } // End of that block.

    public bool IsOutOfBounds(Vector2 localPos) // True when a shot has left the field plus the margin.
    { // Block for: public bool IsOutOfBounds(Vector2 localPos) // True when a shot has l...
        Rect area = playfield.rect; // The playfield rectangle in its own local space.
        area.xMin -= outOfBoundsMargin; // Extend the left edge.
        area.xMax += outOfBoundsMargin; // Extend the right edge.
        area.yMin -= outOfBoundsMargin; // Extend the bottom edge.
        area.yMax += outOfBoundsMargin; // Extend the top edge.
        return !area.Contains(localPos); // Outside the expanded rect counts as gone.
    } // End of that block.
} // End of that block.
