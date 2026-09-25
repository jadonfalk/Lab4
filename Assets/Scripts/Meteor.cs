using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(MeteorOrbitMovement))]
public sealed class Meteor : MonoBehaviour
{
    [SerializeField] private bool isBig;
    [SerializeField, Min(0f)] private float speed = 2f;
    [SerializeField] private float despawnY = -11f;
    [SerializeField] private LayerMask contactTargets;
    [Tooltip("Seconds before removal without kill credit. Set 0 for unlimited lifetime.")]
    [SerializeField, Min(0f)] private float lifetime = 45f;
    public bool IsBig => isBig;
    // True means destroyed by damage; false means collision/escape/external removal.
    public event Action<Meteor, bool> Removed;
    private Rigidbody2D body;
    private Health health;
    private MeteorOrbitMovement orbitMovement;
    private float age;
    private bool removed;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        orbitMovement = GetComponent<MeteorOrbitMovement>();
        health.Died += OnDied;
    }

    public void Initialize(Transform target) => orbitMovement.Initialize(target);

    private void FixedUpdate()
    {
        if (removed) return;
        age += Time.fixedDeltaTime;
        if (lifetime > 0f && age >= lifetime)
        {
            Remove(false);
            return;
        }
        // The movement component owns motion while the player exists.
        if (orbitMovement.HasTarget) return;

        // After player death, resume falling and ordinary offscreen cleanup.
        if (body.position.y < despawnY)
        {
            Remove(false);
            return;
        }
        body.MovePosition(body.position + Vector2.down * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (removed || (contactTargets.value & (1 << other.gameObject.layer)) == 0) return;
        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;
        target.TakeDamage(1);
        Remove(false);
    }

    private void OnDied() => Remove(true);

    private void Remove(bool destroyedByDamage)
    {
        if (removed) return;
        removed = true;
        orbitMovement.enabled = false;
        Removed?.Invoke(this, destroyedByDamage);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (health != null) health.Died -= OnDied;
        // Keeps the active-big count correct if another system removes this object.
        if (!removed)
        {
            removed = true;
            Removed?.Invoke(this, false);
        }
    }
}
