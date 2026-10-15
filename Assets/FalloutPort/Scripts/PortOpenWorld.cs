using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace FalloutPort {
// Original city geometry is streamed as native Unity meshes. No web renderer is used.
public class PortOpenWorld : MonoBehaviour {
 [Serializable] public class Node { public string name;public int geometry;public int[] materials;public float[] matrix; }
 [Serializable] public class Cell { public string id,layer,collider;public float[] center,size,lodCenter;public Node[] nodes;[NonSerialized] public bool loaded;[NonSerialized] public GameObject root;[NonSerialized] public List<int> references=new List<int>();[NonSerialized] public List<Mesh> combined=new List<Mesh>(); }
 [Serializable] public class Surface {public string name,texturePath;public float[] color,emission;public bool vertexColors,repeat,unlit,flipY=true,transparent;public float[] textureScale,textureOffset;public float alpha,cutoff;}
 [Serializable] public class Position {public float x,y,z,yaw;}
 [Serializable] public class Entrance {public string id,name,keyId;public Position pos,target;public int locked;}
 [Serializable] public class Container {public string id,name,owner,loot;public Position pos;public int locked;public PortItemStack[] items;}
 [Serializable] public class Location {public string id,name,type;public float x,z,r;public int level;}
 [Serializable] public class Manifest {public PortItemDefinition[] items;public Cell[] chunks;public Surface[] materials;public Entrance[] doors;public Container[] containers;public Location[] locations;}
 class MeshRef {public Mesh mesh;public int uses;}
 public bool outside,loading;public string locationName="Central Park";public Vector3 lastSafe=new Vector3(-65,.1f,708);
 public Manifest manifest;PortGame game;GameObject worldRoot;GameObject[] vaultRoots;PortInteractable[] vaultInteractions,worldInteractions;
 readonly Dictionary<int,MeshRef> geometry=new Dictionary<int,MeshRef>();readonly Dictionary<int,Material> materials=new Dictionary<int,Material>();
 readonly Dictionary<GameObject,bool> vaultActive=new Dictionary<GameObject,bool>();Coroutine stream;float nextLocation;Color oldFogColor;bool oldFog;float oldFogDensity;CameraClearFlags oldClear;Color oldBackground;float oldFar;
 public static Vector3 Vec(float[] a){return new Vector3(a[0],a[1],a[2]);}
 public static Vector3 Point(Position p){return new Vector3(p.x,p.y,-p.z);}
 public void Initialize(){
  if(game)return;game=GetComponent<PortGame>();vaultInteractions=game.interactions;
  vaultRoots=game.gameObject.scene.GetRootGameObjects().Where(o=>o!=game.gameObject&&o!=game.player.gameObject&&!o.GetComponent<Light>()&&!o.GetComponent<Canvas>()&&!o.GetComponent<UnityEngine.EventSystems.EventSystem>()).ToArray();
 }
 bool ReadManifest(){if(manifest!=null)return true;var data=Resources.Load<TextAsset>("OpenWorld/manifest");if(!data){game.notice="The world package is not installed yet.";return false;}manifest=JsonUtility.FromJson<Manifest>(data.text);Resources.UnloadAsset(data);return manifest!=null;}
 public void Travel(Vector3 destination,float yaw=180){Initialize();if(loading)return;if(!ReadManifest())return;StartCoroutine(Arrive(destination,yaw));}
 IEnumerator Arrive(Vector3 destination,float yaw){
  loading=true;game.player.enabled=false;game.player.GetComponent<CharacterController>().enabled=false;
  if(!outside){foreach(var root in vaultRoots)if(root)vaultActive[root]=root.activeSelf;oldFog=RenderSettings.fog;oldFogColor=RenderSettings.fogColor;oldFogDensity=RenderSettings.fogDensity;oldClear=game.player.view.clearFlags;oldBackground=game.player.view.backgroundColor;oldFar=game.player.view.farClipPlane;}
  outside=true;foreach(var root in vaultRoots)if(root)root.SetActive(false);
  if(!worldRoot){worldRoot=new GameObject("Native Manhattan");BuildInteractions();}
  worldRoot.SetActive(true);game.interactions=worldInteractions;game.player.transform.position=destination;
  RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.45f,.48f,.43f);RenderSettings.fogDensity=.0015f;
  game.player.view.clearFlags=CameraClearFlags.SolidColor;game.player.view.backgroundColor=RenderSettings.fogColor;game.player.view.farClipPlane=1400;
  if(stream!=null){StopCoroutine(stream);stream=null;}
  foreach(var cell in manifest.chunks.Where(c=>c.root&&(!c.loaded||!Wanted(c,destination))))Unload(cell);
  var pending=manifest.chunks.Where(c=>Wanted(c,destination)&&!c.root&&(Distance(c,destination)<65||c.layer=="always")).OrderBy(c=>Distance(c,destination)).ToArray();
  for(int i=0;i<pending.Length;i++){game.notice="Loading "+(i+1)+" / "+pending.Length+" nearby world sections";yield return Load(pending[i]);}
  Physics.SyncTransforms();game.player.Teleport(destination,yaw);lastSafe=destination;game.player.enabled=true;loading=false;game.notice="Central Park — follow Zeke's trail toward Grand Central.";
  game.FindInteraction();stream=StartCoroutine(Stream());
 }
 public void ReturnToVault(Vector3 position,float yaw){Initialize();if(stream!=null)StopCoroutine(stream);stream=null;StopAllCoroutines();loading=false;
  if(worldRoot)worldRoot.SetActive(false);if(manifest!=null)foreach(var c in manifest.chunks)if(c.root)Unload(c);
  foreach(var root in vaultRoots)if(root)root.SetActive(vaultActive.TryGetValue(root,out var active)?active:true);game.interactions=vaultInteractions;
  if(outside){RenderSettings.fog=oldFog;RenderSettings.fogColor=oldFogColor;RenderSettings.fogDensity=oldFogDensity;game.player.view.clearFlags=oldClear;game.player.view.backgroundColor=oldBackground;game.player.view.farClipPlane=oldFar;}
  outside=false;game.player.enabled=true;game.player.Teleport(position,yaw);Physics.SyncTransforms();
 }
 public void Restore(bool surface,Vector3 position,float yaw){Initialize();if(surface)Travel(position,yaw);else if(outside||loading)ReturnToVault(position,yaw);}
 float Distance(Cell c,Vector3 p){return Mathf.Sqrt(new Bounds(Vec(c.center),Vec(c.size)).SqrDistance(p));}
 bool Wanted(Cell c,Vector3 p){
  float d=Distance(c,p);if(!string.IsNullOrEmpty(c.collider))return d<160;
  if(c.layer=="always"||c.layer=="far")return true;
  if(c.layer=="imp")return Vector2.Distance(new Vector2(c.lodCenter[0],c.lodCenter[2]),new Vector2(p.x,p.z))>560&&d<1300;
  if(c.layer=="main")return Vector2.Distance(new Vector2(c.lodCenter[0],c.lodCenter[2]),new Vector2(p.x,p.z))<=560;
  if(c.layer=="mid")return d<900;if(c.layer=="detail")return d<300;if(c.layer=="fine")return d<150;if(c.layer=="near")return d<330;
  return d<350;
 }
 IEnumerator Stream(){while(outside){var p=game.player.transform.position;
   foreach(var c in manifest.chunks)if(c.root&&!Wanted(c,p))Unload(c);
   foreach(var c in manifest.chunks.Where(c=>!c.root&&Wanted(c,p)).OrderBy(c=>Distance(c,p)).ToArray()){if(!outside)yield break;yield return Load(c);}
   yield return new WaitForSecondsRealtime(.35f);
  }}
 Material GetMaterial(int id){if(materials.TryGetValue(id,out var m))return m;var s=manifest.materials[id];var template=Resources.Load<Material>("OpenWorldSurface");m=template?new Material(template):new Material(Shader.Find("FalloutPort/WorldSurface"));m.name=s.name;m.SetColor("_BaseColor",new Color(s.color[0],s.color[1],s.color[2],s.alpha));m.SetColor("_Emission",new Color(s.emission[0],s.emission[1],s.emission[2]));m.SetFloat("_VertexColor",s.vertexColors?1:0);m.SetFloat("_Unlit",s.unlit?1:0);if(s.transparent&&s.alpha<.99f){m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",10);m.SetFloat("_ZWrite",0);m.renderQueue=3000;}m.SetFloat("_Cutoff",Mathf.Max(.05f,s.cutoff));if(!string.IsNullOrEmpty(s.texturePath)){var t=Resources.Load<Texture2D>("OpenWorld/Textures/"+s.texturePath);if(t){t.wrapMode=s.repeat?TextureWrapMode.Repeat:TextureWrapMode.Clamp;m.SetTexture("_BaseMap",t);var scale=s.textureScale!=null?new Vector2(s.textureScale[0],s.textureScale[1]):Vector2.one;var offset=s.textureOffset!=null?new Vector2(s.textureOffset[0],s.textureOffset[1]):Vector2.zero;if(!s.flipY){scale.y=-scale.y;offset.y=1-offset.y;}m.SetTextureScale("_BaseMap",scale);m.SetTextureOffset("_BaseMap",offset);}}materials[id]=m;return m;}
 static Matrix4x4 Matrix(float[] a){var m=new Matrix4x4();for(int i=0;i<16;i++)m[i]=a[i];return m;}
 public static Mesh Decode(byte[] bytes,string name){using(var memory=new MemoryStream(bytes))using(var zip=new GZipStream(memory,CompressionMode.Decompress))using(var r=new BinaryReader(zip)){
   int vc=r.ReadInt32(),ic=r.ReadInt32(),gc=r.ReadInt32();if(vc<0||vc>10000000||ic<0||ic>60000000||gc<1||gc>1000)throw new InvalidDataException("Invalid world mesh "+name);
   var v=new Vector3[vc];var n=new Vector3[vc];var uv=new Vector2[vc];var color=new Color32[vc];var ix=new int[ic];
   for(int i=0;i<vc;i++)v[i]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());for(int i=0;i<vc;i++)n[i]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());for(int i=0;i<vc;i++)uv[i]=new Vector2(r.ReadSingle(),r.ReadSingle());for(int i=0;i<vc;i++)color[i]=new Color32(r.ReadByte(),r.ReadByte(),r.ReadByte(),r.ReadByte());for(int i=0;i<ic;i++)ix[i]=r.ReadInt32();
   var groups=new int[gc][];int submeshes=1;for(int i=0;i<gc;i++){groups[i]=new[]{r.ReadInt32(),r.ReadInt32(),r.ReadInt32()};submeshes=Math.Max(submeshes,groups[i][2]+1);}
   var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.vertices=v;mesh.normals=n;mesh.uv=uv;mesh.colors32=color;mesh.subMeshCount=submeshes;
   for(int sub=0;sub<submeshes;sub++){var triangles=new List<int>();foreach(var g in groups)if(g[2]==sub)for(int j=g[0];j<g[0]+g[1];j++)triangles.Add(ix[j]);mesh.SetTriangles(triangles,sub,false);}mesh.RecalculateBounds();return mesh;
  }}
 IEnumerator Load(Cell c){
  c.loaded=false;c.root=new GameObject(c.id);c.root.transform.SetParent(worldRoot.transform);c.references=new List<int>();c.combined=new List<Mesh>();
  if(!string.IsNullOrEmpty(c.collider)){var request=Resources.LoadAsync<TextAsset>("OpenWorld/Geometry/"+c.collider);yield return request;var asset=request.asset as TextAsset;if(!asset)throw new FileNotFoundException(c.collider);var mesh=Decode(asset.bytes,c.collider);Resources.UnloadAsset(asset);c.combined.Add(mesh);c.root.AddComponent<MeshCollider>().sharedMesh=mesh;c.loaded=true;yield break;}
  var unique=c.nodes.Select(n=>n.geometry).Distinct();
  foreach(int id in unique){if(!geometry.TryGetValue(id,out var reference)){var request=Resources.LoadAsync<TextAsset>("OpenWorld/Geometry/g"+id);yield return request;var asset=request.asset as TextAsset;if(!asset)throw new FileNotFoundException("World geometry "+id);reference=new MeshRef{mesh=Decode(asset.bytes,"World "+id)};Resources.UnloadAsset(asset);geometry[id]=reference;}reference.uses++;c.references.Add(id);}
  int built=0;foreach(var batch in c.nodes.GroupBy(n=>n.geometry+":"+string.Join(",",n.materials))){var nodes=batch.ToArray();var mesh=geometry[nodes[0].geometry].mesh;var mats=nodes[0].materials.Select(GetMaterial).ToArray();
   if(nodes.Length>=4&&mesh.subMeshCount==1){int capacity=Mathf.Max(1,100000/Mathf.Max(1,mesh.vertexCount));for(int offset=0;offset<nodes.Length;offset+=capacity){int count=Math.Min(capacity,nodes.Length-offset);var combine=new CombineInstance[count];for(int i=0;i<count;i++)combine[i]=new CombineInstance{mesh=mesh,transform=Matrix(nodes[offset+i].matrix)};var merged=new Mesh{name="Batched "+nodes[0].name,indexFormat=IndexFormat.UInt32};merged.CombineMeshes(combine,true,true);c.combined.Add(merged);Render(c,merged,mats,Matrix4x4.identity,nodes[0].name);yield return null;}}
   else foreach(var node in nodes){Render(c,mesh,mats,Matrix(node.matrix),node.name);if(++built%12==0)yield return null;}
  }
  c.loaded=true;
 }
 void Render(Cell c,Mesh mesh,Material[] mats,Matrix4x4 matrix,string name){var o=new GameObject(name);o.transform.SetParent(c.root.transform);o.transform.position=matrix.GetColumn(3);o.transform.rotation=matrix.rotation;o.transform.localScale=matrix.lossyScale;o.AddComponent<MeshFilter>().sharedMesh=mesh;var r=o.AddComponent<MeshRenderer>();r.sharedMaterials=mats;r.shadowCastingMode=ShadowCastingMode.Off;
  if(name!=null&&(name.StartsWith("map:asphalt")||name.StartsWith("map:sidewalk")||name.StartsWith("map:grass_dead")||name.StartsWith("map:dirt")))o.AddComponent<MeshCollider>().sharedMesh=mesh;
 }
 void Unload(Cell c){if(c.root)Destroy(c.root);c.root=null;c.loaded=false;foreach(int id in c.references)if(geometry.TryGetValue(id,out var m)&&--m.uses<=0){Destroy(m.mesh);geometry.Remove(id);}c.references.Clear();foreach(var m in c.combined)Destroy(m);c.combined.Clear();}
 void BuildInteractions(){var list=new List<PortInteractable>();foreach(var d in manifest.doors){if(d.target==null||d.pos==null||d.name==null)continue;var o=new GameObject("Entrance "+d.name);o.transform.SetParent(worldRoot.transform);o.transform.position=Point(d.pos)-Vector3.up;var p=o.AddComponent<PortWorldPortal>();p.entrance=d;p.world=this;var it=o.AddComponent<PortInteractable>();it.id="portal:"+d.id;it.displayName=d.name+(d.locked>0?" [Locked]":"");list.Add(it);}
  foreach(var d in manifest.containers){if(d.pos==null)continue;var o=new GameObject("Salvage "+d.name);o.transform.SetParent(worldRoot.transform);o.transform.position=Point(d.pos)-Vector3.up;var loot=o.AddComponent<PortCityContainer>();loot.data=d;loot.game=game;var it=o.AddComponent<PortInteractable>();it.id="cityloot:"+d.id;it.displayName=d.name+(d.locked>0?" [Locked]":"");list.Add(it);}
  worldInteractions=list.ToArray();game.interactions=worldInteractions;
 }
 public void Use(Entrance d){game.CloseDialogue();if(d.name=="Vault 120"){ReturnToVault(new Vector3(0,.1f,81.4f),180);return;}var story=GetComponent<PortQuestRuntime>();if(story.initialized){story.UseDoor(d.id);return;}if(d.locked>0){game.notice="Locked — requires a key or lockpicking.";return;}Travel(Point(d.target)+Vector3.up*.08f,-d.target.yaw*Mathf.Rad2Deg);locationName=d.name;}

 void Update(){if(!outside||loading)return;var p=game.player.transform.position;if(game.player.GetComponent<CharacterController>().isGrounded)lastSafe=p;
  if(Time.unscaledTime<nextLocation)return;nextLocation=Time.unscaledTime+1;var closest=manifest.locations.Where(l=>Vector2.Distance(new Vector2(p.x,p.z),new Vector2(l.x,-l.z))<l.r).OrderBy(l=>Vector2.Distance(new Vector2(p.x,p.z),new Vector2(l.x,-l.z))).FirstOrDefault();if(closest!=null&&closest.name!=locationName){locationName=closest.name;game.notice="Discovered: "+locationName;}
 }
 void OnGUI(){if(!loading)return;GUI.Box(new Rect(Screen.width/2-220,Screen.height/2-45,440,90),"ENTERING MANHATTAN\n\n"+game.notice);}
 void OnDestroy(){foreach(var m in geometry.Values)if(m.mesh)Destroy(m.mesh);foreach(var m in materials.Values)if(m)Destroy(m);if(worldRoot)Destroy(worldRoot);}
}
public class PortWorldPortal:MonoBehaviour {public PortOpenWorld world;public PortOpenWorld.Entrance entrance;public void Open(){world.Use(entrance);}}
}
