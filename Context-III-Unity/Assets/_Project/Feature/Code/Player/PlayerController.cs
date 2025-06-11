using System;
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
    [Header("Player Camera")] // get dir
    public Transform playerCamera;

    [Header("left-right hand")] // get pos
    public Transform leftHand;

    public Transform rightHand;
    public float knockbackSpeed = 8f;
    public float knockbackDuration = 0.4f;
    private float knockbackTimer = 0f;
    private Vector3 knockbackDirection = Vector3.zero;
    [Header("left-right Interactor")] public NearFarInteractor leftInteractor;
    public NearFarInteractor rightInteractor;

    [Header("zeroG")] public ZeroGravity zeroGravity;

    public float zeroGravityFlyForce;

    private float _flyForce;
    // cur player state
    public PlayerActionState currentState = PlayerActionState.Grounded;

    // state switch
    private PlayerActionState _previousState = PlayerActionState.Grounded;

    // cur speed
    private Vector3 velocity = Vector3.zero;

    public DynamicMoveProvider dynamicMoveProvider;


    void stateChange(GameState newState)
    {
        if (newState.Equals(GameState.ZERO_GRAVITY))
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
        if (gameManager.CurrentGameState == GameState.ZERO_GRAVITY && currentState != PlayerActionState.Grounded)
        {
            _flyForce = zeroGravityFlyForce;
            //1. flying
            if (currentState == PlayerActionState.Flying)
            {
                velocity = playerCamera.forward * _flyForce;
            }

            if (currentState == PlayerActionState.Climbing)
            {
                TurnOffZeroGravity();
            }

            // 2. climbing
            if (_previousState == PlayerActionState.Climbing && AreHandsFreeOfClimb())
            {
                if (gameManager.CurrentGameState == GameState.ZERO_GRAVITY)
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
            TurnOffZeroGravity();
        }

        if (currentState != PlayerActionState.Climbing)
        {
            // use CharacterController move player
            characterController.Move(velocity * Time.deltaTime);
        }
        
        // save cur state,for next switch
        _previousState = currentState;
        
        
        if (knockbackTimer > 0f)
        {
            // 持续向 knockbackDirection 的方向移动
            characterController.Move(knockbackDirection * Time.deltaTime);
            knockbackTimer -= Time.deltaTime;
        }
    }
    

    /// <summary>
    /// Update the player's state based on
    /// 1. whether they are grounded
    /// 2. whether their hands are holding the climbing object
    /// </summary>
    private void UpdatePlayerState()
    { 
        // If the character controller detects that it is on the ground, the state is grounded
        if (gameManager.CurrentGameState==GameState.PLAYING)
        {
            currentState = IsAnyHandInteracting() ? PlayerActionState.Climbing : PlayerActionState.Grounded;
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
        dynamicMoveProvider.leftHandMoveInput.inputAction.Enable();
        
    }
    
    private void TurnOnZeroGravity()
    {
        dynamicMoveProvider.leftHandMoveInput.inputAction.Disable();
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
        if (other.gameObject.name.Contains("GameState"))
        {
            // HandelTrigger(other.gameObject.name);
            string[] parts = other.gameObject.name.Split('-');
            string prefix = parts[0];
            string triggerInfo = parts[1];
            if (prefix.Equals("GameState") && System.Enum.TryParse<GameState>(triggerInfo, out GameState newState))
            {
                gameManager.SetGameState(newState);
            } 
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyAttack"))
        {
            Debug.Log("被击中！");
            // 计算击退方向（忽略垂直分量）
            Vector3 dir = -transform.position.normalized;
            dir.y = 0f;
            knockbackDirection = dir * knockbackSpeed;
            // 设置击退计时器
            knockbackTimer = knockbackDuration;
        }
        
        
        
    }

    private void OnTriggerExit(Collider other)
    {
        gameManager.SetGameState(GameState.PLAYING);
    }


    private void HandelTrigger(string triggerName)
    {
        string[] parts = triggerName.Split('_');
        if (parts.Length < 2)
        {
            Debug.Log($"Trigger wrong trigger name: {triggerName}");
            return;
        }

        string prefix = parts[0]; // "GameState"
        string triggerInfo = parts[1]; // "Playing-ZeroGravity"

        if (prefix.Equals("GameState"))
        {
            string[] states = triggerInfo.Split('-');
            if (states.Length < 2)
            {
                Debug.Log($"GameState wrong trigger name: {triggerName}");
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
                    Debug.LogWarning($"No Match State!!! cur state:{gameManager.CurrentGameState}");
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