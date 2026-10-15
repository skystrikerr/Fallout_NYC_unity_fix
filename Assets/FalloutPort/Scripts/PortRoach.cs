using UnityEngine;
namespace FalloutPort {
public class PortRoach : MonoBehaviour {
 public bool dead;public float phase;Vector3 home;float nextAttack;PortGame game;Rigidbody body;bool initialized;
 void EnsureHome(){if(initialized)return;initialized=true;home=transform.position;}
 void Awake(){EnsureHome();}
 void Start(){game=FindFirstObjectByType<PortGame>();}
 public void Restore(bool isDead){EnsureHome();dead=isDead;transform.position=home;transform.rotation=Quaternion.identity;body=GetComponent<Rigidbody>();if(body){if(!body.isKinematic){body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;}body.isKinematic=true;}transform.localScale=new Vector3(1,dead?.35f:1,1);foreach(var c in GetComponentsInChildren<Collider>())c.enabled=!dead;enabled=true;}
 void Update(){if(dead||!game||game.paused||game.dialogueOpen)return;Vector3 player=game.player.transform.position;Vector3 delta=player-transform.position;delta.y=0;float distance=delta.magnitude;bool sees=distance<9&&!Physics.Linecast(transform.position+Vector3.up*.25f,player+Vector3.up*.7f,~(1<<2),QueryTriggerInteraction.Ignore);Vector3 target=sees?player:home+new Vector3(Mathf.Sin(Time.time*.8f+phase)*.65f,0,Mathf.Cos(Time.time*.7f+phase)*.65f);Vector3 move=target-transform.position;move.y=0;if(move.magnitude>.65f){Vector3 dir=move.normalized;RaycastHit block;if(!Physics.SphereCast(transform.position+Vector3.up*.25f,.13f,dir,out block,.35f,~(1<<2),QueryTriggerInteraction.Ignore)){transform.position+=dir*(sees?1.25f:.35f)*Time.deltaTime;transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(dir),240*Time.deltaTime);}}if(sees&&distance<.95f&&Time.time>nextAttack){nextAttack=Time.time+1.15f;PortSurvival.Instance.Damage(12);}}
 public void Hit(){Hit(Vector3.forward,transform.position);}
 public void Hit(Vector3 direction,Vector3 point){if(dead)return;dead=true;if(!game)game=FindFirstObjectByType<PortGame>();if(game)game.RoachKilled();body=GetComponent<Rigidbody>();if(!body)body=gameObject.AddComponent<Rigidbody>();body.mass=1.8f;body.linearDamping=1.8f;body.angularDamping=2;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;body.isKinematic=false;body.AddForce(direction.normalized*3+Vector3.up*1.8f,ForceMode.Impulse);body.AddTorque(Vector3.Cross(Vector3.up,direction)*2,ForceMode.Impulse);}
 }
}
