using UnityEngine;

public class FrankFoxes : MonoBehaviour
{
    enum FoxState { Idle, Patrol, Chase }

    [Header("Movement")]
    public float speed = 0.3f;

    [Header("Chase")]
    public float chaseEnterRadius = 20f;
    public float chaseExitRadius = 24f;

    [Header("Patrol")]
    public float patrolSpeedMultiplier = 0.5f;
    public float patrolChangeMin = 0.8f;
    public float patrolChangeMax = 2f;

    [Header("Idle")]
    public float idleMin = 0.8f;
    public float idleMax = 2f;
    public float wallHitIdle = 0.1f;

    [Header("Wall Avoidance")]
    public float wallAvoidTime = 0.2f;
    public float wallSlideSpeed = 0.35f;

    [Header("Patrol Avoidance")]
    public LayerMask wallLayer;
    public float lookAhead = 0.25f;

    [Header("Separation")]
    public float separationRadius = 0.35f;
    public float separationStrength = 0.8f;
    public LayerMask enemyLayer;

    [Header("Bump")]
    public float bumpForce = 1.5f;

    Rigidbody2D rb;
    Collider2D col;
    Transform target;

    FoxState state = FoxState.Patrol;
    Vector2 patrolDir = Vector2.right;

    float stateTimer;
    float wallAvoidTimer;
    Vector2 wallAvoidDir;

    ContactFilter2D wallFilter;
    readonly RaycastHit2D[] castHits = new RaycastHit2D[8];

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        wallFilter = new ContactFilter2D();
        wallFilter.useLayerMask = true;
        wallFilter.layerMask = wallLayer;
        wallFilter.useTriggers = false;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) target = p.transform;

        EnterPatrol();
    }

    void FixedUpdate()
    {
        wallAvoidTimer -= Time.fixedDeltaTime;
        if (wallAvoidTimer > 0f)
        {
            rb.linearVelocity = wallAvoidDir * wallSlideSpeed;
            return;
        }

        if (target == null) return;

        Vector2 toPlayer = (Vector2)target.position - rb.position;
        float dist = toPlayer.magnitude;

        if (state != FoxState.Chase && dist <= chaseEnterRadius) state = FoxState.Chase;
        else if (state == FoxState.Chase && dist > chaseExitRadius) EnterPatrol();

        Vector2 desired = Vector2.zero;

        switch (state)
        {
            case FoxState.Idle:
                stateTimer -= Time.fixedDeltaTime;
                if (stateTimer <= 0f) EnterPatrol();
                break;

            case FoxState.Patrol:
                stateTimer -= Time.fixedDeltaTime;

                if (Blocked(patrolDir))
                {
                    PickOpenDirection();
                    stateTimer = Random.Range(patrolChangeMin, patrolChangeMax);
                }

                if (stateTimer <= 0f)
                {
                    PickOpenDirection();
                    stateTimer = Random.Range(patrolChangeMin, patrolChangeMax);
                }

                desired = patrolDir * speed * patrolSpeedMultiplier;
                break;

            case FoxState.Chase:
                desired = toPlayer.normalized * GetChaseSpeed();
                break;
        }

        desired += ComputeSeparation() * separationStrength;

        float maxSpeed = state == FoxState.Chase ? GetChaseSpeed() : speed * patrolSpeedMultiplier;
        if (desired.magnitude > maxSpeed) desired = desired.normalized * maxSpeed;

        rb.linearVelocity = desired;
    }

    bool Blocked(Vector2 dir)
    {
        if (dir == Vector2.zero) return false;
        int n = col.Cast(dir, wallFilter, castHits, lookAhead);
        return n > 0;
    }

    void PickOpenDirection()
    {
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        for (int i = 0; i < 8; i++)
        {
            Vector2 d = dirs[Random.Range(0, dirs.Length)];
            if (!Blocked(d))
            {
                patrolDir = d;
                return;
            }
        }

        patrolDir = -patrolDir;
        if (patrolDir == Vector2.zero) patrolDir = Vector2.right;
    }

    Vector2 ComputeSeparation()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, separationRadius, enemyLayer);
        Vector2 force = Vector2.zero;
        int count = 0;

        foreach (var h in hits)
        {
            if (h == null) continue;
            if (h.attachedRigidbody == rb) continue;

            Vector2 away = rb.position - (Vector2)h.transform.position;
            float d = away.magnitude;
            if (d < 0.001f) continue;

            force += away.normalized / d;
            count++;
        }

        if (count > 0) force /= count;
        return force;
    }

    float GetChaseSpeed()
    {
        float s = speed;
        if (ScoreManager.instance != null)
        {
            s += ScoreManager.instance.score * 0.0008f;
            s = Mathf.Min(s, speed * 1.5f);
        }
        return s;
    }

    void EnterPatrol()
    {
        state = FoxState.Patrol;
        PickOpenDirection();
        stateTimer = Random.Range(patrolChangeMin, patrolChangeMax);
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.CompareTag("Player"))
        {
            HealthManager.instance?.TakeHit();
            return;
        }

        if (c.collider.CompareTag("Enemy"))
        {
            Vector2 push = (rb.position - (Vector2)c.collider.transform.position).normalized;
            if (push == Vector2.zero) push = Random.insideUnitCircle.normalized;
            rb.AddForce(push * bumpForce, ForceMode2D.Impulse);
            return;
        }

        if (IsWall(c.collider.gameObject))
        {
            ApplyWallSlide(c);
            state = FoxState.Idle;
            stateTimer = wallHitIdle;
        }
    }

    void OnCollisionStay2D(Collision2D c)
    {
        if (IsWall(c.collider.gameObject))
            ApplyWallSlide(c);
    }

    bool IsWall(GameObject go)
    {
        return (wallLayer.value & (1 << go.layer)) != 0;
    }

    void ApplyWallSlide(Collision2D c)
    {
        if (c.contactCount == 0) return;

        Vector2 n = c.contacts[0].normal;
        Vector2 t1 = new Vector2(-n.y, n.x);
        Vector2 t2 = -t1;

        Vector2 desired =
            state == FoxState.Chase && target != null
            ? ((Vector2)target.position - rb.position).normalized
            : patrolDir;

        wallAvoidDir = Vector2.Dot(t1, desired) > Vector2.Dot(t2, desired) ? t1 : t2;
        wallAvoidTimer = wallAvoidTime;
    }
}
