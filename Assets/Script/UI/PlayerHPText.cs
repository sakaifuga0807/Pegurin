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
        float y = Screen.height * 0.32f;

        GUI.Label(
            new Rect(x, y, 120, 30),
            player.CurrentHp + " / " + player.MaxHp,
            style
        );
    }
}
