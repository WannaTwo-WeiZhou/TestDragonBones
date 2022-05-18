using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System;

[Serializable]
public class FrameMetaData
{
    public int x;
    public int y;
    public int w;
    public int h;
}

[Serializable]
public class Frame
{
    public string filename;
    public FrameMetaData frame;
    public bool rotated;
    public bool trimmed;
    public object spriteSourceSize;
    public object sourceSize;
}

[Serializable]
public class ElementFrames
{
    public Frame[] frames;
    public object size;
}

public class BuildAnimationFromElement : Editor
{
    // 生成出的Prefab的路径
    private static string PrefabPath = "Assets/Prefabs";
    // 生成出的AnimationController的路径
    private static string AnimationControllerPath = "Assets/AnimationController";
    // 生成出的Animation的路径
    private static string AnimationPath = "Assets/Animation";
    // 原始图片路径
    private static string RawPath = "Assets/Raw";

    [MenuItem("Build/BuildAnimationFromElement")]
    private static void GenerateAnimation()
    {
        DirectoryInfo raw = new DirectoryInfo(RawPath);
        foreach (DirectoryInfo directorys in raw.GetDirectories())
        {
            Debug.Log("Directorys Name: " + directorys.Name); // 61f24706540dc200218f8781
            var sn = directorys.Name; // 物件sn
            List<AnimationClip> clips = new List<AnimationClip>();
            foreach (DirectoryInfo elementVersions in directorys.GetDirectories())
            {
                var version = elementVersions.Name;

                // 解析pi文件
                var capsule = new GameCapsule.Capsule();
                // Calling the ReadAllBytes() function
                byte[] pi = File.ReadAllBytes(RawPath + "/" + sn + "/" + version + "/" + sn + ".pi");
                capsule.deserialize(pi);
                Debug.Log("Capsule: " + capsule);

                GameCapsule.ConfigObjects.ElementNode element = (GameCapsule.ConfigObjects.ElementNode)capsule.root.children[0];
                // GameCapsule.ConfigObjects.AnimationsNode[] animations = element.animations;


                // 解析png + json文件
                string jsonStr = File.ReadAllText(RawPath + "/" + sn + "/" + version + "/" + sn + ".json");
                Debug.Log("jsonStr: " + jsonStr);
                ElementFrames ret = JsonUtility.FromJson<ElementFrames>(jsonStr);
                Debug.Log("Ret: " + JsonUtility.ToJson(ret.frames));

                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(RawPath + "/" + sn + "/" + version + "/" + sn + ".png");
                Debug.Log("SpriteSheet: " + texture);
                string path = AssetDatabase.GetAssetPath(texture);
                Debug.Log("SpriteSheet path: " + path);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                Debug.Log("TextureImporter: ", importer);

                List<SpriteMetaData> metaData = new List<SpriteMetaData>();

                foreach (Frame frame in ret.frames)
                {
                    //Debug.Log("Frame name: " + frame.filename);
                    SpriteMetaData smd = new SpriteMetaData();
                    smd.pivot = new Vector2(0.5f, 0.5f);
                    smd.alignment = 0;
                    smd.name = frame.filename;
                    smd.rect = new Rect(frame.frame.x, frame.frame.y, frame.frame.w, frame.frame.h);
                    metaData.Add(smd);
                }

                importer.spritesheet = metaData.ToArray();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);


                if (!File.Exists(AnimationPath + "/" + sn + "_" + version + ".anim"))
                {
                    // 每个文件夹就是一组帧动画，把每个文件夹下的一组图片生成一个动画文件
                    clips.Add(BuildAnimationClip(elementVersions, sn, version));
                }

                // 把所有的动画文件生成在一个AnimationController里
                if (!File.Exists(AnimationControllerPath + "/" + sn + "_" + version + ".controller"))
                {

                    UnityEditor.Animations.AnimatorController controller = BuildAnimationController(clips, sn, version);

                    // 生成程序用的Prefab文件
                    if (!File.Exists(PrefabPath + "/" + sn + "_" + version + ".prefab"))
                    {
                        BuildPrefab(directorys, sn, version, controller);
                    }
                }
            }

        }
    }

    static AnimationClip BuildAnimationClip(DirectoryInfo directorys, string sn, string version)
    {
        string animationName = sn + "_" + version;
        // 查找所有图片
        FileInfo[] images = directorys.GetFiles("*.png");
        AnimationClip clip = new AnimationClip();
        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.type = typeof(SpriteRenderer);
        curveBinding.path = "";
        curveBinding.propertyName = "m_Sprite";

        UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(DataPathToAssetPath(images[0].FullName));

        List<Sprite> newSprites = new List<Sprite>();
        foreach (UnityEngine.Object sprite in sprites)
        {
            if (sprite is Sprite)
            {
                newSprites.Add(sprite as Sprite);
            }
        }

        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[newSprites.Count];

        Debug.Log("sprites.Length: " + sprites.Length);

        float frameTime = 1 / 10f;
        for (int i = 0; i < newSprites.Count; i++)
        {
            //Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images[i].FullName));
            if (newSprites[i] is Sprite)
            {
                Sprite sprite = newSprites[i] as Sprite;

                keyFrames[i] = new ObjectReferenceKeyframe();
                keyFrames[i].time = frameTime * i;
                keyFrames[i].value = sprite;
            }
        }
        // 动画帧率
        clip.frameRate = 30;

        // 动画循环
        if (animationName.IndexOf("idle") >= 0)
        {
            AnimationClipSettings setting = new AnimationClipSettings { loopTime = true };
            AnimationUtility.SetAnimationClipSettings(clip, setting);
        }

        string parentName = System.IO.Directory.GetParent(directorys.FullName).Name;
        if (!Directory.Exists(AnimationPath + "/" + parentName))
        {
            System.IO.Directory.CreateDirectory(AnimationPath + "/" + parentName);
        }
        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);
        AssetDatabase.CreateAsset(clip, AnimationPath + "/" + parentName + "/" + animationName + ".anim");
        AssetDatabase.SaveAssets();
        return clip;
    }

    static UnityEditor.Animations.AnimatorController BuildAnimationController(List<AnimationClip> clips, string sn, string version)
    {
        Debug.Log("BuildAnimationController name : " + sn + "_" + version);
        UnityEditor.Animations.AnimatorController animatorController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(AnimationControllerPath + "/" + sn + "_" + version + ".controller");
        UnityEditor.Animations.AnimatorControllerLayer layer = animatorController.layers[0];
        UnityEditor.Animations.AnimatorStateMachine sm = layer.stateMachine;
        foreach (AnimationClip clip in clips)
        {
            UnityEditor.Animations.AnimatorState state = sm.AddState(clip.name);
            state.motion = clip;
            if (clip.name == "idle")
            {
                sm.defaultState = state;
            }
        }
        AssetDatabase.SaveAssets();
        return animatorController;
    }

    static void BuildPrefab(DirectoryInfo directorys, string sn, string version, UnityEditor.Animations.AnimatorController animatorController)
    {
        // 生成Prefab 添加一张预览用的Sprite
        FileInfo images = directorys.GetDirectories()[0].GetFiles("*.png")[0];
        GameObject go = new GameObject();
        go.name = sn + "_" + version;
        SpriteRenderer spriteRender = go.AddComponent<SpriteRenderer>();
        spriteRender.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images.FullName));
        Animator animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = animatorController;
        PrefabUtility.SaveAsPrefabAsset(go, PrefabPath + "/" + go.name + ".prefab");
        DestroyImmediate(go);
    }

    public static string DataPathToAssetPath(string path)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            return path.Substring(path.IndexOf("Assets\\"));
        }
        else
        {
            return path.Substring(path.IndexOf("Assets/"));
        }
    }
}
