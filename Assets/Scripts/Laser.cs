using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class Laser : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 8f;
    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField, Min(0.1f)] private float lifetime = 4f;
    [SerializeField] private LayerMask targetLayers;
    private Rigidbody2D body;
    private bool spent;

    private void Awake() => body = GetComponent<Rigidbody2D>();
    private void Start() => Destroy(gameObject, lifetime);

    private void FixedUpdate()
    {
        if (!spent) body.MovePosition(body.position + Vector2.up * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spent || (targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;
        // Destroy is deferred, so guard against extra callbacks this frame.
        spent = true;
        target.TakeDamage(damage);
        Destroy(gameObject);
    }
}
