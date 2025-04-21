using UnityEngine;

[CreateAssetMenu(menuName = "BoidSettings")]
public class BoidSettings : ScriptableObject
{
    #region Inspector Fields
    [field: SerializeField] public int MaxCapacity { get; private set; } = 1000;
    [field: SerializeField] public Material Material { get; private set; }
    [field: SerializeField] public Mesh Mesh { get; private set; }
    public int ShaderBufferId { get; private set; }
    public RenderParams Params { get; private set; }

    [field: Header("Detection")]
    [field: SerializeField] public float PerceptionRadius { get; private set; } = 2.5f;
    [field: SerializeField] public float AvoidanceRadius { get; private set; } = 1;

    [field: Space(10)]
    [field: SerializeField] public LayerMask CollisionMask { get; private set; } = 0;
    [field: SerializeField] public bool HitBackfaces { get; private set; } = false;
    [field: SerializeField] public bool HitTriggers { get; private set; } = false;
    [field: SerializeField] public bool HitMultiFace { get; private set; } = false;
    [field: SerializeField] public float CollisionRadius { get; private set; } = 1;
    [field: SerializeField] public float CollisionRange { get; private set; } = 1;

    [field: Header("Priority")]
    [field: SerializeField] public float SeperationWeight { get; private set; } = 2.5f;
    [field: SerializeField] public float AlignmentWeight { get; private set; } = 2;
    [field: SerializeField] public float CohesionWeight { get; private set; } = 1;
    [field: SerializeField] public float CollisionWeight { get; private set; } = 12.5f;
    [field: SerializeField] public float TargetWeight { get; private set; } = 1;

    [field: Header("Velocity")]
    [field: SerializeField] public float MinSpeed { get; private set; } = 5;
    [field: SerializeField] public float MaxSpeed { get; private set; } = 8;

    [field: Header("Steering")]
    [field: SerializeField] public float MaxSteer { get; private set; } = 2;
    #endregion

    void OnEnable()
    {
        ShaderBufferId = Shader.PropertyToID("_BoidsBuffer");

        Params = new(Material)
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one * 100)
        };
    }
}
