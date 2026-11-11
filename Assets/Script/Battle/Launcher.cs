using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Launcher : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private Camera targetCamera;
    [Tooltip("OrbDataに玉のプレハブがないときに使い、予備のプレハブ")]
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform firePoint;

    [Header("発射パラメータ")]
    [SerializeField] private float defaultPower = 1500f;

    [Header("角度制限（下向き・左右に向けられるように拡大）")]
    [SerializeField] private float minAngle = 90f;
    [SerializeField] private float maxAngle = 270f;

    [Header("ガイド線（軌道予測）の設定")]
    [SerializeField] private int maxTrajectorySteps = 30;
    [SerializeField] private float timeStep = 0.01f;

    [Header("救済")]
    [Tooltip("玉が指定した秒数たっても落ちないときは、詰まったとみなして、強制的に落下判定へ持っていく")]
    [SerializeField] private float maxFlightSeconds = 30f;

    [Header("確認用")]
    [Tooltip("クリックしても発射できなかった時にコンソールに表示")]
    [SerializeField] private bool logBlockedClick = true;

    private LineRenderer lineRenderer;
    private GameObject previewOrb;
    private OrbData previewData;
    private float launchTime;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        AimTowardMouse();
        //他の理由で消えた玉を落ちたものとして数える。
        OrbTracker.Prune();

        //詰まった玉の救済。
        if(OrbTracker.Count>0&&Time.time-launchTime>maxFlightSeconds)
        {
            Debug.LogWarning("[Launcher]玉が落ちないので、強制定期に落ちたことにします。");
            OrbTracker.ForceLandAll();
        }

        //撃てる時だけ、引いた玉が帰ってくる。
        OrbData current = GetLaunchableOrb();
        UpdatePreview(current);
        DrawTrajectory(current);

        //クリックしたときだけ、発射できるかを調べる。
        if(Input.GetMouseButtonDown(0))
        {
            if(current!=null&&!GameSpeed.Paused&&!InputGuard.IsBlocked)
            {
                FireOrb(current);
            }
            else if(logBlockedClick)
            {
                Debug.Log($"[Launcher] 発射できません：状態={StateName()}、撃てる玉={(current != null)}、" +
                          $"ポーズ={GameSpeed.Paused}、止めているもの={InputGuard.DescribeBlocker()}");
            }
        }

    }

    //今撃っていい状態なら、引いた玉を返す。入力待ちの時かつ、玉を引いている時だけ。
    private OrbData GetLaunchableOrb()
    {
        var context = BattleContext.Current;
        if (context == null || context.BattleStateMachine == null || context.DeckManager == null) return null;
        if (!context.BattleStateMachine.CanLaunch) return null;
        return context.DeckManager.CurrentOrb;
    }

    //今のステートの名前。(ログに使用)。
    private string StateName()
    {
        var context = BattleContext.Current;
        if (context == null || context.BattleStateMachine == null) return "(BattleContextなし)";
        var state = context.BattleStateMachine.CurrentState;
        return state != null ? state.GetType().Name : "なし";
    }

    private GameObject PrefabFor(OrbData data)
    {
        return (data != null && data.orbPrefab != null) ? data.orbPrefab : orbPrefab;
    }

    private void AimTowardMouse()
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

    //入力待ちの間だけ、発射台の位置に引いた球を見せる。
    private void UpdatePreview(OrbData data)
    {
        if (data == null)
        {
            DestroyPreview();
            return;
        }

        //引いた玉が変わったら作り直す。
        if (previewOrb != null && previewData!= data) DestroyPreview();

        if(previewOrb==null)
        {
            GameObject prefab = PrefabFor(data);
            if (prefab == null || firePoint == null) return;

            previewOrb = Instantiate(prefab, firePoint.position, firePoint.rotation);
            previewData = data;

            Rigidbody2D rb = previewOrb.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
            foreach (var col in previewOrb.GetComponents<Collider2D>()) col.enabled = false;

            SetupOrb(previewOrb, data);
        }
        previewOrb.transform.position = firePoint.position;
        previewOrb.transform.rotation = firePoint.rotation;
    }

    private void DestroyPreview()
    {
        if (previewOrb != null) Destroy(previewOrb);
        previewOrb = null;
        previewData = null;
    }

    private void SetupOrb(GameObject orbObj,OrbData data)
    {
        Orb orb=orbObj.GetComponent<Orb>();
        if (orb != null) orb.SetupOrb(data);
    }

    private void FireOrb(OrbData data)
    {
        GameObject prefab = PrefabFor(data);
        if(prefab==null||firePoint==null)
        {
            Debug.LogWarning("[Launcher]玉のプレハブがありません。OrbData か、Launcher の Orb Prefab に入れてください。");
            return;
        }

        DestroyPreview();

        //本物のオーブを生成する。
        GameObject orbObj = Instantiate(prefab, firePoint.position, firePoint.rotation);
        SetupOrb(orbObj, data);
        OrbTracker.Register(orbObj);
        launchTime = Time.time;

        Rigidbody2D rb = orbObj.GetComponent<Rigidbody2D>();
        if(rb!=null)
        {
            rb.simulated = true;
            Collider2D col = orbObj.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            rb.AddForce(firePoint.up * defaultPower, ForceMode2D.Impulse);
        }

        SE.Play(SeCue.Launch);
        Debug.Log($"[Launcher]オーブを発射しました({data.orbName})");

        //ここで、引いた玉は捨て札へ送られ、currentOrbが空になる。
        BattleContext.Current.BattleStateMachine.OnOrbLaunched();
    }

    private void DrawTrajectory(OrbData data)
    {
        if (lineRenderer == null || firePoint == null || data == null)
        {
            if (lineRenderer != null) lineRenderer.positionCount = 0;
            return;
        }

        GameObject prefab = PrefabFor(data);
        Rigidbody2D prefabRb = prefab != null ? prefab.GetComponent<Rigidbody2D>() : null;
        Orb prefabOrb = prefab != null ? prefab.GetComponent<Orb>() : null;

        float mass = prefabRb != null ? prefabRb.mass : 1f;
        float drag = prefabRb != null ? prefabRb.linearDamping : 0f;
        float maxSpeed = prefabOrb != null ? prefabOrb.MaxSpeed : float.MaxValue;

        //orb.SetupOrbと同じく、プレハブの重力に、OrbDataの比率を書ける。
        float gravityScale = (prefabRb != null ? prefabRb.gravityScale : 1f) * data.gravityRatio;
        Vector2 gravity = Physics2D.gravity * gravityScale;

        //打ち出し方はImpulse1なので、初速は力/質量で。
        Vector2 velocity = (Vector2)(firePoint.up * defaultPower) / mass;
        Vector2 position = firePoint.position;

        lineRenderer.positionCount = maxTrajectorySteps;
        for(int i=0;i<maxTrajectorySteps;i++)
        {
            lineRenderer.SetPosition(i, new Vector3(position.x, position.y, 0f));

            //Unityの物理と同じ順で進める。(速度の上限、重力、空気抵抗、移動)
            velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
            velocity += gravity * timeStep;
            velocity *= 1f / (1f + drag * timeStep);
            position += velocity * timeStep;
        }
    }
}