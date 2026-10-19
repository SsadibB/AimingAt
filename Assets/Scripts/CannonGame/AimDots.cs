using UnityEngine; // Rects and the playfield transform used to place each dot.
using DG.Tweening; // A looping phase value makes the dashes march.

public class AimDots : MonoBehaviour // Places the pooled dots along the ballistic arc.
{ // Block for: public class AimDots : MonoBehaviour // Places the pooled dots along the ballistic arc.
    [SerializeField] RectTransform playfield = null; // Converts playfield-local points into world positions.
    [SerializeField] RectTransform[] dots = null; // The 30 dot rects, in order, already in the scene.
    [SerializeField] float spacing = 40f; // Canvas units between dashes along the arc.
    [SerializeField] float marchTime = 0.5f; // Seconds for a dash to travel one gap.
    [SerializeField] float step = 1f / 60f; // Same order of gravity step the ball uses, sampled at a fixed rate.

    float phase; // 0 to 1, how far the dashes have marched through one gap.
    bool showing; // True while a pull is holding the arc on screen.
    Vector2 shownStart; // Muzzle, in playfield space.
    Vector2 shownVelocity; // Shot velocity the arc is previewing.
    float shownGravity; // Gravity shared with the ball and the target.
    float shownGroundY; // Arc stops at the landing line.

    void Start() // One march loop for the whole session, not one per pull.
    { // Block for: void Start() // One march loop for the whole session, not one per pull.
        DOTween.To(() => phase, value => phase = value, 1f, marchTime).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetId(this).OnUpdate(RefreshIfShowing); // Phase restarts 0 to 1 forever and repositions the dots.
    } // End of that block.

    public void Show(Vector2 startLocal, Vector2 velocity, float gravity, float groundY) // Called while the player is dragging.
    { // Block for: public void Show(Vector2 startLocal, Vector2 velocity, float gravity, float groundY)
        showing = true; // Allow the march to keep moving the dots.
        shownStart = startLocal; // Remember the muzzle.
        shownVelocity = velocity; // Remember the shot the arc is previewing.
        shownGravity = gravity; // Same gravity the ball will use.
        shownGroundY = groundY; // Stop the preview at the ground.
        Place(shownStart, shownVelocity, shownGravity, shownGroundY); // Draw this frame immediately.
    } // End of that block.

    public void Hide() // Pull ended or the shot was fired.
    { // Block for: public void Hide() // Pull ended or the shot was fired.
        showing = false; // The march keeps running, but it no longer moves dots.
        SetAllInactive(); // Every pooled dot goes back off.
    } // End of that block.

    void RefreshIfShowing() // Tween tick: march only while the arc is up.
    { // Block for: void RefreshIfShowing() // Tween tick: march only while the arc is up.
        if (!showing) return; // Hidden pulls do no work.
        Place(shownStart, shownVelocity, shownGravity, shownGroundY); // Same shot, new phase.
    } // End of that block.

    void Place(Vector2 startLocal, Vector2 velocity, float gravity, float groundY) // Steps the shot and drops a dash every spacing.
    { // Block for: void Place(Vector2 startLocal, Vector2 velocity, float gravity, float groundY)
        if (velocity.sqrMagnitude < 0.0001f) // No shot to preview.
        { // Block for: if (velocity.sqrMagnitude < 0.0001f)
            SetAllInactive(); // Clear any dashes from the previous drag.
            return; // Nothing to place.
        } // End of that block.
        Vector2 pos = startLocal; // Walker starts at the muzzle.
        Vector2 vel = velocity; // Walker uses the shot velocity.
        Vector2 prev = pos; // Previous sample, so a dash can sit between steps.
        float traveled = 0f; // Arc length walked so far.
        float next = phase * spacing; // First dash is shifted by the march.
        int placed = 0; // How many pool entries are in use.
        int guard = 0; // Caps the walk so a wild shot cannot loop forever.
        while (placed < dots.Length && guard < 600) // Never more dots than the pool.
        { // Block for: while (placed < dots.Length && guard < 600)
            guard++; // Count this sample.
            vel.y -= gravity * step; // Gravity changes vertical speed first, matching the ball.
            pos += vel * step; // Then the walker moves by the new velocity.
            float seg = Vector2.Distance(prev, pos); // Length of this sample.
            traveled += seg; // Add it to the arc length.
            while (placed < dots.Length && seg > 0.0001f && traveled >= next) // A dash falls inside this sample.
            { // Block for: while (placed < dots.Length && seg > 0.0001f && traveled >= next)
                float along = 1f - ((traveled - next) / seg); // Where the dash sits between the two samples.
                Vector2 at = Vector2.Lerp(prev, pos, Mathf.Clamp01(along)); // Playfield-local point on the arc.
                dots[placed].position = playfield.TransformPoint(at); // Dots are not direct children, so convert to world.
                dots[placed].gameObject.SetActive(true); // Show this dash.
                placed++; // Move to the next pool entry.
                next += spacing; // The following dash is one gap farther.
            } // End of that block.
            if (pos.y <= groundY) break; // The preview ends where the ball would land.
            prev = pos; // This sample becomes the start of the next one.
        } // End of that block.
        for (int i = placed; i < dots.Length; i++) dots[i].gameObject.SetActive(false); // Unused pool entries stay hidden.
    } // End of that block.

    void SetAllInactive() // Hides the whole pool.
    { // Block for: void SetAllInactive() // Hides the whole pool.
        for (int i = 0; i < dots.Length; i++) // Each pooled image.
        { // Block for: for (int i = 0; i < dots.Length; i++)
            dots[i].gameObject.SetActive(false); // Disable, never destroy.
        } // End of that block.
    } // End of that block.

    void OnDestroy() // Stop the phase loop when the scene unloads.
    { // Block for: void OnDestroy() // Stop the phase loop when the scene unloads.
        DOTween.Kill(this); // The march tween was tagged with this object.
    } // End of that block.
} // End of that block.
