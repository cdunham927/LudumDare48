using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    public ScriptableFloat maxHp;
    public ScriptableFloat hp;
    public ScriptableFloat maxStamina;
    public ScriptableFloat stamina;
    public float spd;
    public float runSpd;
    float curSpd;
    public float stamRecovery;
    Rigidbody2D bod;
    Vector2 input;
    public enum item { none, shovel, gun, corpse }
    public item curItem;
    //Current time digging a hole
    float shovelTime;
    //Current time burying a body
    float buryTime;
    //Time it takes to equip an item
    public float equipTime;
    //Time it takes to dig or fill a hole
    public float shovelCools;
    //Time between gun shots
    public float gunCools;
    //Time it takes to bury a body
    public float corpseCools;
    float cools;
    float shCools;
    float iframes;
    public float iframeTime;
    bool canMove = true;
    public bool canInteract;
    public bool canPickup;
    //[HideInInspector]
    public GraveController closestGrave;
    public ItemController closestItem;
    bool interacting;
    public float curThrow;
    public float minThrowForce;
    public float maxThrowForce;
    public float throwLerping;

    //Interact UI
    public GameObject interactParent;
    public Image interactUI;
    public float lerpSpd = 7f;

    //Gun shooty mcfuck
    public GameObject bullet;
    GameObject gun;
    List<GameObject> bulList = new List<GameObject>();

    //Corpse mcshit
    public ItemController curCorpse;

    public GameObject playerSprite;

    GameController cont;

    public GameObject itemLocation;

    public float goodAcc;
    public float badAcc;
    float curAcc;

    AudioSource src;
    public AudioClip hitClip;
    public AudioClip[] throwClip;
    [Range(0, 1)]
    public float hitVolume;
    [Range(0, 1)]
    public float throwVolume;
    [Range(0, 1)]
    public float shootVolume;
    public AudioClip shootClip;
    //Dig clip for digging and filling hole
    //public AudioClip digClip;
    //public AudioClip buryClip;

    public float shakeDuration = 0.3f;
    public float shakeAmplitude = 1.2f;
    public float shakeFrequency = 2.0f;
    float shakeElapsedTime = 0f;
    public CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;

    private void Awake()
    {
        if (virtualCamera != null) virtualCameraNoise = virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        src = GetComponent<AudioSource>();
        bod = GetComponent<Rigidbody2D>();
        cont = FindObjectOfType<GameController>();
    }

    private void OnEnable()
    {
        Heal();
        stamina.val = maxStamina.val;
        curThrow = minThrowForce;
    }

    public void Heal()
    {
        hp.val = maxHp.val;
    }

    private void Update()
    {
        if (!cont.lost)
        {
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            //itemLocation.transform.localPosition = new Vector3(0, -0.15f, 0);

            if (Input.GetButton("Run") && stamina.val > 0)
            {
                stamina.val -= Time.deltaTime;
                curSpd = runSpd;
                curAcc = badAcc;
            }
            else
            {
                curSpd = spd;
                curAcc = goodAcc;

                if (stamina.val < maxStamina.val)
                {
                    stamina.val += Time.deltaTime * stamRecovery;
                }
            }

            if (shCools > 0) shCools -= Time.deltaTime;
            if (iframes > 0) iframes -= Time.deltaTime;

            if (Input.GetMouseButton(0))
            {
                switch (curItem)
                {
                    case item.none:
                        break;
                    case item.shovel:
                        Shovel();
                        break;
                    case item.gun:
                        Gun();
                        break;
                    case item.corpse:
                        Corpse();
                        break;
                }
            }

            //Shake camera
            if (shakeElapsedTime > 0)
            {
                virtualCameraNoise.m_AmplitudeGain = shakeAmplitude;
                virtualCameraNoise.m_FrequencyGain = shakeFrequency;

                shakeElapsedTime -= Time.deltaTime;
            }
            else
            {
                virtualCameraNoise.m_AmplitudeGain = 0f;
                shakeElapsedTime = 0f;
            }

            //Charge throw
            if (Input.GetMouseButton(1))
            {
                if (curItem != item.none) ChargeThrow();
            }

            //Throw current item
            if (Input.GetMouseButtonUp(1))
            {
                if (curItem != item.none && closestItem != null)
                {
                    //closestItem.thrown = true;
                    //closestItem.transform.SetParent(null);
                    //closestItem.equipped = false;
                    //Rigidbody2D iBod = closestItem.GetComponent<Rigidbody2D>();
                    //Vector3 dirrr = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                    //iBod.AddForce(curThrow * dirrr);
                    //closestItem = null;
                    //curItem = item.none;
                    //curThrow = minThrowForce;
                    Throw();
                }
            }

            if (Input.GetButtonDown("Pickup") && canPickup && closestItem != null && !closestItem.stopInteracting)
            {
                curItem = closestItem.item;
                closestItem.equipped = true;
                closestItem.transform.SetParent(itemLocation.transform);

                if (closestItem.item == item.gun)
                {
                    closestItem.SwitchGunSprite();
                    gun = closestItem.bulSpawn;
                }

                if (closestItem.item == item.corpse)
                {
                    curCorpse = closestItem;
                }
            }

            if (interacting)
            {
                switch (curItem)
                {
                    case item.shovel:
                        if (cools <= shovelCools)
                        {
                            cools += Time.deltaTime;
                            //interactUI.fillAmount = Mathf.Lerp(interactUI.fillAmount, cools / shovelCools, lerpSpd * Time.deltaTime);
                            interactUI.fillAmount = cools / corpseCools;
                        }
                        else
                        {
                            closestGrave.Interact();
                            interacting = false;
                            canMove = true;
                            cools = 0f;
                        }
                        break;
                    case item.corpse:
                        if (cools <= corpseCools)
                        {
                            cools += Time.deltaTime;
                            //interactUI.fillAmount = Mathf.Lerp(interactUI.fillAmount, cools / corpseCools, lerpSpd * Time.deltaTime);
                            interactUI.fillAmount = cools / corpseCools;
                        }
                        else
                        {
                            closestGrave.Interact();
                            curCorpse.myGrave = closestGrave;
                            curCorpse.GetComponent<SpriteRenderer>().sortingOrder = -2;
                            curCorpse.transform.SetParent(null);
                            curCorpse.childCollider.enabled = false;
                            curCorpse.equipped = false;
                            curCorpse.buried = true;
                            curCorpse.transform.position = closestGrave.transform.position;
                            curCorpse.stopInteracting = true;
                            interacting = false;
                            canMove = true;
                            cools = 0f;
                            closestItem = null;
                            curItem = item.none;
                        }
                        break;
                }
            }

            interactParent.SetActive(interacting);
        }
    }

    public void Throw()
    {
        if (closestItem.item == item.gun) closestItem.SwitchGunSprite();
        src.volume = throwVolume;
        src.PlayOneShot(throwClip[Random.Range(0, throwClip.Length)]);
        closestItem.thrown = true;
        closestItem.transform.SetParent(null);
        closestItem.equipped = false;
        Rigidbody2D iBod = closestItem.GetComponent<Rigidbody2D>();
        Vector3 dirrr = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        iBod.AddForce(curThrow * dirrr);
        closestItem = null;
        curItem = item.none;
        curThrow = minThrowForce;
    }

    private void FixedUpdate()
    {
        if (!cont.lost)
        {
            if (canMove)
            {
                if (input.x != 0)
                {
                    bod.AddForce(Vector2.right * input.x * curSpd * Time.fixedDeltaTime);
                }
                if (input.y != 0)
                {
                    bod.AddForce(Vector2.up * input.y * curSpd * Time.fixedDeltaTime);
                }
            }

            if (!canMove)
            {
                if (input.x != 0)
                {
                    interacting = false;
                    canMove = true;
                }
                if (input.y != 0)
                {
                    interacting = false;
                    canMove = true;
                }
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        playerSprite.transform.rotation = Quaternion.Lerp(playerSprite.transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * lerpSpd);
        //itemLocation.transform.rotation = Quaternion.Lerp(playerSprite.transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * lerpSpd);
    }

    void ChargeThrow()
    {
        if (curThrow < maxThrowForce) curThrow += Time.deltaTime * throwLerping;
    }

    public void TakeDamage(float amt)
    {
        if (iframes <= 0)
        {
            shakeElapsedTime = shakeDuration;
            src.volume = hitVolume;
            src.PlayOneShot(hitClip);
            hp.val -= amt;
            iframes = iframeTime;

            if (hp.val <= 0) Die();
        }
    }

    void Die()
    {
        //Play Wilhelm scream
        playerSprite.GetComponent<SpriteRenderer>().sprite = null;
        GetComponent<Collider2D>().enabled = false;
        cont.Lose();
    }

    void Shovel()
    {
        if (canInteract && closestGrave != null && (closestGrave.curState == GraveController.gravestates.notdug || closestGrave.curState == GraveController.gravestates.corpsified)) 
        {
            //src.PlayOneShot(digClip);
            cools = 0;
            canMove = false;
            interacting = true;
        }
    }

    void Gun()
    {
        if (shCools <= 0)
        {
            src.volume = shootVolume;
            src.PlayOneShot(shootClip);
            GameObject newBul = GetBullet();
            newBul.transform.position = gun.transform.position;
            newBul.transform.rotation = gun.transform.rotation * Quaternion.Euler(0, 0, -90 + Random.Range(-curAcc, curAcc));
            newBul.SetActive(true);
            shCools = gunCools;
        }
    }

    public GameObject GetBullet()
    {
        for (int i = 0; i < bulList.Count; i++)
        {
            if (!bulList[i].activeInHierarchy) return bulList[i];
        }

        GameObject bul = Instantiate(bullet);
        bulList.Add(bul);
        bul.SetActive(false);

        return bul;
    }

    void Corpse()
    {
        if (canInteract && closestGrave != null && closestGrave.curState == GraveController.gravestates.dug)
        {
            //src.PlayOneShot(buryClip);
            cools = 0;
            canMove = false;
            interacting = true;
        }
    }
}
