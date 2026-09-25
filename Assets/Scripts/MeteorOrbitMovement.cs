using UnityEngine;

// Computes orbital locomotion explicitly. Health and game rules stay elsewhere.
[RequireComponent(typeof(Rigidbody2D))]
public sealed class MeteorOrbitMovement : MonoBehaviour
{
    [Header("Orbit")]
    [SerializeField, Min(0.1f)] private float orbitRadius = 3f;
    [SerializeField, Min(0.1f)] private float radiusCorrection = 2f;
    [SerializeField] private bool clockwise = true;

    [Header("Speed by squared distance")]
    [SerializeField, Min(0f)] private float nearSpeed = 2f;
    [SerializeField, Min(0f)] private float farSpeed = 4f;
    [SerializeField, Min(0.1f)] private float nearDistance = 3f;
    [SerializeField, Min(0.2f)] private float farDistance = 8f;

    [Header("Facing")]
    [Tooltip("A child containing the sprite. Keep colliders/Rigidbody on the root.")]
    [SerializeField] private Transform visual;
    [Tooltip("Up-facing artwork: -90. Right-facing: 0. Down-facing: 90.")]
    [SerializeField] private float facingOffset = -90f;

    private Transform target;
    private Rigidbody2D body;
    public bool HasTarget => target != null && target.gameObject.activeInHierarchy;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (visual == null || visual == transform)
            Debug.LogWarning("Assign a separate Visual child on MeteorOrbitMovement so facing is visible.", this);
    }

    public void Initialize(Transform player) => target = player;

    private void FixedUpdate()
    {
        if (!HasTarget) return;

        // Outward radial vector: from the player to this meteor.
        Vector2 offset = body.position - (Vector2)target.position;
        float distanceSquared = offset.sqrMagnitude;
        float distance = Mathf.Sqrt(distanceSquared);

        // At exact overlap there is no direction to normalize. Pick a fallback.
        Vector2 outward = distanceSquared > 0.000001f
            ? offset / distance
            : Vector2.right;

        // A perpendicular vector gives motion around the player.
        // At the right of the player, clockwise motion points downward.
        Vector2 tangent = new Vector2(outward.y, -outward.x);
        if (!clockwise) tangent = -tangent;

        // Too far: steer inward. Too close: steer outward.
        // At the desired radius, correction is zero and motion is tangential.
        float radiusError = distance - orbitRadius;
        float correction = Mathf.Clamp(radiusError * radiusCorrection, -2f, 2f);
        Vector2 direction = (tangent - outward * correction).normalized;

        // Speed uses squared-distance thresholds; no square root is needed here.
        float nearSquared = nearDistance * nearDistance;
        float farSquared = Mathf.Max(farDistance * farDistance, nearSquared + 0.001f);
        float blend = Mathf.Clamp01((distanceSquared - nearSquared) / (farSquared - nearSquared));
        float speed = Mathf.Lerp(nearSpeed, farSpeed, blend);

        // All movement math is explicit; MovePosition submits the result to physics.
        Vector2 nextPosition = body.position + direction * speed * Time.fixedDeltaTime;
        body.MovePosition(nextPosition);
    }

    private void LateUpdate()
    {
        if (!HasTarget || visual == null || visual == transform) return;

        // Face inward, independently of the direction of orbital travel.
        // Use displayed transforms here so the artwork matches interpolation.
        Vector2 towardPlayer = (Vector2)target.position - (Vector2)transform.position;
        if (towardPlayer.sqrMagnitude < 0.000001f) return;
        float angle = Mathf.Atan2(towardPlayer.y, towardPlayer.x) * Mathf.Rad2Deg;
        visual.rotation = Quaternion.Euler(0f, 0f, angle + facingOffset);
    }

    private void OnValidate()
    {
        orbitRadius = Mathf.Max(0.1f, orbitRadius);
        radiusCorrection = Mathf.Max(0.1f, radiusCorrection);
        nearDistance = Mathf.Max(0.1f, nearDistance);
        farDistance = Mathf.Max(nearDistance + 0.1f, farDistance);
        nearSpeed = Mathf.Max(0f, nearSpeed);
        farSpeed = Mathf.Max(0f, farSpeed);
    }
}
