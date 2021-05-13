using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class GameController : MonoBehaviour
{
    public float score;
    public Text scoreText;
    float curScore;
    public float lerpSpd;

    public GameObject zombie;
    public GameObject coffin;
    List<GameObject> zombieList = new List<GameObject>();
    List<GameObject> coffinList = new List<GameObject>();

    public GameObject[] coffinSpawns;
    public float timeBetweenSpawns;
    float spawnCools;
    public float spawnIncreaseRate;

    public LayerMask enemyMask;
    public float radius;
    bool canSpawn;

    public bool lost;
    public GameObject lostObj;
    Transform curSpawn;

    public GameObject pauseObj;

    AudioSource src;
    [Range(0, 1)]
    public float bellVolume;
    public AudioClip[] bellClip;
    public AudioClip zombieClip;
    [Range(0, 1)]
    public float zombieVolume;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
        lost = false;

        spawnCools = timeBetweenSpawns;

        for (int i = 0; i < 3; i++)
        {
            SpawnCoffin();
        }
    }

    public void PlayZombie()
    {
        src.volume = zombieVolume;
        src.PlayOneShot(zombieClip);
    }

    private const string TWITTER_ADDRESS = "http://twitter.com/intent/tweet";
    private const string TWEET_LANGUAGE = "en";

    public void ShareToTwitter(string textToDisplay)
    {
        Application.OpenURL(TWITTER_ADDRESS +
                    "?text=" + WWW.EscapeURL(textToDisplay) +
                    "&amp;lang=" + WWW.EscapeURL(TWEET_LANGUAGE));
    }

    void SpawnCoffin()
    {
        //Debug.Log("Finding spawn point");
        canSpawn = false;

        for (int i = 0; i < coffinSpawns.Length; i++) 
        {
            if (!canSpawn)
            {
                Collider2D hit = Physics2D.OverlapCircle(coffinSpawns[i].transform.position, radius, enemyMask);
                if (hit == null)
                {
                    canSpawn = true;
                    curSpawn = coffinSpawns[i].transform;
                    break;
                }
            }
        }

        if (canSpawn)
        {
            GameObject cof = GetCoffin();
            cof.transform.position = curSpawn.position;
            cof.transform.rotation = Quaternion.Euler(0, 0, 90);
            cof.SetActive(true);
            spawnCools = timeBetweenSpawns;
            src.volume = bellVolume;
            src.PlayOneShot(bellClip[Random.Range(0, bellClip.Length)]);
        }
        else
        {
            Lose();
        }
    }

    public void Lose()
    {
        pauseObj.SetActive(false);
        lost = true;
        lostObj.SetActive(true);
    }

    public GameObject GetZombie()
    {
        for (int i = 0; i < zombieList.Count; i++)
        {
            if (!zombieList[i].activeInHierarchy) return zombieList[i];
        }

        GameObject zom = Instantiate(zombie);
        zombieList.Add(zom);
        zom.SetActive(false);

        return zom;
    }

    public GameObject GetCoffin()
    {
        for (int i = 0; i < coffinList.Count; i++)
        {
            if (!coffinList[i].activeInHierarchy) return coffinList[i];
        }

        GameObject cof = Instantiate(coffin);
        coffinList.Add(cof);
        cof.SetActive(false);

        return cof;
    }

    private void Update()
    {
        curScore = Mathf.Lerp(curScore, score, lerpSpd * Time.deltaTime);
        scoreText.text = "Score: " + Mathf.RoundToInt(curScore).ToString();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseObj.SetActive(!pauseObj.activeInHierarchy);
        }

        timeBetweenSpawns -= Time.deltaTime * spawnIncreaseRate;
        if (spawnCools > 0) spawnCools -= Time.deltaTime;
        if (spawnCools <= 0) SpawnCoffin();
    }

    public void AddScore(float amt)
    {
        score += amt;
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public Slider musicSlider;
    public Slider soundSlider;
    public AudioMixer musicMixer;
    public AudioMixer soundMixer;

    public void ChangeMusicVolume()
    {
        musicMixer.SetFloat("Volume", musicSlider.value);
    }

    public void ChangeSoundVolume()
    {
        soundMixer.SetFloat("Volume", soundSlider.value);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
