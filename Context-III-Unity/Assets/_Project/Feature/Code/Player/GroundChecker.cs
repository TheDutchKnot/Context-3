using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer; // 在 Inspector 中选择 Ground 层

    private int groundContactCount = 0;
    
    public bool IsGrounded { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        // 检查 other.gameObject 的层是否包含在 groundLayer 中
        if ((groundLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            
            IsGrounded = true;
            Debug.Log("进入地面触发器：" + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((groundLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            
            IsGrounded = false;
            Debug.Log("离开地面触发器：" + other.gameObject.name);
        }
    }
}
