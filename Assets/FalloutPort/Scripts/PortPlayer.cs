using UnityEngine;
using UnityEngine.InputSystem;
namespace FalloutPort {
[RequireComponent(typeof(CharacterController))]
public class PortPlayer : MonoBehaviour {
 public Camera view;public PortGame game;public Transform weapon;public float pitch;public float yaw=-90;public float vertical;float footstep;float recoil;Vector3 weaponRest; public void Kick(){recoil=Mathf.Min(recoil+1,2);pitch=Mathf.Max(-80,pitch-1.2f);}
 public void ToggleHolster(){if(game.hasPistol&&!game.dialogueOpen&&!game.paused)game.holstered=!game.holstered;}
 void Start(){yaw=transform.eulerAngles.y;Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
 void OnDisable(){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
 public void Teleport(Vector3 position,float facing){var c=GetComponent<CharacterController>();c.enabled=false;transform.position=position;c.enabled=true;vertical=0;yaw=facing;transform.rotation=Quaternion.Euler(0,yaw,0);}
 void Update(){
  if(game.menu&&game.menu.Visible)return;
  var k=Keyboard.current;var m=Mouse.current;var pad=Gamepad.current;
  bool esc=k!=null&&k.escapeKey.wasPressedThisFrame ||pad!=null&&pad.startButton.wasPressedThisFrame;
  if(esc){game.TogglePause();return;}
  bool interact=k!=null&&k.eKey.wasPressedThisFrame||pad!=null&&pad.buttonWest.wasPressedThisFrame;
  bool confirm=k!=null&&k.enterKey.wasPressedThisFrame||pad!=null&&pad.buttonSouth.wasPressedThisFrame;
  if(game.dialogueOpen){if(interact||confirm)game.AdvanceDialogue();return;}
  if(game.paused)return;
  if(Cursor.lockState!=CursorLockMode.Locked&&pad==null){if(m!=null&&m.leftButton.wasPressedThisFrame){Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}return;}
  Vector2 look=m!=null?m.delta.ReadValue()*.085f:Vector2.zero;if(pad!=null)look+=pad.rightStick.ReadValue()*115*Time.deltaTime;
  look*=PortPreferences.Sensitivity;yaw+=look.x;pitch=Mathf.Clamp(pitch-look.y*(PortPreferences.Invert?-1:1),-80,80);transform.rotation=Quaternion.Euler(0,yaw,0);view.transform.localRotation=Quaternion.Euler(pitch,0,0);
  Vector2 move=pad!=null?pad.leftStick.ReadValue():Vector2.zero;
  if(k!=null)move+=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
  move=Vector2.ClampMagnitude(move,1);bool sprint=k!=null&&k.leftShiftKey.isPressed||pad!=null&&pad.leftStickButton.isPressed;
  if(PortSurvival.Instance){if(k!=null&&k.qKey.wasPressedThisFrame||pad!=null&&pad.dpad.up.wasPressedThisFrame)PortSurvival.Instance.Heal();sprint=sprint&&move.magnitude>.1f&&PortSurvival.Instance.Sprint(Time.deltaTime);}
  var cc=GetComponent<CharacterController>();if(cc.isGrounded&&vertical<0)vertical=-2;
  if(cc.isGrounded&&(k!=null&&k.spaceKey.wasPressedThisFrame||pad!=null&&pad.buttonSouth.wasPressedThisFrame))vertical=4.7f;
  vertical-=18*Time.deltaTime;cc.Move((transform.TransformDirection(new Vector3(move.x,0,move.y))*(sprint?5:3)+Vector3.up*vertical)*Time.deltaTime);
  if(cc.isGrounded&&move.magnitude>.2f){footstep+=Time.deltaTime;if(footstep>(sprint?.32f:.48f)){game.Sound("step");footstep=0;}}
  if(transform.position.y<-8)Teleport(new Vector3(7.2f,.1f,-8.25f),-90);
  game.FindInteraction();if(interact)game.Interact();
  if(k!=null&&k.tabKey.wasPressedThisFrame||pad!=null&&pad.selectButton.wasPressedThisFrame)game.journal=!game.journal;
  if(k!=null&&k.rKey.wasPressedThisFrame||pad!=null&&pad.buttonNorth.wasPressedThisFrame)game.Reload();
  if(k!=null&&(k.hKey.wasPressedThisFrame||k.bKey.wasPressedThisFrame)||pad!=null&&pad.buttonEast.wasPressedThisFrame)ToggleHolster();
  bool aim=game.hasPistol&&!game.holstered&&(m!=null&&m.rightButton.isPressed||pad!=null&&pad.leftTrigger.ReadValue()>.3f);view.fieldOfView=Mathf.Lerp(view.fieldOfView,aim?(game.GetComponent<PortLoadout>().equipped?Mathf.Clamp(game.GetComponent<PortLoadout>().equipped.stats.adsFov,25,65):48):PortPreferences.Fov,Time.deltaTime*12);
  bool automatic=game.GetComponent<PortLoadout>().Automatic;if(m!=null&&(automatic?m.leftButton.isPressed:m.leftButton.wasPressedThisFrame)||pad!=null&&(automatic?pad.rightTrigger.isPressed:pad.rightTrigger.wasPressedThisFrame))game.Fire();
  recoil=Mathf.MoveTowards(recoil,0,Time.deltaTime*7);if(weapon){var loadout=game.GetComponent<PortLoadout>();bool melee=loadout.equipped&&loadout.equipped.stats.reloadType=="melee";weapon.localRotation=(aim?Quaternion.identity:loadout.equipped?loadout.equipped.hipRotation:Quaternion.identity)*Quaternion.Euler(melee?recoil*35:-recoil*5,melee?recoil*-40:0,0);weapon.gameObject.SetActive(game.hasPistol&&!game.holstered);Vector3 target=aim?game.GetComponent<PortLoadout>().Aim:game.GetComponent<PortLoadout>().Hip;target+=new Vector3(Mathf.Sin(Time.time*8)*.004f,Mathf.Cos(Time.time*16)*.003f,0)*move.magnitude;target.z-=recoil*.035f;if(game.reloadAt>0)target.y-=Mathf.Sin(Mathf.Clamp01((game.reloadAt-Time.time)/game.GetComponent<PortLoadout>().ReloadDuration)*Mathf.PI)*.22f;weapon.localPosition=Vector3.Lerp(weapon.localPosition,target,Time.deltaTime*14);}
 }
}
}
