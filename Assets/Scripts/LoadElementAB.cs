using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class LoadElementAB : MonoBehaviour
{
    public int Count = 100;
    // http://osd-alpha.tooqing.com/61f24706540dc200218f8781_8
    public string[] ABUrls;

    async Task Start()
    {
        foreach (string url in ABUrls)
        {
            await InstantiateObject(url);
        }
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
        GameObject element = bundle.LoadAsset<GameObject>("61f24706540dc200218f8781_8");
        for (int i = 0; i < Count; i++)
        {
            GameObject go = Instantiate(element);
            go.transform.position = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
        }

        Debug.Log($"time: {Time.realtimeSinceStartup}");
    }
}
