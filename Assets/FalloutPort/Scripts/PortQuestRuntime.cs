using System;
using System.Collections.Generic;
using System.Linq;
using Jint;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
namespace FalloutPort {
// Runs trusted original quest content in-process, without browser or CLR access.
public class PortQuestRuntime:MonoBehaviour {
 public bool enableOriginalStory=true;public string savedState;public bool initialized;public int experience;public readonly List<string> faults=new List<string>();
 public double lastTickMilliseconds;Engine engine;System.Threading.Tasks.Task<string> pendingTick;readonly Dictionary<string,int> counts=new Dictionary<string,int>();readonly List<JObject> hostChanges=new List<JObject>();
 void PrepareHost(){counts.Clear();var inv=GetComponent<PortInventory>();inv.Store();foreach(var item in GetComponent<PortItemStore>().items)counts[item.id]=item.count;foreach(var item in inv.ammunition)counts[item.id]=item.count;foreach(var item in inv.weapons)counts[item.id]=1;counts["caps"]=GetComponent<PortSurvival>().caps;counts["stimpak"]=GetComponent<PortSurvival>().stimpaks;}
 void ApplyHost(){foreach(var p in hostChanges){string action=(string)p["action"],id=(string)p["id"];float amount=(float?)p["count"]??1;int n=Mathf.RoundToInt(amount);if(action=="give")GetComponent<PortItemStore>().Add(id,n);else if(action=="take")GetComponent<PortItemStore>().Remove(id,n);else if(action=="damage")GetComponent<PortSurvival>().Damage(amount);else if(action=="rads")GetComponent<PortItemStore>().radiation+=amount;else if(action=="shake")game.player.Kick();}hostChanges.Clear();}
 void CompleteTick(){if(pendingTick==null)return;var task=pendingTick;pendingTick=null;Process(task.GetAwaiter().GetResult());}
 readonly List<string> pendingActions=new List<string>();
PortGame game;PortOpenWorld world;GameObject root;PortCharacterCatalog catalog;float nextTick,lastSimulationTime;
 readonly Dictionary<string,JObject> roster=new Dictionary<string,JObject>();readonly Dictionary<string,GameObject> views=new Dictionary<string,GameObject>();
 public bool initializing;public string loadingStep;System.Collections.IEnumerator InitializeSteps(){
  game=GetComponent<PortGame>();world=GetComponent<PortOpenWorld>();catalog=Resources.Load<PortCharacterCatalog>("PortCharacterCatalog");PrepareHost();engine=new Engine();engine.SetValue("host",new Func<string,string,string>(Host));
  foreach(var script in Resources.LoadAll<TextAsset>("QuestRuntime").Where(a=>a.name!="worldlogic").OrderBy(a=>a.name)){loadingStep=script.name.StartsWith("05_")?"Preparing wasteland encounters":script.name.StartsWith("10_")?"Preparing quests and conversations":"Preparing the world simulation";yield return null;engine.Execute(script.text);}
  loadingStep="Preparing quest locations";yield return null;engine.SetValue("worldData",Resources.Load<TextAsset>("QuestRuntime/worldlogic").text);string result=engine.Evaluate("G.native.initialize(worldData)").ToString();
  world.manifest.items=JsonConvert.DeserializeObject<PortItemDefinition[]>(engine.Evaluate("JSON.stringify(G.items.all())").ToString());root=new GameObject("World story characters and interactions");root.transform.SetParent(transform);initialized=true;
  Process(engine.Evaluate("G.native.surface("+(string.IsNullOrEmpty(savedState)?"null":JsonConvert.SerializeObject(savedState))+")").ToString());loadingStep=result;
 }
 public string Initialize(){var steps=InitializeSteps();while(steps.MoveNext()){}return loadingStep;}
 System.Collections.IEnumerator LoadStory(){initializing=true;bool wasPaused=game.paused;game.paused=true;var steps=InitializeSteps();while(true){bool next;try{next=steps.MoveNext();}catch(Exception e){faults.Add(e.ToString());Debug.LogException(e);enabled=false;break;}if(!next)break;yield return steps.Current;}initializing=false;game.paused=wasPaused||game.menu.Visible;}
 void OnGUI(){if(initializing)GUI.Box(new Rect(Screen.width/2-240,Screen.height/2-45,480,90),"LOADING WASTELAND STORY\n\n"+loadingStep);}

