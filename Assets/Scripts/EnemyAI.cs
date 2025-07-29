using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D), typeof(Animator), typeof(SpriteRenderer))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectionRange = 6f;
    public float recalcInterval = 1f;

    [Header("Collision Points")]
    public GameObject headPoint;
    public GameObject feetPoint;

    [Header("Collision Damage")]
    public float collisionDistance = 0.5f;
    public int collisionDamage = 15;
    public float collisionCooldown = 1f;

    private Animator m_animator;
    private SpriteRenderer m_spriteRenderer;
    private Vector3 lastDirection = Vector3.zero;

    private bool isDamaging = false;
    private List<Vector2Int> path;
    private int pathIdx;
    private float nextRecalc;
    private Vector3 wanderTarget;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();

        if (headPoint == null || feetPoint == null)
        {
            Debug.LogError("EnemyAI on " + gameObject.name + " is missing Head or Feet point references!");
            this.enabled = false;
            return;
        }

        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        while (DungeonGenerator.Instance == null || DungeonGenerator.grid == null)
        {
            yield return null;
        }
        PickWanderTarget();
    }

    void Update()
    {
        if (PlayerController.Instance == null || DungeonGenerator.Instance == null)
        {
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
        if (distToPlayer <= detectionRange)
        {
            if (Time.time >= nextRecalc)
            {
                RecalculatePath();
                nextRecalc = Time.time + recalcInterval;
            }
            FollowPath();
        }
        else
        {
            // --- THIS IS THE FIX ---
            // When the player is out of range, clear the old chase path to force wandering.
            path = null;

            if (wanderTarget == Vector3.zero || Vector3.Distance(transform.position, wanderTarget) < 0.2f)
                PickWanderTarget();
            MoveTowards(wanderTarget);
        }

        if (!isDamaging && distToPlayer <= collisionDistance)
            StartCoroutine(DealCollisionDamage());
    }

    private IEnumerator DealCollisionDamage()
    {
        isDamaging = true;
        m_animator.SetTrigger("Attack");
        PlayerController.Instance.GetComponent<PlayerHealth>().TakeDamage(collisionDamage);
        yield return new WaitForSeconds(collisionCooldown);
        isDamaging = false;
    }

    void PickWanderTarget()
    {
        int w = DungeonGenerator.grid.GetLength(0);
        int h = DungeonGenerator.grid.GetLength(1);
        Vector2Int rnd;
        int attempts = 0;
        do
        {
            attempts++;
            rnd = new Vector2Int(Random.Range(0, w), Random.Range(0, h));
            if (attempts > 50)
            {
                Debug.LogWarning("Could not find a valid wander target for " + gameObject.name);
                return;
            };
        }
        while (!CanMoveTo(new Vector3(rnd.x, rnd.y, 0)));

        wanderTarget = new Vector3(rnd.x, rnd.y, transform.position.z);
    }

    void RecalculatePath()
    {
        Vector2Int start = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        Vector3 playerPos = PlayerController.Instance.transform.position;
        Vector2Int goal = new Vector2Int(Mathf.RoundToInt(playerPos.x), Mathf.RoundToInt(playerPos.y));

        path = Pathfinding.Dijkstra(DungeonGenerator.grid, start, goal);
        pathIdx = 0;
    }

    void FollowPath()
    {
        if (path == null || pathIdx >= path.Count)
        {
            m_animator.SetInteger("AnimState", 0);
            return;
        }
        Vector3 dest = new Vector3(path[pathIdx].x, path[pathIdx].y, transform.position.z);
        if (MoveTowards(dest))
            pathIdx++;
    }

    bool MoveTowards(Vector3 dest)
    {
        Vector3 dir = (dest - transform.position).normalized;

        if (dir.magnitude > 0.1f)
        {
            lastDirection = dir;
            m_animator.SetInteger("AnimState", 2);
        }
        else
        {
            m_animator.SetInteger("AnimState", 0);
        }

        if (lastDirection.x > 0) m_spriteRenderer.flipX = true;
        else if (lastDirection.x < 0) m_spriteRenderer.flipX = false;

        Vector3 proposedPos = transform.position + dir * moveSpeed * Time.deltaTime;

        if (CanMoveTo(proposedPos))
        {
            transform.position = proposedPos;
        }
        else
        {
            // If we are currently following a path (chasing), try to find a new one.
            if (path != null && path.Count > 0)
            {
                RecalculatePath();
            }
            else // Otherwise, we are just wandering, so find a new wander spot.
            {
                PickWanderTarget();
            }
            return false;
        }

        if (Vector3.Distance(transform.position, dest) < 0.1f)
        {
            transform.position = dest;
            return true;
        }
        return false;
    }

    private bool CanMoveTo(Vector3 proposedPos)
    {
        Vector3 headOffset = headPoint.transform.position - transform.position;
        Vector3 feetOffset = feetPoint.transform.position - transform.position;

        Vector3 futureHeadPos = proposedPos + headOffset;
        Vector3 futureFeetPos = proposedPos + feetOffset;

        int headGridX = Mathf.RoundToInt(futureHeadPos.x);
        int headGridY = Mathf.RoundToInt(futureHeadPos.y);
        int feetGridX = Mathf.RoundToInt(futureFeetPos.x);
        int feetGridY = Mathf.RoundToInt(futureFeetPos.y);

        return DungeonGenerator.Instance.IsWalkable(headGridX, headGridY) &&
               DungeonGenerator.Instance.IsWalkable(feetGridX, feetGridY);
    }
}
