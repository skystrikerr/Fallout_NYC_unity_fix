using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
public static class ImportWorldCreatures {
 const string Source="C:/Users/camde/Documents/Codex/2026-10-09/i-x20/outputs/UnityPort/Creatures";
 const string Dest="Assets/Resources/WorldCreatures";
 static Vector3 V(JToken a){return new Vector3((float)a[0],(float)a[1],-(float)a[2]);}
 static Quaternion Q(JToken a){return new Quaternion(-(float)a[0],-(float)a[1],(float)a[2],(float)a[3]);}
 static void Save(UnityEngine.Object asset,string path){var old=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);if(old)EditorUtility.CopySerialized(asset,old);else AssetDatabase.CreateAsset(asset,path);}
 public static string Build(){
  Directory.CreateDirectory(Dest);File.Copy(Source+"/atlas.png",Dest+"/atlas.png",true);AssetDatabase.ImportAsset(Dest+"/atlas.png");
  var ti=(TextureImporter)AssetImporter.GetAtPath(Dest+"/atlas.png");ti.textureType=TextureImporterType.Default;ti.mipmapEnabled=true;ti.filterMode=FilterMode.Trilinear;ti.SaveAndReimport();
  var mat=new Material(Shader.Find("FalloutPort/VaultSurface"));mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Dest+"/atlas.png"));mat.SetFloat("_VertexColor",1);Save(mat,Dest+"/Creature.mat");mat=AssetDatabase.LoadAssetAtPath<Material>(Dest+"/Creature.mat");
  var types=JObject.Parse(File.ReadAllText(Source+"/summary.json"))["types"];int count=0;
  foreach(string type in types){BuildOne(type,mat);count++;File.WriteAllText("Logs/CreatureImport.txt",count+" / "+types.Count()+" imported: "+type);}
  AssetDatabase.SaveAssets();return count+" animated creature prefabs imported";
 }
 static void BuildOne(string type,Material mat){
  var d=JObject.Parse(File.ReadAllText(Source+"/"+type+".json"));string dir=Dest+"/"+type;Directory.CreateDirectory(dir);
  int count=d["p"].Count()/3;var v=new Vector3[count];var n=new Vector3[count];var uv=new Vector2[count];var colors=new Color[count];var weights=new BoneWeight[count];
  for(int i=0;i<count;i++){int j=i*3,k=i*4;v[i]=new Vector3((float)d["p"][j],(float)d["p"][j+1],-(float)d["p"][j+2]);n[i]=new Vector3((float)d["n"][j],(float)d["n"][j+1],-(float)d["n"][j+2]);uv[i]=new Vector2((float)d["uv"][i*2],(float)d["uv"][i*2+1]);colors[i]=new Color((float)d["c"][j],(float)d["c"][j+1],(float)d["c"][j+2]);weights[i]=new BoneWeight{boneIndex0=(int)d["skinIndex"][k],boneIndex1=(int)d["skinIndex"][k+1],boneIndex2=(int)d["skinIndex"][k+2],boneIndex3=(int)d["skinIndex"][k+3],weight0=(float)d["skinWeight"][k],weight1=(float)d["skinWeight"][k+1],weight2=(float)d["skinWeight"][k+2],weight3=(float)d["skinWeight"][k+3]};}
  var ix=d["indices"].ToObject<int[]>();for(int i=0;i<ix.Length;i+=3){int tmp=ix[i+1];ix[i+1]=ix[i+2];ix[i+2]=tmp;}
  var mirror=Matrix4x4.Scale(new Vector3(1,1,-1));var bind=d["bind"].Select(a=>{var m=new Matrix4x4();for(int i=0;i<16;i++)m[i]=(float)a[i];return mirror*m*mirror;}).ToArray();
  var mesh=new Mesh{name=type,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.vertices=v;mesh.normals=n;mesh.uv=uv;mesh.colors=colors;mesh.triangles=ix;mesh.boneWeights=weights;mesh.bindposes=bind;mesh.RecalculateBounds();Save(mesh,dir+"/Body.asset");
  var root=new GameObject(type);try{var rig=new GameObject("Rig").transform;rig.SetParent(root.transform,false);var bones=d["bones"].Select(b=>new GameObject((string)b["name"]).transform).ToArray();for(int i=0;i<bones.Length;i++){var b=d["bones"][i];int parent=(int)b["parent"];bones[i].SetParent(parent<0?rig:bones[parent],false);bones[i].localPosition=V(b["p"]);bones[i].localRotation=Q(b["q"]);}
   var skin=root.AddComponent<SkinnedMeshRenderer>();skin.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(dir+"/Body.asset");skin.bones=bones;skin.rootBone=rig;skin.sharedMaterial=mat;skin.localBounds=new Bounds(mesh.bounds.center,mesh.bounds.size*1.8f);skin.updateWhenOffscreen=false;
   var animation=root.AddComponent<Animation>();foreach(var cd in d["clips"]){string name=(string)cd["name"];var clip=new AnimationClip{name=name,legacy=true,frameRate=15,wrapMode=name=="death"?WrapMode.ClampForever:WrapMode.Loop};var frames=(JArray)cd["frames"];
    Curves(clip,"Rig",frames.Select(f=>V(f["rigP"])).ToArray(),frames.Select(f=>Q(f["rigQ"])).ToArray());for(int i=0;i<bones.Length;i++){int bone=i;Curves(clip,AnimationUtility.CalculateTransformPath(bones[i],root.transform),frames.Select(f=>V(f["bones"][bone]["p"])).ToArray(),frames.Select(f=>Q(f["bones"][bone]["q"])).ToArray());}
    clip.EnsureQuaternionContinuity();Save(clip,dir+"/"+name+".anim");clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(dir+"/"+name+".anim");animation.AddClip(clip,name);if(name=="idle")animation.clip=clip;
   }animation.playAutomatically=true;PrefabUtility.SaveAsPrefabAsset(root,Dest+"/"+type+".prefab");
  }finally{UnityEngine.Object.DestroyImmediate(root);}
 }
 static void Curves(AnimationClip clip,string path,Vector3[] p,Quaternion[] q){for(int channel=0;channel<7;channel++){var keys=new Keyframe[p.Length];for(int f=0;f<p.Length;f++)keys[f]=new Keyframe(f/15f,channel<3?p[f][channel]:q[f][channel-3]);var curve=new AnimationCurve(keys);for(int k=0;k<keys.Length;k++){AnimationUtility.SetKeyLeftTangentMode(curve,k,AnimationUtility.TangentMode.Linear);AnimationUtility.SetKeyRightTangentMode(curve,k,AnimationUtility.TangentMode.Linear);}clip.SetCurve(path,typeof(Transform),(channel<3?"localPosition.":"localRotation.")+"xyzw"[channel<3?channel:channel-3],curve);}}
}
