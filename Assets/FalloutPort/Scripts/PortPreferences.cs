using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace FalloutPort {
public static class PortPreferences {
 public static float Scale { get{return PlayerPrefs.GetFloat("FalloutPort.RenderScale",.75f);} set{PlayerPrefs.SetFloat("FalloutPort.RenderScale",Mathf.Clamp(value,.5f,1));Apply();} }
 public static float Sensitivity {get{return PlayerPrefs.GetFloat("FalloutPort.Sensitivity",1);}set{PlayerPrefs.SetFloat("FalloutPort.Sensitivity",Mathf.Clamp(value,.2f,3));}}
 public static float Fov {get{return PlayerPrefs.GetFloat("FalloutPort.Fov",72);}set{PlayerPrefs.SetFloat("FalloutPort.Fov",Mathf.Clamp(value,55,100));}}
 public static bool Invert {get{return PlayerPrefs.GetInt("FalloutPort.Invert",0)==1;}set{PlayerPrefs.SetInt("FalloutPort.Invert",value?1:0);}}
 public static float Volume {get{return PlayerPrefs.GetFloat("FalloutPort.Volume",.8f);}set{PlayerPrefs.SetFloat("FalloutPort.Volume",Mathf.Clamp01(value));Apply();}}
 public static void Apply(){var rp=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;if(rp)rp.renderScale=Scale;AudioListener.volume=Volume;PlayerPrefs.Save();}
 public static void Defaults(){Scale=.75f;Sensitivity=1;Fov=72;Invert=false;Volume=.8f;Apply();}
}
}
