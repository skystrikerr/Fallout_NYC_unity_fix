using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
namespace FalloutPort {
public class PortLoadout:MonoBehaviour {
 public PortWeaponDefinition[] catalog;public PortWeaponDefinition equipped;PortGame game;Transform original;GameObject instance;AudioSource audio;
 public int Magazine=>equipped?Mathf.Max(1,equipped.stats.magSize):12;public float ReloadDuration=>equipped?Mathf.Max(.35f,equipped.stats.reloadTime):1.1f;public bool Automatic=>equipped&&equipped.stats.automatic;public string DisplayName=>equipped?equipped.displayName:"10mm";
 void Awake(){game=GetComponent<PortGame>();catalog=Resources.LoadAll<PortWeaponDefinition>("WeaponDefinitions").OrderBy(x=>x.displayName).ToArray();}
 void Start(){original=game.player.weapon;audio=gameObject.AddComponent<AudioSource>();audio.playOnAwake=false;}
 public void Equip(string id,bool supply){var def=Array.Find(catalog,x=>x.id==id);if(!def)return;if(!original)original=game.player.weapon;if(instance){instance.SetActive(false);Destroy(instance);}original.gameObject.SetActive(false);equipped=def;instance=Instantiate(def.prefab,game.player.view.transform);instance.name=def.displayName+" viewmodel";game.player.weapon=instance.transform;game.player.weapon.localPosition=Hip;instance.transform.localRotation=def.hipRotation;instance.AddComponent<PortWeaponMechanism>().Setup(def);game.hasPistol=true;game.holstered=false;game.reloadAt=0;if(supply){game.ammo=Magazine;game.reserve=Magazine*5;}instance.SetActive(!game.menu.Visible);}
 public Vector3 Hip {get{return equipped?new Vector3(equipped.hip.x,Mathf.Max(equipped.hip.y,-.18f),Mathf.Max(equipped.hip.z,.42f)):new Vector3(.16f,-.12f,.55f);}}
 public Vector3 Aim {get{return equipped?new Vector3(-equipped.sight.x,-equipped.sight.y,.45f-equipped.sight.z):new Vector3(0,-.08f,.5f);}}
 public void ResetRun(){if(instance){instance.SetActive(false);Destroy(instance);}instance=null;equipped=null;if(original)game.player.weapon=original;}
 public void Shoot(){if(game.paused||game.dialogueOpen||!game.hasPistol||game.holstered||game.reloadAt>0||Time.time<game.nextShot)return;var stats=equipped?equipped.stats:null;bool free=stats!=null&&(stats.reloadType=="melee"||stats.ammo==null);game.nextShot=Time.time+1/Mathf.Max(.3f,stats!=null?stats.rof:4.35f);if(!free&&game.ammo<=0){game.Sound("ui");return;}if(!free)game.ammo--;if(instance)instance.GetComponent<PortWeaponMechanism>().Fire();if(audio==null)audio=gameObject.AddComponent<AudioSource>();string sound=stats!=null&&stats.skill=="energy_weapons"?null:stats!=null&&(stats.ammo=="ammo_308"||stats.ammo=="ammo_556")?"hunting rifle sound":"pistolsound1";var clip=sound==null?null:Resources.Load<AudioClip>("FalloutAudio/Effects/"+sound);if(clip)audio.PlayOneShot(clip,.4f);else game.Sound("shot");int pellets=stats!=null?Mathf.Clamp(stats.pellets,1,16):1;for(int i=0;i<pellets;i++)game.GetComponent<PortCombat>().Shot(equipped,i);}
}
}
