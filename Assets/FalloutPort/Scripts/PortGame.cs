using System;
using System.Collections.Generic;
using UnityEngine;
namespace FalloutPort {
public class PortGame : MonoBehaviour {
 public PortMenu menu;
 void Awake(){if(!GetComponent<PortLoadout>())gameObject.AddComponent<PortLoadout>();if(!GetComponent<PortInventory>())gameObject.AddComponent<PortInventory>();if(!GetComponent<PortSurvival>())gameObject.AddComponent<PortSurvival>();if(!GetComponent<PortCombat>())gameObject.AddComponent<PortCombat>();menu=GetComponent<PortMenu>();if(!menu)menu=gameObject.AddComponent<PortMenu>();}
 public PortPlayer player;public PortDoor mainDoor;public GameObject[] roaches;public PortInteractable[] interactions;
 public int stage,kills,ammo=12,reserve=48;public bool hasPistol,holstered=true,paused,dialogueOpen,journal;
 public string speaker,line;public PortInteractable current;public float nextShot,reloadAt;public string notice="Welcome to Vault 120. Follow the corridor to the atrium.";
 int conversationLine;string conversation;AudioSource audioSource;Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
 readonly string[] objectives={"Attend your birthday party and speak with Overseer Vance.","Collect your birthday present from Chief Doyle.","Exterminate the radroaches near the reactor.","Search Zeke's old locker in your quarters.","Your lease has been terminated. Speak with the Overseer.","Open Vault 120 using the door control terminal.","Leave Vault 120.","Vault introduction complete. The surface is the next porting milestone."};
 public string Objective {get{return objectives[Mathf.Clamp(stage,0,7)]+(stage==2?" ("+kills+"/3)":"");}}
 public bool GrantStage(int expected,int next){if(stage!=expected)return false;stage=next;Sound("quest");return true;}
 void Start(){audioSource=gameObject.AddComponent<AudioSource>();audioSource.volume=.35f;var pistol=Resources.Load<AudioClip>("FalloutAudio/Effects/pistolsound1");if(pistol)clips["shot"]=pistol;if(!GetComponent<PortSoundtrack>())gameObject.AddComponent<PortSoundtrack>();var hum=gameObject.AddComponent<AudioSource>();hum.clip=MakeClip("hum",3);hum.volume=.055f;hum.loop=true;hum.Play();foreach(var r in roaches)r.SetActive(stage==2);}
 void Update(){if(reloadAt>0&&Time.time>=reloadAt){int n=Mathf.Min(GetComponent<PortLoadout>().Magazine-ammo,reserve);ammo+=n;reserve-=n;reloadAt=0;} }
 public void TogglePause(){if(menu){if(dialogueOpen){CloseDialogue();return;}if(menu.Visible)menu.Back();else menu.PauseMenu();return;}if(dialogueOpen){CloseDialogue();return;}paused=!paused;Time.timeScale=paused?0:1;Cursor.lockState=paused?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=paused;}
 void OnDisable(){Time.timeScale=1;}
 public void FindInteraction(){current=null;float nearest=3.1f;foreach(var item in interactions){if(!item||!item.gameObject.activeInHierarchy)continue;Vector3 eye=item.transform.position+Vector3.up;Vector3 d=eye-player.view.transform.position;float dist=d.magnitude;if(dist>nearest||Vector3.Dot(d.normalized,player.view.transform.forward)<.6f)continue;if(Physics.Linecast(player.view.transform.position,eye,out RaycastHit hit)&&hit.collider.GetComponentInParent<PortInteractable>()!=item)continue;nearest=dist;current=item;}}
 public void Interact(){if(current)Perform(current.id);}
 public void Perform(string id){
  if(id.StartsWith("loot:")){var item=Array.Find(interactions,x=>x&&x.id==id);if(item)item.GetComponent<PortWorldLoot>().Open();return;}
  if(id.StartsWith("cache_")){PortSurvival.Instance.Loot(int.Parse(id.Substring(6)));return;}if(id=="range"){PortSurvival.Instance.BeginChallenge();return;}
  conversation=id;conversationLine=0;
  if(id=="overseer") {speaker="Overseer Harriet Vance";line=stage==0?"There's the birthday tenant. Nineteen years in Vault 120 — the Superintendent has logged every single one of them, you know. Including the year you tried to flush Tommy Ruggiero's tooth.":stage==4?"I heard. Of course I heard — the Superintendent broadcast it on every speaker in the vault. Listen to me. Zeke is alive. Two weeks ago something on the surface pinged our relay — and it was Zeke who pinged back.":"Enjoy your birthday. That's an order, and I so rarely get to give them.";}
  else if(id=="doyle"){speaker="Chief Doyle";line=stage==1?"Well, if it isn't the birthday tenant. Vault Regulation 12-9: every tenant of age is enrolled in the Auxiliary Pest Control Service. That means you get a gun. That means I have to give you a gun. [He hands you a worn 10mm pistol.]":"Roaches in the reactor drains again. Keep your weapon pointed away from the residents.";}
  else if(id=="lucy"){speaker="Lucy Vance";line=stage<4?"Happy birthday, old-timer! Nineteen! I made you a card. Well — I made the card, Mom made the cake, and the Superintendent made us fill out a Festivity Request in triplicate.":"They can't do that! ...Can they do that? Be careful out there.";}
  else if(id=="doctor"){speaker="Dr. Priya Nandakumar";line="Happy birthday. Stop by the clinic if you get hurt. And please keep Doyle's pest-control exercise away from the cake.";}
  else if(id=="tommy"){speaker="Tommy Ruggiero";line="Well, well, well. If it ain't the birthday tenant. Pipe Rats don't do birthdays. Pipe Rats do... [he thinks hard] ...leaning.";}
  else if(id=="locker"){speaker="Zeke's old locker";if(stage==3){line="Inside: Zeke's birthday holotape and the Liberty Key.\n\nThe Superintendent's announcement interrupts: ATTENTION TENANTS. Your lease has been TERMINATED. Please join us in wishing you a pleasant apocalypse!";GrantStage(3,4);}else line="Zeke's belongings are still here. Finish the party and Doyle's pest-control duty first.";}
  else if(id=="terminal"){speaker="VAULT 120 — PRIMARY EGRESS CONTROL";if(stage==5){line="OVERSEER OVERRIDE ACCEPTED.\n\nPRIMARY DOOR: OPENING\n\nThe great cog door grinds open. Two hundred years of stale air rushes out to meet the morning.";mainDoor.unlocked=true;GrantStage(5,6);Sound("door");}else line=stage>=6?"PRIMARY DOOR: OPEN":"PRIMARY DOOR: SEALED\nOverseer authorization required.";}
  else if(id=="exit"){speaker="Central Park";if(stage==6){GrantStage(6,7);line="You reached the surface exit.\n\nThis first Unity slice ends here. Central Park, the city, and MQ02 still need to be ported. You can continue exploring the vault.";}else line="Vault 120's lease-renewal sequence is still in progress.";}
  else return;
  dialogueOpen=true;journal=false;Time.timeScale=0;Sound("ui");
 }
 public void AdvanceDialogue(){
  if(!dialogueOpen)return;
  if(conversation=="overseer"&&stage==0&&conversationLine==0){line="Vault tradition: on their nineteenth birthday every tenant receives their very own Pip-Boy 3000. There. Now the Superintendent can measure your productivity to the second. Happy birthday.\n\nChief Doyle has your other present. He's by the punch bowl.";conversationLine++;return;}
  if(conversation=="overseer"&&stage==4&&conversationLine==0){line="This is the override for the door terminal. Memorize it. Find Zeke. If his last signal was right, he was heading for Grand Central.\n\nSay goodbye to Lucy. Then go.";conversationLine++;return;}
  if(conversation=="overseer"){if(stage==0)GrantStage(0,1);else if(stage==4)GrantStage(4,5);}
  if(conversation=="doyle"&&GrantStage(1,2)){GetComponent<PortInventory>().Acquire("pistol_10mm",60,true);hasPistol=true;holstered=false;foreach(var r in roaches)r.SetActive(true);notice="10mm pistol acquired. H / B: holster • Mouse / triggers: aim and fire • R / Y: reload";}
  CloseDialogue();
 }
 public void CloseDialogue(){dialogueOpen=false;Time.timeScale=1;Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
 public void RoachKilled(){if(stage!=2)return;kills=Mathf.Min(kills+1,3);if(kills==3){GrantStage(2,3);notice="Pest control complete. Search Zeke's locker in your room.";}}
 public void Fire(){GetComponent<PortLoadout>().Shoot();}
 public void Reload(){if(paused||dialogueOpen||holstered||!hasPistol||ammo==GetComponent<PortLoadout>().Magazine||reserve==0||reloadAt>0)return;reloadAt=Time.time+GetComponent<PortLoadout>().ReloadDuration;Sound("reload");}
 public void Sound(string id){if(!audioSource)return;if(!clips.ContainsKey(id))clips[id]=MakeClip(id,id=="shot"?.24f:id=="door"?1.5f:.18f);audioSource.PlayOneShot(clips[id],id=="step"?.28f:1);}
 AudioClip MakeClip(string id,float duration){int rate=22050,n=(int)(rate*duration);float[] data=new float[n];var rand=new System.Random(19);for(int i=0;i<n;i++){float t=(float)i/rate,e=Mathf.Exp(-t*(id=="shot"?24:18));float noise=(float)rand.NextDouble()*2-1;data[i]=id=="hum"?.3f*Mathf.Sin(t*2*Mathf.PI*60)+noise*.025f:id=="shot"?(noise*.8f+Mathf.Sin(t*380)*.2f)*e:id=="step"?noise*e*.5f:id=="door"?noise*.3f*Mathf.Sin(Mathf.PI*t/duration):Mathf.Sin(t*2*Mathf.PI*(id=="quest"?660:440))*e*.4f;}var clip=AudioClip.Create("Vault_"+id,n,1,rate,false);clip.SetData(data,0);return clip;}
 void OnGUI(){
  if(menu&&menu.Visible)return;
  float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);
  float w=Screen.width/scale,h=Screen.height/scale;var body=new GUIStyle(GUI.skin.label){fontSize=19,wordWrap=true};body.normal.textColor=new Color(.45f,1,.64f);var title=new GUIStyle(body){fontSize=26,fontStyle=FontStyle.Bold};
  GUI.Box(new Rect(20,18,620,92),"");GUI.Label(new Rect(36,27,600,30),"LEASE ON LIFE",title);GUI.Label(new Rect(36,61,595,45),Objective,body);
  if(stage<7&&!dialogueOpen){string id=stage==0||stage==4?"overseer":stage==1?"doyle":stage==3?"locker":stage==5?"terminal":"exit";var target=Array.Find(interactions,x=>x.id==id);Vector3 position=stage==2?new Vector3(-9,1,-67):target.transform.position+Vector3.up*1.8f;string label=stage==2?"Reactor radroaches":target.displayName;Vector3 screen=player.view.WorldToScreenPoint(position);float distance=Vector3.Distance(player.transform.position,position);float x=screen.z>0?Mathf.Clamp(screen.x/scale,160,w-160):w/2;float y=screen.z>0?Mathf.Clamp((Screen.height-screen.y)/scale,125,h-180):130;GUI.Label(new Rect(x-140,y,300,55),(screen.z<0?"Behind you · ":"◇ ")+label+"\n"+Mathf.RoundToInt(distance)+" m",body);}
  GUI.Label(new Rect(30,h-74,w-60,30),notice,body);GUI.Label(new Rect(30,h-40,w-60,28),"WASD / Left stick: move   •   E / X: interact   •   Tab / View: journal   •   Esc / Menu: pause",body);
  if(hasPistol)GUI.Label(new Rect(w-250,h-116,230,30),reloadAt>0?"RELOADING":(GetComponent<PortLoadout>().DisplayName+"   "+ammo+" / "+reserve),title);
  if(!dialogueOpen&&!paused){GUI.Label(new Rect(w/2-7,h/2-13,20,25),"+",body);if(current)GUI.Label(new Rect(w/2-180,h/2+30,400,60),current.displayName+"\nE / X — Interact",body);}
  if(journal){GUI.Box(new Rect(w/2-300,140,600,290),"");GUI.Label(new Rect(w/2-275,155,550,40),"PIP-BOY 3000 · DATA",title);GUI.Label(new Rect(w/2-275,210,550,170),"LEASE ON LIFE\n\n"+Objective+"\n\nToday is your nineteenth birthday in Vault 120. Zeke Marlow was evicted to the surface a month ago.",body);}
  if(dialogueOpen){GUI.Box(new Rect(w/2-440,h-325,880,220),"");GUI.Label(new Rect(w/2-412,h-308,830,37),speaker,title);GUI.Label(new Rect(w/2-412,h-261,824,135),line,body);GUI.Label(new Rect(w/2+240,h-141,180,30),"E / A — Continue",body);}
  if(paused){GUI.Box(new Rect(w/2-240,h/2-145,480,290),"");GUI.Label(new Rect(w/2-210,h/2-125,420,40),"VAULT 120 · UNITY PORT",title);GUI.Label(new Rect(w/2-210,h/2-73,420,95),"First playable slice. Progress currently lasts for this play session.\n\nEsc / Menu resumes.",body);if(GUI.Button(new Rect(w/2-180,h/2+46,360,40),"Resume"))TogglePause();}
 }
}
}

