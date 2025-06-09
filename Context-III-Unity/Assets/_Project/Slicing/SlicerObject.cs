using UnityEngine;
using EzySlice;
using System;

public interface ISlicedCallBack
{
    public Action OnSlice { get; set; }
}

public class SlicerObject : MonoBehaviour
{
    [SerializeField] 
    Transform startSlicePoint, endSlicePoint;

    [SerializeField] 
    Material crossSliceMaterial;

    [SerializeField]
    VelocityEstimator velEst;

    [SerializeField]
    LayerMask cuttableMask;

    [SerializeField]
    float sliceForce = 10;

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
        Debug.Log("Slice："+obj.name);
        var velocity = velEst.GetVelocityEstimate();

        var planeNormal = Vector3.Cross(
            endSlicePoint.position - startSlicePoint.position, velocity);

        planeNormal.Normalize();

        SlicedHull hull = obj.Slice(endSlicePoint.position, planeNormal);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(obj, crossSliceMaterial);
            SetupSlicedObject(upperHull);

            GameObject lowerHull = hull.CreateLowerHull(obj, crossSliceMaterial);
            SetupSlicedObject(lowerHull);

            Destroy(obj);
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