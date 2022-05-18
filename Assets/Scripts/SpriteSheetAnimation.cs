using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSheetAnimation : MonoBehaviour
{
    public int FrameRate = 24;
    public List<AnimationStruct> Animations = new List<AnimationStruct>();
    public string CurAnimationName { get; private set; } = "";

    private SpriteRenderer SpriteRenderer;
    private Dictionary<string, AnimationStruct> AnimationMap = new Dictionary<string, AnimationStruct>();
    private IEnumerator Runner = null;
    private int CurFrameIndex = 0;

    private void OnEnable()
    {
        SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    public void Start()
    {
        if (Animations.Count > 0)
        {
            ConvertToDictionary();
        }
    }

    public void SetAnimationSprites(List<AnimationStruct> animations)
    {
        StopCurrentAnimation();

        Animations = animations;
        ConvertToDictionary();
    }

    public void Play(string aniName, bool replayWhenSameName = false)
    {
        if (!AnimationMap.ContainsKey(aniName))
        {
            Debug.LogError($"not contain key: {aniName}");
            return;
        }


        if (!String.Equals(aniName, CurAnimationName) || replayWhenSameName)
        {
            CurFrameIndex = 0;
        }
        CurAnimationName = aniName;
        AnimationStruct animationStruct = AnimationMap[aniName];

        StopCurrentAnimation();
        if (animationStruct.Sprites.Length <= CurFrameIndex)
        {
            Debug.LogError("Sprites.Length <= CurFrameIndex: " + animationStruct.Sprites.Length + ";" + CurFrameIndex);
            return;
        }

        Runner = CreateRunner(animationStruct);
        StartCoroutine(Runner);
    }

    private IEnumerator CreateRunner(AnimationStruct animationSprites)
    {
        Sprite[] sprites = animationSprites.Sprites;
        bool loop = animationSprites.Loop;
        int times = 0;

        while (loop || times == 0)
        {
            SpriteRenderer.sprite = sprites[CurFrameIndex];

            CurFrameIndex++;
            if (CurFrameIndex >= sprites.Length)
            {
                CurFrameIndex = 0;
                times++;
            }

            yield return new WaitForSeconds(1f / (float)FrameRate);
        }
    }

    private void StopCurrentAnimation()
    {
        if (Runner != null)
        {
            StopCoroutine(Runner);
            Runner = null;
        }
    }

    private void ConvertToDictionary()
    {
        AnimationMap.Clear();
        foreach (var one in Animations)
        {
            AnimationMap[one.Name] = one;
        }
    }
}

[Serializable]
public struct AnimationStruct
{
    public string Name;
    public Sprite[] Sprites;
    public bool Loop;
}