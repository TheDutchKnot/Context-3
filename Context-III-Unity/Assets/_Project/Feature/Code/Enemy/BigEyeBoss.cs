using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
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
            //boss fight start!!
            manager.TransitionState(StateType.Chase);
        }
    }

    public void OnExit()
    {
        timer = 0;
    }
}

// public class PatrolState : IState
// {
//     private FSM manager;
//     private Parameter parameter;
//
//     private int patrolPosition;
//     public PatrolState(FSM manager)
//     {
//         this.manager = manager;
//         this.parameter = manager.parameter;
//     }
//     public void OnEnter()
//     {
//         parameter.animator.Play("Walk");
//     }
//
//     public void OnUpdate()
//     {
//         manager.FlipTo(parameter.patrolPoints[patrolPosition]);
//
//         manager.transform.position = Vector2.MoveTowards(manager.transform.position,
//             parameter.patrolPoints[patrolPosition].position, parameter.moveSpeed * Time.deltaTime);
//
//         if (parameter.getHit)
//         {
//             manager.TransitionState(StateType.Hit);
//         }
//         if (parameter.target != null &&
//             parameter.target.position.x >= parameter.chasePoints[0].position.x &&
//             parameter.target.position.x <= parameter.chasePoints[1].position.x)
//         {
//             manager.TransitionState(StateType.React);
//         }
//         if (Vector2.Distance(manager.transform.position, parameter.patrolPoints[patrolPosition].position) < .1f)
//         {
//             manager.TransitionState(StateType.Idle);
//         }
//     }
//
//     public void OnExit()
//     {
//         patrolPosition++;
//
//         if (patrolPosition >= parameter.patrolPoints.Length)
//         {
//             patrolPosition = 0;
//         }
//     }
// }

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
        // manager.FlipTo(parameter.target);
        // if (parameter.target)
        //     manager.transform.position = Vector2.MoveTowards(manager.transform.position,
        //     parameter.target.position, parameter.chaseSpeed * Time.deltaTime);
        //
        manager.playerInSightRange =
            Physics.CheckSphere(manager.transform.position, manager.sightRange, manager.whatIsPlayer);
        manager.playerInAttackRange =
            Physics.CheckSphere(manager.transform.position, manager.attackRange, manager.whatIsPlayer);
        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
        }
        // if (parameter.target == null ||
        //     manager.transform.position.x < parameter.chasePoints[0].position.x ||
        //     manager.transform.position.x > parameter.chasePoints[1].position.x)
        // {
        //     manager.TransitionState(StateType.Idle);
        // }
        // if (Physics2D.OverlapCircle(parameter.attackPoint.position, parameter.attackArea, parameter.targetLayer))
        // {
        //     // manager.TransitionState(StateType.Attack);
        // }
        manager.transform.LookAt(manager.player);
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

// public class ReactState : IState
// {
//     private FSM manager;
//     private Parameter parameter;
//
//     private AnimatorStateInfo info;
//     public ReactState(FSM manager)
//     {
//         this.manager = manager;
//         this.parameter = manager.parameter;
//     }
//     public void OnEnter()
//     {
//         parameter.animator.Play("React");
//     }
//
//     public void OnUpdate()
//     {
//         info = parameter.animator.GetCurrentAnimatorStateInfo(0);
//
//         if (parameter.getHit)
//         {
//             manager.TransitionState(StateType.Hit);
//         }
//         if (info.normalizedTime >= .95f)
//         {
//             manager.TransitionState(StateType.Chase);
//         }
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

    private AnimatorStateInfo info;
    public AttackConState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("attack_01");
        Debug.Log("Boss State: Attack Controller");
    }

    public void OnUpdate()
    {
        manager.agent.SetDestination(manager.transform.position);
        manager.transform.LookAt(manager.player);
        if (!manager.alreadyAttacked)
        {
            // attack code...
            Debug.Log("attack");

            manager.alreadyAttacked = true;
            manager.Invoke(nameof(manager.ResetAttack), manager.timeBetweenAttacks);
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

    private AnimatorStateInfo info;
    public Attack1State(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("Attack");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);

        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
        }
        if (info.normalizedTime >= .95f)
        {
            manager.TransitionState(StateType.Chase);
        }
    }

    public void OnExit()
    {

    }
}

public class Attack2State : IState
{
    private FSM manager;
    private Parameter parameter;

    private AnimatorStateInfo info;
    public Attack2State(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("Attack");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);

        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
        }
        if (info.normalizedTime >= .95f)
        {
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
    public Attack3State(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("Attack");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);

        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
        }
        if (info.normalizedTime >= .95f)
        {
            manager.TransitionState(StateType.Chase);
        }
    }

    public void OnExit()
    {

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
        // if (info.normalizedTime >= .95f)
        // {
        //     parameter.target = GameObject.FindWithTag("Player").transform;
        //
        //     manager.TransitionState(StateType.Chase);
        // }
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