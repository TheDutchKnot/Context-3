using UnityEngine;

public class BossHitPartCollider : MonoBehaviour
{
    public FSM fsm;
    public HitPart partType;

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (!other.CompareTag("PlayerAttack")) return;
    //
    //     // sign hit part
    //     fsm.parameter.lastHitPart = partType;
    //     // trigger FSM -> HitState
    //     fsm.parameter.getHit = true;
    // }
}
