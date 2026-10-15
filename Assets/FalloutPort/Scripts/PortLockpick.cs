using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace FalloutPort {
public class PortLockpick:MonoBehaviour {
 PortGame game;Action success;float angle,sweet,tension,durability;int difficulty;bool active;string message;
 public void Open(int level,Action unlocked){game=GetComponent<PortGame>();var story=GetComponent<PortQuestRuntime>();int skill=story.initialized?story.Skill("lockpick"):0;if(skill<level){game.notice="Lockpick "+level+" required. Current: "+skill;return;}if(GetComponent<PortItemStore>().Count("bobby_pin")<1){game.notice="You need a bobby pin.";return;}difficulty=level;success=unlocked;sweet=UnityEngine.Random.Range(-80f,80f);angle=tension=0;durability=1;active=true;game.menu.PauseMenu();game.menu.page="lockpick";game.menu.canvas.enabled=false;message="Find the angle that lets the lock turn.";}
 public void Cancel(){active=false;game.menu.Resume();}
 void Update(){if(!active)return;var k=Keyboard.current;var pad=Gamepad.current;float dt=Time.unscaledDeltaTime;bool turn=(k!=null&&k.spaceKey.isPressed)||(pad!=null&&pad.buttonSouth.isPressed);if(k!=null&&k.escapeKey.wasPressedThisFrame||pad!=null&&pad.buttonEast.wasPressedThisFrame){Cancel();return;}
  if(!turn){float move=(k!=null?(k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0):0)+(pad!=null?pad.leftStick.x.ReadValue():0);angle=Mathf.Clamp(angle+move*70*dt+(Mouse.current!=null?Mouse.current.delta.x.ReadValue()*.2f:0),-90,90);tension=Mathf.MoveTowards(tension,0,dt*2);}
  else {float tolerance=Mathf.Lerp(18,5,Mathf.Clamp01(difficulty/100f));float error=Mathf.Abs(angle-sweet);float limit=Mathf.Clamp01(1-(error-tolerance)/65);tension=Mathf.MoveTowards(tension,limit,dt*.8f);if(error<=tolerance&&tension>.99f){active=false;game.menu.Resume();game.Sound("quest");success();return;}if(tension>=limit-.01f){durability-=dt*.6f;message="Resistance! Release tension and adjust the pin.";if(durability<=0){GetComponent<PortItemStore>().Remove("bobby_pin",1);durability=1;tension=0;game.Sound("ui");message="Bobby pin broke.";if(GetComponent<PortItemStore>().Count("bobby_pin")<1){Cancel();game.notice="Out of bobby pins.";}}}}
 }
 void OnGUI(){if(!active)return;float x=Screen.width/2-250,y=Screen.height/2-180;GUI.Box(new Rect(x,y,500,360),"LOCKPICKING — "+difficulty);GUI.Label(new Rect(x+25,y+45,450,50),message);GUI.Label(new Rect(x+25,y+105,450,35),"Pin angle: "+Mathf.RoundToInt(angle)+"°     Pins: "+GetComponent<PortItemStore>().Count("bobby_pin"));GUI.HorizontalSlider(new Rect(x+25,y+150,450,25),angle,-90,90);GUI.Label(new Rect(x+25,y+190,450,30),"Lock rotation: "+Mathf.RoundToInt(tension*90)+"°");GUI.Box(new Rect(x+25,y+230,450*tension,14),"");GUI.Label(new Rect(x+25,y+275,450,75),"Mouse / A-D / left stick: position pin\nSpace / A: turn lock     Esc / B: leave");}
}
}
