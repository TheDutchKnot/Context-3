using System.Collections;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] Vector3 spawnPos;
    [SerializeField] float bookCount;
    
    //time between books spawning
    [SerializeField] float waitTime;
    
    //how big the circle is the books spawn in
    [SerializeField] float spawnRadius;


    private void Update()
    {
        //put here if the boss has died
        if(Input.GetMouseButtonDown(0))
        {
            StartCoroutine(SpawnBooks());
        }
    }

    IEnumerator SpawnBooks()
    {
        for (int i = 0; i < bookCount; i++)
        {
            // Calculate angle for this book
            float angle = i * Mathf.PI * 2 / bookCount;

            // Determine spawn position in a circle
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * spawnRadius; 
            Vector3 finalSpawnPos = spawnPos + offset;

            Instantiate(prefab, finalSpawnPos, Quaternion.identity);
            yield return new WaitForSeconds(waitTime);
        }
    }

    //somewhere here a thing that makes the dialogue box turn on
}
