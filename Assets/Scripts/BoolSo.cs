using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BoolSo", menuName = "Data Type Wrapper So/Bool")]
public class BoolSo : ScriptableObject
{
    [SerializeField] private bool _value;

    private void OnEnable() => _value = false;

    public bool Value
    {
        get => _value;
        private set => _value = value;
    }

    public void SetValue(bool inValue) => _value = inValue;
}