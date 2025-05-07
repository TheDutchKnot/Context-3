using System.Collections;
using UnityEngine;

public class DialogueFollow : MonoBehaviour
{
    [SerializeField] private Transform playerHead;
    [SerializeField] private float distanceFromPlayer = 1.0f;
    [SerializeField] private float time = 0.3f;
    [SerializeField] private float yPos = 1.2f;

    [SerializeField] public float smoothTime = 0.2f;
    private Vector3 velocity = Vector3.zero;

    public void FollowPlayer()
    {
        //moves the text to where the player looks, flatforward.y = 0f & targetPosition.y = yPos makes it so doesnt move up and down
        Vector3 flatForward = playerHead.forward;
        flatForward.y = 0f;
        flatForward.Normalize();
        Vector3 targetPosition = playerHead.position + flatForward * distanceFromPlayer;
        targetPosition.y = yPos;

        //making moving text smoooooth
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        //rotates the text to face the player & makes it so text doesnt rotate up or down
        Vector3 directionToCamera = playerHead.position - transform.position;
        directionToCamera.y = 0; 
        
        transform.rotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);
    }
    private void Update()
    {
        FollowPlayer();
    }
}
