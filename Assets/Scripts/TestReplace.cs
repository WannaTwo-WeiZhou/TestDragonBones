using System.Collections;
using System.Collections.Generic;
using DragonBones;
using UnityEngine;

public class TestReplace : MonoBehaviour
{
    public UnityArmatureComponent _armatureComp;
    public GameObject _replaceGO;
    public Texture _replaceTexture;

    private Armature _armature;
    private UnitySlot _slot;
    // Start is called before the first frame update
    void Start()
    {
        _armature = _armatureComp.armature;
        _slot = _armature.GetSlot("bleg_spec_3") as UnitySlot;

        var meshRenderer = _replaceGO.GetComponent<MeshRenderer>();
        meshRenderer.material.mainTexture = _replaceTexture;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            _slot.display = _replaceGO;
            //GameObject display = _slot.display as GameObject;
            //display.SetActive(false);
        }
        else if (Input.GetMouseButtonUp(1))
        {

        }
    }
}
