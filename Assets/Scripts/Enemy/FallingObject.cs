using UnityEngine;

public class FallingObject : MonoBehaviour
{
    public float fallSpeed = 5f;

    private void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y <= -10.0f)
        {
            Destroy(gameObject);
        }
    }
}
