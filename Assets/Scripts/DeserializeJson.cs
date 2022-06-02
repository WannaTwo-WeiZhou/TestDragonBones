using GameModel.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class DeserializeJson : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // "components/5f1e48715172b001676d5c3a/4/5f1e48715172b001676d5c3a.json"
        string jsonUrl = "https://osd-alpha.tooqing.com/components/5f1e51e6d48dd501cacfd5f5/5/5f1e51e6d48dd501cacfd5f5.json";
        DownloadJson(jsonUrl);
    }

    async Task DownloadJson(string jsonUrl)
    {
        // download json
        UnityWebRequest requestJson = UnityWebRequest.Get(jsonUrl);
        requestJson.SetRequestHeader("Content-Type", "application/json");
        var jsonOperation = requestJson.SendWebRequest();
        while (!jsonOperation.isDone)
            await Task.Yield();

        if (requestJson.result != UnityWebRequest.Result.Success)
        {
            return;
        }

        var jsonResponse = requestJson.downloadHandler.text;
        //try
        //{
            var jsonResult = SpriteSheetJson.CreateFromJSON(jsonResponse);
            Debug.Log($"parse succeed: {jsonResult.frames}");
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogError($"cannot parse response {jsonResponse} . {ex.Message}");
        //}
    }
}
