using Netick.Unity;
using Reflex.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class WeaponInstaller : MonoBehaviour, IInstaller
{
    [Header("Configuration Assets")]
    [SerializeField] private WeaponConfig _weaponConfig;
    [SerializeField] private ShotConfig shotConfig;
    
    public void InstallBindings(ContainerBuilder builder)
    {
        // Validate configurations
        if (_weaponConfig == null)
        {
            Debug.LogError("WeaponInstaller: WeaponConfig is null!");
            return;
        }
        
        if (shotConfig == null)
        {
            Debug.LogError("WeaponInstaller: ProjectileConfig is null!");
            return;
        }

        // Bind configuration ScriptableObjects as instances 
        builder.AddSingleton(_weaponConfig, typeof(WeaponConfig));
        builder.AddSingleton(shotConfig, typeof(ShotConfig));
        
        // Bind service implementations as singletons 
        builder.AddSingleton(typeof(WeaponService), typeof(IWeaponService));
        builder.AddSingleton(typeof(DamageService), typeof(IDamageService));
        
        // Bind data providers as singletons
        builder.AddSingleton(typeof(ShotFactory), typeof(IShotFactory));
        builder.AddSingleton(typeof(WeaponDataProvider));
        builder.AddSingleton(typeof(ShotDataProvider));
        
        
        Debug.Log("WeaponInstaller: All bindings registered successfully with correct Reflex syntax");
    }
}