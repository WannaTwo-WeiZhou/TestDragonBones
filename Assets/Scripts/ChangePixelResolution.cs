using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ChangePixelResolution : MonoBehaviour
{
    public PixelPerfectCamera cam;
    public Transform scaleRoot;

    public void ChanngeResolition()
    {
        scaleRoot.localScale = scaleRoot.localScale.x == 1 ?
            new Vector3(1.1f, 1.1f, 1.1f) : Vector3.one;
    }
}
