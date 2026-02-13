using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;

/// <summary>
/// Custom asset importer for HAP video files (.hap)
/// Imports HAP files as TextAsset (binary data) similar to .bytes files
/// </summary>
[ScriptedImporter(1, "hap")]
public class HapAssetImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        // Read the .hap file as binary data
        byte[] hapData = File.ReadAllBytes(ctx.assetPath);
        
        // Create a TextAsset with the binary data (same way as .bytes files)
        // Unity creates TextAsset for .bytes files by passing the binary data directly
        var textAsset = new TextAsset(hapData);
        textAsset.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
        
        // Add the asset to the import context
        ctx.AddObjectToAsset("main", textAsset);
        ctx.SetMainObject(textAsset);
        
        // Log import information
        Debug.Log($"Imported HAP file as TextAsset: {ctx.assetPath} ({hapData.Length} bytes)");
    }
}