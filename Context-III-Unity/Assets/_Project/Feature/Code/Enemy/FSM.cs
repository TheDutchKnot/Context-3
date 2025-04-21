using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum StateType
{
    Idle, Chase, AttackCon, Attack1, Attack2, Attack3, Hit, Death
}

[Serializable]
public class Parameter
{
    public int health;
    public float moveSpeed;
    public float chaseSpeed;
    public Animator animator;
    public bool getHit;
    
    //  




}
public class FSM : MonoBehaviour
{

    private IState currentState;
    private Dictionary<StateType, IState> states = new Dictionary<StateType, IState>();

    public Parameter parameter;
    
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    
    //attacking
    public float timeBetweenAttacks;
    public bool alreadyAttacked;
    
    //states
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        player = GameObject.Find("XR Origin (XR Rig)").transform;
        agent = GetComponent<NavMeshAgent>();
        parameter.animator.applyRootMotion = false;
        agent.stoppingDistance = attackRange;
        
    }

    void Start()
    {
        states.Add(StateType.Idle, new IdleState(this));
        states.Add(StateType.Chase, new ChaseState(this));
        states.Add(StateType.AttackCon, new AttackConState(this));
        states.Add(StateType.Attack1, new Attack1State(this));
        states.Add(StateType.Attack2, new Attack2State(this));
        states.Add(StateType.Attack3, new Attack3State(this));
        states.Add(StateType.Hit, new HitState(this));
        states.Add(StateType.Death, new DeathState(this));

        TransitionState(StateType.Idle);

        parameter.animator = transform.GetComponent<Animator>();
    }

    void Update()
    {
        currentState.OnUpdate();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            parameter.getHit = true;
        }
    }

    public void TransitionState(StateType type)
    {
        if (currentState != null)
            currentState.OnExit();
        currentState = states[type];
        currentState.OnEnter();
    }

    public void ResetAttack()
    {
        alreadyAttacked = false;
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}