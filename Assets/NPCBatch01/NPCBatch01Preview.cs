using UnityEngine;
public class NPCBatch01Preview : MonoBehaviour
{
 public GameObject[] characters;
 public Camera previewCamera;
 int selected;int motion;float turn;bool face;
 readonly string[] labels={"Courier","Clinic","Mechanic","Security","Civilian"};
 readonly string[] states={"Idle","Look","Walk"};
 void Start(){Select(0);}
 public void Select(int index){selected=index;for(int i=0;i<characters.Length;i++)characters[i].SetActive(i==selected);ApplyMotion();}
 void ApplyMotion(){var a=characters[selected].GetComponentInChildren<Animator>();if(a!=null)a.Play(states[motion],0,0);}
 void Update(){var g=characters[selected];g.transform.position=Vector3.zero;g.transform.rotation=Quaternion.Euler(0,180+turn,0);float eye=selected==0||selected==2?1.56f:1.67f;previewCamera.transform.position=face?new Vector3(0,eye,-.72f):new Vector3(0,1.10f,-2.8f);previewCamera.transform.LookAt(new Vector3(0,face?eye-.025f:.94f,0));previewCamera.fieldOfView=face?36:43;}
 void OnGUI(){GUILayout.BeginArea(new Rect(18,16,700,165),GUI.skin.box);GUILayout.Label("NPC COLLECTION — BATCH 01 / PROTOTYPES");int next=GUILayout.Toolbar(selected,labels,GUILayout.Height(32));if(next!=selected)Select(next);GUILayout.BeginHorizontal();int m=GUILayout.Toolbar(motion,new[]{"Idle","Look around","Walk review"},GUILayout.Height(30));if(m!=motion){motion=m;ApplyMotion();}if(GUILayout.Button(face?"Outfit":"Face",GUILayout.Height(30)))face=!face;GUILayout.EndHorizontal();GUILayout.BeginHorizontal();GUILayout.Label("Turn",GUILayout.Width(40));turn=GUILayout.HorizontalSlider(turn,-180,180);if(GUILayout.Button("Front",GUILayout.Width(60)))turn=0;GUILayout.EndHorizontal();GUILayout.Label("Five face shapes • five modeled outfits • first review batch");GUILayout.EndArea();}
}
