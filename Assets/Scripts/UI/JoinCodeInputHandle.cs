
using TMPro;
using UnityEngine;

public class JoinCodeInputHandle : MonoBehaviour
{
    [SerializeField] private TMP_InputField _joinCodeInput;
    [SerializeField] private StringSo _joinCodeSo;
    private void LateUpdate() => _joinCodeInput.text = _joinCodeSo.Value;
}
