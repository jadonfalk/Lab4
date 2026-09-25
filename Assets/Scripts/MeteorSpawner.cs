using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MeteorSpawner : MonoBehaviour, IMeteorSpawner
{
    [SerializeField] private Meteor meteorPrefab;
    [SerializeField] private Meteor bigMeteorPrefab;
    [SerializeField] private Vector2 horizontalRange = new Vector2(-8f, 8f);
    [SerializeField] private float spawnY = 7.5f;
    [SerializeField, Min(0f)] private float initialDelay = 1f;
    [SerializeField, Min(0.1f)] private float spawnInterval = 2f;
    public event Action<bool> MeteorDestroyed;
    public event Action<int> BigMeteorCountChanged;
    private readonly HashSet<Meteor> activeMeteors = new HashSet<Meteor>();
    private int bigCount;
    private Transform playerTarget;
    private bool spawning;
    private float nextSpawnTime;

    public void BeginSpawning(Transform target)
    {
        playerTarget = target;
        if (spawning) return;
        spawning = true;
        nextSpawnTime = Time.time + initialDelay;
    }

    public void StopSpawning() => spawning = false;

    private void Update()
    {
        if (!spawning || Time.time < nextSpawnTime) return;
        Spawn(meteorPrefab);
        nextSpawnTime = Time.time + spawnInterval;
    }

    public void SpawnBigMeteor()
    {
        if (spawning) Spawn(bigMeteorPrefab);
    }

    private void Spawn(Meteor prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Assign both meteor prefabs on MeteorSpawner.", this);
            return;
        }
        Vector3 position = new Vector3(UnityEngine.Random.Range(horizontalRange.x, horizontalRange.y), spawnY, 0f);
        Meteor meteor = Instantiate(prefab, position, Quaternion.identity);
        meteor.Initialize(playerTarget);
        activeMeteors.Add(meteor);
        meteor.Removed += OnMeteorRemoved;
        if (meteor.IsBig)
        {
            bigCount++;
            BigMeteorCountChanged?.Invoke(bigCount);
        }
    }

    private void OnMeteorRemoved(Meteor meteor, bool destroyedByDamage)
    {
        if (!activeMeteors.Remove(meteor)) return;
        meteor.Removed -= OnMeteorRemoved;
        if (meteor.IsBig)
        {
            bigCount--;
            BigMeteorCountChanged?.Invoke(bigCount);
        }
        if (destroyedByDamage) MeteorDestroyed?.Invoke(meteor.IsBig);
    }

    private void OnDestroy()
    {
        foreach (Meteor meteor in activeMeteors)
            if (meteor != null) meteor.Removed -= OnMeteorRemoved;
        activeMeteors.Clear();
    }
}
