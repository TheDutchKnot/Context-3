using System.Collections;
using UnityEngine;

public class DialogueFollow : MonoBehaviour
{
    [SerializeField] private Transform playerHead;
    [SerializeField] private float distanceFromPlayer = 1.0f;
    [SerializeField] private float time = 0.3f;
    [SerializeField] private float yPos = 1.2f;

    [SerializeField] public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;

    public void OnEnable()
    {
       // StartCoroutine(Timer());
    }

    public void FollowPlayer()
    {
        //moves the text to where the player looks
        Vector3 targetPosition = playerHead.position + playerHead.forward * distanceFromPlayer;

        //makes it so camera doesnt move up or down
        targetPosition.y = yPos;

        //making it smoooooth
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

    

        //rotates the text to face the player
        Vector3 directionToCamera = playerHead.position - transform.position;
        
        //makes it so camera doesnt rotate up or down
       directionToCamera.y = 0; 
        
        transform.rotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);
    }
    private void Update()
    {
            FollowPlayer();
    }

    //timer to make the object turn off, can maybe use later to sync with audioclip/voice acting
    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(time);
    }
}
