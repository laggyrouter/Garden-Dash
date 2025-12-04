using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Carrot : MonoBehaviour
{
    public int scoreValue = 10;
    public float respawnDelay = 5f;
    public bool shouldRespawn = true;

    [Header("Respawn spots")]
    public string spawnParentName = "CarrotSpawns";

    private Transform[] spawnPoints;
    private bool isCollected = false;
    private SpriteRenderer sr;
    private Collider2D col;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            GameObject parentObj = GameObject.Find(spawnParentName);

            if (parentObj != null)
            {
                List<Transform> spots = new List<Transform>();

                foreach (Transform child in parentObj.transform)
                {
                    spots.Add(child);
                }
                spawnPoints = spots.ToArray();
            }
            
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform spot = spawnPoints[Random.Range(0, spawnPoints.Length)];
                transform.position = spot.position;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;

            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.AddScore(scoreValue);
            }

            sr.enabled = false;
            if (col != null)
            {
                col.enabled = false;
            }

            if (shouldRespawn && spawnPoints != null && spawnPoints.Length > 0)
            {
                StartCoroutine(Respawn());
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        Transform spot = spawnPoints[Random.Range(0, spawnPoints.Length)];
        transform.position = spot.position;
        sr.enabled = true;
        if (col != null)
        {
            col.enabled = true;
        }
        isCollected = false;
    }

    //reduce carrots spawning on top of each other

    private void TryFreeSpot()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }

        for (int attempt = 0; attempt < 10; attempt++)
        {
            Transform spot = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Collider2D[] colliders = Physics2D.OverlapCircleAll(spot.position, 0.1f);

            bool spotFree = true;

            foreach (Collider2D col in colliders)
            {
                if (col.CompareTag("Carrot"))
                {
                    spotFree = false;
                    break;
                }
            }
            if (spotFree)
            {
                transform.position = spot.position;
                return;
            }
        }

        transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
    }
}
