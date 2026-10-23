using UnityEngine;
using System.Collections.Generic;

public class PegBoard : MonoBehaviour
{
    private List<Peg> allPegs = new List<Peg>();

    private void Awake()
    {
        Peg[] foundPegs = GetComponentsInChildren<Peg>(true);
        allPegs = new List<Peg>(foundPegs);
    }

    /// <summary>
    /// ”Õ–Ê‚ÌƒyƒO‚ð‘S•œŠˆ‚³‚¹‚é
    /// </summary>
    public void RefreshAllPegs()
    {
        foreach (var peg in allPegs)
        {
            if (peg != null)
            {
                peg.ResetPegState();
            }
        }
    }
}