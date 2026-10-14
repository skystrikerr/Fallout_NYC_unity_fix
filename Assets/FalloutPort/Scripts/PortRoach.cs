using UnityEngine;
namespace FalloutPort {
public class PortRoach : MonoBehaviour {
 public bool dead; public float phase;Vector3 home;
 void Start(){home=transform.position;}
 void Update(){if(dead)return;float t=Time.time+phase;transform.position=home+new Vector3(Mathf.Sin(t*.8f)*.45f,0,Mathf.Cos(t*.7f)*.45f);transform.rotation=Quaternion.Euler(0,t*38,0);}
 public void Hit(){if(dead)return;dead=true;transform.localScale=new Vector3(1,.22f,1);foreach(var c in GetComponentsInChildren<Collider>())c.enabled=false;var game=FindFirstObjectByType<PortGame>();if(game)game.RoachKilled();}
}
}
