using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class IdleState : IState
{
    private FSM manager;
    private Parameter parameter;

    private float timer;
    public IdleState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("IdleClosedEMLoop_74");
    }

    public void OnUpdate()
    {
        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
            return;
        }
        manager.playerInSightRange =
            Physics.CheckSphere(manager.transform.position, manager.sightRange, manager.whatIsPlayer);
        manager.playerInAttackRange =
            Physics.CheckSphere(manager.transform.position, manager.attackRange, manager.whatIsPlayer);
        timer += Time.deltaTime;
        
        if (manager.playerInSightRange)
        {
            //boss fight start!!
            manager.TransitionState(StateType.React);
        }
    }

    public void OnExit()
    {
        timer = 0;
    }
}


public class ReactState : IState
{
    private FSM manager;
    private Parameter parameter;
    private AnimatorStateInfo info;
    public ReactState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("Inb_TentacleOutSingle_74");
    }

    public void OnUpdate()
    {
        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
            return;
        }
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= .95f)
        { 
            manager.TransitionState(StateType.Chase);
        }
    }
    public void OnExit()
    {

    }
}

public class ChaseState : IState
{
    private FSM manager;
    private Parameter parameter;
    private AnimatorStateInfo info;
    public ChaseState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        Debug.Log("Boos state: chase");
        parameter.animator.Play("IdleOpenEMLoop_74");  
    }

    public void OnUpdate()
    {   
        if (manager.IsDeath())
        {
            manager.parameter.body.layer = LayerMask.NameToLayer("Cuttable");
        }
        
        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
            return;
        }
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        manager.agent.SetDestination(manager.player.position);
        
        manager.playerInSightRange =
            Physics.CheckSphere(manager.transform.position, manager.sightRange, manager.whatIsPlayer);
        manager.playerInAttackRange =
            Physics.CheckSphere(manager.transform.position, manager.attackRange, manager.whatIsPlayer);

        float turnSpeed = manager.agent.angularSpeed;  
        manager.SmoothLookAt(manager.transform, manager.player.position, turnSpeed);
        
        // if (!manager.playerInSightRange)
        // {
        //     manager.TransitionState(StateType.Idle);
        // }
        if (manager.playerInAttackRange)
        {
            manager.TransitionState(StateType.AttackCon);
            
        }
    }
    


    public void OnExit()
    {

    }
}


public class AttackConState : IState
{
    private FSM manager;
    private Parameter parameter;
    private System.Random rand;
    private AnimatorStateInfo info;
    
    // Track last used to avoid repeats
    private StateType lastAttack = StateType.Chase;
    
    public AttackConState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
        rand = new System.Random();
    }
    public void OnEnter()
    {
        parameter.animator.Play("IdleOpenEMLoop_74");
        manager.playerInSightRange =
            Physics.CheckSphere(manager.transform.position, manager.sightRange, manager.whatIsPlayer);
        manager.playerInAttackRange =
            Physics.CheckSphere(manager.transform.position, manager.attackRange, manager.whatIsPlayer);
        
        // Determine distances
        float distance = Vector3.Distance(manager.transform.position, manager.player.position);
        // weighted list of possible attacks
        var options = new List<(StateType type, int weight)>();
        
        // Always allow the base eyeball attack if its cooldown is ready
        if (!parameter.availableAttack2 || !parameter.availableAttack2)
        {
            int w = 10;
            // when boss low on health, bias more toward the swarm attack
            if (parameter.health < 3) w += 30;
            options.Add((StateType.Attack1, w));
        }
        
        // Gravity field: prefer if far away, special available, and off cooldown
        if (parameter.availableAttack2)
        {
            int w = distance > manager.attackRange * 1.5f ? 60 : 10;
            options.Add((StateType.Attack2, w));
        }
        
        // Tentacle sweep: prefer mid‑range if available and off cooldown
        if (parameter.availableAttack3)
        {
            int w = (distance > manager.attackRange * 0.5f && distance <= manager.attackRange * 1.5f) ? 50 : 10;
            options.Add((StateType.Attack3, w));
        } 
        // Remove the last used attack to avoid repetition
        options.RemoveAll(o => o.type == lastAttack);
        
        // If no option left (all on cooldown or unavailable), default to Attack1
        StateType chosen = StateType.Attack2;
        if (options.Count > 0)
        {
            // Weighted random selection
            int totalWeight = options.Sum(o => o.weight);
            int pick = rand.Next(0, totalWeight);
            int cum = 0;
            foreach (var o in options)
            {
                cum += o.weight;
                if (pick < cum)
                {
                    chosen = o.type;
                    break;
                }
            }
        }
        else
        {
            Debug.Log("No Options");
        }
        lastAttack = chosen;
        
        // Transition
        manager.TransitionState(chosen);
    }

    public void OnUpdate()
    {

        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
            return;
        }

    }

    public void OnExit()
    {
        
    }
}






