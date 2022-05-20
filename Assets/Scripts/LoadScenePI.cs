using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class LoadScenePI : MonoBehaviour
{
    public string ScenePIUrl;

    private ElementFactory ElementFactory;

    private void OnEnable()
    {
        ElementFactory = gameObject.GetComponent<ElementFactory>();
    }

    private void Start()
    {
        CreateSceneFromPI(ScenePIUrl);
    }

    private async void CreateSceneFromPI(string url)
    {
        ScenePIData scenePIData = await DownloadPIAndDesrialize(url);
        if (scenePIData == null) return;

        var sceneNode = scenePIData.SceneNode;
        var elementNodes = scenePIData.ElementNodes;

        foreach (var elementNode in elementNodes)
        {
            ElementFactory.CreateElement(elementNode);
        }
    }

    private async Task<ScenePIData> DownloadPIAndDesrialize(string url)
    {
        UnityWebRequest requestJson = UnityWebRequest.Get(url);
        requestJson.SetRequestHeader("Content-Type", "application/arraybuffer");
        var jsonOperation = requestJson.SendWebRequest();
        while (!jsonOperation.isDone)
            await Task.Yield();

        if (requestJson.result != UnityWebRequest.Result.Success)
            return null;


        // ½âÎöpiÎÄ¼þ
        var capsule = new GameCapsule.Capsule();
        byte[] pi = requestJson.downloadHandler.data;

        //byte[] pi = File.ReadAllBytes("Assets/Resources/Scene/1024433542.pi");
        capsule.deserialize(pi);

        var sceneNode = capsule.root.children.Find((x) => (OpDef.NodeType)x.type == OpDef.NodeType.SceneNodeType) as GameCapsule.ConfigObjects.SceneNode;
        var elements = new List<GameCapsule.ConfigObjects.ElementNode>();
        foreach (var obj in capsule.objectList)
        {
            if ((OpDef.NodeType)obj.type == OpDef.NodeType.ElementNodeType)
            {
                elements.Add(obj as GameCapsule.ConfigObjects.ElementNode);
            }
        }

        var scenePIData = new ScenePIData();
        scenePIData.SceneNode = sceneNode;
        scenePIData.ElementNodes = elements.ToArray();

        return scenePIData;
    }
}

class ScenePIData
{
    public GameCapsule.ConfigObjects.SceneNode SceneNode;
    public GameCapsule.ConfigObjects.ElementNode[] ElementNodes;
}