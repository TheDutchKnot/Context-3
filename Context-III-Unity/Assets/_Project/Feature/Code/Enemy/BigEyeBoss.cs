using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        parameter.animator.Play("idle");
    }

    public void OnUpdate()
    {
        manager.playerInSightRange =
            Physics.CheckSphere(manager.transform.position, manager.sightRange, manager.whatIsPlayer);
        manager.playerInAttackRange =
            Physics.CheckSphere(manager.transform.position, manager.attackRange, manager.whatIsPlayer);
        timer += Time.deltaTime;

        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
        }

        if (manager.playerInSightRange)
        {
            //TODO boss fight start!!
            manager.TransitionState(StateType.Chase);
        }
    }

    public void OnExit()
    {
        timer = 0;
    }
}

public class ChaseState : IState
{
    private FSM manager;
    private Parameter parameter;

    public ChaseState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        Debug.Log("Boos state: chase");
        parameter.animator.Play("walk");
    }

    public void OnUpdate()
    {
        manager.playerInSightRange =
            Physics.CheckSphere(manager.transform.position, manager.sightRange, manager.whatIsPlayer);
        manager.playerInAttackRange =
            Physics.CheckSphere(manager.transform.position, manager.attackRange, manager.whatIsPlayer);
        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
        }
        float turnSpeed = manager.agent.angularSpeed;  
        manager.SmoothLookAt(manager.transform, manager.player.position, turnSpeed);
        manager.agent.SetDestination(manager.player.position);
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
    
    // per‑attack cooldown sec
    private Dictionary<StateType, float> cooldowns = new Dictionary<StateType, float>()
    {
        { StateType.Attack1, 0f },
        { StateType.Attack2, 0f },
        { StateType.Attack3, 0f },
    };
    private float cooldownDuration = 1f; // 3s before the same attack can be reused
    
    public AttackConState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
        rand = new System.Random();
    }
    public void OnEnter()
    {
        parameter.animator.Play("attack_01");
        Debug.Log("Boss State: Attack Controller");
        // Update cooldown timers
        List<StateType> keys = new List<StateType>(cooldowns.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            StateType key = keys[i];
            cooldowns[key] = Mathf.Max(0, cooldowns[key] - Time.deltaTime);
        }
        // Determine distances
        float distance = Vector3.Distance(manager.transform.position, manager.player.position);
        // weighted list of possible attacks
        var options = new List<(StateType type, int weight)>();
        
        // Always allow the base eyeball attack if its cooldown is ready
        if (cooldowns[StateType.Attack1] <= 0)
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
        
        // Apply cooldown and remember
        cooldowns[chosen] = cooldownDuration;
        lastAttack = chosen;
        
        // Transition
        manager.TransitionState(chosen);
    }

    public void OnUpdate()
    {
        manager.playerInSightRange =
            Physics.CheckSphere(manager.transform.position, manager.sightRange, manager.whatIsPlayer);
        manager.playerInAttackRange =
            Physics.CheckSphere(manager.transform.position, manager.attackRange, manager.whatIsPlayer);
        
    }

    public void OnExit()
    {
        
    }
}

// public class AttackConState : IState
// {
//     private FSM manager;
//     private Parameter parameter;
//     private System.Random rand = new System.Random();
//
//     // Track last used attack to avoid repeats
//     private StateType lastAttack = StateType.Chase;
//
//     // Cooldowns (in seconds) for each attack
//     private Dictionary<StateType, float> cooldowns = new Dictionary<StateType, float>()
//     {
//         { StateType.Attack1, 0f },
//         { StateType.Attack2, 0f },
//         { StateType.Attack3, 0f },
//     };
//     private const float cooldownDuration = 2f;
//
//     // Retreat‑before‑fallback logic
//     private bool isRetreating = false;
//     private Vector3 retreatTarget;
//     private float savedStoppingDistance;
//
//     public AttackConState(FSM manager)
//     {
//         this.manager   = manager;
//         this.parameter = manager.parameter;
//     }
//
//     public void OnEnter()
//     {
//         ChooseAndDoAttack();
//     }
//
//     public void OnUpdate()
//     {
//         // 1) Tick down all cooldowns each frame
//         foreach (var key in cooldowns.Keys.ToList())
//         {
//             if (cooldowns[key] > 0f)
//                 cooldowns[key] = Mathf.Max(0f, cooldowns[key] - Time.deltaTime);
//         }
//
//         // 2) If retreating, wait until we've reached retreatTarget, then fallback to Attack1
//         // if (isRetreating)
//         // {
//         //     if (!manager.agent.pathPending 
//         //         && manager.agent.remainingDistance <= 0.1f)
//         //     {
//         //         manager.agent.stoppingDistance = savedStoppingDistance;
//         //         isRetreating = false;
//         //         DoAttack(StateType.Attack1);
//         //     }
//         // }
//     }
//
//     public void OnExit()
//     {
//         // no cleanup needed here
//     }
//
//     private void ChooseAndDoAttack()
//     {
//         float distance = Vector3.Distance(
//             manager.transform.position,
//             manager.player.position
//         );
//
//         // Build weighted options
//         var options = new List<(StateType type, int weight)>();
//
//         // Attack1: eyeball swarm
//         // if (cooldowns[StateType.Attack1] <= 0f)
//             options.Add((StateType.Attack1, 50 + (parameter.health < 3 ? 30 : 0)));
//
//         // Attack2: gravity field
//         // if (parameter.availableAttack2 && cooldowns[StateType.Attack2] <= 0f)
//             options.Add((StateType.Attack2, distance > manager.attackRange * 1.2f ? 60 : 10));
//
//         // Attack3: tentacle
//         // if (parameter.availableAttack3 && cooldowns[StateType.Attack3] <= 0f)
//             options.Add((StateType.Attack3,
//                 (distance <= manager.meleeAttackRange) ? 60 : 10));
//
//         // Avoid repeating the last attack
//         options.RemoveAll(o => o.type == lastAttack);
//
//         // Pick by weight, or fallback to Attack1
//         StateType chosen = StateType.Attack3;
//         if (options.Count > 0)
//         {
//             int total = options.Sum(o => o.weight);
//             int roll  = rand.Next(0, total), sum = 0;
//             foreach (var (type, weight) in options)
//             {
//                 sum += weight;
//                 if (roll < sum)
//                 {
//                     chosen = type;
//                     break;
//                 }
//             }
//         }
//
//         DoAttack(chosen);
//     }
//
//     private void DoAttack(StateType attack)
//     {
//         cooldowns[attack] = cooldownDuration;
//         lastAttack        = attack;
//         manager.TransitionState(attack);
//     }
//     
// }



