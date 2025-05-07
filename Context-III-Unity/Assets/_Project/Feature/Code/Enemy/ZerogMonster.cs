using System;
using System.Collections;
using UnityEngine;

public class ZerogMonster : MonoBehaviour
{
    
    public int health;
    // public Animator animator;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public bool playerInSightRange;
    public float  attackRange;
    private Coroutine gravityCoroutine;
    private float pullStrength = 13f;
    public ParticleSystem gravityEffect;
    private void Awake()
    {
        player = GameObject.Find("XR Origin (XR Rig)").transform;
    }
    
    private void Update()
    {
        if (Physics.CheckSphere(transform.position, attackRange, whatIsPlayer) && health>0)
        {
            StartGravityField();
        }
        else
        {
            StopGravityField();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            health--;
        }
        
    }
    
    // Start pulling the player toward the boss
    public void StartGravityField()
    {
        // 1) Disable the player's own movement script, not the CC
        var playerCtrl = player.GetComponent<PlayerController>();
        if (playerCtrl != null)
            playerCtrl.enabled = false;

        // 2) Play gravity‐field VFX
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

                // a slight vertical lift/hover:
                float verticalPull = Mathf.Clamp(dir.y, -1f, 1f) * (pullStrength * 0.2f) * Time.deltaTime;
                move.y = verticalPull;
                
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
        Debug.Log("stop G attack");

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
    
    
    private void OnTriggerEnter(Collider other)
    {
     
        if (!other.CompareTag("PlayerAttack")) return;
        health--;
    }
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
}
