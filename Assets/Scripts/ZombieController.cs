using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ZombieController : MonoBehaviour
{
    public float maxHp = 5;
    float hp;
    float iframes;
    public float iframeTime = 0.2f;

    GameController cont;

    public GameObject bleedFab;
    ParticleSystem bleed;
    public int toEmit;

    public GraveController myGrave;

    //State machine stuff
    public float chaseDistance;
    public float attackRange;
    float distance;
    public enum enemystates { chase, attack }
    public enemystates curState;

    //AI stuff
    Transform target;
    public float nextWaypointDistance = 2f;
    public float spd;
    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;
    Seeker seeker;
    Rigidbody2D bod;
    Vector3 startPos;

    float cools;
    public float timeBetweenAttacks;

    public GameObject hit;

    AudioSource src;
    public AudioClip hitClip;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
        cont = FindObjectOfType<GameController>();
        bleed = Instantiate(bleedFab).GetComponent<ParticleSystem>();
        seeker = GetComponent<Seeker>();
        target = FindObjectOfType<PlayerController>().transform;
        bod = GetComponent<Rigidbody2D>();
        startPos = transform.position;

        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    void UpdatePath()
    {
        target = FindObjectOfType<PlayerController>().transform;
        if (target == null)
        {
            seeker.StartPath(bod.position, startPos, OnPathComplete);
            return;
        }

        if (seeker.IsDone()) seeker.StartPath(bod.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
        else Debug.Log("Error making path");
    }

    public void MovePath()
    {
        if (path == null) return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - bod.position).normalized;
        Vector2 force = direction * spd * Time.deltaTime;
        Vector3 dir = path.vectorPath[currentWaypoint] - transform.position;
        transform.right = dir;

        bod.AddForce(force);

        distance = Vector2.Distance(bod.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    private void OnEnable()
    {
        hp = maxHp;
    }

    public void Update()
    {
        MovePath();

        switch (curState)
        {
            case enemystates.chase:
                Chase();
                break;
            case enemystates.attack:
                Attack();
                break;
        }

        if (cools > 0) cools -= Time.deltaTime;
        if (iframes > 0) iframes -= Time.deltaTime;
    }

    public void TakeDamage(float amt = 1)
    {
        if (iframes <= 0)
        {
            src.PlayOneShot(hitClip);
            hp -= amt;
            iframes = iframeTime;
            bleed.gameObject.transform.position = transform.position;
            bleed.Emit(toEmit);

            if (hp <= 0) Die();
        }
    }

    void Die()
    {
        GameObject cof = cont.GetCoffin();
        cof.transform.position = transform.position;
        cof.transform.rotation = transform.rotation;
        cof.SetActive(true);
        cof.GetComponent<ItemController>().myGrave = myGrave;
        gameObject.SetActive(false);
    }

    public void Chase()
    {
        if (distance < attackRange && cools <= 0)
        {
            curState = enemystates.attack;
        }
    }

    public void Attack()
    {
        hit.SetActive(true);
        cools = timeBetweenAttacks;
        curState = enemystates.chase;
    }
}
