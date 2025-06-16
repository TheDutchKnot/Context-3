using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FrustumArea : MonoBehaviour
{
    [SerializeField] float RenderDistance = 15;
    [SerializeField] float AdjustmentSpeed = 6;
    [SerializeField] float fogOffset = 2f;
    [SerializeField] float fogStart = 5;

    Camera cam;

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
            StopAllCoroutines();
            
            StartCoroutine(Interpolate(
                RenderSettings.fogEndDistance,
                RenderDistance - fogOffset,
                AdjustmentSpeed,

                (float value) => RenderSettings.fogEndDistance = value));

            StartCoroutine(Interpolate(
                cam.farClipPlane,
                RenderDistance,
                AdjustmentSpeed,

                (float value) => cam.farClipPlane = value));

            StartCoroutine(Interpolate(
                RenderSettings.fogStartDistance,
                fogStart,
                AdjustmentSpeed,

                (float value) => RenderSettings.fogStartDistance = value));
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
}
