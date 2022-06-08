using SuperTiled2Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

public class LoadTilemapAB : MonoBehaviour
{
    async Task Start()
    {
        string url = "file:///" + Application.dataPath + "/../TempStreamingAssets/Windows/tilemap/1408385966.ab";
        url = "http://58.246.181.130:1280//game/resource/61dbd5cbbdc7560011839c3b/0.0.6/mapdata/1408385966.ab";
        await InstantiateObject(url);

    }

    async Task InstantiateObject(string url)
    {
        var request
            = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(url, 0);
        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Load asset bundle error, check url: " + url);
            return;
        }

        AssetBundle bundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
        //var tileset = ScriptableObject.FindObjectOfType<SuperTileset>();
        GameObject prefab = bundle.LoadAsset<GameObject>("1408385966-map");
        GameObject go = Instantiate(prefab);
        var superMap = go.GetComponent<SuperMap>();
        var tilemap = go.GetComponentInChildren<Tilemap>();
    }
}
