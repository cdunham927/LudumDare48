using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollider : MonoBehaviour
{
    public Rigidbody2D bod;
    public ItemController parent;
    public float minHitVelocity;
    public float dmg;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && parent.thrown && bod.velocity.magnitude >= minHitVelocity)
        {
            collision.rigidbody.AddForce(transform.up * bod.velocity / 3);
            collision.gameObject.GetComponent<ZombieController>().TakeDamage(dmg);
        }
    }
}
