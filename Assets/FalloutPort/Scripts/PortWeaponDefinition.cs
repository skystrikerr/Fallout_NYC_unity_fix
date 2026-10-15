using UnityEngine;
namespace FalloutPort {
[System.Serializable] public class PortWeaponStats {
 public float damage,rof,reloadTime,range,spread,adsSpread,explosionRadius,projectileSpeed,meleeRange,kick,adsFov;public int magSize,pellets;public bool automatic,twoHanded;public string ammo,projectile,reloadType,damageType,viewmodel,sound,skill;
}
[System.Serializable] public class PortWeaponMotion {public string part;public Vector3 position,rotation;public float duration=.1f;}
public class PortWeaponDefinition:ScriptableObject {
 public Quaternion hipRotation=Quaternion.identity;public string rightPose,leftPose;public PortWeaponMotion[] shotMotions;public string id,displayName,description;public bool unique;public float weight;public int value;public PortWeaponStats stats;public GameObject prefab;public Vector3 muzzle,rightGrip,leftGrip,sight;public Quaternion rightRotation=Quaternion.identity,leftRotation=Quaternion.identity;public bool hasLeftGrip,oneHanded;public Vector3 hip=new Vector3(.16f,-.12f,.55f);public float viewScale=1;
}
}
