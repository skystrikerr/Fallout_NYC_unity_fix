using UnityEditor;
using UnityEngine;
public class WorldTextureImport : AssetPostprocessor {
 void OnPreprocessTexture(){if(!assetPath.StartsWith("Assets/Resources/OpenWorld/Textures/"))return;var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=true;importer.alphaIsTransparency=true;importer.anisoLevel=4;importer.filterMode=FilterMode.Trilinear;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Compressed;}
}