public class Attack1State : IState
{
    private FSM manager;
    private Parameter parameter;

    private AnimatorStateInfo info;
    private float timer;
    
    // ——— Retreat fields ———
    private bool isRetreating = false;
    private Vector3 retreatTarget;
    private float savedStoppingDistance;
    
    public Attack1State(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        timer = 0f;
        StartRetreat();
     
        
    }
    
    private void StartRetreat()
    {
        // 1) Save and zero out stoppingDistance
        savedStoppingDistance = manager.agent.stoppingDistance;
        manager.agent.stoppingDistance = 2f;

        // 2) Compute target and begin retreat
        Vector3 toBoss = (manager.transform.position - manager.player.position).normalized;
        retreatTarget = manager.player.position + toBoss * manager.attackRange;

        isRetreating = true;
        manager.agent.SetDestination(retreatTarget);
        parameter.animator.Play("walk");
        Debug.Log("Retreating before fallback Attack1, target at attackRange from player");
        
    }

    public void OnUpdate()
    {    
        manager.playerInSightRange =
            Physics.CheckSphere(manager.transform.position, manager.sightRange, manager.whatIsPlayer);
        manager.playerInAttackRange =
            Physics.CheckSphere(manager.transform.position, manager.attackRange, manager.whatIsPlayer);
             if (isRetreating)
             {
                 if (!manager.agent.pathPending
                     && manager.agent.remainingDistance <= manager.agent.stoppingDistance + 0.1f)
                 {
                     // 3) Restore stoppingDistance
                     manager.agent.stoppingDistance = savedStoppingDistance;
                     manager.agent.isStopped = true;
                     manager.agent.ResetPath();
                     isRetreating = false;
                     parameter.animator.Play("attack22");
                     Debug.Log("Boss State: Attack_1");
                     manager.SpawnEyeballSwarm();
                 }
             }
             else
             {
                 float turnSpeed = manager.agent.angularSpeed;  
                 manager.SmoothLookAt(manager.transform, manager.player.position, turnSpeed);
                 info = parameter.animator.GetCurrentAnimatorStateInfo(0);
                 
                         if (parameter.getHit)
                         {
                             manager.TransitionState(StateType.Hit);
                         }
                         
                         timer += Time.deltaTime;
                         Debug.Log("Attack 1: "+timer);
                         // if (info.normalizedTime >= .95f)
                         if(timer >= 3f)
                         {
                             // process AAttack2 cd (if >= 2 time -> ready!)
                             if (!parameter.availableAttack2)
                             {
                                 parameter.attack1HealForAttack2++;
                                 Debug.Log("update cd Attack2: " + parameter.attack1HealForAttack2);
                                 if (parameter.attack1HealForAttack2 >= 1)
                                 {
                                     parameter.availableAttack2 = true;
                                     parameter.attack1HealForAttack2 = 0;
                                     Debug.Log("Attack2 available!");
                                 }
                             }
                             // process Attack3 cd
                             if (!parameter.availableAttack3)
                             {
                                 parameter.attack1HealForAttack3++;
                                 Debug.Log("update cd Attack3: " + parameter.attack1HealForAttack3);
                                 if (parameter.attack1HealForAttack3 >= 1)
                                 {
                                     parameter.availableAttack3 = true;
                                     parameter.attack1HealForAttack3 = 0;
                                     Debug.Log("Attack3 available!");
                                 }
                             }
                             manager.TransitionState(StateType.Chase);
                         }
             }
        
        
    }