// eyeball attack
public class Attack1State : IState
{
    private FSM manager;
    private Parameter parameter;

    private float timer;
    private bool isRetreating = false;
    private Vector3 retreatTarget;
    private Vector3 enterPosition;
    private float savedStoppingDistance;

    // Fixed retreat distance
    private const float retreatDistance = 6f;
    // Arrival tolerance
    private const float arriveTolerance = 0.01f;
    // Attack duration
    private const float attackDuration = 10f;

    public Attack1State(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        parameter.animator.Play("IdleOpenEMLoop_74");
        Debug.Log("[Boss Info] Eyeball Attack");
        enterPosition = manager.transform.position;
        timer = 0f;
        StartRetreat();
    }

private void StartRetreat()
{
    savedStoppingDistance = manager.agent.stoppingDistance;
    manager.agent.stoppingDistance = 0f;

    Vector3 backDir = manager.transform.position - manager.player.position;
    backDir.y = 0f;
    backDir.Normalize();

    NavMeshHit groundHit;
    Vector3 startPos;
    if (!NavMesh.SamplePosition(manager.transform.position, out groundHit, 1f, NavMesh.AllAreas))
    {
        Debug.LogWarning("[Boss Info] The starting point is not on the NavMesh and cannot retreat.");
        isRetreating = false;
        return;
    }

    startPos = groundHit.position;

    float maxValidDistance = 0f;
    Vector3 bestTarget = startPos;
    bool foundValid = false;

    int sampleCount = 7; // Take direction every about 30 degrees
    float angleStep = 180f / (sampleCount - 1); // Sector angle spacing

    for (int i = 0; i < sampleCount; i++)
    {
        float angle = -90f + i * angleStep; // [-90, 90] degree
        Vector3 dir = Quaternion.Euler(0f, angle, 0f) * backDir;
        Vector3 rawTarget = startPos + dir * retreatDistance;

        // Use Raycast to detect whether path is accessible
        NavMeshHit rayHit;
        bool blocked = NavMesh.Raycast(startPos, rawTarget, out rayHit, NavMesh.AllAreas);

        Vector3 finalTarget = rawTarget;
        if (blocked)
        {
            finalTarget = rayHit.position;
        }
        else
        {
            // Further adsorption of target points
            NavMeshHit sampleHit;
            if (NavMesh.SamplePosition(rawTarget, out sampleHit, 1f, NavMesh.AllAreas))
            {
                finalTarget = sampleHit.position;
            }
        }

        float dist = Vector3.Distance(startPos, finalTarget);
        if (dist > maxValidDistance)
        {
            maxValidDistance = dist;
            bestTarget = finalTarget;
            foundValid = true;
        }
    }

    if (!foundValid)
    {
        Debug.LogWarning("[Attack1State] All directions are invalid, stay where you are");
        retreatTarget = startPos;
    }
    else
    {
        retreatTarget = bestTarget;
        Debug.Log($"[Attack1State] retreat to {retreatTarget:F2}, dis = {maxValidDistance:F2}");
    }

    isRetreating = true;
    manager.agent.SetDestination(retreatTarget);
    parameter.animator.Play("IdleOpenEMLoop_74");
}

