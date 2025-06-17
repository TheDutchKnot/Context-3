using System;
using System.Collections;
using UnityEngine;

public class DisableAfterSeconds : MonoBehaviour
{
    Material[] materials;

    void Start()
    {
        materials = GetComponent<MeshRenderer>().materials;

        StartCoroutine(Interpolate(1, 0, 5, SetAlpha,
            () => gameObject.SetActive(false)));
    }

    void SetAlpha(float value)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            Color color = materials[i].color;
            color.a = value;
            materials[i].color = color;
        }
    }

    IEnumerator Interpolate(float from, float to, float time, Action<float> result, Action finished)
    {
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            result?.Invoke(Mathf.Lerp(from, to, elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        finished?.Invoke();
    }
}
