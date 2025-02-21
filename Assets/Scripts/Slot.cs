using DG.Tweening;
using System;
using UnityEngine;

public class Slot : MonoBehaviour
{
    private float moveSpeed = 0.15f;
    private float shiftSpeed = 0.1f;

    public ParticleSystem slotVFX;

    private float transistionSpeed = 0;
    private Prop prop;
    public Prop slotProp { get => prop; set => prop = value; }

    private BoxCollider slotBoxCollider;

    private void Awake()
    {
        slotVFX = GetComponentInChildren<ParticleSystem>();

        slotBoxCollider = GetComponent<BoxCollider>();
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

        float propY = slotBoxCollider.bounds.max.y + (prop.propSize.y * prop.transform.localScale.y / 2) - (prop.propCollider.center.y * prop.transform.localScale.y);

        Vector3 targetPosition = new Vector3(transform.position.x, propY, transform.position.z);

        Tween moveTween = prop.transform.DOMove(targetPosition, transistionSpeed)
                                .SetEase(Ease.InQuad)
                                .OnComplete(() => OnCompleteCallback?.Invoke());

        this.prop = prop;

        return moveTween;
    }
}