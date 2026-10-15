using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using FalloutPort;
public static class ExpansionValidation {
 static List<string> checks;static void Check(bool pass,string label){checks.Add((pass?"PASS ":"FAIL ")+label);if(!pass)throw new Exception(label);}
 public static string Run(){checks=new List<string>();var g=UnityEngine.Object.FindFirstObjectByType<PortGame>();var p=g.player;Gamepad pad=null;try{
  g.menu.NewGame();p.enabled=false;g.hasPistol=true;g.holstered=false;
  var module=EventSystem.current.GetComponent<InputSystemUIInputModule>();Check(!module.enabled&&EventSystem.current.currentSelectedGameObject==null,"gameplay disables menu input and clears selection");
  pad=InputSystem.AddDevice<Gamepad>();Cursor.lockState=CursorLockMode.None;InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.East));InputSystem.Update();p.SendMessage("Update");g.menu.SendMessage("Update");Check(g.holstered&&!g.paused&&!g.menu.Visible&&Time.timeScale==1,"B holsters without freezing even with an unlocked cursor");InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.East));InputSystem.Update();p.SendMessage("Update");Check(!g.holstered,"second B draws weapon");InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();
  g.menu.PauseMenu();Check(module.enabled&&Time.timeScale==0,"pause enables menu input");g.menu.Back();Check(!module.enabled&&Time.timeScale==1,"Back resumes gameplay");
  var s=g.GetComponent<PortSurvival>();s.health=40;s.Heal();Check(s.health==85&&s.stimpaks==1,"stimpak restores health and consumes one supply");s.Loot(0);int stock=s.stimpaks;s.Loot(0);Check(s.stimpaks==stock,"supply cache cannot be farmed");float ap=s.stamina;Check(s.Sprint(.5f)&&s.stamina<ap,"sprinting consumes AP");
  s.BeginChallenge();int ammo=g.reserve;s.BeginChallenge();Check(g.reserve==ammo,"range retry cannot duplicate practice ammunition");s.TargetHit();s.TargetHit();s.TargetHit();Check(s.challengeDone&&s.caps==75,"range completion awards reward");int caps=s.caps;s.BeginChallenge();s.TargetHit();Check(s.caps==caps,"range reward cannot be farmed");
  PortSave.Write(g,98);var snapshot=PortSave.Read(98);g.menu.NewGame();PortSave.Apply(g,snapshot);Check(s.health==85&&s.challengeDone&&s.caps==75&&(s.cacheMask&1)!=0,"save/load preserves health, inventory, caches and challenge");
  g.menu.NewGame();p.enabled=false;g.hasPistol=true;g.holstered=false;g.stage=2;foreach(var r in g.roaches)r.SetActive(true);p.Teleport(new Vector3(-9,.08f,-69),0);var roach=g.roaches[0].GetComponent<PortRoach>();roach.enabled=false;p.view.transform.rotation=Quaternion.LookRotation(roach.transform.position+Vector3.up*.14f-p.view.transform.position);Physics.SyncTransforms();g.Fire();Check(roach.dead&&roach.GetComponent<Rigidbody>()&&!roach.GetComponent<Rigidbody>().isKinematic,"shot produces enemy death physics");Check(GameObject.Find("Bullet tracer")!=null,"shot produces visible tracer");Check(p.weapon.Find("First person arms")!=null,"textured first-person arm geometry installed");
  s.health=10;s.hurtUntil=0;s.Damage(12);Check(s.Dead&&g.menu.page=="death"&&Time.timeScale==0,"lethal damage opens recovery menu");g.menu.NewGame();Check(s.health==100&&s.stimpaks==2&&!s.challengeDone&&Time.timeScale==1,"new game resets survival state");
  Check(UnityEngine.Object.FindObjectsByType<PortCitizen>(FindObjectsSortMode.None).Length==5,"five residents have activity controllers");
 }finally{if(pad!=null)InputSystem.RemoveDevice(pad);p.enabled=true;g.menu.ShowTitle();File.WriteAllLines("Assets/FalloutPort/ExpansionValidation.txt",checks);}return string.Join("\n",checks.ToArray());}
}
