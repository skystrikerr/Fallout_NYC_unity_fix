using UnityEngine;
using System.Collections.Generic;
namespace FalloutPort {
public class PortCombat:MonoBehaviour {
 PortGame game; Material tracerMat,impactMat; readonly Queue<GameObject> marks=new Queue<GameObject>();
 void Awake(){game=GetComponent<PortGame>();tracerMat=Resources.Load<Material>("FalloutFX/Tracer");impactMat=Resources.Load<Material>("FalloutFX/Impact");}
 public void Shot(){var camera=game.player.view;Vector3 origin=camera.transform.position;Vector3 end=origin+camera.transform.forward*90;RaycastHit hit;bool struck=Physics.Raycast(origin,camera.transform.forward,out hit,90,~(1<<2),QueryTriggerInteraction.Collide);if(struck)end=hit.point;
  Vector3 muzzle=game.player.weapon.TransformPoint(new Vector3(0,.03f,.23f));RaycastHit close;if(Physics.Linecast(origin,muzzle,out close,~(1<<2),QueryTriggerInteraction.Ignore))muzzle=origin+camera.transform.forward*.1f;
  if(Physics.Linecast(muzzle,end,out close,~(1<<2),QueryTriggerInteraction.Ignore)&&(!struck||close.distance<Vector3.Distance(muzzle,end)-.03f)){hit=close;struck=true;end=close.point;}
  var line=new GameObject("Bullet tracer").AddComponent<LineRenderer>();line.sharedMaterial=tracerMat;line.positionCount=2;line.SetPosition(0,muzzle);line.SetPosition(1,end);line.startWidth=.012f;line.endWidth=.003f;line.startColor=new Color(1,.76f,.25f);line.endColor=new Color(1,.35f,.08f);line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;Destroy(line.gameObject,.065f);
  var flash=new GameObject("Muzzle flash").AddComponent<Light>();flash.transform.position=muzzle;flash.color=new Color(1,.68f,.25f);flash.intensity=2;flash.range=2.5f;Destroy(flash.gameObject,.045f);game.player.Kick();
  if(!struck)return;var bug=hit.collider.GetComponentInParent<PortRoach>();var target=hit.collider.GetComponentInParent<PortRangeTarget>();var citizen=hit.collider.GetComponentInParent<PortCitizen>();if(bug){bug.Hit(camera.transform.forward,hit.point);PortSurvival.Instance.hitUntil=Time.time+.16f;}else if(target){target.Hit();PortSurvival.Instance.hitUntil=Time.time+.16f;}else if(citizen){citizen.React(camera.transform.position,true);game.notice="Watch your fire! Vault residents are essential to the quest.";}else if(hit.rigidbody&&!hit.rigidbody.isKinematic)hit.rigidbody.AddForceAtPosition(camera.transform.forward*6,hit.point,ForceMode.Impulse);
  Impact(hit,bug!=null);foreach(var resident in FindObjectsByType<PortCitizen>(FindObjectsSortMode.None))if(Vector3.Distance(resident.transform.position,end)<8)resident.React(camera.transform.position,false);
 }
 void Impact(RaycastHit hit,bool organic){if(hit.collider.isTrigger)return;var dot=GameObject.CreatePrimitive(PrimitiveType.Quad);dot.name="Impact mark";Destroy(dot.GetComponent<Collider>());dot.layer=2;dot.transform.position=hit.point+hit.normal*.008f;dot.transform.rotation=Quaternion.LookRotation(-hit.normal);dot.transform.localScale=Vector3.one*(organic?.055f:.075f);dot.GetComponent<Renderer>().sharedMaterial=impactMat;if(hit.rigidbody)dot.transform.SetParent(hit.transform,true);marks.Enqueue(dot);while(marks.Count>48){var old=marks.Dequeue();if(old)Destroy(old);}Destroy(dot,35);}
 void OnDestroy(){foreach(var m in marks)if(m)Destroy(m);}
}
}
