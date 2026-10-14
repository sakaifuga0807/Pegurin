using System;
using UnityEngine;

//値付きのイベントチャンネルの土台。
public abstract class EventChannel<T>:ScriptableObject
{
    public event Action<T> OnRaised;
    public void Raise(T value) { OnRaised?.Invoke(value); }
}