using System;
using System.Diagnostics.CodeAnalysis;
using Netick;
using Netick.Unity;
using UnityEngine;

namespace Tank.Scripts
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class Tank : NetworkBehaviour
    {
        private static readonly int EnergyColor = Shader.PropertyToID("_EnergyColor");
        
        [Networked] public byte TankIndex { get; set; }
        [SerializeField] private Material[] _tankMaterials;
        [SerializeField] private TankMoveControl _moveControl;

        public Material PlayerMaterial { get; set; }
        public Color PlayerColor { get; set; }
        
        public TankMoveControl MoveControl => _moveControl;
        
        [OnChanged(nameof(TankIndex))]
        private void OnIndexChange(OnChangedData onChangedData)
        {
            if (TankIndex >= _tankMaterials.Length)
                return;
            PlayerMaterial = Instantiate(_tankMaterials[TankIndex]);
            PlayerColor = PlayerMaterial.GetColor(EnergyColor);
            var tankParts = GetComponentsInChildren<TankPartMesh>();
            foreach (var part in tankParts)
                part.SetMaterial(PlayerMaterial);
        }

        private void OnValidate()=> _moveControl??=GetComponent<TankMoveControl>();
    }
}