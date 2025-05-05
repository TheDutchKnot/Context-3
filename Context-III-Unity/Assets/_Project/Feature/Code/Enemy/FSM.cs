using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum StateType
{
    Idle,
    Chase,
    AttackCon,
    Attack1,
    Attack2,
    Attack3,
    Hit,
    Death
}

public enum HitPart
{
    None,
    Eye,
    Tentacle
}

[Serializable]
public class Parameter
{
    public int health;
    public float moveSpeed;
    public float chaseSpeed;
    public Animator animator;
    public bool getHit;

    // 攻击恢复和可用性变量
    public bool availableAttack2 = true; // Attack2 是否可用（使用后将置为 false）
    public bool availableAttack3 = true; // Attack3 是否可用（使用后将置为 false）
    public int attack1HealForAttack2 = 0; // 累计 Attack1 次数，用于恢复 Attack2
    public int attack1HealForAttack3 = 0; // 累计 Attack1 次数，用于恢复 Attack3
    
    // record hit part
    public HitPart lastHitPart = HitPart.None;
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
    public float sightRange, attackRange, meleeAttackRange;
    public bool playerInSightRange, playerInAttackRange;

    //Attack

    //attack 2
    public ParticleSystem gravityEffect;
    private Coroutine gravityCoroutine;
    private float pullStrength = 13f;

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

// Start pulling the player toward the boss
    public void StartGravityField()
    {
        Debug.Log("pull player to Boss");

        // 1) Disable the player's own movement script, not the CC
        var playerCtrl = player.GetComponent<PlayerController>();
        if (playerCtrl != null)
            playerCtrl.enabled = false;

        // 2) Play your gravity‐field VFX (particle system on FSM)
        if (gravityEffect != null)
            gravityEffect.Play();

        // 3) Begin the pull coroutine
        gravityCoroutine = StartCoroutine(GravityPullRoutine());
    }

    private IEnumerator GravityPullRoutine()
    {
        var cc = player.GetComponent<CharacterController>();
        float offsetDistance = 2f; // how far in front of the boss
        float stopThreshold = 0.7f; // when to consider “close enough”
        while (true)
        {
            // compute the point in front of the boss
            Vector3 pullTarget = transform.position + transform.forward * offsetDistance;

            // direction from player to that point
            Vector3 dir = pullTarget - player.position;

            float dist = dir.magnitude;
            if (dist > stopThreshold)
            {
                // speed scales with distance (farther = faster)
                float speed = pullStrength * Mathf.Clamp01(dist / attackRange);

                // only pull horizontally (optional) — preserves player's current height:
                Vector3 horizontalDir = new Vector3(dir.x, 0, dir.z).normalized;
                Vector3 move = horizontalDir * speed * Time.deltaTime;

                // if you also want a slight vertical lift/hover, add:
                // float verticalPull = Mathf.Clamp(dir.y, -1f, 1f) * (pullStrength * 0.2f) * Time.deltaTime;
                // move.y = verticalPull;

                cc.Move(move);
            }
            else
            {
               
                yield break;
            }

            yield return null;
        }
    }

// Stop pulling and restore control
    public void StopGravityField()
    {
        Debug.Log("重力场停止。");

        // 1) Stop the pull coroutine
        if (gravityCoroutine != null)
        {
            StopCoroutine(gravityCoroutine);
            gravityCoroutine = null;
        }

        // 2) Re‑enable the player's movement script
        var playerCtrl = player.GetComponent<PlayerController>();
        if (playerCtrl != null)
            playerCtrl.enabled = true;

        // 3) Stop the gravity‐field VFX
        if (gravityEffect != null)
            gravityEffect.Stop();
    }

    public void SmoothLookAt(Transform self, Vector3 targetPos, float maxDegreesPerSecond)
    {
        // 2) Compute the desired forward rotation
        Vector3 dir = (targetPos - self.position).normalized;
        if (dir.sqrMagnitude < 0.001f) return;  // nothing to do

        Quaternion targetRot = Quaternion.LookRotation(dir);

        // 3) Rotate from current toward target by at most maxDegreesPerSecond * deltaTime
        self.rotation = Quaternion.RotateTowards(
            self.rotation,
            targetRot,
            maxDegreesPerSecond * Time.deltaTime
        );
    }
    
    public void SpawnEyeballSwarm()
    {
        //TODO 
        Debug.Log("SpawnEyeballSwarm");
        
    }
    
    public void StartTentacleFlail()
    {
        Debug.Log("触手挥舞开始，玩家需小心躲避或切断触手！");
        // TODO: 启用碰撞盒,
    }
    
    // 关闭触手挥舞效果
    public void StopTentacleFlail()
    {
        Debug.Log("触手挥舞停止。");
        // TODO: 关闭碰撞检测，清理特效
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
    }
}


public static class TimerUtility
{
    /// <summary>
    /// Starts a coroutine on the given MonoBehaviour that waits for <paramref name="seconds"/> 
    /// then invokes <paramref name="onComplete"/>.
    /// </summary>
    public static void WaitAndExecute(this MonoBehaviour mb, float seconds, Action onComplete)
    {
        mb.StartCoroutine(_WaitAndExecute(seconds, onComplete));
    }

    private static IEnumerator _WaitAndExecute(float seconds, Action onComplete)
    {
        yield return new WaitForSeconds(seconds);
        onComplete?.Invoke();
    }

    /// <summary>
    /// Overload for UnityAction if you prefer UnityEvent-style callbacks.
    /// </summary>
    public static void WaitAndExecute(this MonoBehaviour mb, float seconds, UnityAction onComplete)
    {
        mb.StartCoroutine(_WaitAndExecute(seconds, onComplete));
    }

    private static IEnumerator _WaitAndExecute(float seconds, UnityAction onComplete)
    {
        yield return new WaitForSeconds(seconds);
        onComplete?.Invoke();
    }
}