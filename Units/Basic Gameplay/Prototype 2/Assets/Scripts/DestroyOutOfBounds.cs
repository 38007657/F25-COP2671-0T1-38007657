using UnityEngine;

public class DestroyOutOfBounds : MonoBehaviour
{
    // variables for destruction bounds
    private float topBound = 30;
    private float lowerBound = -10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if statement checks if food projectile has moved outside the bounds of our scene and destroys it if true
        if (transform.position.z > topBound)
        {
            Destroy(gameObject);
        } else if (transform.position.z < lowerBound) {
            Debug.Log("Game Over!");
            Destroy(gameObject);
        }
    }
}
