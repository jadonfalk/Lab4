using Unity.Cinemachine;
using UnityEngine;

// Only this adapter knows about Cinemachine. Gameplay uses ICameraFeedback.
[RequireComponent(typeof(CinemachineCamera), typeof(CinemachineBasicMultiChannelPerlin))]
public sealed class ShooterCamera : MonoBehaviour, ICameraFeedback
{
    [SerializeField, Min(0.1f)] private float normalSize = 7f;
    [SerializeField, Min(0.1f)] private float bigMeteorSize = 9f;
    [SerializeField, Min(0.01f)] private float zoomSmoothTime = 0.5f;
    [SerializeField, Min(0f)] private float normalShake = 0.5f;
    [SerializeField, Min(0f)] private float strongShake = 1f;
    [SerializeField, Min(0.01f)] private float shakeDuration = 0.25f;
    private CinemachineCamera trackingCamera;
    private CinemachineBasicMultiChannelPerlin noise;
    private float targetSize;
    private float zoomVelocity;
    private float shakeRemaining;
    private float shakeStrength;

    private void Awake()
    {
        trackingCamera = GetComponent<CinemachineCamera>();
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
        targetSize = normalSize;
        trackingCamera.Lens.OrthographicSize = normalSize;
        noise.AmplitudeGain = 0f;
    }

    public void Track(Transform target) => trackingCamera.Follow = target;
    public void SetBigMeteorPresent(bool present) => targetSize = present ? bigMeteorSize : normalSize;

    public void Shake(bool strong)
    {
        // A weaker hit cannot cancel an existing stronger shake.
        shakeStrength = Mathf.Max(shakeStrength, strong ? strongShake : normalShake);
        shakeRemaining = shakeDuration;
    }

    private void Update()
    {
        trackingCamera.Lens.OrthographicSize = Mathf.SmoothDamp(
            trackingCamera.Lens.OrthographicSize, targetSize, ref zoomVelocity, zoomSmoothTime);
        if (shakeRemaining > 0f)
        {
            noise.AmplitudeGain = shakeStrength * (shakeRemaining / shakeDuration);
            shakeRemaining = Mathf.Max(0f, shakeRemaining - Time.deltaTime);
        }
        else
        {
            noise.AmplitudeGain = 0f;
            shakeStrength = 0f;
        }
    }

    private void OnDisable()
    {
        if (noise != null) noise.AmplitudeGain = 0f;
        shakeRemaining = 0f;
        shakeStrength = 0f;
    }
}
