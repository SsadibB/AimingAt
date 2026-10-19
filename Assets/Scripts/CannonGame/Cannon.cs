using UnityEngine; // Pull math and the delayed reset.
using UnityEngine.EventSystems; // Pointer events from the EventSystem, instead of polling the mouse.
using DG.Tweening; // Clears any leftover tween when the round resets.

public class Cannon : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler // Drag anywhere on the screen to aim.
{ // Block for: public class Cannon : MonoBehaviour, IPointerDownHandler, IDragHandle...
    [SerializeField] ArenaSettings arena = null; // Kept with the other scene refs so the shot shares one setup.
    [SerializeField] RectTransform playfield = null; // Pointer positions are converted into this rect.
    [SerializeField] RectTransform barrel = null; // The cannon graphic. It stays at its scene pose.
    [SerializeField] RectTransform muzzle = null; // Tip of the barrel, where the ball and the dashes start.
    [SerializeField] Projectile ball = null; // The single scene ball.
    [SerializeField] FallingTarget target = null; // Released at the same moment the ball launches.
    [SerializeField] AimDots aimLine = null; // Dashes shown only while pulling.
    [SerializeField] float maxPull = 300f; // Longest pull that still counts, in canvas units.
    [SerializeField] float minPull = 40f; // Shorter releases cancel so a tap does not fire.
    [SerializeField] float minSpeed = 900f; // Ball speed at the shortest accepted pull.
    [SerializeField] float maxSpeed = 2600f; // Ball speed at a full pull.
    [SerializeField] float resetDelay = 1.5f; // Seconds after a shot before the round resets itself.

    bool fired; // Blocks another pull until ResetShot.
    bool dragging; // True from pointer down to pointer up.
    float currentPull; // Clamped pull length from the latest pointer sample.
    float speed; // Shot speed that matches currentPull.
    Vector2 aimDir = Vector2.right; // Unit direction the ball will travel.

    public void OnPointerDown(PointerEventData eventData) // Press began anywhere on the screen.
    { // Block for: public void OnPointerDown(PointerEventData eventData) // Press began ...
        if (fired || arena == null) return; // Ignore input until the round resets, and until the shared setup is wired.
        dragging = true; // This press can become a shot.
        UpdateAim(eventData); // Show the line immediately.
    } // End of that block.

    public void OnDrag(PointerEventData eventData) // Pointer moved while held.
    { // Block for: public void OnDrag(PointerEventData eventData) // Pointer moved while...
        if (!dragging || fired) return; // Only an active pull updates the aim.
        UpdateAim(eventData); // Recompute pull and the dashed arc.
    } // End of that block.

    public void OnPointerUp(PointerEventData eventData) // Release: fire or cancel.
    { // Block for: public void OnPointerUp(PointerEventData eventData) // Release: fire ...
        if (!dragging || fired) return; // A press that never started does nothing.
        UpdateAim(eventData); // The release point is the shot that counts.
        dragging = false; // The pull is over.
        aimLine.Hide(); // The arc disappears as the shot leaves.
        if (currentPull >= minPull) Fire(); // A long enough pull becomes a shot.
    } // End of that block.

    void UpdateAim(PointerEventData eventData) // Slingshot: pull back from the pivot, aim the opposite way.
    { // Block for: void UpdateAim(PointerEventData eventData) // Slingshot: pull back fr...
        Vector2 pointerLocal; // Playfield-local pointer, filled by the utility below.
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(playfield, eventData.position, eventData.pressEventCamera, out pointerLocal)) return; // Overlay canvas accepts a null camera.
        Vector2 pivotLocal = playfield.InverseTransformPoint(barrel.position); // Barrel pivot in the same space as the pointer.
        Vector2 pull = pivotLocal - pointerLocal; // Back along the drag is forward along the shot.
        if (pull.sqrMagnitude < 0.0001f) return; // A zero vector has no aim direction.
        currentPull = Mathf.Min(pull.magnitude, maxPull); // Clamp so extra drag does not add power.
        aimDir = pull.normalized; // Unit shot direction. The graphic does not turn to follow it.
        speed = Mathf.Lerp(minSpeed, maxSpeed, currentPull / maxPull); // Power scales with how far the pull was clamped.
        Vector2 muzzleLocal = playfield.InverseTransformPoint(muzzle.position); // Fixed tip, because the cannon is not rotated.
        aimLine.Show(muzzleLocal, aimDir * speed, arena.Gravity, arena.GroundY); // Dashed arc of the shot the ball will actually fly.
    } // End of that block.

    void Fire() // Launches the ball and drops the target on the same call.
    { // Block for: void Fire() // Launches the ball and drops the target on the same call.
        fired = true; // Lock input until the reset.
        Vector2 muzzleLocal = playfield.InverseTransformPoint(muzzle.position); // Start the ball at the tip.
        ball.Launch(muzzleLocal, aimDir * speed); // Velocity is the aim times the pull-based speed.
        target.Release(); // Drop the target this instant so the arcs match.
        Invoke(nameof(ResetShot), resetDelay); // The round resets itself. The cannon stays where the scene put it.
    } // End of that block.

    public void ResetShot() // Public so a later UI button can call the same reset.
    { // Block for: public void ResetShot() // Public so a later UI button can call the s...
        CancelInvoke(nameof(ResetShot)); // A manual call must not stack a second reset.
        barrel.DOComplete(); // Finish a leftover tween so the cannon is exactly on its scene pose.
        fired = false; // Pulls work again.
        dragging = false; // No leftover press.
        currentPull = 0f; // The next press starts from no power.
        aimLine.Hide(); // Make sure no dashes survived.
        if (ball.gameObject.activeSelf) ball.Deactivate(); // Hide a ball that is still in the air.
        target.ResetTarget(); // Return the target and restart its patrol.
    } // End of that block.

    void OnDestroy() // Stop a pending reset and the punch when the scene unloads.
    { // Block for: void OnDestroy() // Stop a pending reset and the punch when the scene...
        CancelInvoke(); // Drop the delayed ResetShot.
        if (barrel != null) barrel.DOKill(); // Stop recoil during teardown.
    } // End of that block.
} // End of that block.
