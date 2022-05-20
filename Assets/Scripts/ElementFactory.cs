using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class ElementFactory : MonoBehaviour
{
    public int Count = 100;
    // https://osd-alpha.tooqing.com/pixelpai/ElementNode/61f24706540dc200218f8781/8/61f24706540dc200218f8781.pi
    public string[] PIUrls;

    private void Start()
    {
        //foreach (string url in PIUrls)
        //{
        //    CreateElement(url);
        //}
    }

    public async void CreateElement(GameCapsule.ConfigObjects.ElementNode sourceNode)
    {
        if (sourceNode.avatar != null) return;

        string imgUrl;
        string jsonUrl;
        GameCapsule.ConfigObjects.ElementNode element;
        if (sourceNode.animations == null || 
            String.IsNullOrEmpty(sourceNode.animations.display?.texturePath) ||
            String.IsNullOrEmpty(sourceNode.animations.display?.dataPath))
        {
            string url = $"https://osd-alpha.tooqing.com/pixelpai/ElementNode/{sourceNode.sn}/{sourceNode.version}/{sourceNode.sn}.pi";
            element = await DownloadPIAndDesrializeToElementNode(url);

            // get all sprites from remote
            imgUrl = url.Replace(".pi", ".png");
            jsonUrl = url.Replace(".pi", ".json");
        }
        else
        {
            imgUrl = "https://osd-alpha.tooqing.com/" + sourceNode.animations.display.texturePath;
            jsonUrl = "https://osd-alpha.tooqing.com/" + sourceNode.animations.display.dataPath;
            element = sourceNode;
        }

        if (element?.animations == null) return;
        if (element?.location == null) return;

        string defaultAniName = element.animations.defaultAnimationName;
        List<Sprite> sprites = await DownloadSpriteSheet(imgUrl, jsonUrl);

        // set animation from PI
        List<string> frameNames = element.animations.getDefaultAnimationData().layer[0].frameName;
        Sprite[] animationSprites = new Sprite[frameNames.Count];
        for (int i = 0; i < frameNames.Count; i++)
        {
            var findSprite = sprites.Find((x) => x.name == frameNames[i]);
            if (!findSprite)
            {
                Debug.LogError($"can not find sprite: {frameNames[i]}");
                return;
            }
            animationSprites[i] = findSprite;
        }

        //for (int i = 0; i < Count; i++)
        //{
            CreateInstance(new Vector2(element.location.x * 0.01f, -element.location.y * 0.01f), 
                defaultAniName, animationSprites);
        //}

        Debug.Log($"time: {Time.realtimeSinceStartup}");
    }

    private void CreateInstance(Vector2 pos, string aniName, Sprite[] animationSprites)
    {
        GameObject ins = Instantiate(Resources.Load<GameObject>("Elements/ElementTemplate")) as GameObject;
        ins.transform.SetParent(transform);
        ins.transform.position = pos;
        var ani = ins.GetComponent<SpriteSheetAnimation>();
        AnimationStruct oneAnimation = new AnimationStruct();
        oneAnimation.Name = aniName;
        oneAnimation.Sprites = animationSprites;
        oneAnimation.Loop = true;
        ani.SetAnimationSprites(new List<AnimationStruct>() { oneAnimation });
        ani.Play(aniName);
    }

    async Task<GameCapsule.ConfigObjects.ElementNode> DownloadPIAndDesrializeToElementNode(string url)
    {
        UnityWebRequest requestJson = UnityWebRequest.Get(url);
        requestJson.SetRequestHeader("Content-Type", "application/json");
        var jsonOperation = requestJson.SendWebRequest();
        while (!jsonOperation.isDone)
            await Task.Yield();

        if (requestJson.result != UnityWebRequest.Result.Success)
            return null;


        // ½âÎöpiÎÄ¼þ
        var capsule = new GameCapsule.Capsule();
        byte[] pi = requestJson.downloadHandler.data;
        capsule.deserialize(pi);
        Debug.Log("Capsule: " + capsule);

        return (GameCapsule.ConfigObjects.ElementNode)capsule.root.children[0];
    }

    async Task<List<Sprite>> DownloadSpriteSheet(string imgUrl, string jsonUrl)
    {
        List<Sprite> sprites = new List<Sprite>();
        // download img
        UnityWebRequest requestImg = UnityWebRequestTexture.GetTexture(imgUrl);
        var imgOperation = requestImg.SendWebRequest();
        while (!imgOperation.isDone)
            await Task.Yield();

        if (requestImg.result != UnityWebRequest.Result.Success)
            return sprites;

        Texture2D tex = ((DownloadHandlerTexture)requestImg.downloadHandler).texture;
        int texWidth = tex.width;
        int texHeight = tex.height;

        // download json
        UnityWebRequest requestJson = UnityWebRequest.Get(jsonUrl);
        requestJson.SetRequestHeader("Content-Type", "application/json");
        var jsonOperation = requestJson.SendWebRequest();
        while (!jsonOperation.isDone)
            await Task.Yield();

        if (requestJson.result != UnityWebRequest.Result.Success)
            return sprites;

        var jsonResponse = requestJson.downloadHandler.text;
        try
        {
            var jsonResult = JsonUtility.FromJson<SpriteSheetTexture>(jsonResponse);
            Debug.Log($"parse succeed: {jsonResult.frames}");
            foreach (var f in jsonResult.frames)
            {
                Sprite sprite = Sprite.Create(tex, SpriteSheetRectToUnitySpriteEditorRect(f.frame, texWidth, texHeight), new Vector2(0.5f, 0.5f));
                sprite.name = f.filename;
                sprites.Add(sprite);
            }

            return sprites;
        }
        catch (Exception ex)
        {
            Debug.LogError($"cannot parse response {jsonResponse} . {ex.Message}");
            return sprites;
        }
    }

    private Rect SpriteSheetRectToUnitySpriteEditorRect(SpriteSheetFrameRect spriteSheetFrameRect, int texWidth, int texHeight)
    {
        return new Rect(
            spriteSheetFrameRect.x,
            texHeight - spriteSheetFrameRect.y - spriteSheetFrameRect.h,
            spriteSheetFrameRect.w,
            spriteSheetFrameRect.h);
    }
}

[Serializable]
public class SpriteSheetTexture
{
    public SpriteSheetFrame[] frames;
}

[Serializable]
public class SpriteSheetFrame
{
    public string filename;
    public SpriteSheetFrameRect frame;
}
[Serializable]
public class SpriteSheetFrameRect
{
    public int x;
    public int y;
    public int w;
    public int h;
}
