using UnityEngine; // Motion, time, and the rect used as the hit circle.
using DG.Tweening; // The pop-in when a shot is fired.

public class Projectile : MonoBehaviour // The one ball already in the scene; it is shown and hidden, never spawned.
{ // Block for: public class Projectile : MonoBehaviour // The one ball already in th...
    [SerializeField] ArenaSettings arena = null; // Gravity, the ground line, and the out-of-bounds test.
    [SerializeField] FallingTarget target = null; // What this shot can hit.
    [SerializeField] float maxLifetime = 8f; // Safety cutoff so a wild shot cannot fly forever.
    [SerializeField] float pop = 0.15f; // Seconds for the scale-in when the shot appears.

    Vector2 velocity; // Canvas units per second; gravity changes Y before the move.
    float life; // Seconds since Launch.
    bool flying; // Update does nothing until Launch.
    Vector3 baseScale; // Authored size, because the pop starts from zero.

    float BallRadius => ((RectTransform)transform).rect.width * 0.5f * baseScale.x; // Authored half-width, so the pop does not shrink the hit circle.

    void Awake() // Runs the first time the ball is shown, before Launch scales it to zero.
    { // Block for: void Awake() // Runs the first time the ball is shown, before Launch ...
        baseScale = transform.localScale; // Remember the scene size for the pop and the radius.
    } // End of that block.

    public void Launch(Vector2 startLocalPos, Vector2 shotVelocity) // Called by the cannon at the muzzle, in playfield space.
    { // Block for: public void Launch(Vector2 startLocalPos, Vector2 shotVelocity) // Ca...
        transform.localPosition = startLocalPos; // Appear at the muzzle in playfield space.
        gameObject.SetActive(true); // The ball already exists; this only shows it.
        transform.DOKill(); // Clear a pop that was still running from a previous shot.
        transform.localScale = Vector3.zero; // Start the pop from nothing.
        transform.DOScale(baseScale, pop).SetEase(Ease.OutBack); // Grow back to the scene size with a little overshoot.
        velocity = shotVelocity; // Direction and power chosen by the pull.
        life = 0f; // Restart the safety timer.
        flying = true; // The next Update takes the first gravity step.
    } // End of that block.

    void Update() // Same gravity step as the target: speed, then position.
    { // Block for: void Update() // Same gravity step as the target: speed, then position.
        if (!flying) return; // Stay put until Launch.
        life += Time.deltaTime; // Count this frame toward the safety cutoff.
        velocity.y -= arena.Gravity * Time.deltaTime; // Gravity changes vertical speed first.
        transform.localPosition += (Vector3)(velocity * Time.deltaTime); // Then move by the new velocity.
        Vector2 pos = transform.localPosition; // Playfield local position after the step.
        float reach = BallRadius + target.Radius + target.HitPadding; // Overlap when the circles meet, plus the Inspector padding.
        if (target.IsFalling && Vector2.Distance(pos, target.LocalPos) < reach) // A hit only counts while the target is falling.
        { // Block for: if (target.IsFalling && Vector2.Distance(pos, target.LocalPos) < reac...
            target.OnHit(); // Flash and freeze the target.
            Deactivate(); // Hide the ball; the object stays in the scene.
            return; // Skip the miss checks after a hit.
        } // End of that block.
        if (pos.y < arena.GroundY || arena.IsOutOfBounds(pos) || life >= maxLifetime) // Ground, off the field, or too old.
        { // Block for: if (pos.y < arena.GroundY || arena.IsOutOfBounds(pos) || life >= maxL...
            Deactivate(); // Misses also just hide the ball.
        } // End of that block.
    } // End of that block.

    public void Deactivate() // Hides the ball without destroying it.
    { // Block for: public void Deactivate() // Hides the ball without destroying it.
        flying = false; // Stop integrating if something calls this mid-frame.
        transform.DOKill(); // Stop the pop so it cannot run on a hidden object.
        gameObject.SetActive(false); // Ready for the next Launch.
    } // End of that block.

    void OnDestroy() // Play mode ending mid-flight must not leave a tween behind.
    { // Block for: void OnDestroy() // Play mode ending mid-flight must not leave a twee...
        transform.DOKill(); // Drop the pop tween as the scene unloads.
    } // End of that block.
} // End of that block.
