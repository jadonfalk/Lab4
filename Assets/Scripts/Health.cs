using System;
using UnityEngine;

// Health reports death; its owner decides what death means for gameplay.
public sealed class Health : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1)] private int maximumHealth = 1;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public event Action Died;

    private void Awake() => CurrentHealth = Mathf.Max(1, maximumHealth);

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        if (IsDead) Died?.Invoke();
    }
}
