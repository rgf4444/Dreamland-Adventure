using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 3f;
    public int damage = 1;

    private Vector2 direction;

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Damage enemy
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        // Destroy when hitting a wall
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
