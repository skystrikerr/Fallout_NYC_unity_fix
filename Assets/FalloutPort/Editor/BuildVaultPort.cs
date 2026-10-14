using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FalloutPort;
public static class BuildVaultPort {
 const string Root="Assets/FalloutPort";
 static Vector3 V(JToken t){return new Vector3((float)t[0],(float)t[1],-(float)t[2]);}
 static Color C(JToken t){return new Color((float)t[0],(float)t[1],(float)t[2],1);}
 static Material Solid(string name,Color color){string p=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.color=color;m.SetFloat("_Smoothness",.2f);return m;}
 [MenuItem("Tools/Fallout Port/Build Vault 120")]
 public static void MenuBuild(){Debug.Log(Build());}
 public static string Build(){
  if(EditorApplication.isPlaying)throw new Exception("Stop Play before building.");
  if(EditorSceneManager.GetActiveScene().isDirty)throw new Exception("Save current scene before building.");
  foreach(string d in new[]{"Meshes","Materials","Scenes"})Directory.CreateDirectory(Root+"/"+d);
  AssetDatabase.Refresh();
  var data=JObject.Parse(File.ReadAllText(Root+"/Source/vault120.json"));
  var textures=new Dictionary<int,Texture2D>();
  for(int i=0;i<(int)data["textureCount"];i++){
   string p=Root+"/Source/texture_"+i+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(p);
   importer.wrapMode=data["materials"].Any(d=>(int)d["texture"]==i && (bool)d["repeat"])?TextureWrapMode.Repeat:TextureWrapMode.Clamp;importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.mipmapEnabled=true;importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=4;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();textures[i]=AssetDatabase.LoadAssetAtPath<Texture2D>(p);
  }
  var materials=new List<Material>();int mi=0;
  foreach(var d in data["materials"]){string p=Root+"/Materials/Surface_"+mi+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("FalloutPort/VaultSurface"));AssetDatabase.CreateAsset(m,p);}m.name=(string)d["name"];m.SetColor("_BaseColor",C(d["color"]));m.SetColor("_Emission",C(d["emission"]));m.SetFloat("_VertexColor",(bool)d["vertexColors"]?1:0);m.SetFloat("_Cutoff",Mathf.Max(.05f,(float)d["cutoff"]));int tx=(int)d["texture"];if(tx>=0){textures[tx].wrapMode=(bool)d["repeat"]?TextureWrapMode.Repeat:TextureWrapMode.Clamp;m.SetTexture("_BaseMap",textures[tx]);}materials.Add(m);mi++;}
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var level=new GameObject("Vault 120 â€” original Three.js map");var dynamicRoot=new GameObject("Moving doors");
  var game=new GameObject("Lease on Life").AddComponent<PortGame>();
  var playerObj=new GameObject("Player");playerObj.layer=2;var cc=playerObj.AddComponent<CharacterController>();cc.height=1.75f;cc.radius=.28f;cc.center=new Vector3(0,.875f,0);cc.stepOffset=.4f;cc.skinWidth=.025f;cc.slopeLimit=50;
  var player=playerObj.AddComponent<PortPlayer>();player.transform.position=new Vector3(7.2f,.08f,-8.25f);player.transform.rotation=Quaternion.Euler(0,-90,0);player.game=game;game.player=player;
  var cam=new GameObject("Port Camera").AddComponent<Camera>();cam.tag="MainCamera";cam.transform.SetParent(player.transform,false);cam.transform.localPosition=new Vector3(0,1.62f,0);cam.nearClipPlane=.04f;cam.farClipPlane=230;cam.fieldOfView=72;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.025f,.035f,.04f);cam.gameObject.AddComponent<AudioListener>();player.view=cam;
  var pipeline=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;if(!pipeline)throw new Exception("URP pipeline missing");var so=new SerializedObject(pipeline);var renderers=so.FindProperty("m_RendererDataList");int rendererIndex=-1;for(int i=0;i<renderers.arraySize;i++)if(renderers.GetArrayElementAtIndex(i).objectReferenceValue is UniversalRendererData){rendererIndex=i;break;}if(rendererIndex<0)throw new Exception("3D renderer missing");cam.GetUniversalAdditionalCameraData().SetRenderer(rendererIndex);
  RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.65f,.69f,.75f);RenderSettings.fog=false;
  var sun=new GameObject("Vault ambient key").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=.8f;sun.color=new Color(1,.92f,.79f);sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.shadows=LightShadows.None;
  var doors=new Dictionary<int,PortDoor>();
  foreach(var d in data["doors"]){int id=(int)d["id"];var door=new GameObject("Automatic door "+id).AddComponent<PortDoor>();door.transform.SetParent(dynamicRoot.transform);door.transform.position=V(d["center"]);door.closedPosition=door.transform.position;door.height=(float)d["size"][1];door.player=player.transform;doors[id]=door;var block=new GameObject("Door blocker "+id).AddComponent<BoxCollider>();block.transform.SetParent(dynamicRoot.transform);block.transform.position=door.transform.position+Vector3.up*door.height/2;block.size=new Vector3((float)d["size"][0],door.height,(float)d["size"][2]);door.blocker=block;}
  var cog=new GameObject("Primary rolling vault door").AddComponent<PortDoor>();cog.transform.SetParent(dynamicRoot.transform);cog.vaultDoor=true;cog.player=player.transform;cog.closedPosition=Vector3.zero;doors[-2]=cog;game.mainDoor=cog;
  var gun=new GameObject("Original 10mm pistol").transform;gun.SetParent(cam.transform,false);player.weapon=gun;
  var roachTemplate=new GameObject("Original radroach artwork");
  int count=0,tris=0;
  foreach(var d in data["meshes"]){int doorId=(int)d["door"];var positions=d["p"].ToObject<float[]>();var ns=d["n"].ToObject<float[]>();var uvs=d["uv"].ToObject<float[]>();var colors=d["c"].ToObject<float[]>();var indices=d["indices"].ToObject<int[]>();var vs=new Vector3[positions.Length/3];var normals=new Vector3[vs.Length];var uv=new Vector2[vs.Length];var cs=new Color[vs.Length];
   for(int j=0;j<vs.Length;j++){vs[j]=new Vector3(positions[j*3],positions[j*3+1],-positions[j*3+2]);if(doors.ContainsKey(doorId))vs[j]-=doors[doorId].transform.position;normals[j]=ns.Length>j*3+2?new Vector3(ns[j*3],ns[j*3+1],-ns[j*3+2]):Vector3.up;uv[j]=uvs.Length>j*2+1?new Vector2(uvs[j*2],uvs[j*2+1]):Vector2.zero;cs[j]=colors.Length>j*3+2?new Color(colors[j*3],colors[j*3+1],colors[j*3+2]):Color.white;}
   for(int j=0;j<indices.Length;j+=3){int temp=indices[j+1];indices[j+1]=indices[j+2];indices[j+2]=temp;}
   string path=Root+"/Meshes/Map_"+count+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);bool exists=mesh;if(!mesh)mesh=new Mesh();mesh.Clear();mesh.indexFormat=IndexFormat.UInt32;mesh.name=(string)d["name"];mesh.vertices=vs;mesh.normals=normals;mesh.uv=uv;mesh.colors=cs;mesh.triangles=indices;mesh.RecalculateBounds();if(!exists)AssetDatabase.CreateAsset(mesh,path);else EditorUtility.SetDirty(mesh);
   var obj=new GameObject("Map_"+count+" "+(string)d["name"]);obj.transform.SetParent(doorId==-3?gun:doorId==-4?roachTemplate.transform:doors.ContainsKey(doorId)?doors[doorId].transform:level.transform,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=obj.AddComponent<MeshRenderer>();renderer.sharedMaterial=materials[(int)d["material"]];renderer.shadowCastingMode=ShadowCastingMode.Off;
   // Original author colliders are used for walls/furniture. Only walkable upward
   // faces become a floor mesh: tiny cables/trim cannot trap the capsule.
   if(doorId==-1){var floorTris=new List<int>();for(int j=0;j<indices.Length;j+=3)if(normals[indices[j]].y>.65f&&normals[indices[j+1]].y>.65f&&normals[indices[j+2]].y>.65f){floorTris.Add(indices[j]);floorTris.Add(indices[j+1]);floorTris.Add(indices[j+2]);}if(floorTris.Count>0){var floor=new Mesh();floor.indexFormat=IndexFormat.UInt32;floor.vertices=vs;floor.triangles=floorTris.ToArray();floor.RecalculateBounds();string fp=Root+"/Meshes/Floor_"+count+".asset";var previous=AssetDatabase.LoadAssetAtPath<Mesh>(fp);if(previous){EditorUtility.CopySerialized(floor,previous);UnityEngine.Object.DestroyImmediate(floor);floor=previous;}else AssetDatabase.CreateAsset(floor,fp);obj.AddComponent<MeshCollider>().sharedMesh=floor;}}
   count++;tris+=indices.Length/3;
  }
  var collidersRoot=new GameObject("Original map collision");int solidCount=0;
  foreach(var d in data["colliders"]){string tag=(string)d["tag"];if(tag=="door")continue;Vector3 a=V(d["min"]),b=V(d["max"]);var box=new GameObject(tag+" "+solidCount++).AddComponent<BoxCollider>();box.transform.SetParent(collidersRoot.transform);box.transform.position=(a+b)/2;box.size=new Vector3(Mathf.Abs(a.x-b.x),Mathf.Abs(a.y-b.y),Mathf.Abs(a.z-b.z));if(d["yaw"]!=null)box.transform.rotation=Quaternion.Euler(0,-(float)d["yaw"]*Mathf.Rad2Deg,0);if(tag=="vault_door")cog.blocker=box;}
  var npcRoot=new GameObject("Vault residents");var interact=new List<PortInteractable>();
  string[] names={"Overseer Harriet Vance","Lucy Vance","Chief Doyle","Dr. Priya Nandakumar","Tommy Ruggiero"};string[] ids={"overseer","lucy","doyle","doctor","tommy"};string[] prefab={"18_VaultOfficer","16_VaultEngineer","04_Security","26_SettlementDoctor","17_VaultMedic"};Vector3[] places={new Vector3(0,.4f,20.9f),new Vector3(1.9f,0,15.4f),new Vector3(5.8f,0,17.9f),new Vector3(-2.3f,0,18.3f),new Vector3(-4.6f,0,14.8f)};
  for(int i=0;i<ids.Length;i++){var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NPCCollection/Prefabs/"+prefab[i]+".prefab");if(!asset)throw new Exception("Missing "+prefab[i]);var npc=(GameObject)PrefabUtility.InstantiatePrefab(asset);npc.name=names[i];npc.transform.SetParent(npcRoot.transform);npc.transform.position=places[i];npc.transform.rotation=Quaternion.LookRotation(new Vector3(0,0,2)-new Vector3(places[i].x,0,places[i].z));var it=npc.AddComponent<PortInteractable>();it.id=ids[i];it.displayName=names[i];it.phase=(i*.213f)%1;interact.Add(it);var col=npc.AddComponent<CapsuleCollider>();col.height=1.7f;col.center=Vector3.up*.85f;col.radius=.3f;col.isTrigger=true;foreach(var r in npc.GetComponentsInChildren<SkinnedMeshRenderer>())r.updateWhenOffscreen=false;}
  interact.Add(Interaction("locker","Zeke's old locker",new Vector3(8.6f,0,-7.7f)));interact.Add(Interaction("terminal","Vault door control",new Vector3(-5.8f,0,50.6f)));interact.Add(Interaction("exit","Exit to Central Park",new Vector3(0,1.4f,85)));
  game.interactions=interact.ToArray();
  var bugs=new List<GameObject>();
  for(int i=0;i<3;i++){var r=UnityEngine.Object.Instantiate(roachTemplate);r.name="Radroach "+(i+1);r.transform.position=new Vector3(-9+Mathf.Cos(i*2*Mathf.PI/3)*1.6f,0,-67-Mathf.Sin(i*2*Mathf.PI/3)*1.6f);var bug=r.AddComponent<PortRoach>();bug.phase=i*2.1f;var hit=r.AddComponent<CapsuleCollider>();hit.direction=2;hit.center=new Vector3(0,.14f,0);hit.radius=.16f;hit.height=.55f;r.SetActive(false);bugs.Add(r);}
  UnityEngine.Object.DestroyImmediate(roachTemplate);game.roaches=bugs.ToArray();gun.localPosition=new Vector3(.22f,-.23f,.43f);gun.gameObject.SetActive(false);
  AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,Root+"/Scenes/Vault120.unity");
  File.WriteAllText(Root+"/BuildReport.json",JsonUtility.ToJson(new Report{meshes=count,triangles=tris,sourceColliders=solidCount,automaticDoors=doors.Count-1,npcs=5,scene=Root+"/Scenes/Vault120.unity"},true));
  return "Vault120 saved: "+count+" meshes, "+tris+" triangles, "+solidCount+" colliders, 16 automatic doors, 5 NPCs.";
 }
 static PortInteractable Interaction(string id,string name,Vector3 p){var obj=new GameObject(name);obj.transform.position=p;var it=obj.AddComponent<PortInteractable>();it.id=id;it.displayName=name;var c=obj.AddComponent<BoxCollider>();c.center=Vector3.up;c.size=new Vector3(.8f,1.8f,.6f);c.isTrigger=true;return it;}
 static Transform Part(Transform parent,PrimitiveType type,string name,Vector3 position,Vector3 scale,Material mat){var obj=GameObject.CreatePrimitive(type);obj.name=name;obj.transform.SetParent(parent,false);obj.transform.localPosition=position;obj.transform.localScale=scale;obj.GetComponent<Renderer>().sharedMaterial=mat;return obj.transform;}
 [Serializable]class Report{public int meshes,triangles,sourceColliders,automaticDoors,npcs;public string scene;}
}



