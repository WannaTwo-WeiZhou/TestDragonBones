using UnityEditor;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class TilemapAssetBundleBuilder
{
    private static string AB_SOURCE_RELATIVE_PATH = "HotFix/Tilemap/";

    private static Dictionary<string, bool> ImportAssetsStatus = new Dictionary<string, bool>();
    private static List<string> AssetBundleNames = new List<string>();

    [MenuItem("Assets/Tilemap/StartBuild")]
    public static void StartBuild()
    {
        ImportAssetsStatus.Clear();
        AssetBundleNames.Clear();

        // copy to Assets/HotFix/Tilemap
        // 最终在外部做
        var targetPath = Path.Combine(Application.dataPath, AB_SOURCE_RELATIVE_PATH);
        //if (Directory.Exists(targetPath))
        //{
        //    Directory.Delete(targetPath, true);
        //}
        //BuildTarget bt = GetBuildTarget();
        //var br = GetBundleRoot(bt);
        //if (Directory.Exists(br))
        //{
        //    Directory.Delete(br, true);
        //}
        //Directory.CreateDirectory(targetPath);
        //var sourcePath = Path.Combine(Application.dataPath, "../Test_TooqingEditor/");
        //CopyDirectory(sourcePath, targetPath);

        // import assets
        DirectoryInfo targetInfo = new DirectoryInfo(targetPath);
        FileInfo[] files = targetInfo.GetFiles();
        List<string> imports = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.EndsWith(".meta")) continue;
            ImportAssetsStatus[files[i].Name] = false;
            var fileRelativePath = Path.Combine("Assets/", Path.Combine(AB_SOURCE_RELATIVE_PATH, files[i].Name));
            Debug.Log("TilemapAssetBundleBuilder import asset: " + fileRelativePath);
            imports.Add(fileRelativePath);
        }
        foreach (var path in imports)
        {
            AssetDatabase.ImportAsset(path);
        }

        // wait for "OnPostprocessAllAssets"
    }

    public static void AssetImported(string assetPath)
    {
        FileInfo info = new FileInfo(assetPath);
        if (!ImportAssetsStatus.ContainsKey(info.Name)) return;
        Debug.Log("TilemapAssetBundleBuilder asset imported: " + assetPath);

        if (info.Name.EndsWith("-map.tmx"))
        {
            var bundleName = "tilemap/" + info.Name.Replace("-map.tmx", "");
            AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(bundleName, "");
            AssetBundleNames.Add(bundleName);
        }
        ImportAssetsStatus[info.Name] = true;

        bool allImported = true;
        foreach (var item in ImportAssetsStatus)
        {
            if (!item.Value)
            {
                allImported = false;
                break;
            }
        }

        if (allImported)
        {
            ImportAssetsStatus.Clear();
            BuildAssetBundles();
        }
    }

    private static void BuildAssetBundles()
    {
        Debug.Log("TilemapAssetBundleBuilder BuildAssetBundles()");
        BuildTarget bt = GetBuildTarget();
        var br = GetBundleRoot(bt);

        AssetDatabase.RemoveUnusedAssetBundleNames();
        var assetBundleBuildList = new List<AssetBundleBuild>();

        foreach (var name in AssetBundleNames)
        {
            Debug.Log("TilemapAssetBundleBuilder build asset bundle: " + name);
            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(name);
            AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
            assetBundleBuild.assetBundleName = name + ".ab";
            assetBundleBuild.assetBundleVariant = null;
            assetBundleBuild.assetNames = assetPaths;
            assetBundleBuildList.Add(assetBundleBuild);
        }
        AssetBundleNames.Clear();

        var result = BuildPipeline.BuildAssetBundles(br, assetBundleBuildList.ToArray(), BuildAssetBundleOptions.StrictMode, bt);
        Debug.Log("TilemapAssetBundleBuilder finish");
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
        //var p = Path.Combine(Application.dataPath, "../TempStreamingAssets/" + GetBundleFolderName(bt));
        var p = Path.Combine(Application.dataPath, "../TempStreamingAssets");
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
            TilemapAssetBundleBuilder.AssetImported(imported);
        }
    }
}