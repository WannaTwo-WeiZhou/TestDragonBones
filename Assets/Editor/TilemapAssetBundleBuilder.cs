using UnityEditor;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class TilemapAssetBundleBuilder
{
    private static string AB_SOURCE_RELATIVE_PATH = "HotFix/Tilemap/";

    [MenuItem("Assets/Tilemap/StartBuild")]
    public static void StartBuild()
    {
        // copy to Assets/HotFix/Tilemap
        // 最终在外部做
        var targetPath = Path.Combine(Application.dataPath, AB_SOURCE_RELATIVE_PATH);
        if (Directory.Exists(targetPath))
        {
            Directory.Delete(targetPath, true);
        }
        Directory.CreateDirectory(targetPath);
        var sourcePath = Path.Combine(Application.dataPath, "../Test_TooqingEditor/");
        CopyDirectory(sourcePath, targetPath);

        // import assets
        DirectoryInfo targetInfo = new DirectoryInfo(targetPath);
        FileInfo[] files = targetInfo.GetFiles();
        for (int i = 0; i < files.Length; i++)
        {
            var fileRelativePath = Path.Combine("Assets/", Path.Combine(AB_SOURCE_RELATIVE_PATH, files[i].Name));
            AssetDatabase.ImportAsset(fileRelativePath);
        }
        
        // wait for "OnPostprocessAllAssets"
    }

    public static void SetBundleName(string assetPath)
    {
        FileInfo info = new FileInfo(assetPath);
        if (!info.Name.EndsWith("-map.tmx")) return;
        var bundleName = "tilemap/" + info.Name.Replace("-map.tmx", "");
        AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(bundleName, "");
    }

    public static void BuildAssetBundles()
    {
        BuildTarget bt = GetBuildTarget();
        var br = GetBundleRoot(bt);

        AssetDatabase.RemoveUnusedAssetBundleNames();
        string[] names = AssetDatabase.GetAllAssetBundleNames();
        var assetBundleBuildList = new List<AssetBundleBuild>();

        foreach (var name in names)
        {
            if (!name.StartsWith("tilemap")) continue;
            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(name);
            AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
            assetBundleBuild.assetBundleName = name + ".ab";
            assetBundleBuild.assetBundleVariant = null;
            assetBundleBuild.assetNames = assetPaths;
            assetBundleBuildList.Add(assetBundleBuild);
        }

        var result = BuildPipeline.BuildAssetBundles(br, assetBundleBuildList.ToArray(), BuildAssetBundleOptions.StrictMode, bt);
    }

    private static void CopyDirectory(string srcDir, string tgtDir)
    {
        DirectoryInfo source = new DirectoryInfo(srcDir);
        DirectoryInfo target = new DirectoryInfo(tgtDir);

        if (target.FullName.StartsWith(source.FullName, System.StringComparison.CurrentCultureIgnoreCase))
        {
            Debug.LogError("父目录不能拷贝到子目录！");
            return;
        }

        if (!source.Exists)
        {
            return;
        }

        if (!target.Exists)
        {
            target.Create();
        }

        FileInfo[] files = source.GetFiles();

        for (int i = 0; i < files.Length; i++)
        {
            File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
        }

        DirectoryInfo[] dirs = source.GetDirectories();

        for (int j = 0; j < dirs.Length; j++)
        {
            CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
        }
    }

    private static string GetBundleRoot(BuildTarget bt)
    {
        var p = Path.Combine(Application.dataPath, "../TempStreamingAssets/" + GetBundleFolderName(bt));
        if (!Directory.Exists(p))
        {
            Directory.CreateDirectory(p);
        }
        return p;
    }

    private static BuildTarget GetBuildTarget()
    {
        BuildTarget bt = BuildTarget.iOS;
#if UNITY_ANDROID
        bt = BuildTarget.Android;
#elif UNITY_IPHONE
		bt=BuildTarget.iOS;	
#elif UNITY_STANDALONE_WIN
        //bt = BuildTarget.StandaloneWindows;
        bt = BuildTarget.StandaloneWindows64;
#elif UNITY_STANDALONE_OSX
        bt = BuildTarget.StandaloneOSX;
#else
        bt = BuildTarget.WebGL;
#endif
        return bt;
    }
    private static string GetBundleFolderName(BuildTarget bt)
    {
        if (bt == BuildTarget.Android)
        {
            return "Android";
        }
        else if (bt == BuildTarget.iOS)
        {
            return "IOS";
        }
        else if (bt == BuildTarget.StandaloneWindows
            || bt == BuildTarget.StandaloneWindows64)
        {
            return "Windows";
        }
        else if (bt == BuildTarget.StandaloneOSX)
        {
            return "MacOS";
        }
        else if (bt == BuildTarget.WebGL)
        {
            return "WebGL";
        }
        else
        {
            return "Web";
        }
    }
}

public class PostProcessImportAsset : AssetPostprocessor
{
    //Based on this example, the output from this function should be:
    //  OnPostprocessAllAssets
    //  Imported: Assets/Artifacts/test_file01.txt
    //
    //test_file02.txt should not even show up on the Project Browser
    //until a refresh happens.
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        //Debug.Log("OnPostprocessAllAssets");

        foreach (var imported in importedAssets)
        {
            Debug.Log("Imported: " + imported);
            if (imported.EndsWith("-map.tmx"))
                TilemapAssetBundleBuilder.SetBundleName(imported);
        }

        TilemapAssetBundleBuilder.BuildAssetBundles();
    }
}