    public void OnUpdate()
    {
        if (manager.IsDeath())
        {
            manager.parameter.body.layer = LayerMask.NameToLayer("Cuttable");
        }
        // if (parameter.getHit)
        // {
        //     manager.TransitionState(StateType.Hit);
        //     return;
        // }
        // —— Retreat ——
        if (isRetreating)
        { 
            // if (parameter.getHit)
            // {
            //     manager.TransitionState(StateType.Hit);
            //     return;
            // }
            parameter.animator.Play("IdleOpenEMLoop_74");
            if (!manager.agent.pathPending
                && manager.agent.remainingDistance <= arriveTolerance)
            {
                float actual = Vector3.Distance(enterPosition, manager.transform.position);
                Debug.Log($"[Attack1State] Actual retreat distance = {actual:F2} m");

                // Stop moving and resume stoppingDistance
                manager.agent.isStopped = true;
                manager.agent.ResetPath();
                manager.agent.stoppingDistance = savedStoppingDistance;

                isRetreating = false;
                PlayAttack();
            }
        }
        else
        {
            // —— Attacking —— //
            // 1) look at player
            manager.SmoothLookAt(manager.transform, manager.player.position, manager.agent.angularSpeed);

            // 2) death?
            // 3) timer
            timer += Time.deltaTime;
            if (timer >= attackDuration)
            {
                manager.EyeballBack();
                // refresh Attack2/3 cd
                if (!parameter.availableAttack2)
                {
                    parameter.attack1HealForAttack2++;
                    if (parameter.attack1HealForAttack2 >= 1)
                    {
                        parameter.availableAttack2 = true;
                        parameter.attack1HealForAttack2 = 0;
                    }
                }
                if (!parameter.availableAttack3)
                {
                    parameter.attack1HealForAttack3++;
                    if (parameter.attack1HealForAttack3 >= 1)
                    {
                        parameter.availableAttack3 = true;
                        parameter.attack1HealForAttack3 = 0;
                    }
                }

                // switch back to Chase
                manager.TransitionState(StateType.React);
            }
        }
    }

    public void OnExit()
    {
        // reset stoppingDistance
        manager.agent.stoppingDistance = savedStoppingDistance;
        if (manager.IsDeath())
        {
            manager.parameter.body.layer = LayerMask.NameToLayer("Cuttable");
        }
    }

    private void PlayAttack()
    {
        parameter.animator.Play("IdleClosedEMLoop_74");
        manager.SpawnEyeballSwarm();
        Debug.Log("[Attack1State] PlayAttack -> attack2 & SpawnEyeballSwarm");
    }
}


public class Attack2State : IState
{
    private FSM manager;
    private Parameter parameter;
    private AnimatorStateInfo info;
    private bool pullTrigger;

    private bool isRetreating = false;
    private Vector3 retreatTarget;
    private Vector3 enterPosition;
    private float savedStoppingDistance;
    private bool A2Anim;

    // Fixed retreat distance
    private const float retreatDistance = 6f;
    // Arrival tolerance
    private const float arriveTolerance = 0.01f;


