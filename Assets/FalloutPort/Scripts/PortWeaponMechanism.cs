using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace FalloutPort {
public class PortWeaponMechanism:MonoBehaviour {
 class Part {public Transform transform;public Vector3 position;public Quaternion rotation;public PortWeaponMotion motion;}
 readonly List<Part> parts=new List<Part>();float fired=-100;PortWeaponDefinition definition;
 public void Setup(PortWeaponDefinition def){definition=def;var nodes=GetComponentsInChildren<Transform>();foreach(var m in def.shotMotions??new PortWeaponMotion[0]){var t=nodes.FirstOrDefault(x=>x.name==m.part);if(t)parts.Add(new Part{transform=t,position=t.localPosition,rotation=t.localRotation,motion=m});}}
 public void Fire(){fired=Time.time;}
 void Update(){foreach(var p in parts){float elapsed=(Time.time-fired)/Mathf.Max(.04f,p.motion.duration);float amount=elapsed<1?Mathf.Sin(Mathf.Clamp01(elapsed)*Mathf.PI):0;p.transform.localPosition=p.position+p.motion.position*amount;p.transform.localRotation=p.rotation*Quaternion.Euler(p.motion.rotation*amount);}}
}
}
