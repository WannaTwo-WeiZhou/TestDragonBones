using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SetImporterRuntime : MonoBehaviour
{
    public Image ImgComp;
    public string ImgUrl = "https://s3.bmp.ovh/imgs/2022/05/24/4cf42a009ddf485b.png";

    private async void Start()
    {
        Texture2D tex = await DownloadImg(ImgUrl);
        
        //tex.Compress(false);
        tex.filterMode = FilterMode.Point;
        ImgComp.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        ImgComp.SetNativeSize();
    }

    async Task<Texture2D> DownloadImg(string imgUrl)
    {
        // download img
        UnityWebRequest requestImg = UnityWebRequestTexture.GetTexture(imgUrl);
        var imgOperation = requestImg.SendWebRequest();
        while (!imgOperation.isDone)
            await Task.Yield();

        if (requestImg.result != UnityWebRequest.Result.Success)
            return null;

        Texture2D tex = ((DownloadHandlerTexture)requestImg.downloadHandler).texture;
        return tex;
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
