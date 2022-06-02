using SuperTiled2Unity;
using SuperTiled2Unity.Editor;
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
        string url = "file:///" + Application.dataPath + "/../TempStreamingAssets/Windows/tilemap/1340942569.ab";
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
        var tileset = ScriptableObject.FindObjectOfType<SuperTileset>();
        GameObject prefab = bundle.LoadAsset<GameObject>("1340942569-map");
        GameObject go = Instantiate(prefab);
        var superMap = go.GetComponent<SuperMap>();
        var tilemap = go.GetComponentInChildren<Tilemap>();
    }
}
