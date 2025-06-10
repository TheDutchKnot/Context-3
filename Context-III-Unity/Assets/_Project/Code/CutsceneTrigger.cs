using System.Collections;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] GameObject bookPrefab;
    [SerializeField] GameObject spawnPos;
    [SerializeField] float bookCount;

    [SerializeField] GameObject dialogueBox;

    //time between books spawning
    [SerializeField] float waitTime;

    //how big the circle is the books spawn in
    [SerializeField] float spawnRadius;


    private void Update()
    {
        //put here if the boss has died, then remove the // infront of the other 2 lines
        if (Input.GetMouseButtonDown(0))
        {
          //  StartCoroutine(SpawnBooks());
           // dialogueBox.SetActive(true);

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
            Vector3 finalSpawnPos = spawnPos.transform.position + offset;

            Instantiate(bookPrefab, finalSpawnPos, Quaternion.identity);
            yield return new WaitForSeconds(waitTime);
        }
    }

    
}