 string Host(string action,string json){var p=JObject.Parse(json);string id=(string)p["id"];int count=(int?)p["count"]??1;int have=id!=null&&counts.TryGetValue(id,out var value)?value:0;if(action=="count")return have.ToString();if(action=="equipped")return "false";if(action=="take"&&(count<0||have<count))return "false";if(action=="give")counts[id]=have+count;if(action=="take")counts[id]=have-count;p["action"]=action;hostChanges.Add(p);return "true";}

 public string Capture(){CompleteTick();return initialized?engine.Evaluate("G.native.save()").ToString():savedState;}
 public void Restore(string state){enabled=true;faults.Clear();if(pendingTick!=null){pendingTick.GetAwaiter().GetResult();pendingTick=null;}hostChanges.Clear();pendingActions.Clear();StopAllCoroutines();initializing=false;savedState=state;if(initialized){engine=null;initialized=false;roster.Clear();views.Clear();if(root)Destroy(root);}}
 public void Talk(string id){if(!initialized)return;CompleteTick();PrepareHost();Process(engine.Evaluate("G.native.interactActor("+JsonConvert.SerializeObject(id)+");G.native.flush()").ToString());}
 public void Close(){CompleteTick();PrepareHost();if(initialized)Process(engine.Evaluate("G.native.close();G.native.flush()").ToString());}
 public void Hit(string id,float amount,string weapon,string part,Vector3 dir){if(!initialized)return;string call="G.native.hit("+JsonConvert.SerializeObject(id)+","+amount.ToString(System.Globalization.CultureInfo.InvariantCulture)+","+JsonConvert.SerializeObject(weapon)+","+JsonConvert.SerializeObject(part)+","+dir.x.ToString(System.Globalization.CultureInfo.InvariantCulture)+","+dir.y.ToString(System.Globalization.CultureInfo.InvariantCulture)+","+(-dir.z).ToString(System.Globalization.CultureInfo.InvariantCulture)+");";pendingActions.Add(call);}

