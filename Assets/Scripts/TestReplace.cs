using System.Collections;
using System.Collections.Generic;
using DragonBones;
using UnityEngine;

public class TestReplace : MonoBehaviour
{
    public UnityArmatureComponent _armatureComp;
    //public GameObject _replaceGO;

    private Armature _armature;
    private UnitySlot _slot;
    // Start is called before the first frame update
    void Start()
    {
        _armature = _armatureComp.armature;
        _slot = _armature.GetSlot("bleg_cost_1") as UnitySlot;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonUp(0))
        //{
        //    _slot.display = _replaceGO;
        //}
        //else if (Input.GetMouseButtonUp(1))
        //{
            
        //}
    }
}
