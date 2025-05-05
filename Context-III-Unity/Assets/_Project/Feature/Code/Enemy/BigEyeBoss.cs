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
    
    public AttackConState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
        rand = new System.Random();
    }
    public void OnEnter()
    {
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
        }

    }

    public void OnExit()
    {
        
    }
}







public class Attack1State : IState
{
    private FSM manager;
    private Parameter parameter;

    private float timer;
    private bool isRetreating = false;
    private Vector3 retreatTarget;
    private Vector3 enterPosition;
    private float savedStoppingDistance;

    // 固定后撤距离：6 米
    private const float retreatDistance = 6f;
    // 到达判定容差
    private const float arriveTolerance = 0.01f;
    // 攻击持续时长
    private const float attackDuration = 3f;

    public Attack1State(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        Debug.Log("[Attack1State] OnEnter 开始后撤");
        enterPosition = manager.transform.position;
        timer = 0f;
        StartRetreat();
    }

    private void StartRetreat()
    {
        // 1) 保存原 stoppingDistance，设置为 0 精确到点
        savedStoppingDistance = manager.agent.stoppingDistance;
        manager.agent.stoppingDistance = 0f;

        // 2) 计算玩家→Boss 水平方向（XYZ 忽略 Y）
        Vector3 dir = manager.transform.position - manager.player.position;
        dir.y = 0f;
        dir.Normalize();

        // 3) 以 Boss 当前点为起点，沿此方向退 retreatDistance 米
        Vector3 rawTarget = manager.transform.position + dir * retreatDistance;
        Debug.Log($"[Attack1State] RawTarget = {rawTarget}");

        // 4) 采样 NavMesh 保证可导航
        NavMeshHit hit;
        if (NavMesh.SamplePosition(rawTarget, out hit, 1f, NavMesh.AllAreas))
        {
            retreatTarget = hit.position;
        }
        else
        {
            Debug.LogWarning("[Attack1State] retreatTarget 不在 NavMesh，上跳过后撤直接攻击");
            isRetreating = false;
            PlayAttack();
            return;
        }

        // 5) 发起后撤
        isRetreating = true;
        manager.agent.SetDestination(retreatTarget);
        parameter.animator.Play("walk");
        Debug.Log($"[Attack1State] StartRetreat: 从 {enterPosition:F2} → {retreatTarget:F2}");
    }

    public void OnUpdate()
    {
        // —— 后撤阶段 ——
        if (isRetreating)
        {
            if (!manager.agent.pathPending
                && manager.agent.remainingDistance <= arriveTolerance)
            {
                float actual = Vector3.Distance(enterPosition, manager.transform.position);
                Debug.Log($"[Attack1State] 实际后撤距离 = {actual:F2} 米");

                // 停止移动并恢复 stoppingDistance
                manager.agent.isStopped = true;
                manager.agent.ResetPath();
                manager.agent.stoppingDistance = savedStoppingDistance;

                isRetreating = false;
                PlayAttack();
            }
        }
        else
        {
            // —— 攻击进行中 —— //
            // 1) 平滑看向玩家
            manager.SmoothLookAt(manager.transform, manager.player.position, manager.agent.angularSpeed);

            // 2) 被击中打断
            if (parameter.getHit)
            {
                manager.TransitionState(StateType.Hit);
                return;
            }

            // 3) 计时结束后收尾
            timer += Time.deltaTime;
            if (timer >= attackDuration)
            {
                // 恢复 Attack2/3 冷却
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

                // 结束后切换到 Chase
                manager.TransitionState(StateType.Chase);
            }
        }
    }

    public void OnExit()
    {
        // reset stoppingDistance
        manager.agent.stoppingDistance = savedStoppingDistance;
    }

    private void PlayAttack()
    {
        parameter.animator.Play("attack22");
        manager.SpawnEyeballSwarm();
        Debug.Log("[Attack1State] PlayAttack -> attack22 & SpawnEyeballSwarm");
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
        if (gravityTimer >= 3f)
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
        parameter.animator.Play("attack_01");
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

            if (dist <= manager.meleeAttackRange + 1f)
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
        parameter.animator.Play("Hit");
        parameter.health--;
        
        switch (parameter.lastHitPart)
        {
            case HitPart.Eye:
                parameter.availableAttack2 = false;
                Debug.Log("[HitState] Eye，Attack2 X");
                break;
            case HitPart.Tentacle:
                parameter.availableAttack3 = false;
                Debug.Log("[HitState] Tentacle, Attack3 X ");
                break;
        }
        
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