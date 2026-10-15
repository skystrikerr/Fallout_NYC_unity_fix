using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace FalloutPort {
[Serializable] public class PortOwnedWeapon {public string id;public int loaded;}
[Serializable] public class PortAmmoStock {public string id;public int count;}
public class PortInventory:MonoBehaviour {
 public List<PortOwnedWeapon> weapons=new List<PortOwnedWeapon>();public List<PortAmmoStock> ammunition=new List<PortAmmoStock>();public List<string> claimed=new List<string>();PortGame game;PortLoadout loadout;
 void Awake(){game=GetComponent<PortGame>();loadout=GetComponent<PortLoadout>();}
 string Ammo(PortWeaponDefinition d){return string.IsNullOrEmpty(d.stats.ammo)?"":d.stats.ammo;}
 PortAmmoStock Stock(string id){var s=ammunition.Find(x=>x.id==id);if(s==null){s=new PortAmmoStock{id=id};ammunition.Add(s);}return s;}
 public void Store(){if(!loadout.equipped)return;var w=weapons.Find(x=>x.id==loadout.equipped.id);if(w==null)return;w.loaded=game.ammo;Stock(Ammo(loadout.equipped)).count=game.reserve;}
 public void AddAmmo(string id,int count){Store();Stock(id).count+=count;if(loadout.equipped&&Ammo(loadout.equipped)==id)game.reserve=Stock(id).count;}
 public bool Owns(string id){return weapons.Any(x=>x.id==id);}
 public void Acquire(string id,int rounds,bool equip=false){var d=Array.Find(loadout.catalog,x=>x.id==id);if(!d)return;Store();bool fresh=!Owns(id);if(fresh){int loaded=string.IsNullOrEmpty(d.stats.ammo)?0:Mathf.Min(rounds,Mathf.Max(1,d.stats.magSize));weapons.Add(new PortOwnedWeapon{id=id,loaded=loaded});rounds-=loaded;}Stock(Ammo(d)).count+=Mathf.Max(0,rounds);if(loadout.equipped&&Ammo(d)==Ammo(loadout.equipped))game.reserve=Stock(Ammo(d)).count;if(equip||!game.hasPistol)Equip(id);game.notice=d.displayName+" acquired. Pause > Weapons to equip; mouse wheel / D-pad left-right to switch.";game.Sound("quest");}
 public bool Equip(string id){if(!Owns(id))return false;Store();var w=weapons.Find(x=>x.id==id);loadout.Equip(id,false);game.ammo=w.loaded;game.reserve=Stock(Ammo(loadout.equipped)).count;return true;}
 public void Cycle(int direction){if(game.paused||game.dialogueOpen||weapons.Count==0)return;int i=weapons.FindIndex(x=>loadout.equipped&&x.id==loadout.equipped.id);Equip(weapons[(i+direction+weapons.Count)%weapons.Count].id);}
 public bool Take(PortWorldLoot loot,int index){if(index<0||index>=loot.weaponIds.Length||game.stage<loot.requiredStage)return false;string key=loot.lootId+":"+index;if(claimed.Contains(key))return false;var d=Array.Find(loadout.catalog,x=>x.id==loot.weaponIds[index]);if(!d)return false;claimed.Add(key);Acquire(d.id,string.IsNullOrEmpty(d.stats.ammo)?0:Mathf.Max(1,d.stats.magSize)*2);loot.Refresh();return true;}
 public void Restore(PortSave.Snapshot s){weapons=s.ownedWeapons!=null?s.ownedWeapons.Select(x=>new PortOwnedWeapon{id=x.id,loaded=x.loaded}).ToList():new List<PortOwnedWeapon>();ammunition=s.ammoStocks!=null?s.ammoStocks.Select(x=>new PortAmmoStock{id=x.id,count=x.count}).ToList():new List<PortAmmoStock>();claimed=s.claimedLoot!=null?new List<string>(s.claimedLoot):new List<string>();if(s.hasPistol&&loadout.equipped&&!Owns(loadout.equipped.id)){weapons.Add(new PortOwnedWeapon{id=loadout.equipped.id,loaded=s.ammo});Stock(Ammo(loadout.equipped)).count=s.reserve;}foreach(var loot in FindObjectsByType<PortWorldLoot>(FindObjectsInactive.Include))loot.Refresh();}
}
}
