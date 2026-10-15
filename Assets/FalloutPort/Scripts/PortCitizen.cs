using UnityEngine;
namespace FalloutPort {
public class PortCitizen:MonoBehaviour {
 public Vector3 home; public string activity="At ease";Vector3 destination;float next,alarm;PortGame game;PortIdle idle;System.Random random;
 void Start(){home=transform.position;destination=home;game=FindFirstObjectByType<PortGame>();idle=GetComponent<PortIdle>();random=new System.Random(name.GetHashCode());next=Time.time+2+(float)random.NextDouble()*5;}
 public void React(Vector3 threat,bool direct){alarm=Time.time+(direct?5:2.5f);destination=transform.position;activity="Taking cover";if(idle)idle.Activity("Idle_GuardWatch");Vector3 away=transform.position-threat;away.y=0;if(away.sqrMagnitude>.01f)transform.rotation=Quaternion.LookRotation(-away);}
 public void ResetHome(){transform.position=home;destination=home;next=Time.time+4;alarm=0;}
 void Update(){if(!game||!idle||Time.timeScale==0)return;float dt=Time.deltaTime;Vector3 player=game.player.transform.position;Vector3 delta=player-transform.position;delta.y=0;if(Time.time<alarm)return;
  if(delta.magnitude<2.6f){activity="Greeting visitor";idle.Activity(null);if(delta.sqrMagnitude>.1f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(delta),65*dt);next=Time.time+3;return;}
  if(Time.time>=next){next=Time.time+9+(float)random.NextDouble()*9;float a=(float)random.NextDouble()*Mathf.PI*2;float radius=.6f+(float)random.NextDouble()*1.2f;var trial=home+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*radius;RaycastHit floor;if(Physics.Raycast(trial+Vector3.up,Vector3.down,out floor,1.6f,~(1<<2),QueryTriggerInteraction.Ignore)&&Mathf.Abs(floor.point.y-home.y)<.15f&&!Physics.CheckCapsule(trial+Vector3.up*.4f,trial+Vector3.up*1.5f,.32f,~(1<<2),QueryTriggerInteraction.Ignore))destination=new Vector3(trial.x,floor.point.y,trial.z);}
  Vector3 move=destination-transform.position;move.y=0;if(move.magnitude>.12f){Vector3 direction=move.normalized;RaycastHit obstacle;if(Physics.CapsuleCast(transform.position+Vector3.up*.42f,transform.position+Vector3.up*1.45f,.3f,direction,out obstacle,.42f,~(1<<2),QueryTriggerInteraction.Ignore)){destination=transform.position;idle.Activity(null);return;}foreach(var other in FindObjectsByType<PortCitizen>(FindObjectsSortMode.None)){if(other!=this&&Vector3.Distance(transform.position+direction*.4f,other.transform.position)<.65f){destination=transform.position;idle.Activity(null);return;}}transform.position+=direction*Mathf.Min(move.magnitude,.55f*dt);transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(direction),100*dt);activity="Walking";idle.Activity("Walk");}else{activity="At ease";idle.Activity(null);}
 }
}
}