 public JObject CharacterStats(){CompleteTick();if(!initialized)return null;return JObject.Parse(engine.Evaluate("JSON.stringify({level:G.rpg.level,xp:G.rpg.xp,next:G.rpg.xpToNext(),karma:G.rpg.karma,points:G.rpg.unspentSkillPoints(),skills:G.rpg.skills,special:G.rpg.special})").ToString());}
 public void RaiseSkill(string id){CompleteTick();if(!initialized)return;PrepareHost();Process(engine.Evaluate("G.rpg.addSkillPoints("+JsonConvert.SerializeObject(id)+",1);G.native.flush()").ToString());}
 public int Skill(string id){CompleteTick();return initialized?(int)engine.Evaluate("G.rpg.skill("+JsonConvert.SerializeObject(id)+")").AsNumber():0;}
 public void UseDoor(string id){CompleteTick();PrepareHost();Process(engine.Evaluate("G.native.useDoor("+JsonConvert.SerializeObject(id)+");G.native.flush()").ToString());}
 public void Service(string action,string argument="null"){if(!initialized)return;CompleteTick();PrepareHost();string code=action=="unlock"?"G.native.unlock()":action=="terminal"?"G.native.terminalChoose("+argument+")":action=="trade"?"G.native.trade("+JsonConvert.SerializeObject(argument)+")":action=="tradeMode"?"G.native.tradeView(G.native.tradeSession.id,"+argument+",G.native.tradeSession.resume)":action=="tradeEnd"?"G.native.enter(G.native.tradeSession.resume)":"G.native.terminalSession=null;G.native.close()";Process(engine.Evaluate(code+";G.native.flush()").ToString());}
 public void Choose(int index){CompleteTick();PrepareHost();Process(engine.Evaluate("G.native.choose("+index+");G.native.flush()").ToString());}
 public string objective="Travel to Grand Central Market.",questTitle="All Aboard";public JObject journalData;
 public void UpdateJournal(){journalData=JObject.Parse(Journal());var tracked=journalData["quests"].FirstOrDefault(t=>(bool?)t["tracked"]==true);if(tracked!=null){objective=(string)tracked["stageText"];questTitle=(string)tracked["name"];}}
 public string Journal(){return initialized?engine.Evaluate("G.native.journal()").ToString():"{\"quests\":[],\"markers\":[]}";}
 public void Event(string name,string id){CompleteTick();PrepareHost();if(initialized)Process(engine.Evaluate("G.emit("+JsonConvert.SerializeObject(name)+","+JsonConvert.SerializeObject(id)+");G.native.flush()").ToString());}
 void Process(string json){ApplyHost();var packet=JObject.Parse(json);foreach(var error in packet["errors"]??new JArray()){faults.Add(error.ToString());Debug.LogError("Quest adapter: "+error);}
  foreach(var command in packet["commands"]??new JArray()){var d=(JObject)command["data"];string type=(string)command["type"];string id=(string)d["id"];
   if(type=="actorStates"){ApplyActors(d["states"]);}
   else if(type=="actor"){if(id.StartsWith("v120_"))continue;roster[id]=d;}
   else if(type=="projectileStates"){var effects=GetComponent<PortWorldEffects>();if(!effects)effects=gameObject.AddComponent<PortWorldEffects>();effects.Projectiles(d["states"]);}
   else if(type=="fx"){var effects=GetComponent<PortWorldEffects>();if(!effects)effects=gameObject.AddComponent<PortWorldEffects>();effects.Play(d);}
   else if(type=="notice"||type=="bark")game.notice=(string)d["text"];
   else if(type=="dialogue"){game.menu.StoryDialogue(this,d);Focus((string)d["actorId"]);}
   else if(type=="doorLock"){var door=Array.Find(world.manifest.doors,x=>x.id==id);if(door!=null)door.locked=(int)d["locked"];}
   else if(type=="lockpick"){var pick=GetComponent<PortLockpick>();if(!pick)pick=gameObject.AddComponent<PortLockpick>();pick.Open((int)d["level"],()=>Service("unlock"));}
   else if(type=="terminal"||type=="trade")game.menu.StoryService(this,type,d);
   else if(type=="dialogueEnd")game.menu.Resume();
   else if(type=="travel")world.Travel(new Vector3((float)d["x"],(float?)d["y"]??0,-(float)d["z"]),-((float?)d["yaw"]??0)*Mathf.Rad2Deg);
   else if(type=="remove"){if(views.TryGetValue(id,out var view))Destroy(view);views.Remove(id);}
   else if(type=="pickup"||type=="prop"){if(type=="prop"&&(string)d["kind"]!="terminal")continue;CreateObject(type,d);}
  }UpdateJournal();RefreshInteractions();
 }
 void CreateObject(string kind,JObject d){string id=(string)d["id"];if(views.ContainsKey(id)||GetComponent<PortInventory>().claimed.Contains("quest:"+id))return;var pos=d[kind=="pickup"?"pos":"opts"];float x=(float?)pos["x"]??0,z=(float?)pos["z"]??0;if(x>3900&&x<4100&&z>-120&&z<120)return;
  var o=new GameObject(id);o.transform.SetParent(root.transform);o.transform.position=new Vector3(x,(float?)pos["y"]??0,-z);var interact=o.AddComponent<PortInteractable>();interact.id="storyitem:"+id;interact.displayName=kind=="pickup"?GetComponent<PortItemStore>().Label((string)d["itemId"]):"Terminal";
  var target=o.AddComponent<PortStoryObject>();target.runtime=this;target.data=d;target.kind=kind;
  var model=GameObject.CreatePrimitive(PrimitiveType.Cube);model.name=kind=="pickup"?"Quest item":"Terminal housing";model.transform.SetParent(o.transform,false);model.transform.localScale=kind=="pickup"?new Vector3(.18f,.04f,.24f):new Vector3(.55f,.5f,.45f);model.transform.localPosition=Vector3.up*(kind=="pickup"?.04f:1.1f);var collider=model.GetComponent<Collider>();collider.isTrigger=true;var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=kind=="pickup"?new Color(.65f,.58f,.38f):new Color(.15f,.22f,.16f);model.GetComponent<Renderer>().material=material;views[id]=o;
 }
 public void InteractObject(PortStoryObject target){CompleteTick();PrepareHost();var d=target.data;if(target.kind=="pickup"){string id=(string)d["itemId"];GetComponent<PortInventory>().claimed.Add("quest:"+(string)d["id"]);GetComponent<PortItemStore>().Add(id,(int)d["count"]);Event("itemAdded",id);Destroy(target.gameObject);views.Remove((string)d["id"]);RefreshInteractions();}else Process(engine.Evaluate("G.terminals.open("+JsonConvert.SerializeObject((string)d["opts"]["terminalId"])+");G.native.flush()").ToString());}
 void SpawnActor(string id,JObject d){if(!catalog||catalog.prefabs.Length==0)return;string type=(string)d["type"];
  string gender=(string)d["gender"]=="f"?"Female":"Male";var pool=Enumerable.Range(0,catalog.prefabs.Length).Where(i=>catalog.genders[i]==gender).ToArray();string role=(string)d["outfit"]??"";string[] hints=role.Contains("doctor")||role.Contains("lab")?new[]{"Doctor","Medic","Clinic","Surgeon","Research"}:role.Contains("merchant")?new[]{"Merchant","Store","BarTender","Workshop","Caravan"}:role.Contains("armor")||role.Contains("security")||role.Contains("guard")||type=="raider"?new[]{"Guard","Riot","Militia","Sentry","Armor","Mercenary","Ranger","Security"}:role.Contains("mechanic")?new[]{"Mechanic","Engineer","Technician","Crew"}:new string[0];var suited=pool.Where(i=>hints.Any(h=>catalog.prefabs[i].name.Contains(h))).ToArray();if(suited.Length>0)pool=suited;uint hash=120;foreach(char letter in id)hash=hash*31+letter;var prefab=(type=="human"||type=="ghoul"||type=="raider"||type=="brotherhood_paladin"||type=="enclave_soldier")?catalog.prefabs[pool[(int)(hash%(uint)pool.Length)]]:Resources.Load<GameObject>("WorldCreatures/"+(type=="mister_gutsy"?"mister_handy":type));if(!prefab){Debug.LogWarning("Missing story character: "+type);return;}
  var o=Instantiate(prefab,root.transform);o.name=(string)d["name"];var p=d["pos"];o.transform.position=new Vector3((float)p["x"],(float?)p["y"]??0,-(float)p["z"]);o.transform.rotation=Quaternion.Euler(0,-((float?)d["yaw"]??0)*Mathf.Rad2Deg,0);
  var actor=o.AddComponent<PortWorldActor>();actor.runtime=this;actor.actorId=id;actor.height=(float?)d["height"]??1.8f;var it=o.AddComponent<PortInteractable>();it.id="story:"+id;it.displayName=o.name;it.phase=(hash%1000)/1000f;var col=o.AddComponent<CapsuleCollider>();col.height=(float?)d["height"]??1.7f;col.center=Vector3.up*col.height*.5f;col.radius=Mathf.Min((float?)d["radius"]??.3f,col.height*.45f);col.isTrigger=true;views[id]=o;string weapon=(string)d["weapon"];if(!string.IsNullOrEmpty(weapon)&&o.GetComponentInChildren<Animator>()){var def=Array.Find(GetComponent<PortLoadout>().catalog,x=>x.id==weapon);if(def)o.AddComponent<PortActorWeapon>().definition=def;}foreach(var renderer in o.GetComponentsInChildren<SkinnedMeshRenderer>())renderer.updateWhenOffscreen=false;
 }
 void ApplyActors(JToken states){var active=new HashSet<string>();foreach(var data in states){string id=(string)data["id"];if(!roster.TryGetValue(id,out var def))continue;active.Add(id);if(!views.ContainsKey(id))SpawnActor(id,def);if(views.TryGetValue(id,out var view)&&view){view.SetActive((bool)data["visible"]);var actor=view.GetComponent<PortWorldActor>();if(actor)actor.Apply(data);}}foreach(var pair in roster)if(!active.Contains(pair.Key)&&views.TryGetValue(pair.Key,out var view)&&view)view.SetActive(false);}
 void Focus(string id){if(id==null||!views.TryGetValue(id,out var view)||!view)return;var actor=view.GetComponent<PortWorldActor>();float height=actor?actor.height:1.8f;Vector3 target=view.transform.position+Vector3.up*height*.88f;Vector3 away=game.player.transform.position-view.transform.position;away.y=0;if(away.sqrMagnitude<.01f)away=-view.transform.forward;Vector3 eye=target+away.normalized*Mathf.Clamp(height*.5f,.65f,1.6f);if(Physics.Linecast(target,eye,out var hit,~(1<<2),QueryTriggerInteraction.Ignore))eye=Vector3.Lerp(target,hit.point,.8f);game.player.view.transform.position=eye;game.player.view.transform.rotation=Quaternion.LookRotation(target-eye);}
 public void RefreshInteractions(){if(!game||!world.outside||!root)return;game.interactions=game.interactions.Where(x=>x&&!x.id.StartsWith("story")).Concat(root.GetComponentsInChildren<PortInteractable>(true)).ToArray();}
 void Update(){if(pendingTick!=null){if(!pendingTick.IsCompleted)return;try{CompleteTick();}catch(Exception e){faults.Add(e.ToString());Debug.LogException(e);enabled=false;return;}}if(!enableOriginalStory)return;if(!game)game=GetComponent<PortGame>();if(!world)world=GetComponent<PortOpenWorld>();if(!world.outside){if(root)root.SetActive(false);return;}if(world.loading||game.paused||game.dialogueOpen)return;
  if(!initialized){if(!initializing)StartCoroutine(LoadStory());return;}root.SetActive(true);if(Time.time<nextTick)return;nextTick=Time.time+.1f;
  var p=game.player.transform.position;
  float simulationStep=Mathf.Clamp(Time.time-lastSimulationTime,.01f,.25f);lastSimulationTime=Time.time;var direction=game.player.view.transform.forward;bool alive=!GetComponent<PortSurvival>().Dead;PrepareHost();string actions=string.Join("",pendingActions);pendingActions.Clear();
  pendingTick=System.Threading.Tasks.Task.Run(()=>{var watch=System.Diagnostics.Stopwatch.StartNew();engine.SetValue("nativeDt",simulationStep);engine.SetValue("nativeFx",direction.x);engine.SetValue("nativeFy",direction.y);engine.SetValue("nativeFz",-direction.z);engine.SetValue("nativeAlive",alive);engine.SetValue("nativeX",p.x);engine.SetValue("nativeY",p.y);engine.SetValue("nativeZ",-p.z);engine.Evaluate("G.player.forward.set(nativeFx,nativeFy,nativeFz);G.player.alive=nativeAlive");if(actions.Length>0)engine.Execute(actions);string result=engine.Evaluate("G.native.tick(nativeX,nativeY,nativeZ,nativeDt)").ToString();lastTickMilliseconds=watch.Elapsed.TotalMilliseconds;return result;});

 }
 void OnDestroy(){if(pendingTick!=null)try{pendingTick.GetAwaiter().GetResult();}catch{} }
}
public class PortStoryObject:MonoBehaviour {public PortQuestRuntime runtime;public JObject data;public string kind;public void Open(){runtime.InteractObject(this);}}
}
