using System;
using UnityEngine;

[CreateAssetMenu(fileName ="VoidEvent",menuName ="Game/Events/Void")]
public class VoidEventChannel:ScriptableObject
{
    public event Action OnRaised;
    public void Raise() { OnRaised?.Invoke(); }
}