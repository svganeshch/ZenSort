using DG.Tweening;
using System;
using UnityEngine;

public class Slot : MonoBehaviour
{
    private float moveSpeed = 0.15f;
    private float shiftSpeed = 0.05f;

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

    public Tween SetSlotPositionTween(Prop setProp, bool isShift = false, Action OnCompleteCallback = null)
    {
        if (isShift)
        {
            transistionSpeed = shiftSpeed;
        }
        else
        {
            transistionSpeed = moveSpeed;
        }

        if (setProp.scaleTween != null) setProp.scaleTween.Complete();

        float propY = slotBoxCollider.bounds.max.y + (setProp.propCollider.size.y * setProp.transform.localScale.y / 2) - (setProp.propCollider.center.y * setProp.transform.localScale.y);

        Vector3 targetPosition = new Vector3(transform.position.x, propY, transform.position.z);

        Tween moveTween = setProp.transform.DOMove(targetPosition, transistionSpeed)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() =>
                        {
                            setProp.transform.SetParent(transform);
                            OnCompleteCallback?.Invoke();
                        });

        this.prop = setProp;
        
        return moveTween;
    }
}