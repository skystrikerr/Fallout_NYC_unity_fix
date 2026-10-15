using UnityEngine;
using Newtonsoft.Json.Linq;
namespace FalloutPort {
// Native view of an actor simulated by the original world AI.
public class PortWorldActor:MonoBehaviour {
 public string actorId;public PortQuestRuntime runtime;public bool dead,hostile;public float health,height=1.8f;Vector3 destination;Quaternion facing;bool received;string state;Animation legacy;PortIdle idle;Animator animator;Transform body;
 void Start(){legacy=GetComponent<Animation>();idle=GetComponent<PortIdle>();animator=GetComponentInChildren<Animator>();destination=transform.position;facing=transform.rotation;body=animator?animator.transform:null;}
 public void Apply(JToken data){destination=new Vector3((float)data["pos"]["x"],(float)data["pos"]["y"],-(float)data["pos"]["z"]);facing=Quaternion.Euler(0,-(float)data["yaw"]*Mathf.Rad2Deg,0);received=true;bool wasDead=dead;dead=(bool)data["dead"];hostile=(bool)data["hostile"];health=(float)data["hp"];string next=(string)data["animation"];if(dead)next="death";
  if(!wasDead&&dead){if(idle)idle.enabled=false;if(animator)animator.enabled=false;var col=GetComponent<CapsuleCollider>();if(col){col.height=.5f;col.center=Vector3.up*.25f;}}
  if(next!=state){state=next;if(legacy){string clip=dead?"death":next=="walk"?"walk":next=="run"?"run":next.StartsWith("attack")?"attack_melee":"idle";if(legacy.GetClip(clip))legacy.CrossFade(clip,.15f);}if(idle&&!dead)idle.Activity(next=="walk"||next=="run"?"Walk":next=="talk"?"Idle_Conversation":null);}
 }
 void Update(){if(!idle)idle=GetComponent<PortIdle>();if(!received||Time.timeScale==0)return;transform.position=Vector3.Lerp(transform.position,destination,1-Mathf.Exp(-18*Time.deltaTime));transform.rotation=Quaternion.Slerp(transform.rotation,facing,1-Mathf.Exp(-12*Time.deltaTime));if(dead&&body)body.localRotation=Quaternion.Slerp(body.localRotation,Quaternion.Euler(85,0,0),1-Mathf.Exp(-5*Time.deltaTime));}
 public void Hit(float amount,string weapon,Vector3 point,Vector3 direction){if(dead)return;string part=point.y-transform.position.y>height*.8f?"head":"torso";runtime.Hit(actorId,amount,weapon,part,direction);}
}
}
