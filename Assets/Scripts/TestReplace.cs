using System.Collections;
using System.Collections.Generic;
using DragonBones;
using UnityEngine;

public class TestReplace : MonoBehaviour
{
    public UnityEngine.Transform _armatureParent;
    public UnityArmatureComponent _armatureComp;
    public GameObject _replaceGO;
    public Texture _replaceTexture;

    private Armature _armature;
    private UnitySlot _slot;

    void Start()
    {
        UnityFactory.factory.LoadDragonBonesData($"DragonBones/bones_human01_ske");
        UnityFactory.factory.LoadTextureAtlasData($"DragonBones/bones_human01_tex");

        if (_armatureComp != null)
        {
            GameObject.Destroy(_armatureComp.gameObject);
        }

        _armatureComp = UnityFactory.factory.BuildArmatureComponent("Armature");

        foreach (UnityEngine.Transform item in _armatureParent)
        {
            item.position = new Vector3(Random.Range(-4f, 4f), Random.Range(-5f, 5f));
            UnityArmatureComponent armComp = item.gameObject.GetComponent<UnityArmatureComponent>();
            var animation = armComp.armature.animation;
            animation.Play(animation.animationNames[Random.Range(0, animation.animationNames.Count)]);
        }

        _armature = _armatureComp.armature;
        _slot = _armature.GetSlot("bleg_spec_3") as UnitySlot;

        var mesh = new Mesh();
        var texWidthUnit = _replaceTexture.width * 0.01f;
        var texHeightUnit = _replaceTexture.height * 0.01f;
        Vector3[] vertices = new Vector3[4]
            {
                new Vector3(-0.5f * texWidthUnit, -0.5f * texHeightUnit),
                new Vector3(0.5f * texWidthUnit, -0.5f * texHeightUnit),
                new Vector3(-0.5f * texWidthUnit, 0.5f * texHeightUnit),
                new Vector3(0.5f * texWidthUnit, 0.5f * texHeightUnit)
            };
        mesh.vertices = vertices;
        Vector2[] uvs = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
        mesh.uv = uvs;
        int[] triangles = new int[6] { 0, 3, 1, 3, 0, 2 };
        mesh.triangles = triangles;

        var uiDisplay = _replaceGO.GetComponent<UnityUGUIDisplay>();
        var meshRenderer = _replaceGO.GetComponent<MeshRenderer>();
        var meshFilter = _replaceGO.GetComponent<MeshFilter>();
        if (uiDisplay != null)
        {
            uiDisplay.texture = _replaceTexture;
            uiDisplay.sharedMesh = mesh;
        }
        else if (meshRenderer != null && meshFilter != null)
        {
            meshRenderer.material.mainTexture = _replaceTexture;
            meshFilter.mesh = mesh;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (_slot.display != null)
            {
                _replaceGO.layer = (_slot.display as GameObject).layer;
            }
            _slot.display = _replaceGO;

            // Òþ²ØµÄ²å²Û
            //_slot.display = null;
        }
        else if (Input.GetMouseButtonUp(1))
        {

        }
    }
}
