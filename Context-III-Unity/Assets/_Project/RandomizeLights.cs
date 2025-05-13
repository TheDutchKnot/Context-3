using UnityEngine;

public class RandomizeLights : MonoBehaviour
{
    [ContextMenu("Randomize")]
    public void RandomizeLight()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (Random.Range(0, 4) == 1)
            {
                transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}
