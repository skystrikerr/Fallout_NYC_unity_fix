using UnityEngine;
namespace FalloutPort {
 public class PortRangeTarget:MonoBehaviour {public bool hit;Quaternion rest;void Awake(){rest=transform.rotation;}public void ResetTarget(){hit=false;transform.rotation=rest;}public void Hit(){if(hit||!PortSurvival.Instance.challengeActive)return;hit=true;transform.rotation=rest*Quaternion.Euler(65,0,0);PortSurvival.Instance.TargetHit();}}
}
