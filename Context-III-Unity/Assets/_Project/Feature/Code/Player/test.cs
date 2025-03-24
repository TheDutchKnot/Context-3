using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Test : MonoBehaviour
{
    [Header("1) Reference the left-hand Grip action (from XR Interaction Toolkit or a custom Input Action)")]
    public InputActionReference leftG;
    
    public bool isFloating;
    public Transform leftHand;
    public Transform rightHand;

    [Header("3) Character Controller")]
    public CharacterController characterController;

    [Header("Movement Speed (in meters/second)")]
    public float moveSpeed = 2f;

    // Current movement velocity
    private Vector3 currentVelocity = Vector3.zero;

    // Flag to indicate if the Grip button is pressed
    private bool btn;
    
    [Header("Force Sampling Time")]
    public float captureTime = 0.5f;

    [Header("Force Multiplier")]
    public float forceMultiplier = 10f;

    [Header("Damping Factor (simulating air resistance; set to 0 if not needed)")]
    public float dampingFactor = 0.0f;

    // Flag to indicate whether the roaming process is active to avoid repeated triggers
    private bool isRoaming = false;

    [SerializeField]
    NearFarInteractor left_interactor;

    [SerializeField]
    NearFarInteractor right_interactor;
    
    
    private void Start()
    {
        leftG.action.started += Gpressed;
        leftG.action.canceled += Greleased;
        btn = false;
    }

    /// <summary>
    /// Called when the Grip is pressed.
    /// </summary>
    /// <param name="context"></param>
    private void Gpressed(InputAction.CallbackContext context)
    {
        // if in zero gravity environment
        if (GameManager.Instance.CurrentGameState == GameState.ZeroGravity)
        {
            Debug.Log("Left Grip Pressed. Left Controller Position: " + leftHand.position);
            
            isFloating = true;
            btn = true;
        }
    }

    /// <summary>
    /// Called when the Grip is released.
    /// </summary>
    private void Greleased(InputAction.CallbackContext context)
    {
        // ...
    }

    private void Update()
    {
        // Execute zero-gravity movement logic 
        if (btn)
        {
            ZeroGravityMove();
        }

        // Apply movement logic here
        if (dampingFactor > 0f)
        {
            // Simulate air resistance
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, dampingFactor * Time.deltaTime);
        }

        // Use the CharacterController to move the player
        characterController.Move(currentVelocity * Time.deltaTime);
    }

    /// <summary>
    /// Get all objects currently being grabbed by specified hand.
    /// </summary>
    private List<GameObject> GetCurInteractionGameObject(int hand)
    {
        var leftHandselectedObj = left_interactor.interactablesSelected;
        var rightHandselectedObj = right_interactor.interactablesSelected; 
        List<GameObject> leftObj = new List<GameObject>();
        List<GameObject> rightObj = new List<GameObject>();

        if (leftHandselectedObj.Count > 0)
        {
            foreach (var interactable in leftHandselectedObj)
            {
                var obj = interactable.transform.gameObject;
                leftObj.Add(obj);
                Debug.Log($"Left hand is grabbing: {obj.name}");
            }
        }
        
        if (rightHandselectedObj.Count > 0)
        {
            foreach (var interactable in rightHandselectedObj)
            {
                var obj = interactable.transform.gameObject;
                rightObj.Add(obj);
                Debug.Log($"Right hand is grabbing: {obj.name}");
            }
        }

        return hand == 1 ? rightObj : leftObj;
    }

    /// <summary>
    /// Determine if both hands have released "Handles (Climbable)".
    /// </summary>
    private bool AreHandsFreeOfClimb()
    {
        bool left = GetCurInteractionGameObject(0)
            .Any(obj => obj.name == "Handles (Climbable)");
        bool right = GetCurInteractionGameObject(1)
            .Any(obj => obj.name == "Handles (Climbable)");
        Debug.Log("left: " + left + " Right: " + right + " Result: " + (!left && !right));
        
        return !left && !right;
    }

    /// <summary>
    /// Entry point for zero-gravity movement logic
    /// </summary>
    private void ZeroGravityMove()
    {
        if (AreHandsFreeOfClimb() && !isRoaming && isFloating)
        {
            StartCoroutine(RoamingCoroutine());
        }
    }
    
    /// <summary>
    /// Roaming coroutine: Samples the movement of the left hand over captureTime (0.5 seconds)
    /// to give the player an initial velocity.
    /// </summary>
    private IEnumerator RoamingCoroutine()
    {
        isRoaming = true;
        
        Vector3 startLeftPos = GetLeftHandPosition();
        
        yield return new WaitForSeconds(captureTime);
        
        Vector3 endLeftPos = GetLeftHandPosition();
       
        // Calculate the displacement vector for the left hand
        Vector3 moveLeft = endLeftPos - startLeftPos;

        // Calculate the average speed as displacement divided by time
        float averageSpeed = moveLeft.magnitude / captureTime;

        // Calculate the final speed by applying the force multiplier
        float finalSpeed = averageSpeed * forceMultiplier;

        // Determine the direction: from the left hand's final position toward the player's center,
        Vector3 direction = (transform.position - endLeftPos).normalized;
        Debug.Log("left final p: " + endLeftPos + "  T final p: " + transform.position);

        // Final velocity vector
        Vector3 finalVelocity = direction * finalSpeed;

        // Update the player's velocity
        currentVelocity = finalVelocity;
        
        isRoaming = false;
        isFloating = false;
    }
    
    /// <summary>
    /// Get the current world position of the left hand.
    /// </summary>
    private Vector3 GetLeftHandPosition()
    {
        return leftHand.position;
    }

    /// <summary>
    /// Get the current world position of the right hand.
    /// </summary>
    private Vector3 GetRightHandPosition()
    {
        return rightHand.position;
    }
}

// Player state: climbing, and the player has not reached the top or bottom.
//    In a zero-gravity space:
//        When the player releases both hands {
//                 The player enters a floating state,
//                 A coroutine is started to capture hand movement over 0.3 seconds,
//                 Setting the movement direction and speed.
//                 }
//         When the player collides with another object, exit the floating state.
