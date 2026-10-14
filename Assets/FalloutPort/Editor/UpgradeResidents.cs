using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
public static class UpgradeResidents {
 const string Root="Assets/NPCCollection";const string Upgrade=Root+"/Refinements";
 public static string Build(){
  Directory.CreateDirectory(Upgrade);AssetDatabase.Refresh();int done=0;
  var files=Directory.GetFiles(Root+"/Prefabs","*.prefab").OrderBy(x=>x).ToArray();
  AssetDatabase.StartAssetEditing();try{
   foreach(string path in files){
    string name=Path.GetFileNameWithoutExtension(path);string hp="Assets/FalloutPort/HairSource/"+name+".json";if(!File.Exists(hp))continue;
    var root=PrefabUtility.LoadPrefabContents(path);
    try{
     var scalp=root.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(r=>r.name=="FittedHair");if(!scalp)continue;
     var data=JObject.Parse(File.ReadAllText(hp));var p=data["p"].ToObject<float[]>();var uv=data["uv"].ToObject<float[]>();var indices=data["indices"].ToObject<int[]>();var vs=new Vector3[p.Length/3];var uvs=new Vector2[vs.Length];var colors=new Color[vs.Length];var weights=new BoneWeight[vs.Length];int head=Array.FindIndex(scalp.bones,x=>x.name=="head");if(head<0)throw new Exception("Head bone missing: "+name);
     var rng=new System.Random(int.Parse(name.Substring(0,2))*73);
     for(int i=0;i<vs.Length;i++){vs[i]=new Vector3(p[i*3],p[i*3+1],p[i*3+2])*.01f;uvs[i]=new Vector2(uv[i*2],uv[i*2+1]);float t=(i/26%17)/16f;colors[i]=new Color(t,(i/26%11)/10f,1,1);weights[i]=new BoneWeight{boneIndex0=head,weight0=1};}
     for(int j=0;j<indices.Length;j+=3){int v=indices[j+1];indices[j+1]=indices[j+2];indices[j+2]=v;}var mesh=new Mesh{name=name+"_HairLocks"};mesh.vertices=vs;mesh.uv=uvs;mesh.colors=colors;mesh.triangles=indices;mesh.boneWeights=weights;mesh.bindposes=scalp.sharedMesh.bindposes;mesh.RecalculateNormals();mesh.RecalculateBounds();string mp=Upgrade+"/"+name+"_Hair.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(mp);if(old){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,mp);
     string matPath=Upgrade+"/"+name+"_Hair.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(!mat){mat=new Material(Shader.Find("FalloutPort/StrandHair"));AssetDatabase.CreateAsset(mat,matPath);}mat.SetTexture("_BaseMap",scalp.sharedMaterial.GetTexture("_BaseMap"));mat.SetColor("_BaseColor",Color.white);
     var existing=root.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(r=>r.name=="StrandHair");if(!existing){var o=new GameObject("StrandHair");o.transform.SetParent(scalp.transform.parent,false);o.transform.localPosition=scalp.transform.localPosition;o.transform.localRotation=scalp.transform.localRotation;o.transform.localScale=scalp.transform.localScale;existing=o.AddComponent<SkinnedMeshRenderer>();}
     existing.sharedMesh=mesh;existing.bones=scalp.bones;existing.rootBone=scalp.rootBone;existing.localBounds=mesh.bounds;existing.sharedMaterial=mat;existing.updateWhenOffscreen=false;scalp.enabled=false;
     // Preserve tied styles while replacing the solid scalp cap.
     foreach(var r in root.GetComponentsInChildren<SkinnedMeshRenderer>())if(r.name.Contains("HairBun")||r.name.Contains("Ponytail"))r.enabled=true;
     BuildIdles(root,name);
     PrefabUtility.SaveAsPrefabAsset(root,path);done++;File.WriteAllText("Logs/ResidentUpgrade.log",done+"/50 "+name);
    }finally{PrefabUtility.UnloadPrefabContents(root);}
   }
  }finally{AssetDatabase.StopAssetEditing();}
  AssetDatabase.SaveAssets();return "Updated "+done+" characters with strand hair and seven idle clips each.";
 }
 static void BuildIdles(GameObject root,string name){
  var anim=root.GetComponentInChildren<Animator>();var ctrl=anim.runtimeAnimatorController as AnimatorController;if(!ctrl)throw new Exception("Controller missing");var original=ctrl.animationClips.First(x=>x.name.Contains("Idle_Breathe"));var bones=anim.GetComponentsInChildren<Transform>();var rotations=bones.Select(t=>t.localRotation).ToArray();original.SampleAnimation(anim.gameObject,0);
  var pose=bones.Select(t=>t.localRotation).ToArray();var axesX=bones.Select(t=>t.parent?Quaternion.Inverse(t.parent.rotation)*Vector3.right:Vector3.right).ToArray();var axesY=bones.Select(t=>t.parent?Quaternion.Inverse(t.parent.rotation)*Vector3.up:Vector3.up).ToArray();var axesZ=bones.Select(t=>t.parent?Quaternion.Inverse(t.parent.rotation)*Vector3.forward:Vector3.forward).ToArray();
  string[] names={"Relaxed","LookAround","WeightShift","CheckWrist","AdjustSleeve","GuardWatch","Conversation"};
  for(int kind=0;kind<names.Length;kind++){
   var clip=new AnimationClip{name="Idle_"+names[kind],frameRate=30};
   for(int b=0;b<bones.Length;b++){
    if(bones[b]==anim.transform)continue;string bn=bones[b].name;var curves=new[]{new AnimationCurve(),new AnimationCurve(),new AnimationCurve(),new AnimationCurve()};
    bool moving=bn=="spine02"||bn=="head"||bn=="pelvis"||bn.StartsWith("upperarm01")||bn.StartsWith("lowerarm01");int samples=moving?32:1;for(int k=0;k<=samples;k++){float t=(float)k/samples,cycle=Mathf.Sin(t*Mathf.PI*2),gesture=Mathf.Pow(Mathf.Sin(t*Mathf.PI),4);float x=0,y=0,z=0;
     if(bn=="spine02")x=.7f*Mathf.Sin(t*Mathf.PI*4);
     if(bn=="head"){y=(kind==1?16:kind==5?9:3)*cycle;x=kind==3?gesture*12:kind==6?-3*Mathf.Sin(t*Mathf.PI*4):1.2f*cycle;}
     if(kind==2){if(bn=="pelvis")z=cycle*1.4f;if(bn=="spine02")z=-cycle*1.2f;}
     if(kind==3){if(bn=="upperarm01.L")x=-gesture*25;if(bn=="lowerarm01.L")x=-gesture*58;}
     if(kind==4){if(bn=="upperarm01.R"){x=-gesture*22;z=gesture*10;}if(bn=="lowerarm01.R"){x=-gesture*62;z=gesture*12;}}
     if(kind==6){if(bn=="upperarm01.L")x=-gesture*12;if(bn=="lowerarm01.L")x=-gesture*28;}
     Quaternion q=Quaternion.AngleAxis(z,axesZ[b])*Quaternion.AngleAxis(y,axesY[b])*Quaternion.AngleAxis(x,axesX[b])*pose[b];curves[0].AddKey(t*8,q.x);curves[1].AddKey(t*8,q.y);curves[2].AddKey(t*8,q.z);curves[3].AddKey(t*8,q.w);
    }
    string path=AnimationUtility.CalculateTransformPath(bones[b],anim.transform);string[] fields={"x","y","z","w"};for(int c=0;c<4;c++)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(path,typeof(Transform),"m_LocalRotation."+fields[c]),curves[c]);
   }
   clip.EnsureQuaternionContinuity();var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;settings.loopBlend=true;AnimationUtility.SetAnimationClipSettings(clip,settings);string cp=Upgrade+"/"+name+"_"+clip.name+".anim";var previous=AssetDatabase.LoadAssetAtPath<AnimationClip>(cp);if(previous){EditorUtility.CopySerialized(clip,previous);UnityEngine.Object.DestroyImmediate(clip);clip=previous;}else AssetDatabase.CreateAsset(clip,cp);
   var sm=ctrl.layers[0].stateMachine;var state=sm.states.FirstOrDefault(s=>s.state.name=="Idle_"+names[kind]).state;if(!state)state=sm.AddState("Idle_"+names[kind]);state.motion=clip;EditorUtility.SetDirty(ctrl);
  }
  for(int i=0;i<bones.Length;i++)bones[i].localRotation=rotations[i];
 }
}



