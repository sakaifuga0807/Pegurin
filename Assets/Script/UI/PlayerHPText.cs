
using UnityEngine;

public class PlayerHPText : MonoBehaviour
{
    [SerializeField] private Player player;

    private void OnGUI()
    {
        if (player == null) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

        float x = Screen.width * 0.17f;
        float y = Screen.height * 0.32f + 55f;

        string hpText = player.CurrentHp + " / " + player.MaxHp;

        // 元のGUI設定を保存
        Matrix4x4 oldMatrix = GUI.matrix;

        // 横幅を1.3倍にする（縦幅はそのまま）
        float widthScale = 1.3f;

        GUI.matrix = Matrix4x4.TRS(
            new Vector3(x * (1f - widthScale), 0f, 0f),
            Quaternion.identity,
            new Vector3(widthScale, 1f, 1f)
        ) * oldMatrix;

        // 黒い縁取り
        GUIStyle outlineStyle = new GUIStyle(style);
        outlineStyle.normal.textColor = Color.black;

        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                GUI.Label(
                    new Rect(x + dx, y + dy, 120, 30),
                    hpText,
                    outlineStyle
                );
            }
        }

        // 白い文字を少し重ねて太くする
        GUI.Label(new Rect(x - 1, y, 120, 30), hpText, style);
        GUI.Label(new Rect(x + 1, y, 120, 30), hpText, style);
        GUI.Label(new Rect(x, y - 1, 120, 30), hpText, style);
        GUI.Label(new Rect(x, y + 1, 120, 30), hpText, style);

        // 中央に白い文字を表示
        GUI.Label(new Rect(x, y, 120, 30), hpText, style);

        // GUI設定を元に戻す
        GUI.matrix = oldMatrix;
    }
}
