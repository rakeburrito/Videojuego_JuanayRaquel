using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    public float speed = 2f;
    public Transform leftBound;
    public Transform rightBound;
    private bool movingRight = true;

    void Update()
    {
        if (leftBound == null || rightBound == null) return;

        if (movingRight)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            if (transform.position.x >= rightBound.position.x) movingRight = false;
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
            if (transform.position.x <= leftBound.position.x) movingRight = true;
        }
    }
}