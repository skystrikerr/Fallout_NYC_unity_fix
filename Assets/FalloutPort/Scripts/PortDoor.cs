using UnityEngine;
namespace FalloutPort {
public class PortDoor : MonoBehaviour {
 public Transform player; public bool vaultDoor;public bool unlocked;public float height=3;public Vector3 closedPosition;public BoxCollider blocker;public float openness;
 public void Step(float dt){
  bool nearby=player && Vector2.Distance(new Vector2(player.position.x,player.position.z),new Vector2(closedPosition.x,closedPosition.z))<2.4f && Mathf.Abs(player.position.y-closedPosition.y)<2.3f;
  float target=vaultDoor?(unlocked?1:0):(nearby?1:0);
  openness=Mathf.MoveTowards(openness,target,dt*(vaultDoor?.16f:1.7f));
  transform.position=closedPosition+(vaultDoor?Vector3.right*11:Vector3.up*(height+.15f))*openness;
  // A closed door blocks movement. It only clears when there is headroom.
  if(blocker)blocker.enabled=openness<(vaultDoor?.7f:.85f);
 }
 void Update(){Step(Time.deltaTime);}
}
}
