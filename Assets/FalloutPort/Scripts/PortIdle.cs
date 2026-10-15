using UnityEngine;
namespace FalloutPort {
public class PortIdle : MonoBehaviour {
 public int seed; public string current;Animator animator;System.Random random;float next;string activity;
 static readonly string[] Clips={"Relaxed","LookAround","WeightShift","CheckWrist","AdjustSleeve","GuardWatch","Conversation"};
 void Start(){animator=GetComponentInChildren<Animator>();random=new System.Random(seed*317+29);if(!animator){enabled=false;return;}animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.applyRootMotion=false;animator.speed=.88f+(float)random.NextDouble()*.22f;Choose(true);}
 public void Choose(bool first=false){if(!animator)return;int pick=random.Next(Clips.Length);string selected="Idle_"+Clips[pick];if(selected==current)selected="Idle_"+Clips[(pick+1)%Clips.Length];current=selected;int hash=Animator.StringToHash(current);if(!animator.HasState(0,hash))return;if(first)animator.Play(hash,0,(float)random.NextDouble());else animator.CrossFadeInFixedTime(hash,.8f);next=Time.time+7+(float)random.NextDouble()*7;}
 public void Activity(string state){if(activity==state)return;activity=state;if(!animator)return;if(state==null){Choose();return;}if(animator.HasState(0,Animator.StringToHash(state)))animator.CrossFadeInFixedTime(state,.4f);}
 void Update(){if(activity==null&&Time.time>=next)Choose();}
}
}
