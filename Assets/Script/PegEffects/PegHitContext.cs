using UnityEngine;

public struct PegHitContext
{
    public Orb orb;       // “–‚Ä‚½‹Ê
    public Peg peg;       // “–‚½‚Á‚½ƒyƒO
    public PegBoard board; // ”Õ–Ê

    public PegHitContext(Orb orb, Peg peg, PegBoard board)
    {
        this.orb = orb;
        this.peg = peg;
        this.board = board;
    }
}