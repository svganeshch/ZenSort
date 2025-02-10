using DG.Tweening;
using System;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public float shiftSpeed = 0.15f;

    public ParticleSystem slotVFX;

    private float transistionSpeed = 0;
    private Prop prop;
    public Prop slotProp { get => prop; set => prop = value; }

    private void Awake()
    {
        slotVFX = GetComponentInChildren<ParticleSystem>();
    }

    public Tween SetSlotPositionTween(Prop prop, bool isShift = false, Action OnCompleteCallback = null)
    {
        if (isShift)
        {
            transistionSpeed = shiftSpeed;
        }
        else
        {
            transistionSpeed = moveSpeed;
        }

        prop.transform.parent = transform;

        Tween moveTween = prop.transform.DOMove(transform.position, transistionSpeed)
                                .SetEase(Ease.InQuad)
                                .OnComplete(() => OnCompleteCallback?.Invoke());

        this.prop = prop;

        return moveTween;
    }
}