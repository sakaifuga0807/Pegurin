using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[Serializable] public class FloatUnityEvent : UnityEvent<float> { }
[Serializable] public class StringUnityEvent : UnityEvent<string> { }
[Serializable] public class BoolUnityEvent : UnityEvent<bool> { }
[Serializable] public class SpriteUnityEvent : UnityEvent<Sprite> { }
