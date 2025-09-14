using UnityEngine;

public class MoveForward : MonoBehaviour
{
    // controls how fast the food moves
    public float speed = 40.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Make the food fly through the air
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }
}
