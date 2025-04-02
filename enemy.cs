using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float speed = 100f;
    [SerializeField] private float targetDistance;
    private Player target;
    Rigidbody2D rb;
    SpriteRenderer sr;
    public Vector3 startpoint;
    void Start()
    {
        startpoint = transform.position;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        target = GameObject.FindObjectOfType<Player>();
    }


    void Update()
    {
        if (Vector2.Distance(transform.position, target.transform.position) - targetDistance < 0.1)
        {

            int direct = 1;
            if (transform.position.x > target.transform.position.x)
            {
                direct = -1;
            }
            rb.velocity = new Vector2(direct * speed, rb.velocity.y);
        }
        else if (Vector2.Distance(transform.position, startpoint) < 0.05f)
        {
            rb.velocity = new Vector2(0, 0);
        }
        else
        {
            int direct = 1;
            if (transform.position.x > startpoint.x)
            {
                direct = -1;
            }
            rb.velocity = new Vector2(direct * speed, rb.velocity.y);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
