using System.Collections;
using System.IO;
using UnityEngine;
namespace FalloutPort {
// Explicit opt-in build verification. Normal player launches never create this component.
public class PortBuildSmoke:MonoBehaviour {
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Begin(){if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-port-smoke")<0)return;new GameObject("Port build verification").AddComponent<PortBuildSmoke>();}
 IEnumerator Start(){yield return null;var game=FindAnyObjectByType<PortGame>();if(!game){Debug.LogError("PORT_SMOKE: missing game");Application.Quit(2);yield break;}game.menu.NewGame();var world=game.GetComponent<PortOpenWorld>();world.Travel(new Vector3(-65,.1f,708),180);float deadline=Time.realtimeSinceStartup+240;var quest=game.GetComponent<PortQuestRuntime>();while((world.loading||!quest.initialized||quest.initializing)&&Time.realtimeSinceStartup<deadline)yield return null;
  if(world.loading||!quest.initialized||quest.faults.Count>0){Debug.LogError("PORT_SMOKE: world/story initialization failed "+string.Join(";",quest.faults));Application.Quit(3);yield break;}
  game.menu.PauseMenu();string state=quest.Capture();RaycastHit hit;bool floor=Physics.Raycast(game.player.transform.position+Vector3.up*2,Vector3.down,out hit,6);bool valid=floor&&state.Length>100&&world.manifest.doors.Length==368&&world.manifest.containers.Length==1711;
  Debug.Log("PORT_SMOKE: "+(valid?"PASS":"FAIL")+" floor="+floor+" doors="+world.manifest.doors.Length+" containers="+world.manifest.containers.Length+" questBytes="+state.Length+" errors="+quest.faults.Count);Application.Quit(valid?0:4);
 }
}
}
