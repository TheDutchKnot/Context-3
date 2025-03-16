using UnityEngine;

public class DialogueFollow : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] Vector3 offsetFromCamera;
    [SerializeField] float turnBufferInDegrees = 30f;
    [SerializeField] float turnSpeed = 0.03f;
    [SerializeField] bool lockPitch = false;

    private void Update()
    {
        transform.localPosition = cameraTransform.localPosition + offsetFromCamera;
        Vector3 cameraEuler = cameraTransform.rotation.eulerAngles;

        cameraEuler.z = 0f;

        if(lockPitch)
        {
            cameraEuler.x = 0f;
        }

        Quaternion targetQuat = Quaternion.Euler(cameraEuler);

        float angle = Quaternion.Angle(transform.rotation, targetQuat);

        if(angle > turnBufferInDegrees)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetQuat, turnSpeed);
        }
    }
}
