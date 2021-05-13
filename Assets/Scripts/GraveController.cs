using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveController : MonoBehaviour
{
    public bool graveReady = false;
    public SpriteRenderer rend;
    public SpriteRenderer childRend;
    public Sprite hole;
    public Sprite filled;

    PlayerController player;
    GameController cont;

    public float digScore;
    public float fillScore;
    public bool hasCorpse = false;

    //3 cases: not dug, dug but not filled, and filled
    public enum gravestates { notdug, dug, corpsified, filled }
    public gravestates curState;

    PlayerController.item requiredItem;

    AudioSource src;
    public AudioClip digClip;
    public AudioClip buryClip;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
        requiredItem = PlayerController.item.shovel;
        player = FindObjectOfType<PlayerController>();
        cont = FindObjectOfType<GameController>();
    }

    public void Interact()
    {
        switch(curState)
        {
            case gravestates.notdug:
                DigHole();
                break;
            case gravestates.dug:
                BuryCorpse();
                break;
            case gravestates.corpsified:
                FillHole();
                break;
            case gravestates.filled:
                EmptyHole();
                break;
        }
    }

    private void OnEnable()
    {
        childRend.sprite = null;
    }

    void DigHole()
    {
        if (player.curItem == PlayerController.item.shovel)
        {
            src.PlayOneShot(digClip);
            requiredItem = PlayerController.item.corpse;
            curState = gravestates.dug;
            cont.AddScore(digScore);
            childRend.sprite = hole;
            childRend.sortingOrder = -3;
        }
    }

    public void BuryCorpse()
    {
        if (player.curItem == PlayerController.item.corpse && !hasCorpse)
        {
            src.PlayOneShot(buryClip);
            requiredItem = PlayerController.item.shovel;
            //Debug.Log("Buried body");
            hasCorpse = true;
            curState = gravestates.corpsified;
        }
    }

    void FillHole()
    {
        if (player.curItem == PlayerController.item.shovel && hasCorpse)
        {
            src.PlayOneShot(digClip);
            requiredItem = PlayerController.item.shovel;
            curState = gravestates.filled;
            cont.AddScore(fillScore);
            childRend.sprite = filled;
            childRend.sortingOrder = -1;
        }
    }

    public void EmptyHole()
    {
        //src.PlayOneShot(emptyGraveClip);
        requiredItem = PlayerController.item.corpse;
        hasCorpse = false;
        curState = gravestates.dug;
        childRend.sprite = hole;
        childRend.sortingOrder = -3;
    }

    private void Update()
    {
        rend.color = (player.canInteract && player.closestGrave == this && player.curItem == requiredItem && curState != gravestates.filled) ? Color.green : Color.white;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player.canInteract = true;
            player.closestGrave = this;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && player.closestGrave != this)
        {
            player.canInteract = true;
            player.closestGrave = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && player.closestGrave == this)
        {
            player.canInteract = false;
            player.closestGrave = null;
        }
    }
}
