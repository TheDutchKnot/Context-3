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
        if (!manager.playerInSightRange)
        {
            manager.TransitionState(StateType.Idle);
        }

        if (manager.playerInAttackRange)
        {
            manager.TransitionState(StateType.AttackCon);
            
        }
    }
    


    public void OnExit()
    {

    }
}


// public class AttackConState : IState
// {
//     private FSM manager;
//     private Parameter parameter;
//     private System.Random rand;
//     private AnimatorStateInfo info;
//     
//     // Track last used to avoid repeats
//     private StateType lastAttack = StateType.Chase;
//     
//     // per‑attack cooldown sec
//     private Dictionary<StateType, float> cooldowns = new Dictionary<StateType, float>()
//     {
//         { StateType.Attack1, 1f },
//         { StateType.Attack2, 1f },
//         { StateType.Attack3, 1f },
//     };
//     private float cooldownDuration = 2f; // 3s before the same attack can be reused
//     
//     public AttackConState(FSM manager)
//     {
//         this.manager = manager;
//         this.parameter = manager.parameter;
//         rand = new System.Random();
//     }
//     public void OnEnter()
//     {
        // parameter.animator.Play("attack_01");
        // Debug.Log("Boss State: Attack Controller");
        // // Update cooldown timers
        // List<StateType> keys = new List<StateType>(cooldowns.Keys);
        // for (int i = 0; i < keys.Count; i++)
        // {
        //     StateType key = keys[i];
        //     cooldowns[key] = Mathf.Max(0, cooldowns[key] - Time.deltaTime);
        // }
        // // Determine distances
        // float distance = Vector3.Distance(manager.transform.position, manager.player.position);
        // // weighted list of possible attacks
        // var options = new List<(StateType type, int weight)>();
        //
        // // Always allow the base eyeball attack if its cooldown is ready
        // if (cooldowns[StateType.Attack1] <= 0)
        // {
        //     int w = 50;
        //     // when boss low on health, bias more toward the swarm attack
        //     if (parameter.health < 3) w += 30;
        //     options.Add((StateType.Attack1, w));
        // }
        //
        // // Gravity field: prefer if far away, special available, and off cooldown
        // if (parameter.availableAttack2 && cooldowns[StateType.Attack2] <= 0)
        // {
        //     int w = distance > manager.attackRange * 1.5f ? 60 : 10;
        //     options.Add((StateType.Attack2, w));
        // }
        //
        // // Tentacle sweep: prefer mid‑range if available and off cooldown
        // if (parameter.availableAttack3 && cooldowns[StateType.Attack3] <= 0)
        // {
        //     int w = (distance > manager.attackRange * 0.5f && distance <= manager.attackRange * 1.5f) ? 50 : 10;
        //     options.Add((StateType.Attack3, w));
        // } 
        // // Remove the last used attack to avoid repetition
        // options.RemoveAll(o => o.type == lastAttack);
        //
        // // If no option left (all on cooldown or unavailable), default to Attack1
        // StateType chosen = StateType.Attack1;
        // if (options.Count > 0)
        // {
        //     // Weighted random selection
        //     int totalWeight = options.Sum(o => o.weight);
        //     int pick = rand.Next(0, totalWeight);
        //     int cum = 0;
        //     foreach (var o in options)
        //     {
        //         cum += o.weight;
        //         if (pick < cum)
        //         {
        //             chosen = o.type;
        //             break;
        //         }
        //     }
        // }
        //
        // // Apply cooldown and remember
        // cooldowns[chosen] = cooldownDuration;
        // lastAttack = chosen;
        //
        // // Transition
        // manager.TransitionState(chosen);
//     }
//
//     public void OnUpdate()
//     {
//         
//     }
//
//     public void OnExit()
//     {
//         
//     }
// }

public class AttackConState : IState
{
    private FSM manager;
    private Parameter parameter;
    private System.Random rand = new System.Random();

    // Track last used attack to avoid repeats
    private StateType lastAttack = StateType.Chase;

    // Cooldowns (in seconds) for each attack
    private Dictionary<StateType, float> cooldowns = new Dictionary<StateType, float>()
    {
        { StateType.Attack1, 0f },
        { StateType.Attack2, 0f },
        { StateType.Attack3, 0f },
    };
    private const float cooldownDuration = 2f;

    // Retreat‑before‑fallback logic
    private bool isRetreating = false;
    private Vector3 retreatTarget;
    private float savedStoppingDistance;

    public AttackConState(FSM manager)
    {
        this.manager   = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        // // If we just did Attack2, enforce the rule: try Attack3 next
        // if (lastAttack == StateType.Attack2)
        // {
        //     if (parameter.availableAttack3 && cooldowns[StateType.Attack3] <= 0f)
        //     {
        //         DoAttack(StateType.Attack3);
        //     }
        //     else
        //     {
        //         StartRetreat();
        //     }
        //     return;
        // }

        // Otherwise pick and perform an attack immediately
        ChooseAndDoAttack();
    }

    public void OnUpdate()
    {
        // 1) Tick down all cooldowns each frame
        foreach (var key in cooldowns.Keys.ToList())
        {
            if (cooldowns[key] > 0f)
                cooldowns[key] = Mathf.Max(0f, cooldowns[key] - Time.deltaTime);
        }

        // 2) If retreating, wait until we've reached retreatTarget, then fallback to Attack1
        // if (isRetreating)
        // {
        //     if (!manager.agent.pathPending 
        //         && manager.agent.remainingDistance <= 0.1f)
        //     {
        //         manager.agent.stoppingDistance = savedStoppingDistance;
        //         isRetreating = false;
        //         DoAttack(StateType.Attack1);
        //     }
        // }
    }

