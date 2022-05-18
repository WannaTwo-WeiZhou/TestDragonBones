using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadElementAB : MonoBehaviour
{
    public int Count = 100;
    // http://osd-alpha.tooqing.com/61f24706540dc200218f8781_8
    public string[] ABUrls;

    void Start()
    {
        foreach (string url in ABUrls)
        {
            StartCoroutine(InstantiateObject(url));
        }
    }

    IEnumerator InstantiateObject(string url)
    {
        var request
            = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(url, 0);
        yield return request.Send();
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
