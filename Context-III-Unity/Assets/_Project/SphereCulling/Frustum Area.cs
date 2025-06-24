using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FrustumArea : MonoBehaviour
{
    const float fogOffset = 2.5f;

    [SerializeField] Transform backgroundSphere;

    [SerializeField] float RenderDistance = 15;
    [SerializeField] float AdjustmentSpeed = 6;

    [SerializeField] float fogOffsetExtra = 0f;

    Camera cam;

    static IEnumerator fadeIntrpl;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Start()
    {
        cam = Camera.main;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            if (fadeIntrpl != null)
                StopCoroutine(fadeIntrpl);

            fadeIntrpl = Interpolate(
                backgroundSphere.transform.localScale.x,
                RenderDistance,
                AdjustmentSpeed,

                SetDrawDistance);

            StartCoroutine(fadeIntrpl);
        }
    }

    IEnumerator Interpolate(float from, float to, float time, Action<float> result)
    {
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            result?.Invoke(Mathf.Lerp(from, to, elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void SetDrawDistance(float radius)
    {
        backgroundSphere.transform.localScale = new Vector3(radius, radius, radius);
        RenderSettings.fogEndDistance = radius / 2 - fogOffset - fogOffsetExtra;
        RenderSettings.fogStartDistance = radius / 5;
        cam.farClipPlane = radius / 2;
    }
}
