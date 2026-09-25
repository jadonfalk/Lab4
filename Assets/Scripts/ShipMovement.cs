using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class ShipMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 6f;
    [SerializeField, Min(0.1f)] private float acceleration = 20f;
    [SerializeField] private Vector2 minimumPosition = new Vector2(-8f, -5f);
    [SerializeField] private Vector2 maximumPosition = new Vector2(8f, 5f);

    private IMoveInput input;
    private Rigidbody2D body;
    private Vector2 velocity;

    private void Awake() => body = GetComponent<Rigidbody2D>();
    public void Initialize(IMoveInput source) => input = source;

    private void FixedUpdate()
    {
        if (input == null) return;
        Vector2 desired = Vector2.ClampMagnitude(input.Move, 1f) * speed;
        velocity = Vector2.MoveTowards(velocity, desired, acceleration * Time.fixedDeltaTime);
        Vector2 next = body.position + velocity * Time.fixedDeltaTime;
        next.x = Mathf.Clamp(next.x, minimumPosition.x, maximumPosition.x);
        next.y = Mathf.Clamp(next.y, minimumPosition.y, maximumPosition.y);
        body.MovePosition(next);
    }
}
