using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
//【開発用】サウンドテスト。SE・BGMを、ボタン1つで鳴らして確認する。音量も、その場で変えられる。
//本番と同じ SE.Play と AudioDirector.PlayBgm を呼ぶので、鳴らす経路も確認できる。
public class SoundTest : MonoBehaviour
{
    [SerializeField] private AudioDirector director;
    [Tooltip("SEの割り当て表（登録の有無を表示するために使う）")]
    [SerializeField] private SoundBank bank;
    [SerializeField] private bool openOnStart = false;
    [Tooltip("画面の拡大率。Gameビューが小さくて文字が読みにくいときに上げる（画面の中の A- / A+ でも変えられる）")]
    [Range(0.8f, 2.5f)]
    [SerializeField] private float zoom = 1.3f;

    private readonly Dictionary<SeCue, SoundEffect> table = new Dictionary<SeCue, SoundEffect>();
    private bool open;
    private Vector2 scroll;
    private bool autoPlaying;
    private bool onlyMissing;
    private float master = 1f, se = 1f, bgm = 1f;

    //ボタンの操作は、次の Layout のときにまとめて反映する（IMGUI の描画の途中で、部品の数が変わらないように）。
    private bool pendingToggleOpen, pendingToggleMissing, pendingReload;
    private float pendingZoomDelta;

    private GUIStyle labelStyle, headerStyle, buttonStyle, okStyle, jingleStyle, ngStyle;

    //GUIにゲームが干渉しないように。
    private Rect lastToggleRect, lastPanelRect;
    private float lastScale = 1f;
    private bool panelDrawn;

    private void Awake()
    {
        open = openOnStart;
        BuildTable();
        ReadVolumes();
    }

