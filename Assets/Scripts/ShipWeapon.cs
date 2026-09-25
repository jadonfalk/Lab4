using UnityEngine;

public sealed class ShipWeapon : MonoBehaviour
{
    [SerializeField] private Laser laserPrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField, Min(0.01f)] private float cooldown = 1f;
    private IFireInput input;
    private float nextShotTime;

    public void Initialize(IFireInput source) => input = source;

    private void Update()
    {
        if (input == null || !input.FirePressed || Time.time < nextShotTime) return;
        if (laserPrefab == null || muzzle == null) return;
        Instantiate(laserPrefab, muzzle.position, Quaternion.identity);
        nextShotTime = Time.time + cooldown;
    }
}
