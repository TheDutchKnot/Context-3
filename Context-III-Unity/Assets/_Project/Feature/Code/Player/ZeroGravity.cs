using System.Collections;
using UnityEngine;

public class ZeroGravity : MonoBehaviour
{
    private Vector3 currentVelocity = Vector3.zero;
    public bool IsClimbingPushActive { get; private set; } = false;

    /// <summary>
    /// Gets the current push-off velocity calculated during zero gravity (after releasing from climbing)
    /// </summary>
    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }

    /// <summary>
    /// External interface to start the climbing push-off sampling coroutine
    /// </summary>
    /// <param name="leftHand">Left hand Transform</param>
    /// <param name="rightHand">Right hand Transform</param>
    /// <param name="playerTransform">Player's Transform (used to calculate push-off direction)</param>
    /// <param name="cameraTransform">Camera's Transform (used to add a slight offset)</param>
    public void StartClimbingPush(Transform leftHand, Transform rightHand, Transform playerTransform, Transform cameraTransform)
    {
        StartCoroutine(ClimbingPushCoroutine(leftHand, rightHand, playerTransform, cameraTransform));
    }

    /// <summary>
    /// Climbing push-off coroutine:
    /// 1. Record the initial positions of both hands;
    /// 2. Sample hand movement over 0.3 seconds;
    /// 3. Calculate the displacement for both hands and choose the hand with the greater movement;
    /// 4. Calculate the push-off direction: from the selected hand's final position towards the player's center,
    ///    with a slight offset in the direction the camera is facing;
    /// 5. Set the player's velocity and direction.
    /// </summary>
    private IEnumerator ClimbingPushCoroutine(Transform leftHand, Transform rightHand, Transform playerTransform, Transform cameraTransform)
    {
        IsClimbingPushActive = true;

        // Record hands initial positions 
        Vector3 startLeftPos = leftHand.position;
        Vector3 startRightPos = rightHand.position;

        // Sampling duration
        float sampleTime = 0.3f;
        yield return new WaitForSeconds(sampleTime);

        // Record both hands  positions
        Vector3 endLeftPos = leftHand.position;
        Vector3 endRightPos = rightHand.position;

        // Calculate displacement vectors and distances
        Vector3 leftDisplacement = endLeftPos - startLeftPos;
        Vector3 rightDisplacement = endRightPos - startRightPos;

        float leftDistance = leftDisplacement.magnitude;
        float rightDistance = rightDisplacement.magnitude;

        // Choose hand with the greater displacement
        Vector3 chosenHandFinalPos;
        float chosenDistance;
        if (leftDistance >= rightDistance)
        {
            chosenHandFinalPos = endLeftPos;
            chosenDistance = leftDistance;
        }
        else
        {
            chosenHandFinalPos = endRightPos;
            chosenDistance = rightDistance;
        }

        // Calculate the push-off direction
        Vector3 pushDirection = (playerTransform.position - chosenHandFinalPos).normalized;

        // Add a slight offset in the direction the camera is facing
        Vector3 cameraOffset = cameraTransform.forward * 0.1f;
        pushDirection = (pushDirection + cameraOffset).normalized;

        // Set the final push-off velocity to be the chosen displacement multiplied by the direction
        currentVelocity = pushDirection * chosenDistance;
        
        yield return null;

        // Mark push-off sampling complete
        IsClimbingPushActive = false;
    }
}
