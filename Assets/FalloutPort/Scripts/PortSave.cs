using System;
using System.IO;
using UnityEngine;
namespace FalloutPort {
public static class PortSave {
 [Serializable] public class Snapshot {public int version=1,stage,kills,ammo,reserve;public bool hasPistol,holstered,doorUnlocked;public float doorOpen,pitch,yaw;public Vector3 position;public bool[] dead;public string savedAt;}
 public static string PathFor(int slot){return Path.Combine(Application.persistentDataPath,"FalloutPort","vault-slot-"+slot+".json");}
 public static Snapshot Read(int slot){try{if(!File.Exists(PathFor(slot)))return null;var s=JsonUtility.FromJson<Snapshot>(File.ReadAllText(PathFor(slot)));if(s==null||s.version!=1||s.stage<0||s.stage>7||s.dead==null||s.dead.Length!=3||!float.IsFinite(s.position.x)||!float.IsFinite(s.position.y)||!float.IsFinite(s.position.z)||s.ammo<0||s.ammo>12||s.reserve<0)return null;return s;}catch{return null;}}
 public static int Latest(){int best=-1;DateTime time=DateTime.MinValue;for(int i=0;i<3;i++){if(Read(i)==null)continue;var t=File.GetLastWriteTimeUtc(PathFor(i));if(t>time){best=i;time=t;}}return best;}
 public static void Write(PortGame g,int slot){
  var s=new Snapshot{stage=g.stage,kills=g.kills,ammo=g.ammo,reserve=g.reserve,hasPistol=g.hasPistol,holstered=g.holstered,doorUnlocked=g.mainDoor.unlocked,doorOpen=g.mainDoor.openness,position=g.player.transform.position,pitch=g.player.pitch,yaw=g.player.yaw,savedAt=DateTime.Now.ToString("MMM dd, HH:mm"),dead=new bool[g.roaches.Length]};for(int i=0;i<s.dead.Length;i++)s.dead[i]=g.roaches[i].GetComponent<PortRoach>().dead;
  string file=PathFor(slot);Directory.CreateDirectory(Path.GetDirectoryName(file));string temp=file+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(s,true));if(File.Exists(file))File.Replace(temp,file,file+".bak");else File.Move(temp,file);
 }
 public static void Apply(PortGame g,Snapshot s){
  g.stage=s.stage;g.kills=s.kills;g.ammo=s.ammo;g.reserve=s.reserve;g.hasPistol=s.hasPistol;g.holstered=s.holstered;g.reloadAt=0;g.nextShot=0;g.dialogueOpen=false;g.journal=false;g.mainDoor.unlocked=s.doorUnlocked;g.mainDoor.openness=s.doorOpen;g.mainDoor.Step(0);g.player.Teleport(s.position,s.yaw);g.player.pitch=s.pitch;g.player.view.transform.localPosition=new Vector3(0,1.62f,0);g.player.view.transform.localRotation=Quaternion.Euler(s.pitch,0,0);
  for(int i=0;i<g.roaches.Length;i++){var o=g.roaches[i];var r=o.GetComponent<PortRoach>();o.SetActive(s.stage>=2);r.dead=s.dead[i];o.transform.localScale=new Vector3(1,r.dead?.22f:1,1);foreach(var c in o.GetComponentsInChildren<Collider>())c.enabled=!r.dead;}
  g.player.weapon.gameObject.SetActive(s.hasPistol&&!s.holstered);g.notice="Loaded · Lease on Life";Physics.SyncTransforms();
 }
}
}
