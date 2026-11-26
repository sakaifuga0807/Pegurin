using System;
using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
//【開発用】ステージの切り替えと、確認に便利な操作のパネル。左上のボタンで開閉する。
//確認が終わったら、このファイルとオブジェクトを消す。
public class StageDebugPanel : MonoBehaviour
{
    [SerializeField] private RunManager runManager;
    [Tooltip("画面の拡大率。Gameビューが小さくて文字が読みにくいときに上げる")]
    [Range(0.8f, 2.5f)]
    [SerializeField] private float zoom = 1.2f;
    [SerializeField] private bool openOnStart = false;

    private bool open;
    private bool keepProgress = true;
    private Vector2 scroll;
    private GUIStyle labelStyle, buttonStyle, headerStyle;

    //ボタンの操作は、OnGUI の中では実行せず、Update で実行する（描画の途中で、状態が変わらないように）。
    private Action pending;
    private bool pendingToggleOpen;

    //マウスが、パネルの上にあるか調べるための、直前の描画の位置。
    private Rect lastToggleRect, lastPanelRect;
    private float lastScale = 1f;
    private bool panelDrawn;

    private void Awake()
    {
        open = openOnStart;
    }

    private void Update()
    {
        if (pendingToggleOpen)
        {
            pendingToggleOpen = false;
            open = !open;
        }

        var action = pending;
        pending = null;
        if (action != null) action();

        //マウスが、パネルの上にあるときは、ゲームの操作（発射など）を止める。
        float scale = Mathf.Max(0.0001f, lastScale);
        Vector2 mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y) / scale;
        bool over = lastToggleRect.Contains(mouse) || (panelDrawn && lastPanelRect.Contains(mouse));
        InputGuard.SetBlocked(this, over);
    }

    private void OnDisable()
    {
        InputGuard.SetBlocked(this, false);
    }

    private void EnsureStyles()
    {
        if (labelStyle != null) return;

        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
        labelStyle.normal.textColor = Color.white;
        headerStyle = new GUIStyle(labelStyle) { fontSize = 16, fontStyle = FontStyle.Bold };
        headerStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 15 };
    }

    private void OnGUI()
    {
        if (runManager == null) return;

        float scale = Screen.height / 720f * zoom;
        if (scale <= 0f) return;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        float virtualHeight = Screen.height / scale;
        EnsureStyles();

        //開閉のボタンは、画面の左上に置く。
        lastToggleRect = new Rect(8f, 8f, 150f, 30f);
        lastScale = scale;
        if (GUI.Button(lastToggleRect, open ? "閉じる" : "ステージ切替", buttonStyle)) pendingToggleOpen = true;

        if (!open)
        {
            panelDrawn = false;
            return;
        }

        lastPanelRect = new Rect(8f, 42f, 340f, Mathf.Min(520f, virtualHeight - 52f));
        panelDrawn = true;

        var old = GUI.color;
        GUI.color = new Color(0.1f, 0.1f, 0.12f, 0.97f);
        GUI.DrawTexture(lastPanelRect, Texture2D.whiteTexture);
        GUI.color = old;

        GUILayout.BeginArea(new Rect(lastPanelRect.x + 10f, lastPanelRect.y + 8f, lastPanelRect.width - 20f, lastPanelRect.height - 16f));
        scroll = GUILayout.BeginScrollView(scroll);

        DrawStatus();
        GUILayout.Space(6f);
        DrawStageButtons();
        GUILayout.Space(6f);
        DrawActions();

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawStatus()
    {
        var context = BattleContext.Current;
        string hp = context != null && context.Player != null ? $"{context.Player.CurrentHp}/{context.Player.MaxHp}" : "－";
        GUILayout.Label($"フェーズ：{runManager.Phase}\nステージ：{runManager.StageIndex + 1} / {runManager.StageCount}　HP：{hp}", labelStyle);
    }

    private void DrawStageButtons()
    {
        GUILayout.Label("ステージを切り替える", headerStyle);
        keepProgress = GUILayout.Toggle(keepProgress, "HP・デッキ・パッシブを引き継ぐ");

        for (int i = 0; i < runManager.StageCount; i++)
        {
            int target = i;
            bool keep = keepProgress;
            string mark = i == runManager.StageIndex && runManager.Phase != RunPhase.Title ? "▶ " : "";
            string label = $"{mark}ステージ{i + 1}：{runManager.StageNameAt(i)}";
            if (GUILayout.Button(label, buttonStyle, GUILayout.Height(30f)))
            {
                pending = () => runManager.DebugJumpToStage(target, keep);
            }
        }
    }

    private void DrawActions()
    {
        GUILayout.Label("操作", headerStyle);

        if (GUILayout.Button("このステージをクリア（敵を全員消す）", buttonStyle, GUILayout.Height(28f)))
            pending = runManager.DebugClearStage;
        if (GUILayout.Button("HPを全回復", buttonStyle, GUILayout.Height(28f)))
            pending = runManager.DebugHealPlayer;
        if (GUILayout.Button("パッシブを全部入手", buttonStyle, GUILayout.Height(28f)))
            pending = runManager.DebugGrantAllPassives;
        if (GUILayout.Button("報酬オーブを全部デッキへ（次のステージから）", buttonStyle, GUILayout.Height(28f)))
            pending = runManager.DebugGrantRewardOrbs;
    }
}
#endif