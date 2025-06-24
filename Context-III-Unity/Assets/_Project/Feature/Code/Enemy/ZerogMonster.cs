using System;
using System.Collections;
using UnityEngine;

public class ZerogMonster : MonoBehaviour, ISlicedCallBack
{
    public Transform player;
    public LayerMask whatIsPlayer;

    [Header("Pull Settings")]
    public float attackRange;         
    public float pullStrength;

    public float stopThresholdZ;
    // public ParticleSystem gravityEffect;

    [Header("Box Attack Range (Half-extents)")]
    public float boxWidth = 2f;       // total width on X axis 
    public float boxHeight = 1.5f;    // total height on Y axis
    

    [Header("Vision")]
    public float sphereCastRadius = 0.5f;
    public LayerMask obstacleMask;
    public float eyeHeight = 1.5f;

    private Coroutine gravityCoroutine;
    private Animator animator;
    private bool _isEyeOpen, _isDie, _isAttack;
    private AnimatorStateInfo info;
    public Action OnSlice { get; set; }

    public bool MaySlice => true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
        player = GameObject.Find("XR Origin (XR Rig)").transform;
        OnSlice += OnDeath;
    }

    private void Update()
    {
        if (_isDie) return;

        //  box-check 
        Vector3 halfExtents = new Vector3(boxWidth / 2f, boxHeight / 2f, attackRange / 2f);          
        Vector3 boxCenter = transform.position
                            + transform.forward * (attackRange / 2f)  // forward half-depth
                            + Vector3.down   * (boxHeight   / 2f);  // downward half-height

        bool playerInBox = Physics.CheckBox(
            boxCenter,
            halfExtents,
            transform.rotation,
            whatIsPlayer
        );                                                                                          
        if (playerInBox && !_isAttack )                                          
        {
            StartGravityField();
        }
    }

    void OnDeath()
    {
        StopGravityField();
        _isDie = true;
    }

    public void StartGravityField()
    {
        info = animator.GetCurrentAnimatorStateInfo(0);
        if (!_isEyeOpen)
        {
            animator.Play("Start_Open");

            if (info.normalizedTime >= .95f)
                _isEyeOpen = true;

            if (info.normalizedTime >= .35f)
            {
                SetMovement(false);
                // gravityEffect?.Play();
                gravityCoroutine = StartCoroutine(GravityPullRoutine());
            }
        }
    }

    private IEnumerator GravityPullRoutine()
    {
        var cc = player.GetComponent<CharacterController>();
        float offsetDistance = 0.6f;
        

        while (true)
        {
            Vector3 pullTarget = transform.position + transform.forward * offsetDistance;
            Vector3 dir = pullTarget - player.position;

            float distZ = Mathf.Abs(pullTarget.z - player.position.z);
            if (distZ > stopThresholdZ)
            {
                float speedZ = pullStrength * Mathf.Clamp01(distZ / attackRange);

                Vector3 move = Vector3.zero;
                move.z = Mathf.Sign(pullTarget.z - player.position.z) * speedZ * Time.deltaTime;
                move.x = (pullTarget.x - player.position.x) * 0.1f * pullStrength * Time.deltaTime;
                move.y = (pullTarget.y - player.position.y) * 0.1f * pullStrength  * Time.deltaTime;

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

    public void StopGravityField()
    {
        if (gravityCoroutine != null)
        {
            StopCoroutine(gravityCoroutine);
            gravityCoroutine = null;
        }
        SetMovement(true);
        // gravityEffect?.Stop();
    }

    public void SetMovement(bool enable)
    {
        var locoSys = player.Find("Locomotion");
        if (locoSys == null) { Debug.LogWarning("cannot find Locomotion System"); return; }
        var moveGO = locoSys.Find("Move");
        if (moveGO == null) { Debug.LogWarning("cant find Move"); return; }
        moveGO.gameObject.SetActive(enable);
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * eyeHeight;
        Vector3 dir = (target - origin).normalized;
        float dist = Vector3.Distance(origin, target);
        Debug.DrawLine(origin, origin + dir * dist, Color.red);

        if (Physics.SphereCast(origin, sphereCastRadius, dir, out RaycastHit hit, dist, obstacleMask))
            if (!hit.collider.transform.IsChildOf(player))
                return false;

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
        // draw the attack box in yellow for debugging ← Modified
        Vector3 halfExtents = new Vector3(boxWidth / 2f, boxHeight / 2f, attackRange / 2f);
        Vector3 boxCenter = transform.position
                            + transform.forward * (attackRange / 2f)
                            + Vector3.down   * (boxHeight   / 2f);

        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
    }

    private void OnDestroy()
    {
        OnSlice -= OnDeath;
    }
}
