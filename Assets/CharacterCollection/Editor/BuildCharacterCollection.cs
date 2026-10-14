using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class BuildCharacterCollection
{
 const string Root="Assets/CharacterCollection";
 public static string Build()
 {
    if(EditorSceneManager.GetActiveScene().isDirty)throw new Exception("Save the current scene before building the preview.");
    Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Prefabs");Directory.CreateDirectory(Root+"/Animations");
    AssetDatabase.Refresh();
    string[] names={"Vault_Female","Vault_Male","Mechanic_Female","Mechanic_Male","Scout_Female","Scout_Male","Guard_Female","Guard_Male"};
    var prefabs=new GameObject[8];int totalRenderers=0;
    for(int i=0;i<names.Length;i++)
    {
       string path=Root+"/Models/"+names[i]+".fbx";
       var importer=(ModelImporter)AssetImporter.GetAtPath(path);
       importer.animationType=ModelImporterAnimationType.Generic;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
       importer.importAnimation=true;importer.importCameras=false;importer.importLights=false;importer.isReadable=true;
       var clips=importer.defaultClipAnimations;
       foreach(var clip in clips){clip.loopTime=true;clip.loopPose=true;}
       importer.clipAnimations=clips;importer.SaveAndReimport();
       var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
       if(model==null)throw new Exception("Missing model "+path);
       var root=new GameObject(names[i]);var instance=(GameObject)PrefabUtility.InstantiatePrefab(model);instance.transform.SetParent(root.transform,false);
       foreach(var renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
       {
          totalRenderers++;renderer.updateWhenOffscreen=true;
          var mats=renderer.sharedMaterials;
          for(int j=0;j<mats.Length;j++)
          {
             string name=mats[j].name;string matPath=Root+"/Materials/"+name+".mat";
             var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);
             if(material==null)
             {
                material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.name=name;
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/"+name+"_Albedo.png");
                if(texture==null)texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/"+name+".png");
                material.SetColor("_BaseColor",Color.white);material.SetTexture("_BaseMap",texture);
                material.SetFloat("_Smoothness",name.Contains("Eyes")?.55f:.18f);material.SetFloat("_Metallic",name.Contains("Hardware")?.5f:0);
                material.SetFloat("_Cull",0);
                AssetDatabase.CreateAsset(material,matPath);
             }
             mats[j]=material;
          }
          renderer.sharedMaterials=mats;
       }
       var animator=instance.GetComponentInChildren<Animator>();if(animator==null)animator=instance.AddComponent<Animator>();
       string controllerPath=Root+"/Animations/"+names[i]+".controller";
       var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
       if(controller==null)
       {
          controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
          var anims=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();
          string[] keys={"Idle_Breathe","Idle_Look","Walk_Review"};string[] states={"Idle","Look","Walk"};
          for(int k=0;k<3;k++)
          {
             var clip=anims.FirstOrDefault(c=>c.name.Contains(keys[k]));if(clip==null)throw new Exception("Missing animation "+names[i]+" "+keys[k]);
             var state=controller.layers[0].stateMachine.AddState(states[k]);state.motion=clip;
             if(k==0)controller.layers[0].stateMachine.defaultState=state;
          }
       }
       animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;
       prefabs[i]=PrefabUtility.SaveAsPrefabAsset(root,Root+"/Prefabs/"+names[i]+".prefab");UnityEngine.Object.DestroyImmediate(root);
    }
    string rendererPath=Root+"/StudioRenderer.asset";
    if(AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath)==null)
       AssetDatabase.CopyAsset("Packages/com.unity.render-pipelines.universal/Runtime/Data/UniversalRendererData.asset",rendererPath);
    var rendererData=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
    var pipeline=GraphicsSettings.currentRenderPipeline;
    var so=new SerializedObject(pipeline);var list=so.FindProperty("m_RendererDataList");int rendererIndex=-1;
    for(int i=0;i<list.arraySize;i++)if(list.GetArrayElementAtIndex(i).objectReferenceValue==rendererData)rendererIndex=i;
    if(rendererIndex<0){rendererIndex=list.arraySize;list.InsertArrayElementAtIndex(rendererIndex);list.GetArrayElementAtIndex(rendererIndex).objectReferenceValue=rendererData;so.ApplyModifiedProperties();}
    var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
    scene.name="CharacterCollectionPreview";
    var objects=new GameObject[8];
    for(int i=0;i<8;i++)
    {
       objects[i]=(GameObject)PrefabUtility.InstantiatePrefab(prefabs[i]);objects[i].transform.position=new Vector3(i%2==0?-.55f:.55f,0,0);objects[i].transform.rotation=Quaternion.Euler(0,180,0);objects[i].SetActive(i<2);
    }
    var cameraObject=new GameObject("Preview Camera");var camera=cameraObject.AddComponent<Camera>();cameraObject.tag="MainCamera";
    cameraObject.transform.position=new Vector3(0,1.12f,-3.2f);cameraObject.transform.LookAt(new Vector3(0,.95f,0));camera.fieldOfView=43;camera.nearClipPlane=.05f;
    camera.backgroundColor=new Color(.085f,.1f,.12f);camera.clearFlags=CameraClearFlags.SolidColor;camera.GetUniversalAdditionalCameraData().SetRenderer(rendererIndex);
    cameraObject.AddComponent<AudioListener>();
    var main=new GameObject("Key Light").AddComponent<Light>();main.type=LightType.Directional;main.intensity=1.8f;main.transform.rotation=Quaternion.Euler(35,-25,0);main.shadows=LightShadows.None;
    var fill=new GameObject("Fill Light").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.6f;fill.transform.rotation=Quaternion.Euler(20,150,0);
    RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.4f,.43f,.47f);
    var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);floor.name="Studio Floor";floor.transform.localScale=Vector3.one*.8f;
    var floorMat=new Material(Shader.Find("Universal Render Pipeline/Lit"));floorMat.color=new Color(.12f,.14f,.16f);AssetDatabase.CreateAsset(floorMat,Root+"/Materials/StudioFloor.mat");floor.GetComponent<Renderer>().sharedMaterial=floorMat;
    var preview=new GameObject("Character Review Controls").AddComponent<CharacterCollectionPreview>();preview.characters=objects;preview.previewCamera=camera;
    EditorSceneManager.SaveScene(scene,Root+"/CharacterCollectionPreview.unity");AssetDatabase.SaveAssets();
    Selection.activeGameObject=objects[0];if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.FrameSelected();
    return "Created eight character prefabs, review animations and preview scene; renderers="+totalRenderers;
 }
}