    public void OnExit()
    {
        // no cleanup needed here
    }

    private void ChooseAndDoAttack()
    {
        float distance = Vector3.Distance(
            manager.transform.position,
            manager.player.position
        );

        // Build weighted options
        var options = new List<(StateType type, int weight)>();

        // Attack1: eyeball swarm
        // if (cooldowns[StateType.Attack1] <= 0f)
            options.Add((StateType.Attack1, 50 + (parameter.health < 3 ? 30 : 0)));

        // Attack2: gravity field
        // if (parameter.availableAttack2 && cooldowns[StateType.Attack2] <= 0f)
            options.Add((StateType.Attack2, distance > manager.attackRange * 1.2f ? 60 : 10));

        // Attack3: tentacle
        // if (parameter.availableAttack3 && cooldowns[StateType.Attack3] <= 0f)
            options.Add((StateType.Attack3,
                (distance <= manager.meleeAttackRange) ? 60 : 10));

        // Avoid repeating the last attack
        options.RemoveAll(o => o.type == lastAttack);

        // Pick by weight, or fallback to Attack1
        StateType chosen = StateType.Attack3;
        if (options.Count > 0)
        {
            int total = options.Sum(o => o.weight);
            int roll  = rand.Next(0, total), sum = 0;
            foreach (var (type, weight) in options)
            {
                sum += weight;
                if (roll < sum)
                {
                    chosen = type;
                    break;
                }
            }
        }

        DoAttack(chosen);
    }

    private void DoAttack(StateType attack)
    {
        cooldowns[attack] = cooldownDuration;
        lastAttack        = attack;
        manager.TransitionState(attack);
    }
    
}



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
        // 2) 已经在安全距离，直接发动Attack1
        //play anim
     
        
    }
    
    private void StartRetreat()
    {
        // 1) Save and zero out stoppingDistance
        savedStoppingDistance = manager.agent.stoppingDistance;
        manager.agent.stoppingDistance = 0f;

        // 2) Compute target and begin retreat
        Vector3 dir = (manager.transform.position - manager.player.position).normalized;
        retreatTarget = manager.transform.position + dir * (manager.attackRange+0.01f);
        isRetreating = true;
        manager.agent.SetDestination(retreatTarget);
        parameter.animator.Play("walk");
        Debug.Log("Retreating before fallback Attack1");
        
    }

    public void OnUpdate()
    {    
             if (isRetreating)
             {
                 if (!manager.agent.pathPending && manager.agent.remainingDistance <= 0.5f)
                 {
                     // 3) Restore stoppingDistance
                     manager.agent.stoppingDistance = savedStoppingDistance;
                     Debug.Log("kkkkkkkkkkk");
                     isRetreating = false;
                     parameter.animator.Play("attack_01");
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
                         if(timer >= 8f)
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
            Debug.Log("Attack2 不可用，跳过攻击。");
            
            manager.TransitionState(StateType.Chase);
            return;
        }
        
        // 标记 Attack2 已使用，后续需通过 Attack1 的累计恢复
        parameter.availableAttack2 = false;
        
        // 播放 Attack2 动画（需要在 Animator 中配置专用动画）
        parameter.animator.Play("Attack2");
        Debug.Log("Boss 状态：Attack2 - 重力场攻击激活");
        
        // 启动重力场效果（例如，将玩家拉向 Boss）
        manager.StartGravityField();
        parameter.animator.Play("Attack");
    }

    public void OnUpdate()
    {
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
        
        // 如果此攻击不可用则直接跳转回 Chase 状态
        if (!parameter.availableAttack3)
        {
            Debug.Log("Attack3 不可用，跳过攻击。");
            manager.TransitionState(StateType.Chase);
            return;
        }
        
        float distance = Vector3.Distance(
            manager.transform.position,
            manager.player.position
        );

        // 1) 如果距离超出 meleeAttackRange，就先走过去
        if (distance > manager.meleeAttackRange)
        {
            isApproaching = true;

            // 临时把 stoppingDistance 设为 meleeAttackRange，这样 agent 会正好停在距离玩家 meleeAttackRange 的位置
            manager.agent.stoppingDistance = manager.meleeAttackRange;
            manager.agent.SetDestination(manager.player.position);

            parameter.animator.Play("walk");
            Debug.Log($"[Attack3] 过远，先接近玩家 ({distance:0.00}m)，目标距离 {manager.meleeAttackRange}m");
            return;
        }

        // 2) 如果已经在范围内，直接发动触手攻击
        BeginTentacleFlail();
        
    }
    private void BeginTentacleFlail()
    {
        // 如果你在 Parameter 里跟踪可用性，就在这里做：parameter.availableAttack3 = false;
        parameter.availableAttack3 = false;

        parameter.animator.Play("Attack3");
        manager.StartTentacleFlail();
    }

    public void OnUpdate()
    {
        
        //look at player
        float turnSpeed = manager.agent.angularSpeed;  
        manager.SmoothLookAt(manager.transform, manager.player.position, turnSpeed);
        
        timer += Time.deltaTime;

        // 如果还在接近阶段，等 path 结束后再发动
        if (isApproaching)
        {
            if (!manager.agent.pathPending 
                && manager.agent.remainingDistance <= manager.meleeAttackRange + 0.1f)
            {
                isApproaching = false;
                Debug.Log("[Attack3] 接近完成，开始触手攻击");
                BeginTentacleFlail();
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
        // 恢复原本的 stoppingDistance，免得影响其他状态
        manager.agent.stoppingDistance = manager.attackRange;
    }
    // 开启触手挥舞效果，激活碰撞检测及特效

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
        // 当受击动画播放完毕后返回 Chase 状态
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