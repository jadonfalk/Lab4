using UnityEngine;
using UnityEngine.InputSystem;

// Actions are created here: no generated class or PlayerInput component needed.
public sealed class ShooterInput : MonoBehaviour, IMoveInput, IFireInput, IRestartInput
{
    private InputAction move;
    private InputAction fire;
    private InputAction restart;

    public Vector2 Move => move.ReadValue<Vector2>();
    public bool FirePressed => fire.WasPressedThisFrame();
    public bool RestartPressed => restart.WasPressedThisFrame();

    private void Awake()
    {
        move = new InputAction("Move", InputActionType.Value);
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        move.AddBinding("<Gamepad>/leftStick");
        fire = new InputAction("Fire", InputActionType.Button, "<Keyboard>/space");
        fire.AddBinding("<Gamepad>/buttonSouth");
        restart = new InputAction("Restart", InputActionType.Button, "<Keyboard>/r");
        restart.AddBinding("<Gamepad>/start");
    }

    private void OnEnable()
    {
        move.Enable();
        fire.Enable();
        restart.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        fire.Disable();
        restart.Disable();
    }

    private void OnDestroy()
    {
        move.Dispose();
        fire.Dispose();
        restart.Dispose();
    }
}
