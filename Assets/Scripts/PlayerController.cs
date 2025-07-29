using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer), typeof(SpriteRenderer), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Collision Points")]
    public GameObject headPoint;
    public GameObject feetPoint;


    [Header("Combat Settings")]
    public int currentWeaponDamage = 10;
    public float attackRange = 1.2f;
    public float attackRate = 1f;

    private Animator m_animator;
    private SpriteRenderer m_spriteRenderer;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;

    [Header("Tool")]
    public bool hasPickaxe = false;

    private Vector3 lastDir = Vector3.right;
    private LineRenderer lr;

    public static PlayerController Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        lr = GetComponent<LineRenderer>();

        if (headPoint == null || feetPoint == null)
        {
            Debug.LogError("PlayerController is missing Head or Feet point references!");
            this.enabled = false;
        }
    }

    void Update()
    {
        m_timeSinceAttack += Time.deltaTime;

        HandleMovementAndAnimation();
        HandleAttack();
        HandleMining();
    }

    void HandleMovementAndAnimation()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, v, 0f).normalized;

        if (dir.magnitude > 0.1f)
        {
            lastDir = dir;
            Vector3 newPos = transform.position + dir * moveSpeed * Time.deltaTime;

            if (CanMoveTo(newPos))
            {
                transform.position = newPos;
            }

            m_delayToIdle = 0.1f;
            m_animator.SetInteger("AnimState", 1);
        }
        else
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }

        if (dir.x > 0)
        {
            m_spriteRenderer.flipX = false;
        }
        else if (dir.x < 0)
        {
            m_spriteRenderer.flipX = true;
        }
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

    void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f)
        {
            m_currentAttack++;
            if (m_currentAttack > 3) m_currentAttack = 1;
            if (m_timeSinceAttack > 1.0f) m_currentAttack = 1;

            m_animator.SetTrigger("Attack" + m_currentAttack);
            PerformAttack();
            StartCoroutine(ShowAttackCircle());
            m_timeSinceAttack = 0.0f;
        }
    }

    // --- MODIFIED ---
    void HandleMining()
    {
        if (hasPickaxe && Input.GetKeyDown(KeyCode.E))
        {
            m_animator.SetTrigger("Attack1");

            // Calculate the target positions for both head and feet based on the last direction
            Vector3 targetHeadPos = headPoint.transform.position + lastDir;
            Vector3 targetFeetPos = feetPoint.transform.position + lastDir;

            // Convert world positions to grid coordinates
            int headMineX = Mathf.RoundToInt(targetHeadPos.x);
            int headMineY = Mathf.RoundToInt(targetHeadPos.y);

            int feetMineX = Mathf.RoundToInt(targetFeetPos.x);
            int feetMineY = Mathf.RoundToInt(targetFeetPos.y);

            // Mine both blocks
            DungeonGenerator.Instance.MineAt(headMineX, headMineY);
            DungeonGenerator.Instance.MineAt(feetMineX, feetMineY);
        }
    }

    private void PerformAttack()
    {
        float effectiveRange = attackRange + 0.05f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, effectiveRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                var enemyHealth = hit.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamage(currentWeaponDamage);
            }
        }
    }

    private IEnumerator ShowAttackCircle()
    {
        lr.positionCount = 51;
        float r = attackRange;
        for (int i = 0; i < 51; i++)
        {
            float angle = i * Mathf.PI * 2f / 50f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
            lr.SetPosition(i, transform.position + pos);
        }
        lr.enabled = true;
        yield return new WaitForSeconds(0.2f);
        lr.enabled = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange + 0.05f);
    }
}
