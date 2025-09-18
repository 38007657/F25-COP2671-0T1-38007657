using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // empty array of GameObjects to hold our animalPrefabs
    public GameObject[] animalPrefabs;

    // values to aide in random animal generation location
    private float spawnRangeX = 20;
    private float spawnPosZ = 20;
    private float startDelay = 2;
    private float spawnInterval = 1.5f;

    // index to aide random animal generation
    // public int animalIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Spawn animals based on an interval
        InvokeRepeating("SpawnRandomAnimal", startDelay, spawnInterval);
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.S)) {
        //   SpawnRandomAnimal();
        // }
    }

    void SpawnRandomAnimal() {
          // Randomly generate animal index and spawn position
        int animalIndex = Random.Range(0, animalPrefabs.Length);
        Vector3 spawnPos = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), 0, spawnPosZ);
        Instantiate(animalPrefabs[animalIndex], spawnPos, animalPrefabs[animalIndex].transform.rotation);
    }
}
