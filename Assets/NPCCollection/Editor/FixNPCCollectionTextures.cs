using UnityEngine;
using UnityEditor;
using System.IO;
public static class FixNPCCollectionTextures
{
 public static string Run(){
  string root="Assets/NPCCollection";int count=0;
  foreach(var guid in AssetDatabase.FindAssets("t:Texture2D",new[]{root+"/Textures"})){
   string path=AssetDatabase.GUIDToAssetPath(guid);var t=AssetImporter.GetAtPath(path) as TextureImporter;if(t==null)continue;
   if(t.textureType!=TextureImporterType.Default||!t.mipmapEnabled||t.filterMode!=FilterMode.Trilinear){t.textureType=TextureImporterType.Default;t.mipmapEnabled=true;t.filterMode=FilterMode.Trilinear;t.anisoLevel=4;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.CompressedHQ;t.SaveAndReimport();}count++;
   File.WriteAllText(Path.Combine(Application.dataPath,"../Logs/NPCTextureFix.log"),"Textures "+count);
  }
  Color[] tones={new Color(1,.98f,.96f),new Color(.78f,.64f,.54f),new Color(.53f,.36f,.28f),new Color(.93f,.81f,.68f),new Color(.68f,.52f,.40f),new Color(.98f,.92f,.85f),new Color(.85f,.73f,.65f),new Color(.60f,.44f,.34f)};
  foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{root+"/Materials"})){
   var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));int id;
   if(m.name.EndsWith("_Skin")&&int.TryParse(m.name.Substring(0,2),out id)){m.SetColor("_BaseColor",tones[(id*3)%8].gamma);EditorUtility.SetDirty(m);}
  }
  AssetDatabase.SaveAssets();File.WriteAllText(Path.Combine(Application.dataPath,"../Logs/NPCTextureFix.log"),"COMPLETE "+count);return "Fixed "+count+" textures and skin material color conversion";
 }
}
