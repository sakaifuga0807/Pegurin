using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossWarningEffect : MonoBehaviour
{
    [Header("見た目の参照")]
    [SerializeField] private GameObject visualRoot;     //演出の見た目をまとめた子オブジェクト。普段は非アクティブ。
    [SerializeField] private CanvasGroup overlayGroup;  //テープと帯をまとめたもの。全体のフェード用。
    [SerializeField] private CanvasGroup bannerGroup;   //WARNINGの帯。点滅用。overlayGroupの子にする。
    [SerializeField] private RawImage topTape;          //上のテープ
    [SerializeField] private RawImage bottomTape;       //下のテープ
    [SerializeField] private Image redBackground;       //背景を赤く染める全画面Image（overlayGroupの一番奥に配置）

    [Header("背景の赤み設定")]
    [SerializeField] private Color bgColor = new Color(0.85f, 0.05f, 0.05f, 0.35f); //赤色と最大の濃さ(Alpha)
    [SerializeField] private bool pulseBackground = true;

    [Header("テープの流れ")]
    [Tooltip("オンなら、テープの大きさと画像の縦横比から、くり返しの回数を自動で決める(模様が伸び縮みしない)")]
    [SerializeField] private bool autoTileAspect = true;
    [Tooltip("自動がオフのときに使う、画面の横幅に模様が何回くり返されるか")]
    [SerializeField] private float tilesAcross = 8f;
    [Tooltip("1秒あたりに流れる量(模様1つ分が1)。プラスで左へ、マイナスで右へ流れる")]
    [SerializeField] private float topScrollSpeed = 0.6f;
    [SerializeField] private float bottomScrollSpeed = -0.6f;

    [Header("タイミング(秒)")]
    [SerializeField] private float durationMultiplier = 2.0f; //全体の時間倍率
    [SerializeField] private float overlayFadeIn = 0.3f;    //テープが現れるまで
    [SerializeField] private int flashCount = 3;            //WARNINGの点滅の回数
    [SerializeField] private float flashFadeIn = 0.2f;      //点滅:現れるまで
    [SerializeField] private float flashHold = 0.25f;       //点滅:表示しておく時間
    [SerializeField] private float flashFadeOut = 0.2f;     //点滅:消えるまで
    [SerializeField] private float overlayFadeOut = 0.4f;   //最後に、全体が消えるまで

    private Func<object, IEnumerator> handler;
    private bool scrolling;

    private void Awake()
    {
        handler = PlayRoutine;

        SetupTape(topTape);
        SetupTape(bottomTape);
        SetupBackground();

        SetVisible(false);
    }

    private void OnEnable()
    {
        BattleDirector director = BattleDirector.Instance;
        director.Register(Beat.BossAppear, handler);
        director.Cancelled += HandleCancelled;
    }

    private void OnDisable()
    {
        BattleDirector director = BattleDirector.Existing;
        if (director == null) return;

        director.Unregister(Beat.BossAppear, handler);
        director.Cancelled -= HandleCancelled;
    }

    private void Update()
    {
        if (!scrolling) return;

        Scroll(topTape, topScrollSpeed);
        Scroll(bottomTape, bottomScrollSpeed);
    }

    private IEnumerator PlayRoutine(object data)
    {
        if (overlayGroup == null || bannerGroup == null)
        {
            Debug.LogWarning("[BossWarning] overlayGroup / bannerGroup が未設定のため、演出を飛ばします。");
            yield break;
        }

        SetVisible(true);
        overlayGroup.alpha = 0f;
        bannerGroup.alpha = 0f;
        SetupBackground();

        Canvas.ForceUpdateCanvases();
        SetupTape(topTape);
        SetupTape(bottomTape);

        scrolling = true;

        Color dimBgColor = new Color(bgColor.r, bgColor.g, bgColor.b, bgColor.a * 0.4f);
        if (redBackground != null && pulseBackground)
        {
            redBackground.color = dimBgColor;
        }

        yield return Fade(overlayGroup, 0f, 1f, overlayFadeIn * durationMultiplier);

        for (int i = 0; i < flashCount; i++)
        {
            if (redBackground != null && pulseBackground) redBackground.color = bgColor;
            yield return Fade(bannerGroup, 0f, 1f, flashFadeIn * durationMultiplier);

            yield return new WaitForSeconds(flashHold * durationMultiplier);

            if (redBackground != null && pulseBackground) redBackground.color = dimBgColor;
            yield return Fade(bannerGroup, 1f, 0f, flashFadeOut * durationMultiplier);
        }
        yield return Fade(overlayGroup, 1f, 0f, overlayFadeOut * durationMultiplier);

        scrolling = false;
        SetVisible(false);
    }

    private static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            group.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
    }

    private void Scroll(RawImage tape, float speed)
    {
        if (tape == null) return;

        Rect uv = tape.uvRect;
        uv.x = Mathf.Repeat(uv.x + speed * Time.deltaTime, 1f);
        tape.uvRect = uv;
    }

    private void SetupTape(RawImage tape)
    {
        if (tape == null) return;

        float across = tilesAcross;
        if (autoTileAspect && tape.texture != null)
        {
            Rect r = tape.rectTransform.rect;
            if (r.height > 0f)
            {
                across = (r.width / r.height) * ((float)tape.texture.height / tape.texture.width);
            }
        }

        Rect uv = tape.uvRect;
        tape.uvRect = new Rect(uv.x, 0f, across, 1f);
        tape.raycastTarget = false;
    }

    private void SetupBackground()
    {
        if (redBackground == null) return;

        redBackground.color = bgColor;
        redBackground.raycastTarget = false;
    }

    private void SetVisible(bool visible)
    {
        if (visualRoot != null) visualRoot.SetActive(visible);
    }

    private void HandleCancelled()
    {
        scrolling = false;
        SetVisible(false);
    }

    [ContextMenu("テスト再生")]
    private void TestPlay()
    {
        BattleDirector.Instance.Run(PlayRoutine(null));
    }
}