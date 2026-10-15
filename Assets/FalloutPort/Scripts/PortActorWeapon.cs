using System.Linq;
using UnityEngine;
namespace FalloutPort {
// Fits each weapon's authored grip sockets to the approved character skeleton.
public class PortActorWeapon:MonoBehaviour {
 public PortWeaponDefinition definition;Transform model;Transform[] bones;PortWorldActor actor;
 void Start(){actor=GetComponent<PortWorldActor>();bones=GetComponentsInChildren<Transform>();if(!definition||!Bone("wrist.R")){enabled=false;return;}model=Instantiate(definition.prefab,transform).transform;model.name="Equipped "+definition.displayName;var arms=model.Find("First person arms");if(arms){arms.gameObject.SetActive(false);Destroy(arms.gameObject);}foreach(var part in model.GetComponentsInChildren<Transform>())part.gameObject.layer=0;}
 Transform Bone(string name){return bones.FirstOrDefault(b=>b.name==name);}
 void LateUpdate(){if(!model||actor.dead)return;bool melee=definition.stats.reloadType=="melee";model.position=transform.TransformPoint(new Vector3(.12f,melee?1.05f:1.25f,melee?.4f:.5f));model.rotation=transform.rotation*definition.hipRotation;Fit("R",definition.rightGrip,definition.rightRotation,definition.rightPose);if(definition.hasLeftGrip&&!definition.oneHanded)Fit("L",definition.leftGrip,definition.leftRotation,definition.leftPose);}
 void Fit(string side,Vector3 grip,Quaternion rotation,string pose){var upper=Bone("upperarm01."+side);var elbow=Bone("lowerarm01."+side);var wrist=Bone("wrist."+side);var index=Bone("finger2-1."+side);var little=Bone("finger5-1."+side);if(!upper||!elbow||!wrist||!index||!little)return;
  Quaternion frame=model.rotation*rotation;Vector3 forward=frame*Vector3.forward,back=frame*Vector3.up,across=frame*Vector3.right;Vector3 target=model.TransformPoint(grip)-forward*.085f+back*.028f;Vector3 origin=upper.position;float a=Vector3.Distance(origin,elbow.position),b=Vector3.Distance(elbow.position,wrist.position);Vector3 delta=target-origin;float distance=Mathf.Clamp(delta.magnitude,.01f,(a+b)*.995f);Vector3 axis=delta.normalized,pole=transform.TransformDirection(new Vector3(side=="R"?1:-1,-.6f,-.2f));Vector3 bend=Vector3.ProjectOnPlane(pole,axis).normalized;float along=(a*a+distance*distance-b*b)/(2*distance);Vector3 joint=origin+axis*along+bend*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));upper.rotation=Quaternion.FromToRotation(elbow.position-origin,joint-origin)*upper.rotation;elbow.rotation=Quaternion.FromToRotation(wrist.position-elbow.position,target-elbow.position)*elbow.rotation;
  Quaternion oldFrame=Quaternion.LookRotation((index.position+little.position)*.5f-wrist.position,little.position-index.position);wrist.rotation=Quaternion.LookRotation(forward,across)*Quaternion.Inverse(oldFrame)*wrist.rotation;
  for(int digit=2;digit<=5;digit++){var one=Bone("finger"+digit+"-1."+side);var two=Bone("finger"+digit+"-2."+side);var three=Bone("finger"+digit+"-3."+side);if(!one||!two||!three)continue;one.rotation=Quaternion.FromToRotation(two.position-one.position,-back)*one.rotation;two.rotation=Quaternion.FromToRotation(three.position-two.position,-forward)*two.rotation;}
 }
}
}
