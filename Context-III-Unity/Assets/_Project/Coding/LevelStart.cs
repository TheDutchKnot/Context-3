using UnityEngine;

public class LevelStart : MonoBehaviour
{
    [SerializeField] GameObject firstLight;

   
    void Start()
    {
        firstLight.SetActive(true);
    }

}
