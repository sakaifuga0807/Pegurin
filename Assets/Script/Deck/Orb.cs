using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Orb : MonoBehaviour
{
    // インスペクター設定（パラメータ）
    [Header("物理・運動設定")]
    [SerializeField] private float maxSpeed = 30f; // 速度の限界値（暴走防止）

    [Header("壁・隙間暴走対策")]
    [SerializeField] private float bounceCooldown = 0.05f; // 連続衝突による計算暴走を防ぐ間隔
    private float lastBounceTime = 0f;

    [Header("テール（軌跡）設定")]
    [SerializeField] private GameObject trailNodePrefab;     // 軌跡に残すプレハブ
    [SerializeField] private int trailCount = 30;              // 軌跡の長さ（数）
    [SerializeField] private float recordIntervalFrames = 0.01f; // 軌跡を記録する頻度

    [SerializeField] private OrbData currentOrbData;      //インスペクターから入れられるようにする。
    private Rigidbody2D rb;              // 物理演算用コンポーネント
    private float defaultGravityScale = 1f;

    private List<GameObject> trailNodes = new List<GameObject>();
    private Queue<Vector3> positionHistory = new Queue<Vector3>();
    private int frameCounter = 0;

    // 初期化処理 (Awake / Start)
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 物理挙動の安定化設定
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        defaultGravityScale = rb.gravityScale;
    }

    void Start()
    {
        InitializeTrailNodes();

        if (currentOrbData != null)
        {
            SetupOrb(currentOrbData);
        }
    }

    //当たったペグのダメージ計算に使う威力。
    public int Power => currentOrbData != null ? currentOrbData.power : 0;

    public void SetupOrb(OrbData data)
    {
        currentOrbData = data;
        if (currentOrbData == null) return;

        // サイズと重力の比率適用
        transform.localScale = Vector3.one * currentOrbData.sizeRatio;
        defaultGravityScale = 1f * currentOrbData.gravityRatio;
        rb.gravityScale = defaultGravityScale;
    }

    // 更新処理 (Update / FixedUpdate)
    void Update()
    {
        UpdateTrailPositions();
        UpdateOrientation();
    }

    void FixedUpdate()
    {
        ClampVelocityToMaxSpeed();
    }

    // 物理挙動・移動ロジック
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Peg"))
        {
            if (Time.time - lastBounceTime < bounceCooldown) return;
            lastBounceTime = Time.time;

            if (collision.contactCount > 0)
            {
                ContactPoint2D contact = collision.contacts[0];
                Vector2 reflectVector = Vector2.Reflect(rb.linearVelocity.normalized, contact.normal);
                Vector2 centerToOrbDirection = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;

                Vector2 finalDirection = Vector2.Lerp(reflectVector, centerToOrbDirection, 1.0f).normalized;
                if (finalDirection == Vector2.zero) finalDirection = contact.normal;

                float dynamicMinBounce = 5f * (currentOrbData != null ? currentOrbData.bounceRatio : 1f);
                float currentSpeed = rb.linearVelocity.magnitude;
                float targetSpeed = Mathf.Clamp(currentSpeed, dynamicMinBounce, maxSpeed);

                rb.linearVelocity = finalDirection * targetSpeed;
            }
            ClampVelocityToMaxSpeed();
        }
    }

    private void ClampVelocityToMaxSpeed()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void UpdateOrientation()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    // テール（軌跡エフェクト）の管理
    private void InitializeTrailNodes()
    {
        if (trailNodePrefab == null) return;

        SpriteRenderer orbSr = GetComponent<SpriteRenderer>();
        string orbSortingLayer = (orbSr != null) ? orbSr.sortingLayerName : "Default";
        int orbSortingOrder = (orbSr != null) ? orbSr.sortingOrder : 0;

        for (int i = 0; i < trailCount; i++)
        {
            GameObject node = Instantiate(trailNodePrefab, transform.position, Quaternion.identity);
            float progress = (float)i / Mathf.Max(1, trailCount - 1);
            node.transform.localScale = transform.localScale * Mathf.Lerp(0.85f, 0.15f, progress);

            SpriteRenderer sr = node.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // テール側を必ずオーブ本体の1つ奥のレイヤーにする
                sr.sortingLayerName = orbSortingLayer;
                sr.sortingOrder = orbSortingOrder - 1;

                Color c = sr.color;
                c.a = Mathf.Lerp(0.7f, 0.1f, progress);
                sr.color = c;
            }
            trailNodes.Add(node);
        }
    }

    private void UpdateTrailPositions()
    {
        if (trailNodes.Count == 0) return;

        frameCounter++;
        if (frameCounter >= recordIntervalFrames)
        {
            frameCounter = 0;
            positionHistory.Enqueue(transform.position);
            while (positionHistory.Count > trailCount) positionHistory.Dequeue();
        }

        Vector3[] historyArray = positionHistory.ToArray();
        for (int i = 0; i < trailNodes.Count; i++)
        {
            if (i < historyArray.Length)
            {
                trailNodes[i].transform.position = historyArray[historyArray.Length - 1 - i];
                trailNodes[i].SetActive(true);
            }
            else
            {
                trailNodes[i].transform.position = transform.position;
            }
        }
    }

    public void ResetTrailPositions()
    {
        positionHistory.Clear();
        frameCounter = 0;
        foreach (var node in trailNodes)
        {
            if (node != null) node.transform.position = transform.position;
        }
    }
}