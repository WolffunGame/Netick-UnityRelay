using UnityEngine;

[CreateAssetMenu(fileName = "StringSo", menuName = "Data Type Wrapper So/String")]
public class StringSo : ScriptableObject
{
    [SerializeField] private string _value;
    public string Value
    {
        get => _value;
        private set => _value = value;
    }

    public void SetValue(string inValue) => _value = inValue;
}