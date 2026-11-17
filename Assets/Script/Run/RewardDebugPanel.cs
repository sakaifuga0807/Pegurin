using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor.MemoryProfiler;
using UnityEngine;

#if UNITY_EDITOR||DEVELOPMENT_BUILD
//開発用。報酬を選ぶ簡易パネル。実際にはパネルを作ってねー。
public class RewardDebugPanel : MonoBehaviour
{
    [SerializeField] private RunManager runManager;

    private List<OrbData> orbs = new List<OrbData>();
    private List<PassiveEffect> passives = new List<PassiveEffect>();
    private int healed;
    private bool visible;
    private bool orbChosen, passiveChosen;

    //ボタンの操作はOnGUIの中では実行せずにUpdateでまとめて実行する。
    private OrbData pendingOrb;
    private PassiveEffect pendingPassive;

    private void OnEnable()
    {
        if (runManager != null) runManager.OnRewardStarted += OnRewardStarted;
    }

    private void Disabled()
    {
        if (runManager != null) runManager.OnRewardStarted -= OnRewardStarted;
    }

    private void OnRewardStarted(List<OrbData> orbList, List<PassiveEffect> passiveList, int healedHp)
    {
        orbs = orbList;
        passives = passiveList;
        healed = healedHp;
        orbChosen = false;
        passiveChosen = false;
    }

    private void Update()
    {
        if (runManager == null) return;

        if (pendingOrb != null)
        {
            runManager.SelectRewardOrb(pendingOrb);
            orbChosen = true;
            pendingOrb = null;
        }
        if (pendingPassive != null)
        {
            runManager.SelectRewardPassive(pendingPassive);
            passiveChosen = true;
            pendingPassive = null;
        }

        visible = runManager.Phase == RunPhase.Reward;
    }

    private void OnGUI()
    {
        if (!visible) return;

        float scale = Screen.height / 720f;
        if (scale <= 0f) return;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        float virtualWidth = Screen.width / scale;

        var panel = new Rect(virtualWidth / 2f - 320f, 150f, 640f, 420f);
        GUI.Box(panel, "報酬を選択してください。");

        GUILayout.BeginArea(new Rect(panel.x + 16f, panel.y + 30f, panel.width - 32f, panel.height - 40f));
        GUILayout.Label($"ステージクリア！HP+{healed}");
        GUILayout.Space(8);

        GUILayout.Label("オーブを1つ選ぶ");
        if (orbChosen) GUILayout.Label("(選びました)");
        else
        {
            foreach (var orb in orbs)
            {
                if (GUILayout.Button(orb.orbName, GUILayout.Height(30))) pendingOrb = orb;
            }
        }

        GUILayout.Space(8);
        GUILayout.Label("パッシブを1つ選ぶ。");
        if (passiveChosen) GUILayout.Label("(選びました)");
        else if (passives.Count == 0) GUILayout.Label("(選べるパッシブはありません)");
        else
        {
            foreach (var passive in passives)
            {
                if (GUILayout.Button(passive.PassiveName, GUILayout.Height(30))) pendingPassive = passive;
            }
        }
        GUILayout.EndArea();
    }
}


#endif