    public void OnExit()
    {
        manager.agent.stoppingDistance = savedStoppingDistance;
    }

}

public class Attack2State : IState
{
    private FSM manager;
    private Parameter parameter;
    private AnimatorStateInfo info;
    private float gravityTimer;
    
    public Attack2State(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
      
        gravityTimer = 0f;
        if (!parameter.availableAttack2)
        {
            Debug.Log("Attack2 unavailable, skip!");
            manager.TransitionState(StateType.Chase);
            return;
        }
        
        // mark Attack2 as used
        parameter.availableAttack2 = false;
        
        // play Attack2 anim
        parameter.animator.Play("Attack2");
        Debug.Log("Boss state: Attack2 - Gravity attack");
        
        // pull player to boss
        manager.StartGravityField();
    }

    public void OnUpdate()
    {
        manager.playerInSightRange =
            Physics.CheckSphere(manager.transform.position, manager.sightRange, manager.whatIsPlayer);
        manager.playerInAttackRange =
            Physics.CheckSphere(manager.transform.position, manager.attackRange, manager.whatIsPlayer);
        //look at player
        float turnSpeed = manager.agent.angularSpeed;  
        manager.SmoothLookAt(manager.transform, manager.player.position, turnSpeed);
        
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        gravityTimer += Time.deltaTime;
        Debug.Log("Attack 2: "+gravityTimer);
        if (parameter.getHit)
        {
            manager.StopGravityField();
            manager.TransitionState(StateType.Hit);
        }
        if (gravityTimer >= 4f)
        {
            manager.StopGravityField();
            manager.TransitionState(StateType.Chase);
        }
    }

    public void OnExit()
    {

    }
    

}



public class Attack3State : IState
{
    private FSM manager;
    private Parameter parameter;
    private AnimatorStateInfo info;
     private bool isApproaching = false;
     private float timer;
    public Attack3State(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        timer = 0f;
        parameter.animator.Play("attack_01");
        if (!parameter.availableAttack3)
        {
            Debug.Log("Attack3 unavailable, skip!");
            manager.TransitionState(StateType.Chase);
            return;
        }
        
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

            parameter.animator.Play("walk");
            Debug.Log($"[Attack3] too far, walk toward player ({distance:0.00}m), target distance {manager.meleeAttackRange}m");
            return;
        }

        // 2) if in melee attack range
        BeginTentacleFlail();
        
    }
    private void BeginTentacleFlail()
    {
        // 如果你在 Parameter 里跟踪可用性，就在这里做：parameter.availableAttack3 = false;
        parameter.availableAttack3 = false;

        
        manager.StartTentacleFlail();
    }

    public void OnUpdate()
    {
        manager.playerInSightRange =
            Physics.CheckSphere(manager.transform.position, manager.sightRange, manager.whatIsPlayer);
        manager.playerInAttackRange =
            Physics.CheckSphere(manager.transform.position, manager.attackRange, manager.whatIsPlayer);
        
        //look at player
        float turnSpeed = manager.agent.angularSpeed;  
        manager.SmoothLookAt(manager.transform, manager.player.position, turnSpeed);
        
        timer += Time.deltaTime;

        // If still approaching, wait until the path ends
        if (isApproaching)
        {
            manager.agent.SetDestination(manager.player.position);
            float dist = Vector3.Distance(
                manager.transform.position,
                manager.player.position
            );

            if (dist <= manager.meleeAttackRange + 2f)
            {
                
                manager.agent.isStopped = true;
                manager.agent.ResetPath();
                Debug.Log("[Attack3] start attack3");
                BeginTentacleFlail();
                isApproaching = false;
            }
            return;
        }

        if (parameter.getHit)
        {
            manager.StopTentacleFlail();
            manager.TransitionState(StateType.Hit);
        }
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        // if (info.normalizedTime >= .95f)
        // {
        //     manager.StopTentacleFlail();
        //     manager.TransitionState(StateType.Chase);
        // }
        Debug.Log("Attack 3: "+timer);
        if (timer>=4f)
        {
            manager.StopTentacleFlail();
            manager.TransitionState(StateType.Chase);
        }
    }

    public void OnExit()
    {
        // double check stop attack
        manager.StopTentacleFlail();
        // reset
        manager.agent.stoppingDistance = manager.attackRange;
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
        parameter.animator.Play("Hit");
        parameter.health--;
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);

        if (parameter.health <= 0)
        {
            manager.TransitionState(StateType.Death);
        }
        
        if (info.normalizedTime >= 0.95f)
        {
            manager.TransitionState(StateType.Chase);
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
        parameter.animator.Play("Dead");
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}