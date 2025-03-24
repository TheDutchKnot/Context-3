using UnityEngine;

using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PlayerController : MonoBehaviour
{
    // def player states
    public enum PlayerActionState
    {
        Grounded,
        Flying,
        Climbing
    }

    [Header("Player Controller")]
    public CharacterController characterController;

    [Header("Player Camera")] // get dir
    public Transform playerCamera;

    [Header("left-right hand")] // get pos
    public Transform leftHand;
    public Transform rightHand;

    [Header("left-right Interactor")]
    public NearFarInteractor leftInteractor;
    public NearFarInteractor rightInteractor;

    [Header("zeroG")]
    public ZeroGravity zeroGravity;

    // cur player state
    public PlayerActionState currentState = PlayerActionState.Grounded;
    // state switch
    private PlayerActionState _previousState = PlayerActionState.Grounded;

    // cur speed
    private Vector3 velocity = Vector3.zero;

    private void Update()
    {
        // update player state
        UpdatePlayerState();

        // check game state = zero G
        if (GameManager.Instance.CurrentGameState == GameState.ZeroGravity && currentState != PlayerActionState.Grounded)
        {
            //1. flying
            if (currentState == PlayerActionState.Flying)
            {
                velocity = playerCamera.forward * 0.5f;
            }

            // 2. climbing
            if (_previousState == PlayerActionState.Climbing && AreHandsFreeOfClimb())
            {
                if (!zeroGravity.IsClimbingPushActive)
                {
                    // climbing-push logic
                    zeroGravity.StartClimbingPush(leftHand, rightHand, transform, playerCamera);
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

        // use CharacterController move player
        characterController.Move(velocity * Time.deltaTime);

        // save cur state,,for next switch
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
        if (characterController.isGrounded)
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
    }

    /// <summary>
    /// Determine whether at least one of the left and right hands is holding the climbing object
    /// </summary>
    private bool IsAnyHandInteracting()
    {
        return !AreHandsFreeOfClimb();
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