    private void Update()
    {
        //マウスが、サウンドテストの上にあるときは、ゲームの操作を止める。
        float scale = Mathf.Max(0.0001f, lastScale);
        Vector2 mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y) / scale;
        bool over = lastToggleRect.Contains(mouse) || (panelDrawn && lastPanelRect.Contains(mouse));
        InputGuard.SetBlocked(this, over);
    }

    private void nDisable()
    {
        InputGuard.SetBlocked(this, false);
    }

    //SoundBank の中身を読み直す（再生中に音を入れ替えたときのため）。
    private void BuildTable()
    {
        table.Clear();
        if (bank == null) return;
        foreach (var entry in bank.Entries)
        {
            if (entry.effect != null) table[entry.cue] = entry.effect;
        }
    }

    private void ReadVolumes()
    {
        if (director == null) return;
        master = director.MasterVolume;
        se = director.SeVolume;
        bgm = director.BgmVolume;
    }

    //文字の見た目（OnGUI の中でしか作れない）。
    private void EnsureStyles()
    {
        if (labelStyle != null) return;

        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
        labelStyle.normal.textColor = Color.white;

        headerStyle = new GUIStyle(labelStyle) { fontSize = 17, fontStyle = FontStyle.Bold };
        headerStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);

        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 15 };

        okStyle = new GUIStyle(labelStyle) { fontStyle = FontStyle.Bold, wordWrap = false };
        okStyle.normal.textColor = new Color(0.45f, 0.95f, 0.5f);

        jingleStyle = new GUIStyle(okStyle);
        jingleStyle.normal.textColor = new Color(1f, 0.75f, 0.3f);

        ngStyle = new GUIStyle(okStyle);
        ngStyle.normal.textColor = new Color(0.6f, 0.6f, 0.65f);
    }

    private void ApplyPending()
    {
        if (pendingToggleOpen)
        {
            pendingToggleOpen = false;
            open = !open;
            if (open) { BuildTable(); ReadVolumes(); }
        }
        if (pendingToggleMissing) { pendingToggleMissing = false; onlyMissing = !onlyMissing; }
        if (pendingReload) { pendingReload = false; BuildTable(); }
        if (pendingZoomDelta != 0f)
        {
            zoom = Mathf.Clamp(zoom + pendingZoomDelta, 0.8f, 2.5f);
            pendingZoomDelta = 0f;
        }
    }

    private void OnGUI()
    {
        if (Event.current.type == EventType.Layout) ApplyPending();

        //解像度が変わっても同じ見た目になるよう、縦720を基準に拡大縮小する（zoomで、さらに拡大できる）。
        float scale = Screen.height / 720f * zoom;
        if (scale <= 0f) return;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        float virtualWidth = Screen.width / scale;
        float virtualHeight = Screen.height / scale;
        EnsureStyles();

        //開閉のボタンは、画面の右上に置く。
        var toggleRect = new Rect(virtualWidth - 160f, 8f, 150f, 32f);
        lastToggleRect = toggleRect;
        lastScale = scale;
        if (GUI.Button(toggleRect,open?"閉じる":"サウンドテスト",buttonStyle))
        {
            pendingToggleOpen = true;
        }
        if (!open)
        {
            panelDrawn = false;
            return;
        }

        //パネルは、画面の右端に寄せて、残りの高さを使う。
        float width = Mathf.Min(560f, virtualWidth - 20f);
        var panel = new Rect(virtualWidth - width - 10f, 48f, width, virtualHeight - 58f);
        lastPanelRect = panel;
        panelDrawn = true;
        FillRect(panel, new Color(0.1f, 0.1f, 0.12f, 0.97f));

        GUILayout.BeginArea(new Rect(panel.x + 12f, panel.y + 8f, panel.width - 24f, panel.height - 16f));

        GUILayout.BeginHorizontal();
        GUILayout.Label("サウンドテスト", headerStyle);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("-", buttonStyle, GUILayout.Width(44))) pendingZoomDelta = -0.1f;
        if (GUILayout.Button("+", buttonStyle, GUILayout.Width(44))) pendingZoomDelta = 0.1f;
        GUILayout.EndHorizontal();

        GUILayout.Space(4);
        DrawVolumes();
        GUILayout.Space(8);
        DrawBgm();
        GUILayout.Space(8);
        DrawSe();

        GUILayout.EndArea();
    }

    // ---- 音量 ----

    private void DrawVolumes()
    {
        GUILayout.Label("音量", headerStyle);
        if (director == null)
        {
            GUILayout.Label("AudioDirector が未設定です（Director の欄に入れてください）", labelStyle);
            return;
        }
        master = Slider("全体", master, director.SetMasterVolume);
        se = Slider("SE", se, director.SetSeVolume);
        bgm = Slider("BGM", bgm, director.SetBgmVolume);
    }

    private float Slider(string label, float value, System.Action<float> apply)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, labelStyle, GUILayout.Width(56));
        float next = GUILayout.HorizontalSlider(value, 0f, 1f, GUILayout.ExpandWidth(true));
        GUILayout.Label(next.ToString("0.00"), labelStyle, GUILayout.Width(44));
        GUILayout.EndHorizontal();

        if (!Mathf.Approximately(next, value)) apply(next);
        return next;
    }

    // ---- BGM ----

    private void DrawBgm()
    {
        GUILayout.Label("BGM", headerStyle);
        GUILayout.Label("同じ曲を続けて押しても、鳴り直しません。聞き直すときは、先に「停止」を押します。", labelStyle);

        GUI.enabled = director != null;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("タイトル", buttonStyle, GUILayout.Height(30))) director.PlayBgm(BgmCue.Title);
        if (GUILayout.Button("戦闘", buttonStyle, GUILayout.Height(30))) director.PlayBgm(BgmCue.Battle);
        if (GUILayout.Button("ボス", buttonStyle, GUILayout.Height(30))) director.PlayBgm(BgmCue.Boss);
        if (GUILayout.Button("停止", buttonStyle, GUILayout.Height(30))) director.StopBgm();
        GUILayout.EndHorizontal();
        GUI.enabled = true;
    }

    // ---- SE ----

    private void DrawSe()
    {
        int missing = 0;
        foreach (SeCue cue in System.Enum.GetValues(typeof(SeCue)))
        {
            if (!table.TryGetValue(cue, out var effect) || !effect.HasClip) missing++;
        }

        GUILayout.Label($"SE（音が未設定：{missing}個）", headerStyle);

        GUILayout.BeginHorizontal();
        if (!autoPlaying)
        {
            if (GUILayout.Button("全部を順に鳴らす", buttonStyle, GUILayout.Height(28))) StartCoroutine(PlayAllRoutine());
        }
        else
        {
            if (GUILayout.Button("止める", buttonStyle, GUILayout.Height(28))) { StopAllCoroutines(); autoPlaying = false; }
        }
        if (GUILayout.Button(onlyMissing ? "未設定だけ：ON" : "未設定だけ：OFF", buttonStyle, GUILayout.Height(28)))
        {
            pendingToggleMissing = true;
        }
        if (GUILayout.Button("表を読み直す", buttonStyle, GUILayout.Height(28))) pendingReload = true;
        GUILayout.EndHorizontal();

        //一覧は、残りの高さを全部使う。はみ出た分は、スクロールする。
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
        foreach (SeCue cue in System.Enum.GetValues(typeof(SeCue)))
        {
            bool has = table.TryGetValue(cue, out var effect) && effect.HasClip;
            if (onlyMissing && has) continue;

            GUILayout.BeginHorizontal();

            GUIStyle statusStyle = !has ? ngStyle : (effect.Kind == SoundKind.Jingle ? jingleStyle : okStyle);
            string statusText = !has ? "未設定" : (effect.Kind == SoundKind.Jingle ? "ジングル" : "設定済");
            GUILayout.Label(statusText, statusStyle, GUILayout.Width(70));

            if (GUILayout.Button(cue.ToString(), buttonStyle, GUILayout.Width(200), GUILayout.Height(28)))
            {
                SE.Play(cue);
            }

            GUILayout.Label(Describe(cue, effect, has), labelStyle);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }

    private string Describe(SeCue cue, SoundEffect effect, bool has)
    {
        if (effect == null) return table.ContainsKey(cue) ? "" : "（未登録）";
        if (!has) return $"{effect.name}（音源なし）";
        return $"{effect.name}　音量{effect.Volume:0.00}";
    }

    //登録済みの全SEを、順に鳴らす（ジングルは、長いので少し長く待つ）。
    private IEnumerator PlayAllRoutine()
    {
        autoPlaying = true;
        foreach (SeCue cue in System.Enum.GetValues(typeof(SeCue)))
        {
            if (!table.TryGetValue(cue, out var effect) || !effect.HasClip) continue;
            SE.Play(cue);
            yield return new WaitForSecondsRealtime(effect.Kind == SoundKind.Jingle ? 2.5f : 0.8f);
        }
        autoPlaying = false;
    }

    //背景などを、色で塗りつぶす。
    private static void FillRect(Rect rect, Color color)
    {
        var old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = old;
    }
}
#endif