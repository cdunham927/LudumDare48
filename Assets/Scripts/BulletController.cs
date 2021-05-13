using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float spd;
    Rigidbody2D bod;

    private void Awake()
    {
        bod = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        Invoke("Disable", 2f);
        bod.AddForce(transform.up * spd);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    void Disable()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<ZombieController>().TakeDamage();
            Invoke("Disable", 0.001f);
        }

        if (collision.CompareTag("Wall"))
        {
            Invoke("Disable", 0.001f);
        }
    }
}