    public Attack2State(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    

    public void OnEnter()
    {
        pullTrigger = true;
        A2Anim = true;
        if (!parameter.availableAttack2)
        {
            Debug.Log("Attack2 unavailable, skip!");
            manager.TransitionState(StateType.Chase);
            return;
        }

        parameter.availableAttack2 = false;
        parameter.animator.Play("IdleOpenEMLoop_74");
        Debug.Log("[Boss Info] Attack2: Enter Start retreating");
        enterPosition = manager.transform.position;
        StartRetreat();
    }

   private void StartRetreat() 
   {
    savedStoppingDistance = manager.agent.stoppingDistance;
    manager.agent.stoppingDistance = 0f;

    Vector3 backDir = manager.transform.position - manager.player.position;
    backDir.y = 0f;
    backDir.Normalize();

    NavMeshHit groundHit;
    Vector3 startPos;
    if (!NavMesh.SamplePosition(manager.transform.position, out groundHit, 1f, NavMesh.AllAreas))
    {
        Debug.LogWarning("[Boss Info] The starting point is not on the NavMesh and cannot retreat.");
        isRetreating = false;
        return;
    }

    startPos = groundHit.position;

    float maxValidDistance = 0f;
    Vector3 bestTarget = startPos;
    bool foundValid = false;

    int sampleCount = 7; // Take a direction every about 30 degrees
    float angleStep = 180f / (sampleCount - 1); // Sector angle spacing

    for (int i = 0; i < sampleCount; i++)
    {
        float angle = -90f + i * angleStep; // [-90, 90] degree
        Vector3 dir = Quaternion.Euler(0f, angle, 0f) * backDir;
        Vector3 rawTarget = startPos + dir * retreatDistance;

        // Use Raycast to detect whether the path is accessible
        NavMeshHit rayHit;
        bool blocked = NavMesh.Raycast(startPos, rawTarget, out rayHit, NavMesh.AllAreas);

        Vector3 finalTarget = rawTarget;
        if (blocked)
        {
            finalTarget = rayHit.position;
        }
        else
        {
            // Further adsorption of target points
            NavMeshHit sampleHit;
            if (NavMesh.SamplePosition(rawTarget, out sampleHit, 1f, NavMesh.AllAreas))
            {
                finalTarget = sampleHit.position;
            }
        }

        float dist = Vector3.Distance(startPos, finalTarget);
        if (dist > maxValidDistance)
        {
            maxValidDistance = dist;
            bestTarget = finalTarget;
            foundValid = true;
        }
    }

    if (!foundValid)
    {
        Debug.LogWarning("[Attack1State] All directions are invalid, stay there");
        retreatTarget = startPos;
    }
    else
    {
        retreatTarget = bestTarget;
        Debug.Log($"[Attack1State] retreat to {retreatTarget:F2}, dis = {maxValidDistance:F2}");
    }

    isRetreating = true;
    manager.agent.SetDestination(retreatTarget);
    parameter.animator.Play("IdleOpenEMLoop_74"); 
   }


    public void OnUpdate()
    {
        if (parameter.getHit)
        {
            manager.StopGravityField();
            manager.TransitionState(StateType.Hit);
            return;
        }
        // —— Retreat —— 
        if (isRetreating)
        {
            if (parameter.getHit)
            {
                manager.StopGravityField();
                manager.TransitionState(StateType.Hit);
                return;
            }
            
            // parameter.animator.Play("IdleOpenEMLoop_74");
            if (!manager.agent.pathPending
                && manager.agent.remainingDistance <= arriveTolerance)
            {
                float actual = Vector3.Distance(enterPosition, manager.transform.position);
                Debug.Log($"[Attack2State] Retreat = {actual:F2} m");

                // Stop Move & Resume stoppingDistance
                manager.agent.isStopped = true;
                manager.agent.ResetPath();
                manager.agent.stoppingDistance = savedStoppingDistance;

                isRetreating = false;
                if (A2Anim)
                {
                    parameter.animator.Play("Atk_ZeroG_74");
                    Debug.Log("Boss state: Attack2 - Gravity attack");
                    A2Anim = false;
                }
            }
        }
        else
        {
            manager.playerInSightRange =
                Physics.CheckSphere(manager.transform.position, manager.sightRange, manager.whatIsPlayer);
            manager.playerInAttackRange =
                Physics.CheckSphere(manager.transform.position, manager.attackRange, manager.whatIsPlayer);
            // look at player
            float turnSpeed = manager.agent.angularSpeed;
            manager.SmoothLookAt(manager.transform, manager.player.position, turnSpeed);

            info = parameter.animator.GetCurrentAnimatorStateInfo(0);
            var msg = parameter.animator.GetCurrentAnimatorClipInfo(0);
            string curClipName = "";
            if (msg != null && msg.Length > 0)
            {
                curClipName = msg[0].clip != null ? msg[0].clip.name : "Unknown Clip";
            }

            if (curClipName == "Atk_ZeroG_74" && info.normalizedTime >= .45f && pullTrigger)
            {
                pullTrigger = false;
                manager.StartGravityField();
            }
            
            if (info.normalizedTime >= .95f)
            {
                manager.StopGravityField();
                manager.TransitionState(StateType.React);
            }
        }
    }

    public void OnExit()
    {
        // Resume on exit stoppingDistance
        manager.agent.stoppingDistance = savedStoppingDistance;
    }
}




public class Attack3State : IState
{
    private FSM manager;
    private Parameter parameter;
    private AnimatorStateInfo info;
     private bool isApproaching = false;
    public Attack3State(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("IdleOpenEMLoop_74");
        float distance = Vector3.Distance(
            manager.transform.position,
            manager.player.position
        );

        // 1) if distance> meleeAttackRange, walk toward player first
        if (distance > manager.meleeAttackRange)
        {
            isApproaching = true;

            // change stoppingDistance -> meleeAttackRange (agent stop at meleeAttackRange area)
            manager.agent.stoppingDistance = manager.meleeAttackRange;
            manager.agent.SetDestination(manager.player.position);

            
            Debug.Log($"[Attack3] too far, walk toward player ({distance:0.00}m), target distance {manager.meleeAttackRange}m");
            return;
        }

        // 2) if in melee attack range
        BeginTentacleFlail();
        
    }
    private void BeginTentacleFlail()
    {
        parameter.availableAttack3 = false;
        
        // manager.StartTentacleFlail();
    }

