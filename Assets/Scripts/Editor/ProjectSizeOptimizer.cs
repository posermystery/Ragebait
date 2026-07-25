using UnityEngine;
using UnityEditor;

public class ProjectSizeOptimizer : EditorWindow
{
    // Option 1: Top menu bar
    [MenuItem("Ragebait Tools/1-Click Optimize Game Size (Mobile)")]
    // Option 2: Right-click inside Assets folder!
    [MenuItem("Assets/Optimize Game Size (Mobile)", false, 20)]
    public static void OptimizeProject()
    {
        if (!EditorUtility.DisplayDialog("Optimize Project Size", 
            "This will automatically compress all Textures, Sprites, and Audio clips in your project for minimum Android APK size WITHOUT changing any visual object sizes or scene layouts.\n\nDo you want to proceed?", 
            "Yes, Optimize Now", "Cancel"))
        {
            return;
        }

        int texturesUpdated = OptimizeTextures();
        int audioUpdated = OptimizeAudio();
        OptimizePlayerSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Optimization Complete!", 
            $"Successfully optimized your game for minimal mobile size!\n\n• Textures/Sprites Compressed: {texturesUpdated}\n• Audio Clips Compressed (Mono/Vorbis): {audioUpdated}\n• Player Settings: Code Stripping Enabled\n\nYour scene object sizes and scales were 100% untouched!", 
            "Awesome!");
    }

    private static int OptimizeTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // Skip Unity Editor internal icons or package files
            if (path.StartsWith("Packages/") || path.Contains("/Editor/")) continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;

                // 1. Disable Read/Write Enabled (Saves 50% RAM in mobile memory!)
                if (importer.isReadable)
                {
                    importer.isReadable = false;
                    changed = true;
                }

                // 2. Disable Mip Maps for 2D Sprites/UI (Saves ~33% file storage per sprite!)
                if (importer.textureType == TextureImporterType.Sprite || importer.textureType == TextureImporterType.GUI)
                {
                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;
                        changed = true;
                    }
                }

                // 3. Set Mobile Android Compression Settings
                TextureImporterPlatformSettings androidSettings = importer.GetPlatformTextureSettings("Android");
                if (!androidSettings.overridden || androidSettings.maxTextureSize > 1024)
                {
                    androidSettings.overridden = true;
                    androidSettings.maxTextureSize = 1024; // High-def enough for mobile without taking huge MB
                    androidSettings.textureCompression = TextureImporterCompression.Compressed; // Auto-selects best mobile compression
                    androidSettings.compressionQuality = 50;
                    importer.SetPlatformTextureSettings(androidSettings);
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                    count++;
                }
            }
        }
        return count;
    }

    private static int OptimizeAudio()
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith("Packages/") || path.Contains("/Editor/")) continue;

            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer != null)
            {
                bool changed = false;

                // 1. Force To Mono (Instantly cuts audio file size in HALF! Phone speakers are mostly mono/stereo anyway)
                if (!importer.forceToMono)
                {
                    importer.forceToMono = true;
                    changed = true;
                }

                // 2. Set Vorbis Compression
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                if (settings.compressionFormat != AudioCompressionFormat.Vorbis || settings.quality > 0.7f)
                {
                    settings.compressionFormat = AudioCompressionFormat.Vorbis;
                    settings.quality = 0.65f; // Great sound quality, super tiny storage footprint
                    importer.defaultSampleSettings = settings;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                    count++;
                }
            }
        }
        return count;
    }

    private static void OptimizePlayerSettings()
    {
        // 1. Strip unused C# engine code to make APK significantly smaller
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);
        
        Debug.Log("<b>[ProjectSizeOptimizer]</b> Player Settings updated: Managed Stripping Level set to Medium for minimal APK size!");
    }
}
