using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FalloutPort;
public static class MenuValidation {
 static List<string> checks;static void Check(bool v,string s){checks.Add((v?"PASS ":"FAIL ")+s);if(!v)throw new Exception(s);}
 public static string Run(){checks=new List<string>();var g=UnityEngine.Object.FindFirstObjectByType<PortGame>();var m=g.menu;
  Check(m&&m.Visible&&m.page=="title","title menu opens at startup");Check(Time.timeScale==0&&g.paused,"title freezes gameplay");Check(Mathf.Approximately((GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).renderScale,.75f),"3D render scale 75 percent");Check(m.canvas.renderMode==RenderMode.ScreenSpaceOverlay,"menu renders at display resolution");
  m.NewGame();Check(!m.Visible&&g.stage==0&&Time.timeScale==1,"New Game resets and resumes vault");m.PauseMenu();Check(m.page=="pause"&&g.paused&&Time.timeScale==0,"pause menu freezes game");m.Settings();Check(m.page=="settings","settings opens from pause");m.Back();Check(m.page=="pause","settings Back returns to pause");m.Resume();
  g.stage=2;g.kills=1;g.ammo=5;g.reserve=37;g.hasPistol=true;g.holstered=false;g.roaches[0].SetActive(true);g.roaches[0].GetComponent<PortRoach>().dead=true;g.player.Teleport(new Vector3(1,0,8),32);g.player.pitch=11;PortSave.Write(g,99);var s=PortSave.Read(99);Check(s!=null&&s.stage==2&&s.dead[0],"save serializes quest and enemy state");m.NewGame();PortSave.Apply(g,s);Check(g.stage==2&&g.kills==1&&g.ammo==5&&g.reserve==37&&g.hasPistol,"load restores quest, weapon and ammunition");Check(Vector3.Distance(g.player.transform.position,new Vector3(1,0,8))<.01f&&Mathf.Abs(g.player.pitch-11)<.01f,"load restores position and view");Check(g.roaches[0].GetComponent<PortRoach>().dead&&!g.roaches[0].GetComponent<Collider>().enabled,"load restores dead enemy collision");
  m.NewGame();Check(g.stage==0&&g.kills==0&&!g.hasPistol&&!g.mainDoor.unlocked&&!g.roaches[0].GetComponent<PortRoach>().dead,"New Game clears prior progress without deleting save slots");m.ShowTitle();Check(m.page=="title"&&m.Visible,"Quit to Main Menu returns to title");
  File.WriteAllLines("Assets/FalloutPort/MenuValidation.txt",checks);return string.Join("\n",checks.ToArray());
 }
}
