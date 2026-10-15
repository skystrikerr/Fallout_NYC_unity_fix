using UnityEngine;
namespace FalloutPort {
public class PortWorldLoot:MonoBehaviour {
 public string lootId,title;public string[] weaponIds;public int requiredStage;public bool loose;public Transform visual;PortGame game;
 void Start(){game=FindAnyObjectByType<PortGame>();Refresh();}
 public bool Taken(int index){if(!game)game=FindAnyObjectByType<PortGame>();return game&&game.GetComponent<PortInventory>().claimed.Contains(lootId+":"+index);}
 public void Refresh(){if(!game)game=FindAnyObjectByType<PortGame>();bool empty=true;for(int i=0;i<weaponIds.Length;i++)if(!Taken(i))empty=false;var it=GetComponent<PortInteractable>();it.displayName=title+(empty?" [Empty]":"");if(visual)visual.gameObject.SetActive(!empty);}
 public void Open(){if(!game)game=FindAnyObjectByType<PortGame>();if(game.stage<requiredStage){game.notice=requiredStage>=6?"Evacuation seal: obtain the Overseer's exit authorization first.":"Security seal: speak with Chief Doyle after the birthday ceremony.";return;}if(loose){if(!game.GetComponent<PortInventory>().Take(this,0))game.notice="Already collected.";}else game.menu.OpenLoot(this);}
}
}
