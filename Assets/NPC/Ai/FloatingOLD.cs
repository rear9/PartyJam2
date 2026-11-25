using UnityEngine;
using UnityEngine.Rendering;

public class Floating : MonoBehaviour
{

    public float speed = 2f;
    public float hieght = 0.1f; 

    private Vector3 startPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * speed) * hieght;
        transform.position = new Vector3(transform.position.x, newY, 0);
    }
}
