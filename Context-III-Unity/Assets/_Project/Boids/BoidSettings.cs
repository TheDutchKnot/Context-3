using UnityEngine;

[CreateAssetMenu(menuName = "BoidSettings")]
public class BoidSettings : ScriptableObject
{
    #region Inspector Fields
    [field: Header("Detection")]
    [field: SerializeField] public float perceptionRadius { get; private set; } = 2.5f;
    [field: SerializeField] public float avoidanceRadius { get; private set; } = 1;

    [field: Space(10)]
    [field: SerializeField] public LayerMask collisionMask { get; private set; } = 0;
    [field: SerializeField] public bool hitBackfaces { get; private set; } = false;
    [field: SerializeField] public bool hitTriggers { get; private set; } = false;
    [field: SerializeField] public bool hitMultiFace { get; private set; } = false;
    [field: SerializeField] public float collisionRadius { get; private set; } = 1;
    [field: SerializeField] public float collisionRange { get; private set; } = 1;

    [field: Header("Priority")]
    [field: SerializeField] public float seperationWeight { get; private set; } = 2.5f;
    [field: SerializeField] public float alignmentWeight { get; private set; } = 2;
    [field: SerializeField] public float cohesionWeight { get; private set; } = 1;
    [field: SerializeField] public float collisionWeight { get; private set; } = 12.5f;
    [field: SerializeField] public float targetWeight { get; private set; } = 1;

    [field: Header("Velocity")]
    [field: SerializeField] public float minSpeed { get; private set; } = 5;
    [field: SerializeField] public float maxSpeed { get; private set; } = 8;

    [field: Header("Steering")]
    [field: SerializeField] public float maxSteer { get; private set; } = 2;
    #endregion
}
