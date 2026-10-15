using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace FalloutPort {
[Serializable] public class PortItemStack {public string id;public int count;}
[Serializable] public class PortAidEffects {public float hp,ap,rads;public float[] hpOverTime,radsOverTime;}
[Serializable] public class PortItemDefinition {public string id,name,type,desc;public float weight;public int value;public PortAidEffects effects;}
public class PortItemStore:MonoBehaviour {
 public List<PortItemStack> items=new List<PortItemStack>();public float radiation;PortGame game;
 void Awake(){game=GetComponent<PortGame>();}
 public PortItemDefinition Definition(string id){var world=GetComponent<PortOpenWorld>();return world.manifest?.items?.FirstOrDefault(x=>x.id==id);}
 public string Label(string id){var d=Definition(id);return d!=null?d.name:id.Replace('_',' ');}
 public int Count(string id){if(id=="caps")return GetComponent<PortSurvival>().caps;if(id=="stimpak")return GetComponent<PortSurvival>().stimpaks;var inv=GetComponent<PortInventory>();inv.Store();if(inv.Owns(id))return 1;var ammo=inv.ammunition.Find(x=>x.id==id);if(ammo!=null)return ammo.count;return items.Find(x=>x.id==id)?.count??0;}
 public void Add(string id,int count){if(count==0)return;var d=Definition(id);if(d==null){Debug.LogError("Unknown item "+id);return;}var inventory=GetComponent<PortInventory>();if(d.type=="weapon"){inventory.Acquire(id,id.Contains("grenade")||id.Contains("mine")?count:0);return;}if(d.type=="ammo"){inventory.AddAmmo(id,count);return;}if(id=="caps"){GetComponent<PortSurvival>().caps=Mathf.Max(0,GetComponent<PortSurvival>().caps+count);return;}if(id=="stimpak"){GetComponent<PortSurvival>().stimpaks=Mathf.Max(0,GetComponent<PortSurvival>().stimpaks+count);return;}var item=items.Find(x=>x.id==id);if(item==null){item=new PortItemStack{id=id};items.Add(item);}item.count=Mathf.Max(0,item.count+count);if(item.count==0)items.Remove(item);}
 public bool Remove(string id,int count){if(count<0||Count(id)<count)return false;var inv=GetComponent<PortInventory>();if(inv.Owns(id)){inv.Store();inv.weapons.RemoveAll(x=>x.id==id);var loadout=GetComponent<PortLoadout>();if(loadout.equipped&&loadout.equipped.id==id){if(inv.weapons.Count>0)inv.Equip(inv.weapons[0].id);else{game.hasPistol=false;game.holstered=true;game.player.weapon.gameObject.SetActive(false);}}return true;}if(inv.ammunition.Exists(x=>x.id==id)){inv.AddAmmo(id,-count);return true;}Add(id,-count);return true;}
 public bool Take(PortCityContainer container,int index){if(container.Locked||index<0||index>=container.data.items.Length)return false;var inventory=GetComponent<PortInventory>();string key="city:"+container.data.id+":"+index;if(inventory.claimed.Contains(key))return false;var stack=container.data.items[index];if(stack.count<=0||Definition(stack.id)==null)return false;
  inventory.claimed.Add(key);var d=Definition(stack.id);
  if(d.type=="weapon")inventory.Acquire(d.id,d.id.Contains("grenade")||d.id.Contains("mine")?stack.count:0);
  else if(d.type=="ammo")inventory.AddAmmo(d.id,stack.count);
  else if(d.id=="caps")GetComponent<PortSurvival>().caps+=stack.count;
  else if(d.id=="stimpak")GetComponent<PortSurvival>().stimpaks+=stack.count;
  else {var owned=items.Find(x=>x.id==d.id);if(owned==null){owned=new PortItemStack{id=d.id};items.Add(owned);}owned.count+=stack.count;}
  game.notice="Collected "+stack.count+" × "+d.name;game.Sound("ui");return true;
 }
 public bool Use(string id){var owned=items.Find(x=>x.id==id);var d=Definition(id);if(owned==null||owned.count<=0||d?.type!="aid"||d.effects==null)return false;var e=d.effects;
  if(e.hp==0&&e.ap==0&&e.rads==0&&(e.hpOverTime==null||e.hpOverTime.Length==0)&&(e.radsOverTime==null||e.radsOverTime.Length==0)){game.notice="This item's special effect is not implemented yet.";return false;}
  var survival=GetComponent<PortSurvival>();survival.health=Mathf.Min(100,survival.health+e.hp);survival.stamina=Mathf.Min(100,survival.stamina+e.ap);radiation=Mathf.Max(0,radiation+e.rads);if(e.hpOverTime?.Length==2)StartCoroutine(OverTime(e.hpOverTime[0],e.hpOverTime[1],false));if(e.radsOverTime?.Length==2)StartCoroutine(OverTime(e.radsOverTime[0],e.radsOverTime[1],true));owned.count--;if(owned.count==0)items.Remove(owned);game.notice="Used "+d.name;game.Sound("ui");return true;
 }
 System.Collections.IEnumerator OverTime(float amount,float seconds,bool rads){float elapsed=0;while(elapsed<seconds){yield return null;float dt=Mathf.Min(Time.deltaTime,seconds-elapsed);elapsed+=dt;if(rads)radiation=Mathf.Max(0,radiation+amount*dt/seconds);else {var survival=GetComponent<PortSurvival>();survival.health=Mathf.Min(100,survival.health+amount*dt/seconds);}}}
 public void Restore(PortSave.Snapshot s){StopAllCoroutines();items=s.items!=null?s.items.Select(x=>new PortItemStack{id=x.id,count=x.count}).ToList():new List<PortItemStack>();radiation=s.radiation;}
}
public class PortCityContainer:MonoBehaviour {
 public PortOpenWorld.Container data;public PortGame game;
 public bool Taken(int index){return game.GetComponent<PortInventory>().claimed.Contains("city:"+data.id+":"+index);}
 public bool Locked=>data.locked>0&&!game.GetComponent<PortInventory>().claimed.Contains("unlock:city:"+data.id);
 public void Refresh(){GetComponent<PortInteractable>().displayName=data.name+(Locked?" [Locked]":"");}
 public void Open(){Refresh();if(Locked){var pick=game.GetComponent<PortLockpick>();if(!pick)pick=game.gameObject.AddComponent<PortLockpick>();pick.Open(data.locked,()=>{var claims=game.GetComponent<PortInventory>().claimed;string key="unlock:city:"+data.id;if(!claims.Contains(key))claims.Add(key);Refresh();game.menu.OpenCityLoot(this);});return;}game.menu.OpenCityLoot(this);}
}
}
