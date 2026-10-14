using UnityEngine;

public class CharacterCollectionPreview : MonoBehaviour
{
    public GameObject[] characters;
    public Camera previewCamera;
    public int selectedRole;
    int selectedMotion;
    float rotation;
    bool closeup;
    readonly string[] roles = {"Vault dwellers", "Mechanics", "Scouts", "Guards"};
    readonly string[] motions = {"Idle", "Look around", "Walk review"};
    void Start() { SelectRole(0); }
    void SelectRole(int role)
    {
        selectedRole=role;
        for(int i=0;i<characters.Length;i++) characters[i].SetActive(i/2==role);
        ApplyMotion();
    }
    void ApplyMotion()
    {
        foreach(var go in characters) if(go.activeSelf)
        {
            var animator=go.GetComponentInChildren<Animator>();
            if(animator!=null) animator.Play(new[]{"Idle","Look","Walk"}[selectedMotion],0,0);
        }
    }
    void Update()
    {
        for(int i=0;i<characters.Length;i++) if(characters[i].activeSelf) { characters[i].transform.rotation=Quaternion.Euler(0,180+rotation,0); characters[i].transform.position=new Vector3(i%2==0 ? (closeup?-.23f:-.55f) : (closeup?.23f:.55f), closeup && i%2==0?.10f:0,0); }
        if(previewCamera!=null)
        {
            previewCamera.transform.position=closeup ? new Vector3(0,1.64f,-1.3f) : new Vector3(0,1.12f,-3.2f);
            previewCamera.transform.LookAt(new Vector3(0,closeup?1.62f:0.95f,0));
            previewCamera.fieldOfView=closeup?36:43;
        }
    }
    void OnGUI()
    {
        GUI.skin.button.fontSize=16;GUI.skin.label.fontSize=17;
        GUILayout.BeginArea(new Rect(18,16,700,150),GUI.skin.box);
        GUILayout.Label("CHARACTER COLLECTION  /  FIRST PASS");
        int next=GUILayout.Toolbar(selectedRole,roles,GUILayout.Height(32));if(next!=selectedRole)SelectRole(next);
        GUILayout.BeginHorizontal();
        int motion=GUILayout.Toolbar(selectedMotion,motions,GUILayout.Height(30));if(motion!=selectedMotion){selectedMotion=motion;ApplyMotion();}
        if(GUILayout.Button(closeup?"Full body":"Faces",GUILayout.Height(30)))closeup=!closeup;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();GUILayout.Label("Turn",GUILayout.Width(45));rotation=GUILayout.HorizontalSlider(rotation,-180,180);if(GUILayout.Button("Front",GUILayout.Width(65)))rotation=0;GUILayout.EndHorizontal();
        GUILayout.EndArea();
        GUI.Label(new Rect(20,Screen.height-38,1000,30),"Female + male  •  original skin rig  •  separate garments and hair  •  review animations");
    }
}
