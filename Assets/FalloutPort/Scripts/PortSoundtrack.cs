using UnityEngine;
namespace FalloutPort {
// Music uses unscaled time so title, pause and dialogue screens stay musical.
public class PortSoundtrack : MonoBehaviour {
 AudioClip[] tracks; AudioSource current,previous; PortGame game; int index; float fade;
 void Start(){game=GetComponent<PortGame>();tracks=Resources.LoadAll<AudioClip>("FalloutAudio/Music");System.Array.Sort(tracks,(a,b)=>string.CompareOrdinal(a.name,b.name));current=Voice();previous=Voice();if(tracks.Length>0)Next();}
 AudioSource Voice(){var s=gameObject.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=0;s.volume=0;return s;}
 void Next(){var swap=previous;previous=current;current=swap;current.Stop();current.clip=tracks[index];index=(index+1)%tracks.Length;current.volume=0;current.Play();fade=0;}
 void Update(){if(tracks==null||tracks.Length==0)return;float target=game&&game.dialogueOpen?.12f:.24f;fade=Mathf.Min(1,fade+Time.unscaledDeltaTime/3f);current.volume=target*fade;previous.volume=target*(1-fade);if(fade>=1&&previous.isPlaying)previous.Stop();if(fade>=1&&(!current.isPlaying||current.clip.length-current.time<3f))Next();}
 }
}
