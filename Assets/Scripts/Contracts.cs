using System;
using UnityEngine;

// Small interfaces keep consumers independent of keyboards, cameras and prefabs.
public interface IMoveInput { Vector2 Move { get; } }
public interface IFireInput { bool FirePressed { get; } }
public interface IRestartInput { bool RestartPressed { get; } }
public interface IDamageable { void TakeDamage(int amount); }

public interface IMeteorSpawner
{
    event Action<bool> MeteorDestroyed; // Argument: was this a big meteor?
    event Action<int> BigMeteorCountChanged;
    void BeginSpawning(Transform target);
    void StopSpawning();
    void SpawnBigMeteor();
}

public interface ICameraFeedback
{
    void Track(Transform target);
    void SetBigMeteorPresent(bool present);
    void Shake(bool strong);
}
