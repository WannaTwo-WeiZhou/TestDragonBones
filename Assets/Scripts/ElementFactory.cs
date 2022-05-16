using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class ElementFactory : MonoBehaviour
{
    public SpriteSheetAnimation Target;

    private void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if (!Target) return;

        if (Input.GetKeyDown(KeyCode.Space) && !Target)
        {
            //string name = Target.CurAnimationName == "idle" ? "attack" : "idle";
            //Target.Play(name);

            //CreateElement();

            CreateElement();
        }
    }

    private async void CreateElement()
    {
        List<Sprite> sprites = new List<Sprite>();
        // load from local
        // 1
        //Object[] data = AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/Elements/61f24706540dc200218f8781/8/61f24706540dc200218f8781_edited.png");
        //foreach (Object o in data)
        //{
        //    if (o is Sprite)
        //    {
        //        sprites.Add(o as Sprite);
        //    }
        //}
        // 2
        //var resSprites = Resources.LoadAll<Sprite>("Elements/61f24706540dc200218f8781/8/61f24706540dc200218f8781_edited");
        //foreach (Sprite one in resSprites)
        //{
        //    sprites.Add(one);
        //}

        // load from url
        // https://osd-alpha.tooqing.com/user_avatar/texture/0cfe7348443bd59fb224891811e10626dd29c94dv1.png

        sprites = await DownloadSpriteSheet();

        GameObject ins = Instantiate(Resources.Load<GameObject>("Elements/ElementTemplate")) as GameObject;
        Target = ins.GetComponent<SpriteSheetAnimation>();
        AnimationStruct oneAnimation = new AnimationStruct();
        oneAnimation.Name = "idle";
        oneAnimation.Sprites = sprites.ToArray();
        oneAnimation.Loop = true;
        Target.SetAnimationSprites(new List<AnimationStruct>() { oneAnimation });
        Target.Play("idle");
    }

    async Task<List<Sprite>> DownloadSpriteSheet()
    {
        List<Sprite> sprites = new List<Sprite>();
        // download img
        UnityWebRequest requestImg = UnityWebRequestTexture.GetTexture("https://osd-alpha.tooqing.com/pixelpai/ElementNode/61f24706540dc200218f8781/8/61f24706540dc200218f8781.png");
        var imgOperation = requestImg.SendWebRequest();
        while (!imgOperation.isDone)
            await Task.Yield();

        if (requestImg.result != UnityWebRequest.Result.Success)
            return sprites;

        Texture2D tex = ((DownloadHandlerTexture)requestImg.downloadHandler).texture;
        int texWidth = tex.width;
        int texHeight = tex.height;

        // download json
        UnityWebRequest requestJson = UnityWebRequest.Get("https://osd-alpha.tooqing.com/pixelpai/ElementNode/61f24706540dc200218f8781/8/61f24706540dc200218f8781.json");
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
            Debug.Log($"parse succeed: {jsonResult}");
            foreach (var f in jsonResult.frames)
            {
                Sprite sprite = Sprite.Create(tex, SpriteSheetRectToUnitySpriteEditorRect(f.frame, texWidth, texHeight), new Vector2(0, 0));
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