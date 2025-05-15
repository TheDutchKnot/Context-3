using System;
using System.Collections;
using UnityEngine;

public class ZerogMonster : MonoBehaviour
{
    // public Animator animator;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public bool playerInSightRange;
    public float  attackRange;
    private Coroutine gravityCoroutine;
    public float pullStrength;
    public ParticleSystem gravityEffect;
    public Animator animator;
    private bool _isEyeOpen;
    private bool _isDie;
    private bool _isAttack;
    private AnimatorStateInfo info;
    
    [Header("Vision")]
    [Tooltip("Ray thickness")]
    public float sphereCastRadius = 0.5f;

    [Tooltip("obs layer")]
    public LayerMask obstacleMask;

    // monster eye height
    public float eyeHeight = 1.5f;
    
    private void Awake()
    {
        animator.applyRootMotion = false;
        player = GameObject.Find("XR Origin (XR Rig)").transform;
    }

    private void Start()
    {
        _isEyeOpen = false;
        _isDie = false;
        _isAttack = false;
        animator = transform.GetComponent<Animator>();
    }

    private void Update()
    {
        if (!_isDie)
        {
            if (Physics.CheckSphere(transform.position, attackRange, whatIsPlayer)
                && !_isAttack
                && HasLineOfSight())
            {
                StartGravityField();
            }
        }
        
        // for test
        if (Input.GetKeyDown(KeyCode.J))
        {
            StopGravityField();
            _isDie = true;
        }
        
    }
    
    // Start pulling the player toward the boss
    public void StartGravityField()
    {
        info = animator.GetCurrentAnimatorStateInfo(0);
        if (!_isEyeOpen)
        {
            animator.Play("Start_Open");
            
            if (info.normalizedTime >= .95f)
            {
                _isEyeOpen = true;
            }
            
            if (info.normalizedTime >= .35f)
            {
                // 1) Disable the player's own movement script, not the CC
                SetMovement(false);

                // 2) Play gravity‐field VFX
                if (gravityEffect != null)
                    gravityEffect.Play();

                // 3) Begin the pull coroutine
                gravityCoroutine = StartCoroutine(GravityPullRoutine());
            }
        }
    }

    private IEnumerator GravityPullRoutine()
    {
        var cc = player.GetComponent<CharacterController>();
        float offsetDistance = 1.1f; // how far in front of the boss
        float stopThreshold = 0.3f; // when to consider “close enough”
        while (true)
        {
            // compute the point in front of the boss
            Vector3 pullTarget = transform.position + transform.forward * offsetDistance;

            // direction from player to that point
            Vector3 dir = pullTarget - player.position;

            // if has Y pull
            //float dist = dir.magnitude;
            float dist = new Vector3(dir.x, 0, dir.z).magnitude;
            if (dist > stopThreshold)
            {
                // speed scales with distance (farther = faster)
                float speed = pullStrength * Mathf.Clamp01(dist / attackRange);

                // only pull horizontally (optional) — preserves player's current height:
                Vector3 horizontalDir = new Vector3(dir.x, 0, dir.z).normalized;
                Vector3 move = horizontalDir * speed * Time.deltaTime;

                // a slight vertical lift/hover:
                // float verticalPull = Mathf.Clamp(dir.y, -1f, 1f) * (pullStrength * 0.2f) * Time.deltaTime;
                // move.y = verticalPull;
                
                cc.Move(move);
            }
            else
            {
               _isAttack = true;
                yield break;
            }

            yield return null;
        }
    }

// Stop pulling and restore control
    public void StopGravityField()
    {
        Debug.Log("stop G attack");

        // 1) Stop the pull coroutine
        if (gravityCoroutine != null)
        {
            StopCoroutine(gravityCoroutine);
            gravityCoroutine = null;
        }

        // 2) Re‑enable the player's movement script
        SetMovement(true);

        // 3) Stop the gravity‐field VFX
        if (gravityEffect != null)
            gravityEffect.Stop();
    }
    
    public void SetMovement(bool _switch)
    {
        // get Locomotion System
        var locoSys = player.Find("Locomotion");
        if (locoSys == null)
        {
            Debug.LogWarning("cannot find Locomotion System");
            return;
        }
        
        var moveGO = locoSys.Find("Move");
        if (moveGO == null)
        {
            Debug.LogWarning("cant find Move");
            return;
        }
        
        moveGO.gameObject.SetActive(_switch);
    }
    
    private bool HasLineOfSight()
    {
        // 1) start at monster eye
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        // target: player transforme
        Vector3 target  = player.position + Vector3.up * eyeHeight;
        Vector3 dir     = (target - origin).normalized;
        float   dist    = Vector3.Distance(origin, target); 
        Debug.DrawLine(origin, origin + dir * dist, Color.red);
        // create ray
        if (Physics.SphereCast(origin,
                sphereCastRadius,
                dir,
                out RaycastHit hit,
                dist,
                obstacleMask))
        {
            // collider -> obs, not player
            if (!hit.collider.transform.IsChildOf(player))
                return false;
        }
        // no obs
        return true;
    }
    
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerAttack")) return;
        StopGravityField();
        _isDie = true;
    }
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
}
