using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class InvincibilityPickup : MonoBehaviour
{
    public static bool IsInvincible = false;

    [Header("Timing")]
    public float invincibleSeconds = 30f;
    public float respawnAfterInvincibleSeconds = 90f;

    [Header("On-map Lifetime (if not picked up)")]
    public float visibleSeconds = 30f;          // how long it stays on-screen uncollected
    public float visibleWarningSeconds = 5f;    // last seconds it blinks faster
    public float normalBlinkSpeed = 0.3f;
    public float fastBlinkSpeed = 1f;

    [Header("Start Delay")]
    public float startDelaySeconds = 60f; // wait 1 minute before first spawn

    [Header("Spawn Points")]
    public Transform spawnPointsParent; // Invincibility spawns
    private Transform[] spawnPoints;

    [Header("Player Tag")]
    public string playerTag = "Player";

    private Collider2D col;
    private SpriteRenderer sr;
    private bool available = true;
    private Coroutine visibleRoutine;
    private Animator pickupAnimator;

    public Animator bonniAnimator; // Reference to the Animator component
    public float warningTime = 5f; // Time before the pickup disappears after being collected

    public AudioClip powerupClip;
    private AudioSource audioSource;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        sr = GetComponent<SpriteRenderer>();

        // collect spawn points from parent
        if (spawnPointsParent != null)
        {
            int count = spawnPointsParent.childCount;
            spawnPoints = new Transform[count];
            for (int i = 0; i < count; i++)
                spawnPoints[i] = spawnPointsParent.GetChild(i);
        }

        pickupAnimator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Hide at start and spawn later
        HidePickup();
        available = false;
        StartCoroutine(InitialSpawnRoutine());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!available) return;
        if (!other.CompareTag(playerTag)) return;

        if (audioSource != null && powerupClip != null)
        {
            audioSource.PlayOneShot(powerupClip);
        }

        if (visibleRoutine != null) StopCoroutine(visibleRoutine);

        StartCoroutine(InvincibilityRoutine());
    }

    IEnumerator InitialSpawnRoutine()
    {
        yield return new WaitForSeconds(startDelaySeconds);
        MoveToRandomSpawn();
        ShowPickup();
        available = true;

        if (visibleRoutine != null) StopCoroutine(visibleRoutine);
        visibleRoutine = StartCoroutine(VisibleLifetimeRoutine());
    }

    IEnumerator InvincibilityRoutine()
    {
        // Grant invincibility
        IsInvincible = true;

        // Hide pickup
        HidePickup();
        available = false;

        yield return new WaitForSeconds(invincibleSeconds - warningTime);

        if (bonniAnimator != null)
        {
            bonniAnimator.SetBool("IsEnding", true);
        }

        yield return new WaitForSeconds(warningTime);

        IsInvincible = false;

        if (bonniAnimator != null)
        {
            bonniAnimator.SetBool("IsEnding", false);
        }

        // Wait before respawn
        yield return new WaitForSeconds(respawnAfterInvincibleSeconds);

        // Respawn somewhere else
        MoveToRandomSpawn();
        ShowPickup();
        available = true;

        //respawn if not collected
        if (visibleRoutine != null) StopCoroutine(visibleRoutine);
        visibleRoutine = StartCoroutine(VisibleLifetimeRoutine());
    }

    IEnumerator VisibleLifetimeRoutine()
    {
        // normal blink while it's available on the map
        if (pickupAnimator != null) pickupAnimator.speed = normalBlinkSpeed;

        // wait until warning window
        float safeTime = Mathf.Max(0f, visibleSeconds - visibleWarningSeconds);
        yield return new WaitForSeconds(safeTime);

        // speed up blinking near the end
        if (pickupAnimator != null) pickupAnimator.speed = fastBlinkSpeed;

        yield return new WaitForSeconds(visibleWarningSeconds);

        // time ran out, hide + schedule respawn
        HidePickup();
        available = false;

        // reset blink speed so next time starts normal
        if (pickupAnimator != null) pickupAnimator.speed = normalBlinkSpeed;

        yield return new WaitForSeconds(respawnAfterInvincibleSeconds);

        MoveToRandomSpawn();
        ShowPickup();
        available = true;

        // restart the lifetime timer again
        visibleRoutine = StartCoroutine(VisibleLifetimeRoutine());
    }

    void MoveToRandomSpawn()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        int idx = Random.Range(0, spawnPoints.Length);
        transform.position = spawnPoints[idx].position;
    }

    void HidePickup()
    {
        if (sr != null) sr.enabled = false;
        col.enabled = false;
    }

    void ShowPickup()
    {
        if (sr != null) sr.enabled = true;
        col.enabled = true;
    }
}