    public void OnUpdate()
    {
        if (parameter.getHit)
        {
            manager.StopTentacleFlail();
            manager.TransitionState(StateType.Hit);
            return;
        }
        
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        manager.playerInSightRange =
            Physics.CheckSphere(manager.transform.position, manager.sightRange, manager.whatIsPlayer);
        manager.playerInAttackRange =
            Physics.CheckSphere(manager.transform.position, manager.attackRange, manager.whatIsPlayer);
        
        //look at player
        float turnSpeed = manager.agent.angularSpeed;  
        manager.SmoothLookAt(manager.transform, manager.player.position, turnSpeed);

        // If still approaching, wait until the path ends
        if (isApproaching)
        {
            if (parameter.getHit)
            {
                manager.StopTentacleFlail();
                manager.TransitionState(StateType.Hit);
                return;
            }
            
            // parameter.animator.Play("IdleOpenEMLoop_74");
            manager.agent.SetDestination(manager.player.position);
            float dist = Vector3.Distance(
                manager.transform.position,
                manager.player.position
            );

            if (dist <= manager.meleeAttackRange + 3f)
            {
                manager.agent.isStopped = true;
                manager.agent.ResetPath();
                Debug.Log("[Attack3] start attack3");
                parameter.animator.Play("Atk_Tentacle_85");
                BeginTentacleFlail();
                isApproaching = false;
            }
            return;
        }
        else
        { 
            info = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (info.normalizedTime >= .10f)
            { 
                manager.parameter.tentacleCollider.GetComponent<BoxCollider>().enabled = true;
            }

           
            if (info.normalizedTime >= .45f) {
                manager.parameter.tentacleCollider.GetComponent<CapsuleCollider>().enabled = true;
            }

            if (info.normalizedTime >= .95f) {
                manager.StopTentacleFlail(); 
                manager.TransitionState(StateType.Chase); 
            }  
        }




    }

    public void OnExit()
    {
        // double check stop attack
        manager.StopTentacleFlail();
        // reset
        manager.parameter.tentacleCollider.GetComponent<BoxCollider>().enabled = false;
        manager.parameter.tentacleCollider.GetComponent<CapsuleCollider>().enabled = false;
        manager.agent.stoppingDistance = 1;
    }


}




public class HitState : IState
{
    private FSM manager;
    private Parameter parameter;
    private AnimatorStateInfo info;
    
    public HitState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        // Debug.Log("HHHHIT: "+ parameter.lastHitPart);
        
        parameter.health--;
        
        switch (parameter.lastHitPart)
        {
            case HitPart.Eye:
                parameter.animator.Play("Inb_EyeCloseMSingle_74");
                parameter.availableAttack2 = false;
                // Debug.Log("[HitState] Eye, Attack2 X");
                break;
            case HitPart.Tentacle: 
                parameter.animator.Play("Inb_TentacleCloseESingle_74");
                parameter.availableAttack3 = false;
                // Debug.Log("[HitState] Tentacle, Attack3 X ");
                break;
        }
        
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        if (info.normalizedTime >= 0.95f)
        {
            switch (parameter.lastHitPart)
            {
                case HitPart.Eye:
                    Debug.Log("HHHHIT: change "+ parameter.lastHitPart);
                    manager.parameter.eye.layer = LayerMask.NameToLayer("Cuttable");
                    break;
                case HitPart.Tentacle:
                    Debug.Log("HHHHIT: change "+ parameter.lastHitPart);
                    manager.parameter.tentacle.layer = LayerMask.NameToLayer("Cuttable");
                    break;
            }
            manager.TransitionState(StateType.React);
        }
    }

    public void OnExit()
    {
        parameter.getHit = false;
    }
}

public class DeathState : IState
{
    private FSM manager;
    private Parameter parameter;

    public DeathState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("Death_17");
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {

    }
}