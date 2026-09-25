using UnityEngine;
using UnityEngine.SceneManagement;

// The composition root wires scene components together and applies game rules.
public sealed class GameManager : MonoBehaviour
{
    [SerializeField] private Health playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [Tooltip("A component implementing IMoveInput, IFireInput and IRestartInput.")]
    [SerializeField] private MonoBehaviour inputProvider;
    [Tooltip("A component implementing IMeteorSpawner.")]
    [SerializeField] private MonoBehaviour spawnerProvider;
    [Tooltip("A component implementing ICameraFeedback.")]
    [SerializeField] private ShooterCamera cameraProvider;
    [SerializeField, Min(1)] private int killsPerBigMeteor = 5;

    public bool IsGameOver { get; private set; }
    public int RegularMeteorKills { get; private set; }
    private int killsTowardBigMeteor;
    private IRestartInput restartInput;
    private IMeteorSpawner spawner;
    private ICameraFeedback cameraFeedback;
    private Health player;
    private bool initialized;

    private void Start()
    {
        Debug.Log("GameManager Start is running.", this);

        IMoveInput movementInput = inputProvider as IMoveInput;
        IFireInput fireInput = inputProvider as IFireInput;
        restartInput = inputProvider as IRestartInput;
        spawner = spawnerProvider as IMeteorSpawner;
        cameraFeedback = cameraProvider as ICameraFeedback;

        Debug.Log(
            $"Movement input: {movementInput != null}\n" +
            $"Fire input: {fireInput != null}\n" +
            $"Restart input: {restartInput != null}\n" +
            $"Spawner: {spawner != null}\n" +
            $"Camera feedback: {cameraFeedback != null}\n" +
            $"Player prefab: {playerPrefab != null}\n" +
            $"ShipMovement: {playerPrefab != null && playerPrefab.GetComponent<ShipMovement>() != null}\n" +
            $"ShipWeapon: {playerPrefab != null && playerPrefab.GetComponent<ShipWeapon>() != null}", this
        );

        if (movementInput == null || fireInput == null || restartInput == null ||
            spawner == null || cameraFeedback == null || playerPrefab == null ||
            playerPrefab.GetComponent<ShipMovement>() == null ||
            playerPrefab.GetComponent<ShipWeapon>() == null)
        {
            Debug.LogError("GameManager setup is incomplete.", this);
            enabled = false;
            return;
        }

        Vector3 spawn = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;

        Debug.Log("About to spawn player.", this);

        player = Instantiate(playerPrefab, spawn, Quaternion.identity);

        Debug.Log("Player created: " + player.name, player);

        player.GetComponent<ShipMovement>().Initialize(movementInput);
        player.GetComponent<ShipWeapon>().Initialize(fireInput);
        player.Died += OnPlayerDied;
        spawner.MeteorDestroyed += OnMeteorDestroyed;
        spawner.BigMeteorCountChanged += OnBigMeteorCountChanged;
        cameraFeedback.Track(player.transform);
        cameraFeedback.SetBigMeteorPresent(false);
        initialized = true;
        spawner.BeginSpawning(player.transform);
    }

    private void Update()
    {
        if (initialized && IsGameOver && restartInput.RestartPressed)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnMeteorDestroyed(bool isBig)
    {
        cameraFeedback.Shake(isBig);
        if (IsGameOver || isBig) return;
        RegularMeteorKills++;
        killsTowardBigMeteor++;
        if (killsTowardBigMeteor >= Mathf.Max(1, killsPerBigMeteor))
        {
            killsTowardBigMeteor = 0;
            spawner.SpawnBigMeteor();
        }
    }

    private void OnBigMeteorCountChanged(int count) => cameraFeedback.SetBigMeteorPresent(count > 0);

    private void OnPlayerDied()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        spawner.StopSpawning();
        cameraFeedback.Track(null);
        cameraFeedback.Shake(true);
        player.Died -= OnPlayerDied;
        // Stop actions immediately; Unity destroys the object at the end of the frame.
        player.gameObject.SetActive(false);
        Destroy(player.gameObject);
        Debug.Log("Game over. Press R (or gamepad Start) to restart.");
    }

    private void OnDestroy()
    {
        if (!initialized) return;
        if (player != null) player.Died -= OnPlayerDied;
        spawner.MeteorDestroyed -= OnMeteorDestroyed;
        spawner.BigMeteorCountChanged -= OnBigMeteorCountChanged;
        spawner.StopSpawning();
    }
}
