using Tdk.PhysXcastBatchProcessor;
using Tdk.PlayerLoopSystems.Indirect;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace tdk.Boids
{
    public class BoidManager : MonoBehaviour
    {
        [SerializeField] AnimatedIndirectMeshSettings rendererSettings;
        [SerializeField] BoidSettings settings;
        [SerializeField] Transform target;

        [SerializeField]
        BoidSettings[] options;

        NativeList<Boid> boids;
        NativeArray<float3> vel;

        NativeArray<SpherecastCommand> commands;
        NativeArray<RaycastHit> hitResults;
        NativeArray<Matrix4x4> boidTRS;

        AnimatedIndirectMesh renderer;

        [SerializeField] Transform summingvfx;
        Transform trackAnim;

        [SerializeField] VfxHandler particle;
        [SerializeField] Vector3 particleOffset;

        [SerializeField] bool bossSummon = false;

        [SerializeField] AudioSource source;
        bool sounding;

        void Awake()
        {
            vel = new NativeArray<float3>(settings.MaxCapacity, Allocator.Persistent);
            boids = new NativeList<Boid>(settings.MaxCapacity, Allocator.Persistent);

            renderer = rendererSettings.Create();
        }

        [ContextMenu("Boid")]
        public void AddEditorBoid()
        {
            for (int i = 0; i < 50; i++)
            {
                Add(transform);
            }
        }

        public void SetOption(int value)
        {
            settings = options[value];
        }

        public void Add(Transform origin)
        {
            boids.AddNoResize(new Boid
            {
                position = origin.position + (Vector3.up * 0.1f) +UnityEngine.Random.insideUnitSphere * 0.001f,
                direction = Vector3.up
            });

            if (!bossSummon)
                PlayAnimation(origin);
        }

        public void PlayAnimation(Transform origin)
        {
            trackAnim = origin;
            particle.Play();
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        public void ResetTarget()
        {
            target = transform;
            trackAnim = null;
            particle.Stop();
        }

        public void ResetTargetBook() {
            target = transform;
        }

        public int GetBoidCount() => boids.Length;

        void Update()
        {
            if (trackAnim != null)
            {
                summingvfx.transform.position = trackAnim.position + particleOffset;
            }

            if (boids.Length >= 2 && !sounding)
            {
                source.Play();
                sounding = true;
            }
            else if (sounding)
            {
                source.Stop();
                sounding = false;
            }

                using (commands = new NativeArray<SpherecastCommand>(boids.Length, Allocator.TempJob))
                using (hitResults = new NativeArray<RaycastHit>(boids.Length, Allocator.TempJob))
                using (boidTRS = new NativeArray<Matrix4x4>(boids.Length, Allocator.TempJob))
                {
                    var queryJobHandle = PhysXcastBatchProcessor.PerformSpherecasts(commands, hitResults, boids.AsArray(), settings.CollisionMask.value, settings.HitBackfaces, settings.HitTriggers, settings.HitMultiFace, settings.CollisionRange, settings.CollisionRadius);

                    var steerJob = new SteerBoids
                    {
                        boidVelocities = vel,
                        boids = boids.AsArray(),
                        hits = hitResults,

                        perceptionRadius = settings.PerceptionRadius,
                        avoidanceRadius = settings.AvoidanceRadius,

                        seperationWeight = settings.SeperationWeight,
                        alignmentWeight = settings.AlignmentWeight,
                        cohesionWeight = settings.CohesionWeight,

                        collisionWeight = settings.CollisionWeight,
                        targetWeight = settings.TargetWeight,

                        minSpeed = settings.MinSpeed,
                        maxSpeed = settings.MaxSpeed,
                        maxSteer = settings.MaxSteer,

                        targetPosition = target.position,
                        deltaTime = Time.deltaTime
                    };

                    var steerJobHandle = steerJob.Schedule(boids.Length, 1, queryJobHandle);

                    var syncJob = new SyncBoids
                    {
                        Boids = boids.AsArray(),
                        Vel = vel,
                        deltaTime = Time.deltaTime,
                        scale = rendererSettings.Scale,
                        TRS = boidTRS
                    };

                    var syncJobHandle = syncJob.Schedule(boids.Length, 1, steerJobHandle);

                    syncJobHandle.Complete();

                    for (int i = 0; i < boids.Length; i++)
                    {
                        if (hitResults[i].collider != null)
                        {
                            try
                            {
                                if (settings.DeathLayer.Contains(hitResults[i].transform.gameObject.layer))
                                {
                                    boids.RemoveAtSwapBack(i);
                                }
                            }
                            catch (System.Exception e)
                            {
                                Debug.Log(e);
                            }
                        }
                    }

                    renderer.RenderInstancedManual(boidTRS);
                }
        }

        void OnDestroy()
        {
            if (boids.IsCreated) boids.Dispose();
            if (vel.IsCreated) vel.Dispose();
            renderer.Dispose();
        }
    }
}
