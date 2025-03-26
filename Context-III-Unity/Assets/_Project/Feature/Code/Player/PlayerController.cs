using UnityEngine;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class PlayerController : MonoBehaviour
{
    // def player states
    public enum PlayerActionState
    {
        Grounded,
        Flying,
        Climbing
    }

    public GameManager gameManager;
    [Header("Player Controller")] public CharacterController characterController;
    [Header("Ground Checker")]
    public GroundChecker groundChecker;
    [Header("Player Camera")] // get dir
    public Transform playerCamera;

    [Header("left-right hand")] // get pos
    public Transform leftHand;

    public Transform rightHand;

    [Header("left-right Interactor")] public NearFarInteractor leftInteractor;
    public NearFarInteractor rightInteractor;

    [Header("zeroG")] public ZeroGravity zeroGravity;

    // cur player state
    public PlayerActionState currentState = PlayerActionState.Grounded;

    // state switch
    private PlayerActionState _previousState = PlayerActionState.Grounded;

    // cur speed
    private Vector3 velocity = Vector3.zero;

    public DynamicMoveProvider dynamicMoveProvider;


    void stateChange(GameState newState)
    {
        if (newState.Equals(GameState.ZeroGravity))
        {
            dynamicMoveProvider.useGravity = false;
            dynamicMoveProvider.enableFly = true;
        }
        else
        {
            dynamicMoveProvider.useGravity = true;
            dynamicMoveProvider.enableFly = false;
        }
    }

    private void Start()
    {
        gameManager.StateChange += stateChange;
    }

    private void Update()
    {
        // update player state
        UpdatePlayerState();

        // check game state = zero G
        if (gameManager.CurrentGameState == GameState.ZeroGravity && currentState != PlayerActionState.Grounded)
        {
            //1. flying
            if (currentState == PlayerActionState.Flying)
            {
                
                velocity = playerCamera.forward * 0.2f;
            }

            if (currentState == PlayerActionState.Climbing)
            {
                TurnOffZeroGravity();
            }

            // 2. climbing
            if (_previousState == PlayerActionState.Climbing && AreHandsFreeOfClimb())
            {
                if (gameManager.CurrentGameState == GameState.ZeroGravity)
                {
                    TurnOnZeroGravity();
                }
                if (!zeroGravity.IsClimbingPushActive)
                {
                    // climbing-push logic
                    zeroGravity.StartClimbingPush(leftHand, rightHand,  playerCamera);
                }
            }

            // climbing push, set soeed
            if (zeroGravity.IsClimbingPushActive)
            {
                velocity = zeroGravity.GetCurrentVelocity();
            }
        }
        else
        {
            //stop moving
            velocity = Vector3.zero;
        }

        if (currentState != PlayerActionState.Climbing)
        {
            // use CharacterController move player
            characterController.Move(velocity * Time.deltaTime);
        }
        
        // save cur state,for next switch
        _previousState = currentState;
    }
    

    /// <summary>
    /// Update the player's state based on
    /// 1. whether they are grounded
    /// 2. whether their hands are holding the climbing object
    /// </summary>
    private void UpdatePlayerState()
    {
        // If the character controller detects that it is on the ground, the state is grounded
        if (gameManager.CurrentGameState==GameState.Playing)
        {
            // Debug.Log("on ground!");
            currentState = PlayerActionState.Grounded;
        }
        else
        {
            // If either hand is holding a climbing object, the player is considered to be in a climbing state
            if (IsAnyHandInteracting())
            {
                currentState = PlayerActionState.Climbing;
            }
            else
            {
                // switch to flying
                currentState = PlayerActionState.Flying;
            }
        }

        if (!_previousState.Equals(currentState))
        {
            Debug.Log("Player switch state: " + _previousState + " -> " + currentState);
        }
    }

    private void TurnOffZeroGravity()
    {
        dynamicMoveProvider.useGravity = true;
        dynamicMoveProvider.enableFly = false;
    }
    
    private void TurnOnZeroGravity()
    {
        dynamicMoveProvider.useGravity = false;
        dynamicMoveProvider.enableFly = true;
    }
    

    /// <summary>
    /// Determine whether at least one of the left and right hands is holding the climbing object
    /// </summary>
    private bool IsAnyHandInteracting()
    {
        return !AreHandsFreeOfClimb();
    }

    private void OnTriggerEnter(Collider other)
    {
        HandelTrigger(other.gameObject.name);
    }

    
    
    private void HandelTrigger(string triggerName)
    {
        string[] parts = triggerName.Split('_');
        if (parts.Length < 2)
        {
            Debug.LogWarning($"Trigger wrong trigger name: {triggerName}");
            return;
        }

        string prefix = parts[0]; // "GameState"
        string triggerInfo = parts[1]; // "Playing-ZeroGravity"

        if (prefix.Equals("GameState"))
        {
            string[] states = triggerInfo.Split('-');
            if (states.Length < 2)
            {
                Debug.LogWarning($"GameState wrong trigger name: {triggerName}");
                return;
            }
            string oldStateStr = states[0]; // "Playing"
            string newStateStr = states[1]; // "ZeroGravity"
            if (System.Enum.TryParse<GameState>(oldStateStr, out GameState oldState) &&
                System.Enum.TryParse<GameState>(newStateStr, out GameState newState))
            {
                //Switch cur state
                if (gameManager.CurrentGameState == oldState)
                {
                    gameManager.SetGameState(newState);
                }
                else if (gameManager.CurrentGameState == newState)
                {
                    gameManager.SetGameState(oldState);
                }
                else
                {
                    Debug.Log($"No Match State!!! cur state:{gameManager.CurrentGameState}");
                }
            }
            else
            {
                Debug.LogWarning($"cant convert string to Game state. String: {oldStateStr}  {newStateStr}");
            }
        }
    }

    /// <summary>
    /// Checks if neither the left or right hand is holding an climb object 
    /// </summary>
    private bool AreHandsFreeOfClimb()
    {
        bool leftHolding = leftInteractor.interactablesSelected.Any(interactable =>
            interactable.transform.gameObject.name == "Handles (Climbable)");
        bool rightHolding = rightInteractor.interactablesSelected.Any(interactable =>
            interactable.transform.gameObject.name == "Handles (Climbable)");
        return (!leftHolding && !rightHolding);
    }
}