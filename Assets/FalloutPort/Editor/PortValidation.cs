using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FalloutPort;
public static class PortValidation {
 static List<string> results;
 static void Check(bool condition,string name){results.Add((condition?"PASS ":"FAIL ")+name);if(!condition){Save();throw new Exception(name);}}
 static void Save(){File.WriteAllLines("Assets/FalloutPort/Validation.txt",results);}
 static void Walk(PortPlayer p,Vector3 dest){var cc=p.GetComponent<CharacterController>();var doors=UnityEngine.Object.FindObjectsByType<PortDoor>(FindObjectsSortMode.None);for(int i=0;i<650;i++){foreach(var d in doors)d.Step(.02f);Physics.SyncTransforms();Vector3 delta=dest-p.transform.position;delta.y=0;if(delta.magnitude<.15f)break;cc.Move(delta.normalized*.055f+Vector3.down*.065f);}Vector3 error=dest-p.transform.position;error.y=0;Check(error.magnitude<.2f&&p.transform.position.y>-1,"capsule route to "+dest+" reached "+p.transform.position);}
 public static string Run(){
  results=new List<string>();var g=UnityEngine.Object.FindFirstObjectByType<PortGame>();Check(g!=null&&Application.isPlaying,"playable scene running");var p=g.player;p.enabled=false;g.stage=0;g.kills=0;g.ammo=12;g.reserve=48;
  p.Teleport(new Vector3(7.2f,.08f,-8.25f),-90);Walk(p,new Vector3(0,0,-8.25f));Walk(p,new Vector3(0,0,7));
  Check(g.mainDoor.blocker.enabled&&!g.mainDoor.unlocked,"vault exit locked before authorization");
  p.Teleport(new Vector3(1.5f,.42f,19.7f),0);p.view.transform.rotation=Quaternion.LookRotation(g.interactions[0].transform.position+Vector3.up-p.view.transform.position);Physics.SyncTransforms();g.FindInteraction();Check(g.current!=null&&g.current.id=="overseer","Overseer reachable through normal interaction ray");g.Interact();g.AdvanceDialogue();g.AdvanceDialogue();Check(g.stage==1,"birthday conversation advances to present");
  g.Perform("doyle");g.AdvanceDialogue();Check(g.stage==2&&g.hasPistol&&!g.holstered,"Doyle grants pistol and starts pest control");
  p.Teleport(new Vector3(-9,.08f,-69),0);foreach(var r in g.roaches){var bug=r.GetComponent<PortRoach>();bug.enabled=false;p.view.transform.rotation=Quaternion.LookRotation(r.transform.position+Vector3.up*.14f-p.view.transform.position);Physics.SyncTransforms();g.nextShot=0;g.Fire();Check(bug.dead,"actual shot kills "+r.name);}
  Check(g.stage==3&&g.kills==3&&g.ammo==9,"three hits advance quest and consume three rounds");
  g.Perform("locker");g.AdvanceDialogue();Check(g.stage==4,"locker starts eviction");g.Perform("overseer");g.AdvanceDialogue();g.AdvanceDialogue();Check(g.stage==5,"Overseer grants door override");g.Perform("terminal");g.AdvanceDialogue();g.mainDoor.Step(10);Physics.SyncTransforms();Check(g.stage==6&&!g.mainDoor.blocker.enabled,"authorized cog door opens and clears collision");
  p.Teleport(new Vector3(0,.1f,55),0);Walk(p,new Vector3(0,0,65));Walk(p,new Vector3(0,1.4f,84));g.Perform("exit");g.AdvanceDialogue();Check(g.stage==7,"exit completes this playable slice");
  int completed=g.stage;g.Perform("doyle");g.AdvanceDialogue();Check(g.stage==completed,"repeat dialogue does not regress quest");
  Check(UnityEngine.Object.FindObjectsByType<PortDoor>(FindObjectsSortMode.None).Length==17,"16 automatic doors plus main vault door");
  var audio=g.GetComponents<AudioSource>();Check(audio.Length>=2&&audio[1].clip!=null&&audio[1].isPlaying,"vault ambience audio source playing");
  Save();return string.Join("\n",results.ToArray());
 }
}
