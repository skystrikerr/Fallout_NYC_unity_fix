using UnityEngine;
public class NPCCollectionPreview : MonoBehaviour
{
 public GameObject[] characters;
 public Camera previewCamera;
 public string[] labels;
 public bool[] female;
 public int selected;
 public int motion;
 public float turn;
 public bool face;
 bool controlsVisible=true;
 readonly string[] states={"Idle","Look","Walk"};
 void Start(){Select(0);}
 public void Select(int index){selected=Mathf.Clamp(index,0,characters.Length-1);for(int i=0;i<characters.Length;i++)characters[i].SetActive(i==selected);ApplyMotion();Update();}
 public void ApplyMotion(){var a=characters[selected].GetComponentInChildren<Animator>();if(a!=null)a.Play(states[motion],0,0);}
 void Update(){if(characters==null||characters.Length==0)return;var g=characters[selected];g.transform.position=Vector3.zero;g.transform.rotation=Quaternion.Euler(0,180+turn,0);float eye=female[selected]?1.56f:1.67f;previewCamera.transform.position=face?new Vector3(0,eye,-.72f):new Vector3(0,1.10f,-2.8f);previewCamera.transform.LookAt(new Vector3(0,face?eye-.025f:.94f,0));previewCamera.fieldOfView=face?36:43;}
 void OnGUI(){
  if(!controlsVisible){if(GUI.Button(new Rect(18,Screen.height-48,150,30),"Show controls"))controlsVisible=true;return;}
  float width=Mathf.Min(850,Screen.width-36);
  GUILayout.BeginArea(new Rect(18,Mathf.Max(12,Screen.height-221),width,205),GUI.skin.box);
  GUILayout.BeginHorizontal();GUILayout.Label("NPC COLLECTION — 50 FACE SHAPES / 50 OUTFIT DESIGNS");if(GUILayout.Button("Hide controls",GUILayout.Width(110)))controlsVisible=false;GUILayout.EndHorizontal();
  GUILayout.BeginHorizontal();
  if(GUILayout.Button("Previous 10",GUILayout.Width(110)))Select((selected/10+4)%5*10);
  GUILayout.Label("Set "+(selected/10+1)+" / 5 — "+labels[selected]);
  if(GUILayout.Button("Next 10",GUILayout.Width(110)))Select((selected/10+1)%5*10);
  GUILayout.EndHorizontal();
  int start=selected/10*10;string[] choices=new string[Mathf.Min(10,characters.Length-start)];for(int i=0;i<choices.Length;i++)choices[i]=labels[start+i];
  int next=GUILayout.SelectionGrid(selected-start,choices,5,GUILayout.Height(58));if(next!=selected-start)Select(start+next);
  GUILayout.BeginHorizontal();int m=GUILayout.Toolbar(motion,new[]{"Idle","Look around","Walk review"},GUILayout.Height(28));if(m!=motion){motion=m;ApplyMotion();}if(GUILayout.Button(face?"Show outfit":"Show face",GUILayout.Height(28)))face=!face;GUILayout.EndHorizontal();
  GUILayout.BeginHorizontal();GUILayout.Label("Turn",GUILayout.Width(40));turn=GUILayout.HorizontalSlider(turn,-180,180);if(GUILayout.Button("Front",GUILayout.Width(60)))turn=0;GUILayout.EndHorizontal();
  GUILayout.Label("Review collection • shared skin atlases • gameplay port follows asset review");GUILayout.EndArea();
 }
}
