using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
namespace FalloutPort {
[DefaultExecutionOrder(100)]
public class PortMenu : MonoBehaviour {
 public PortGame game;public bool Visible {get{return page!="play";}}public string page="title";public Canvas canvas;
 RectTransform root;Font font;List<Button> buttons=new List<Button>();Text heading,detail,footer;RectTransform list;string home="title";Action confirmed;bool began;float nextNav;Vector3 cameraPos;Quaternion cameraRot;
 static readonly Color Green=new Color(.58f,.88f,.66f),White=new Color(.86f,.84f,.75f),Amber=new Color(.8f,.63f,.31f);
 void Start(){game=GetComponent<PortGame>();PortPreferences.Apply();font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");Build();ShowTitle();}
 RectTransform Box(string name,Transform parent,float x,float y,float w,float h,Color? color=null){var o=new GameObject(name,typeof(RectTransform));o.transform.SetParent(parent,false);var r=o.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);if(color.HasValue)o.AddComponent<Image>().color=color.Value;return r;}
 Text Label(string name,Transform parent,float x,float y,float w,float h,string text,int size,Color color){var r=Box(name,parent,x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.font=font;t.fontSize=size;t.color=color;t.text=text;t.raycastTarget=false;return t;}
 void Build(){
  var go=new GameObject("Fallout menu",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.transform.SetParent(transform);canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
  var cover=new GameObject("Full screen shade",typeof(RectTransform),typeof(Image));cover.transform.SetParent(go.transform,false);var cr=cover.GetComponent<RectTransform>();cr.anchorMin=Vector2.zero;cr.anchorMax=Vector2.one;cr.offsetMin=cr.offsetMax=Vector2.zero;cover.GetComponent<Image>().color=new Color(.01f,.02f,.015f,.72f);
  root=Box("Menu frame",go.transform,0,0,1600,900);root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.pivot=new Vector2(.5f,.5f);root.anchoredPosition=Vector2.zero;
  Box("Shade",root,0,0,1600,900,new Color(.012f,.022f,.019f,.91f));Box("Top letterbox",root,0,0,1600,38,Color.black);Box("Bottom letterbox",root,0,860,1600,40,Color.black);
  Label("FALLOUT",root,105,89,850,130,"FALLOUT",106,White).fontStyle=FontStyle.Bold;
  int[] heights={14,28,19,38,25,45,29,19,34,52,35,20,40,66,100,68,42,22,45,28,16,31,21,37,15,24};for(int i=0;i<heights.Length;i++)Box("Skyline",root,112+i*20,283-heights[i]*.64f,19,heights[i]*.64f,Amber);
  Label("New York",root,107,291,650,65,"N E W   Y O R K",44,White);Label("Subtitle",root,110,359,830,32,"A POST-ATOMIC ROLE-PLAYING GAME · 2287",17,Amber);
  heading=Label("Page heading",root,110,417,1100,48,"",30,Green);list=Box("Menu choices",root,108,474,690,345);
  Box("Detail rule",root,918,429,2,340,new Color(.5f,.7f,.55f,.3f));detail=Label("Detail",root,961,440,505,340,"",24,White);
  footer=Label("Navigation",root,110,811,1380,35,"↑ ↓ / D-pad  Select     Enter / A  Confirm     Esc / B  Back",18,Green);
  Label("Credit footer",root,110,870,1360,26,"UNITY PORT · VAULT 120                         An unofficial fan recreation",14,new Color(.55f,.57f,.51f));
  for(int y=42;y<855;y+=5){var scan=Box("CRT scanline",root,0,y,1600,1,new Color(0,0,0,.055f));scan.GetComponent<Image>().raycastTarget=false;}
  if(!EventSystem.current){var events=new GameObject("Menu EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}
 }
 void Clear(string title,string text){foreach(Transform child in list)Destroy(child.gameObject);buttons.Clear();heading.text=title;detail.text=text;nextNav=Time.unscaledTime+.15f;}
 void Choice(string label,Action action,bool enabled=true){int n=buttons.Count;var r=Box(label,list,0,n*45,720,39,new Color(.1f,.2f,.13f,.04f));var b=r.gameObject.AddComponent<Button>();b.interactable=enabled;var colors=b.colors;colors.normalColor=new Color(.18f,.28f,.2f,.25f);colors.highlightedColor=new Color(.35f,.65f,.43f,.8f);colors.selectedColor=colors.highlightedColor;colors.pressedColor=Amber;b.colors=colors;b.targetGraphic=r.GetComponent<Image>();Label("Text",r,14,2,690,37,label,25,enabled?Green:new Color(.34f,.4f,.35f));b.onClick.AddListener(()=>{game.Sound("ui");action();});buttons.Add(b);if(enabled&&buttons.FindAll(x=>x.interactable).Count==1)EventSystem.current.SetSelectedGameObject(b.gameObject);}
 void Freeze(){game.paused=true;game.journal=false;game.dialogueOpen=false;game.current=null;Time.timeScale=0;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;canvas.enabled=true;}
 public void ShowTitle(){if(!Visible){cameraPos=game.player.view.transform.localPosition;cameraRot=game.player.view.transform.localRotation;}page=home="title";Freeze();TitleCamera();game.player.weapon.gameObject.SetActive(false);Clear("MAIN MENU","VAULT 120\n\nYour lease on life begins below Central Park.\n\nThe surface awaits.");Choice("CONTINUE",()=>Load(PortSave.Latest()),PortSave.Latest()>=0);Choice("NEW GAME",()=>{if(PortSave.Latest()>=0)Confirm("Start a new game?","Your existing save slots will be kept.",NewGame);else NewGame();});Choice("LOAD GAME",()=>Slots(false),PortSave.Latest()>=0);Choice("SETTINGS",Settings);Choice("CREDITS",Credits);}
 public void PauseMenu(){home=page="pause";cameraPos=game.player.view.transform.localPosition;cameraRot=game.player.view.transform.localRotation;Freeze();Clear("PAUSED","Wanderer · Vault 120\n\nLEASE ON LIFE\n\n"+game.Objective);Choice("RESUME",Resume);Choice("SAVE GAME",()=>Slots(true));Choice("LOAD GAME",()=>Slots(false),PortSave.Latest()>=0);Choice("SETTINGS",Settings);Choice("HELP",Help);Choice("QUIT TO MAIN MENU",()=>Confirm("Quit to the main menu?","Unsaved progress will be lost.",ShowTitle));}
 public void Resume(){page="play";began=true;canvas.enabled=false;game.paused=false;Time.timeScale=1;Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;game.player.view.transform.localPosition=cameraPos;game.player.view.transform.localRotation=cameraRot;nextNav=Time.unscaledTime+.15f;}
 public void NewGame(){var s=new PortSave.Snapshot{stage=0,ammo=12,reserve=48,holstered=true,position=new Vector3(7.2f,.08f,-8.25f),yaw=-90,dead=new bool[3]};PortSave.Apply(game,s);cameraPos=game.player.view.transform.localPosition;cameraRot=game.player.view.transform.localRotation;game.notice="Welcome to Vault 120. Follow the corridor to the atrium.";Resume();}
 public void Load(int slot){var s=PortSave.Read(slot);if(s==null){detail.text="This save is unavailable or damaged. Choose another slot.";return;}PortSave.Apply(game,s);cameraPos=game.player.view.transform.localPosition;cameraRot=game.player.view.transform.localRotation;Resume();}
 public void Save(int slot){try{PortSave.Write(game,slot);Slots(true);detail.text="GAME SAVED\n\nSlot "+(slot+1)+" · "+game.Objective;}catch(Exception e){detail.text="Save failed. Your previous save is kept.\n\n"+e.Message;}}
 void Slots(bool save){page=save?"save":"load";Clear(save?"SAVE GAME":"LOAD GAME","Three local save slots.\n\nUnity saves include your position, quest progress, ammunition, enemies, and the vault door.");for(int i=0;i<3;i++){int slot=i;var s=PortSave.Read(i);string label="SLOT "+(i+1)+"   "+(s==null?"EMPTY":s.savedAt+" · Lease on Life");Choice(label,()=>{if(save){if(s==null)Save(slot);else Confirm("Overwrite slot "+(slot+1)+"?","The other slots will be kept.",()=>Save(slot));}else Load(slot);},save||s!=null);}Choice("BACK",Back);}
 public void Settings(){page="settings";Clear("SETTINGS","75% resolution scales the 3D world. Menus and HUD remain at your display's full resolution.\n\nSelect an option to adjust it. Settings save automatically.");Choice("RESOLUTION SCALE     "+Mathf.RoundToInt(PortPreferences.Scale*100)+"%",()=>{float v=PortPreferences.Scale;PortPreferences.Scale=v>=.99f?.5f:v+.25f;Settings();});Choice("MASTER VOLUME          "+Mathf.RoundToInt(PortPreferences.Volume*100)+"%",()=>{PortPreferences.Volume=PortPreferences.Volume>=.99f?0:PortPreferences.Volume+.1f;Settings();});Choice("LOOK SENSITIVITY       "+PortPreferences.Sensitivity.ToString("0.0")+"x",()=>{PortPreferences.Sensitivity=PortPreferences.Sensitivity>=2.99f?.2f:PortPreferences.Sensitivity+.2f;Settings();});Choice("INVERT Y AXIS             "+(PortPreferences.Invert?"ON":"OFF"),()=>{PortPreferences.Invert=!PortPreferences.Invert;Settings();});Choice("FIELD OF VIEW             "+PortPreferences.Fov.ToString("0"),()=>{PortPreferences.Fov=PortPreferences.Fov>=100?55:PortPreferences.Fov+5;Settings();});Choice("RESTORE DEFAULTS",()=>{PortPreferences.Defaults();Settings();});Choice("BACK",Back);}
 void Help(){page="help";Clear("HELP","Move: WASD / left stick\nLook: mouse / right stick\nInteract: E / X\nFire / aim: mouse / triggers\nReload: R / Y · Holster: H / B\nJump: Space / A\nJournal: Tab / View\nPause: Esc / Menu\nQuicksave: F5 · Quickload: F9");Choice("BACK",Back);}
 void Credits(){page="credits";Clear("CREDITS","FALLOUT: NEW YORK\n\nAn unofficial fan recreation.\n\nOriginal Three.js world, story and procedural assets.\nUnity adaptation with the new character collection.\n\nFallout belongs to its respective rights holders.");Choice("BACK",Back);}
 void Confirm(string title,string text,Action action){page="confirm";confirmed=action;Clear(title.ToUpperInvariant(),text);Choice("CONFIRM",()=>confirmed());Choice("CANCEL",Back);}
 public void Back(){if(page=="pause"){Resume();return;}if(home=="title")ShowTitle();else PauseMenu();}
 void Update(){if(!canvas)return;var k=Keyboard.current;var pad=Gamepad.current;if(!Visible){if(k!=null&&k.f5Key.wasPressedThisFrame){try{PortSave.Write(game,0);game.notice="Quicksaved to slot 1.";}catch{game.notice="Save failed. Previous save kept.";}}if(k!=null&&k.f9Key.wasPressedThisFrame&&PortSave.Read(0)!=null){Load(0);}return;}
  Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
  if((k!=null&&k.escapeKey.wasPressedThisFrame)||(pad!=null&&(pad.buttonEast.wasPressedThisFrame||pad.startButton.wasPressedThisFrame))){if(Time.unscaledTime>nextNav)Back();}
 }
 void LateUpdate(){if(page=="title"||home=="title"&&Visible)TitleCamera();}
 void TitleCamera(){float t=Time.unscaledTime*.06f;var c=game.player.view.transform;c.position=new Vector3(Mathf.Sin(t)*1.5f,2.2f,8);c.rotation=Quaternion.LookRotation(new Vector3(0,1.9f,21)-c.position);}
 void OnDestroy(){if(canvas)Destroy(canvas.gameObject);Time.timeScale=1;}
}
}

