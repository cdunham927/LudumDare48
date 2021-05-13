using UnityEngine;

public class ItemController : MonoBehaviour
{
    protected PlayerController player;
    public PlayerController.item item;
    public bool equipped = false;

    [Space]
    [Header("Gun stuff")]
    public GameObject bulSpawn;

    public float lerpSpd = 10f;

    [Space]
    [Header("Zombie/Coffin Stuff")]
    float regenCools;
    public float slowRegen;
    public float fastRegen;
    public float timeToRegen;
    public float vibrateThreshold;
    public bool buried;
    public GraveController myGrave;
    public bool stopInteracting = false;
    public Collider2D childCollider;

    GameController cont;
    public bool hasSpawned = false;

    public bool thrown = false;

    public Sprite gunFlat;
    public Sprite gunTop;

    SpriteRenderer rend;
    Animator anim;

    [Range(0, 2)]
    public float spawnSpread;

    private void Awake()
    {
        if (item == PlayerController.item.corpse) anim = GetComponent<Animator>();
        cont = FindObjectOfType<GameController>();
        player = FindObjectOfType<PlayerController>();
        rend = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        buried = false;
        hasSpawned = false;
        stopInteracting = false;
        regenCools = 0f - Random.Range(0, spawnSpread);
        if (childCollider != null) childCollider.enabled = true;
        GetComponent<SpriteRenderer>().sortingOrder = 1;
    }

    public void SwitchGunSprite()
    {
        if (rend.sprite == gunFlat) rend.sprite = gunTop;
        else rend.sprite = gunFlat;
    }

    private void Update()
    {
        if (equipped)
        {
            Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * lerpSpd);

            transform.localPosition = Vector3.zero;
        }

        if (item == PlayerController.item.corpse)
        {
            regenCools += Time.deltaTime * ((buried) ? slowRegen : fastRegen);

            if (regenCools >= vibrateThreshold) anim.Play("Vibrate");

            if (regenCools >= timeToRegen)
            {
                if (!hasSpawned)
                {
                    if (player.closestItem == this) player.Throw();
                    GameObject zom = cont.GetZombie();
                    zom.transform.position = transform.position;
                    zom.transform.rotation = transform.rotation;
                    zom.SetActive(true);
                    hasSpawned = true;
                    if (buried) zom.GetComponent<ZombieController>().myGrave = myGrave;
                    cont.PlayZombie();
                }

                gameObject.SetActive(false);
                if (buried) myGrave.EmptyHole();
            }
        }

        if (thrown) Invoke("NotThrown", 0.3f);
    }

    void NotThrown()
    {
        thrown = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && player.curItem == PlayerController.item.none && !stopInteracting)
        {
            player.canPickup = true;
            player.closestItem = this;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && player.closestItem != this && player.curItem == PlayerController.item.none && !stopInteracting)
        {
            player.canPickup = true;
            player.closestItem = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && player.closestItem == this && !stopInteracting)
        {
            player.canPickup = false;
            player.closestItem = null;
        }
    }
}
