using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
public static class BuildWindowsPort {
 public static string Output="C:/Users/camde/Documents/FalloutNYC-Windows/Fallout NYC.exe";
 public static void Queue(){EditorApplication.delayCall+=Build;}
 static void Build(){
  string log="Logs/WindowsBuild.txt";
  try{
   Directory.CreateDirectory(Path.GetDirectoryName(Output));
   PlayerSettings.productName="Fallout NYC";
   var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/FalloutPort/Scenes/Vault120.unity"},locationPathName=Output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
   File.WriteAllText(log,result.summary.result+"\nErrors: "+result.summary.totalErrors+"\nBytes: "+result.summary.totalSize+"\nOutput: "+Output);
  }catch(System.Exception e){File.WriteAllText(log,"FAILED\n"+e);}
 }
}
