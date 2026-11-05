using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class  SoundBankEntry
{
    public SeCue cue;
    public SoundEffect effect;
}

//音の名前の表。SeCueの全部の名前の行が自動で並ぶ。
[CreateAssetMenu(fileName ="SoundBank",menuName ="Game/SoundBank")]
public class SoundBank:ScriptableObject
{
    [SerializeField] private List<SoundBankEntry> entries = new List<SoundBankEntry>();

    public IReadOnlyList<SoundBankEntry> Entries => entries;

    private void Reset()
    {
        AddMissingRows();
    }
    private void OnValidate()
    {
        AddMissingRows();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void AddMissingRows()
    {
        var existing = new HashSet<SeCue>();
        foreach (var entry in entries) existing.Add(entry.cue);

        foreach(SeCue cue in Enum.GetValues(typeof(SeCue)))
        {
            if(!existing.Contains(cue))
            {
                entries.Add(new SoundBankEntry { cue = cue });
            }
        }
    }
}