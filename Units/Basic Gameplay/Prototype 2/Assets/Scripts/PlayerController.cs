using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public float horizontalInput;
    public float speed = 10.0f;
    public float xRange = 10;

    // Update is called once per frame
    void Update()
    {
        // Get player horizontal movement
        horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontalInput * Time.deltaTime * speed);

        // Keep the payer in bounds
        if (transform.position.x < -xRange) {
            transform.position = new Vector3(-xRange, transform.position.y, transform.position.z);
        }
        if (transform.position.x > xRange) {
           transform.position = new Vector3(xRange, transform.position.y, transform.position.z);
        }
    }
}
