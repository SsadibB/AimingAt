using UnityEngine; // Transform, time, and colour.
using UnityEngine.UI; // Image is what flashes when the target is hit.
using DG.Tweening; // Patrol, return, flash, and punch all go through DOTween.

public class FallingTarget : MonoBehaviour // The hanging target that patrols, then drops when the shot is fired.
{ // Block for: public class FallingTarget : MonoBehaviour // The hanging target that...
    [SerializeField] ArenaSettings arena = null; // Shared gravity and the playfield converter.
    [SerializeField] RectTransform patrolTop = null; // Highest point of the patrol, placed in the scene.
    [SerializeField] RectTransform patrolBottom = null; // Lowest point of the patrol, placed in the scene.
    [SerializeField] Image image = null; // Recolored on a hit; also killed with the tweens.
    [SerializeField] float patrolSpeed = 400f; // Canvas units per second; duration is distance divided by this.
    [SerializeField] Ease patrolEase = Ease.InOutSine; // Slow at each end so the hang reads as a patrol.
    [SerializeField] float hitPadding = 0f; // Extra canvas units added to the ball's overlap test.

    float velocityY; // Vertical speed while falling; starts at rest on release.
    bool falling; // True only between Release and a landing or a hit.
    Vector3 baseScale; // Scene scale, restored after the hit punch.
    Color baseColor; // Scene color, restored after the red flash.

    public bool IsFalling => falling; // The ball only scores while the target is actually dropping.
    public float HitPadding => hitPadding; // Exposed so the ball can use the Inspector value.
    public float Radius => ((RectTransform)transform).rect.width * 0.5f * transform.localScale.x; // Half the rect, scaled, is the hit circle.
    public Vector2 LocalPos => transform.localPosition; // Direct child of Playfield, so this is playfield space.

    void Awake() // Cache the authored look before any tween changes it.
    { // Block for: void Awake() // Cache the authored look before any tween changes it.
        baseScale = transform.localScale; // Punch and reset both return to this.
        baseColor = image.color; // The flash returns to this instead of a hardcoded color.
    } // End of that block.

    void Start() // Patrol begins after the markers and the image exist.
    { // Block for: void Start() // Patrol begins after the markers and the image exist.
        StartPatrol(); // The tween owns the motion from here.
    } // End of that block.

    public void Release() // Called at the same instant the ball is fired.
    { // Block for: public void Release() // Called at the same instant the ball is fired.
        transform.DOKill(); // Stop the patrol or a return trip so gravity can take over.
        image.DOKill(); // Drop a leftover flash so it does not fight the fall.
        falling = true; // Update starts applying gravity.
        velocityY = 0f; // Released from rest, matching a target that was hanging.
    } // End of that block.

    void Update() // Manual fall, same step and order as the ball.
    { // Block for: void Update() // Manual fall, same step and order as the ball.
        if (!falling) return; // Patrol and the landed state are not integrated here.
        velocityY -= arena.Gravity * Time.deltaTime; // Gravity changes speed first.
        Vector3 pos = transform.localPosition; // Copy so only Y is integrated.
        pos.y += velocityY * Time.deltaTime; // Then move by the new speed, Y only.
        transform.localPosition = pos; // X stays where the patrol left it.
        if (pos.y < arena.GroundY) // Crossed the landing line authored in the scene.
        { // Block for: if (pos.y < arena.GroundY) // Crossed the landing line authored in th...
            pos.y = arena.GroundY; // Rest on that line instead of sinking past it.
            transform.localPosition = pos; // Write the landed spot.
            velocityY = 0f; // No leftover speed into the next round.
            falling = false; // Stop integrating until the next release.
        } // End of that block.
    } // End of that block.

    public void OnHit() // Ball overlap calls this once, then hides itself.
    { // Block for: public void OnHit() // Ball overlap calls this once, then hides itself.
        falling = false; // Freeze in the air for the hit feedback.
        image.DOColor(Color.red, 0.12f).SetLoops(2, LoopType.Yoyo); // Flash red and return.
        transform.DOPunchScale(Vector3.one * 0.25f, 0.4f, 8, 0.5f); // A short bounce on the hit.
    } // End of that block.

    public void ResetTarget() // Brings the target back and starts the next patrol.
    { // Block for: public void ResetTarget() // Brings the target back and starts the ne...
        transform.DOKill(); // Clear the punch, the fall is already stopped.
        image.DOKill(); // Clear the flash before restoring the color.
        falling = false; // The return trip is a tween, not physics.
        velocityY = 0f; // Next release starts from rest again.
        transform.localScale = baseScale; // Undo the punch.
        image.color = baseColor; // Undo the flash.
        float bottomY = MarkerY(patrolBottom); // Return height comes from the marker, not a number here.
        transform.DOLocalMoveY(bottomY, 0.4f).SetEase(Ease.OutBack).OnComplete(StartPatrol); // Settle on the low marker, then patrol.
    } // End of that block.

    void StartPatrol() // Yoyo between the two markers, keeping the target's current X.
    { // Block for: void StartPatrol() // Yoyo between the two markers, keeping the targe...
        float bottomY = MarkerY(patrolBottom); // Low end, read when the patrol starts.
        float topY = MarkerY(patrolTop); // High end, read the same moment.
        float distance = Mathf.Abs(topY - bottomY); // The span the tween has to cover.
        float duration = distance / patrolSpeed; // Speed is canvas units per second.
        transform.DOKill(); // Drop the return tween before starting the loop.
        transform.DOLocalMoveY(topY, duration).From(bottomY, true, false).SetEase(patrolEase).SetLoops(-1, LoopType.Yoyo); // From the low marker up, then back forever.
    } // End of that block.

    float MarkerY(RectTransform marker) // Marker world position converted into playfield local Y.
    { // Block for: float MarkerY(RectTransform marker) // Marker world position converte...
        return arena.ToPlayfieldLocal(marker.position).y; // Same space as the target's localPosition.
    } // End of that block.

    void OnDisable() // Tweens must not keep running on a disabled graphic.
    { // Block for: void OnDisable() // Tweens must not keep running on a disabled graphic.
        transform.DOKill(); // Stop patrol, return, and punch.
        if (image != null) image.DOKill(); // Stop the flash if the image is still here.
    } // End of that block.

    void OnDestroy() // Same cleanup when the scene unloads.
    { // Block for: void OnDestroy() // Same cleanup when the scene unloads.
        transform.DOKill(); // Stop movement tweens during teardown.
        if (image != null) image.DOKill(); // Stop the flash during teardown.
    } // End of that block.
} // End of that block.
