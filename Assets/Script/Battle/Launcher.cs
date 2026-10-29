using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Launcher : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform firePoint;

    [Header("発射パラメータ")]
    [SerializeField] private float defaultPower = 1500f;

    [Header("角度制限（下向き・左右に向けられるように拡大）")]
    [SerializeField] private float minAngle = 120f;
    [SerializeField] private float maxAngle = 270f;

    [Header("ガイド線（軌道予測）の設定")]
    [SerializeField] private int maxTrajectorySteps = 30;
    [SerializeField] private float timeStep = 0.01f;

    private LineRenderer lineRenderer;
    private GameObject previewOrb;
    private GameObject activeFiredOrb;

    private bool canFire = false;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        SpawnPreviewOrb();
    }

    private void Update()
    {
        AimTowardsMouse();
        UpdatePreviewOrbPosition();
        DrawTrajectory();

        if (canFire && Input.GetMouseButtonDown(0))
        {
            FireOrb();
        }

        if (!canFire && activeFiredOrb == null)
        {
            SpawnPreviewOrb();
        }
    }

    private void AimTowardsMouse()
    {
        if (targetCamera == null) return;

        Vector3 mouseWorldPos = targetCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector3 direction = mouseWorldPos - transform.position;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        targetAngle = Mathf.Repeat(targetAngle, 360f);
        targetAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);
        transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
    }

    private void SpawnPreviewOrb()
    {
        if (previewOrb != null || orbPrefab == null || firePoint == null) return;

        previewOrb = Instantiate(orbPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = previewOrb.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        Collider2D col = previewOrb.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        canFire = true;
    }

    private void UpdatePreviewOrbPosition()
    {
        if (previewOrb != null && firePoint != null)
        {
            previewOrb.transform.position = firePoint.position;
            previewOrb.transform.rotation = firePoint.rotation;
        }
    }

    public void FireOrb()
    {
        if (!canFire || orbPrefab == null || firePoint == null) return;

        canFire = false;

        if (previewOrb != null)
        {
            Destroy(previewOrb);
        }

        // 本物のオーブを生成
        GameObject orbObj = Instantiate(orbPrefab, firePoint.position, firePoint.rotation);

        activeFiredOrb = orbObj;

        Rigidbody2D rb = orbObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;

            Collider2D col = orbObj.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            rb.AddForce(firePoint.up * defaultPower, ForceMode2D.Impulse);
        }

        Debug.Log("[Launcher] オーブを発射しました！");
    }

    private void DrawTrajectory()
    {
        if (lineRenderer == null || firePoint == null || !canFire)
        {
            if (lineRenderer != null) lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = maxTrajectorySteps;

        Vector3 startPosition = firePoint.position;

        float gravityScale = 1f;
        if (orbPrefab != null)
        {
            Rigidbody2D prefabRb = orbPrefab.GetComponent<Rigidbody2D>();
            if (prefabRb != null) gravityScale = prefabRb.gravityScale;
        }
        float gravity = Physics2D.gravity.y * gravityScale;

        float orbMass = 1f;
        if (orbPrefab != null)
        {
            Rigidbody2D prefabRb = orbPrefab.GetComponent<Rigidbody2D>();
            if (prefabRb != null) orbMass = prefabRb.mass;
        }

        Vector2 initialVelocity = (Vector2)(firePoint.up * defaultPower) / orbMass;

        Vector3[] points = new Vector3[maxTrajectorySteps];

        for (int i = 0; i < maxTrajectorySteps; i++)
        {
            float t = i * timeStep;
            float x = startPosition.x + initialVelocity.x * t;
            float y = startPosition.y + initialVelocity.y * t + 0.5f * gravity * t * t;

            points[i] = new Vector3(x, y, 0f);
        }

        lineRenderer.SetPositions(points);
    }
}