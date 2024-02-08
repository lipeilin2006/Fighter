using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fighter;
using Data;

public class NetworkAnimator : Fighter.Component
{
    Animator animator;
	public override void Init()
	{
        animator = gameObject.GetComponent<Animator>();
	}
	public override void Update()
    {
		AnimatorData data = new();
		int layerCount = animator.layerCount;
        data.layerWeights = new float[layerCount];
        data.layerDatas = new LayerData[layerCount];
		for (int i = 0; i < layerCount; i++)
        {
            data.layerWeights[i] = animator.GetLayerWeight(i);
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(i);
            data.layerDatas[i]=new LayerData
            {
                fullPathHash = state.fullPathHash,
                normalizedTime = state.normalizedTime
            };
        }
    }
}
