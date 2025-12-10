using UnityEngine;

public class FrankFoxes : MonoBehaviour
{
    public float speed = 0.1f;
    public float chaseRadius = 200f;
    public float bumpForce = 1.5f;

    Transform target;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) target = p.transform;
    }

    void FixedUpdate()
    {
        if (target == null) return;

        Vector2 toPlayer = (Vector2)target.position - rb.position;

        if (toPlayer.magnitude <= chaseRadius)
        {
            float currentSpeed = speed;

            if (ScoreManager.instance != null)
            {
                currentSpeed = speed + ScoreManager.instance.score * 0.0008f;
                currentSpeed = Mathf.Min(currentSpeed, speed * 0.8f); // speed cap
            }

            Vector2 move = toPlayer.normalized * speed;
            rb.linearVelocity = move;
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.CompareTag("Wall"))
        {
            // simple bounce off wall
            Vector2 normal = c.contacts[0].normal;
            rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, normal) * 0.7f;
        }
        else if (c.collider.CompareTag("Enemy"))
        {
            // gently separate foxes
            Vector2 push = (rb.position - (Vector2)c.collider.transform.position).normalized;
            rb.AddForce(push * bumpForce, ForceMode2D.Impulse);
        }
        else if (c.collider.CompareTag("Player"))
        {
            HealthManager.instance?.TakeHit();
        }
    }
}