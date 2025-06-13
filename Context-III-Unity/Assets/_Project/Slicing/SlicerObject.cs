using UnityEngine;
using EzySlice;
using System;

public interface ISlicedCallBack
{
    public Action OnSlice { get; set; }
}

public class SlicerObject : MonoBehaviour
{
    [SerializeField] Transform startSlicePoint, endSlicePoint;
    
    public FSM fsm;

    [SerializeField] Material crossSliceMaterial;

    [SerializeField] VelocityEstimator velEst;

    [SerializeField] LayerMask cuttableMask;

    [SerializeField] float sliceForce = 10;

    void FixedUpdate()
    {
        if (Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, cuttableMask))
        {
            if (hit.transform.TryGetComponent(out ISlicedCallBack c))
            {
                c.OnSlice?.Invoke();
            }

            Slice(hit.transform.gameObject);
        }
    }

    void Slice(GameObject obj)
    {
        Debug.Log("Slice：" + obj.name);
        var velocity = velEst.GetVelocityEstimate();

        var planeNormal = Vector3.Cross(
            endSlicePoint.position - startSlicePoint.position, velocity);

        planeNormal.Normalize();

        SlicedHull hull = obj.Slice(endSlicePoint.position, planeNormal);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(obj, crossSliceMaterial);
            if (obj.CompareTag("Boss"))
            {
                upperHull.transform.SetParent(null); 
                upperHull.transform.position = obj.transform.position;
                upperHull.transform.rotation = obj.transform.rotation;
                upperHull.transform.localScale = obj.transform.lossyScale; 
            }

            SetupSlicedObject(upperHull);

            GameObject lowerHull = hull.CreateLowerHull(obj, crossSliceMaterial);
            if (obj.CompareTag("Boss"))
            {
                lowerHull.transform.SetParent(null);
                lowerHull.transform.position = obj.transform.position;
                lowerHull.transform.rotation = obj.transform.rotation;
                lowerHull.transform.localScale = obj.transform.lossyScale;
            }
            
            SetupSlicedObject(lowerHull);

            if (!obj.CompareTag("Boss"))
            {
                Destroy(obj);
            }
            else
            {
                obj.layer = LayerMask.NameToLayer("Default");
                string hitName = obj.name;
                if (Enum.TryParse<HitPart>(hitName, true, out HitPart part))
                {
                    fsm.parameter.lastHitPart = part;
                    fsm.parameter.getHit = true;
                }
                else
                {
                    part = HitPart.None;
                    Debug.LogWarning($"Unknown hit part \"{hitName}\", defaulting to None.");
                }
            }
        }
    }

    void SetupSlicedObject(GameObject obj)
    {
        var col = obj.AddComponent<MeshCollider>();
        col.convex = true;

        var rb = obj.AddComponent<Rigidbody>();
        rb.AddExplosionForce(sliceForce,
            obj.transform.position, 1);
    }
}