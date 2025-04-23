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
    public void StartClimbingPush(Transform leftHand, Transform rightHand,  Transform cameraTransform)
    {
        StartCoroutine(ClimbingPushCoroutine(leftHand, rightHand, cameraTransform));
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
    private IEnumerator ClimbingPushCoroutine(Transform leftHand, Transform rightHand, Transform cameraTransform)
    {
        IsClimbingPushActive = true;

        // Record the initial positions of the left and right hands
        Vector3 startLeftPos = leftHand.position;
        Vector3 startRightPos = rightHand.position;

        // sample time
        float sampleTime = 0.1f;
        yield return new WaitForSeconds(sampleTime);

        // Record the hand position at the end of sampling
        Vector3 endLeftPos = leftHand.position;
        Vector3 endRightPos = rightHand.position;

        // Calculate the displacement and distance of the hand
        Vector3 leftDisplacement = endLeftPos - startLeftPos;
        Vector3 rightDisplacement = endRightPos - startRightPos;
        float leftDistance = leftDisplacement.magnitude;
        float rightDistance = rightDisplacement.magnitude;

        // Choose the hand with the larger displacement
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
        Vector3 pushDirection = (cameraTransform.position - chosenHandFinalPos).normalized;

        // get speed
        currentVelocity = pushDirection * (chosenDistance*20);

        // Set a push-off decay duration based on the distance the hand moves
        float pushDuration = chosenDistance * 20f;
        float elapsed = 0f;
        Vector3 initialVelocity = currentVelocity;
        
        while (elapsed < pushDuration)
        {
            elapsed += Time.deltaTime;
            // speed -> 0
            currentVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, elapsed / pushDuration);
            yield return null;
        }
        currentVelocity = Vector3.zero;
        
        IsClimbingPushActive = false;
    }
}
