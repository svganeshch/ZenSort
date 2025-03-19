using DG.Tweening;
using System;
using UnityEngine;

public class Slot : MonoBehaviour
{
    private float moveSpeed = 0.2f;
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
                        .SetEase(Ease.OutQuart)
                        .OnComplete(() =>
                        {
                            setProp.transform.SetParent(transform);

                            if (!isShift)
                            {
                                setProp.transform.DOShakeScale(0.15f, 
                                        Vector3.Scale(new Vector3(-0.075f, -0.075f, 0) , setProp.transform.localScale), 
                                        randomnessMode: ShakeRandomnessMode.Harmonic)
                                    .SetEase(Ease.OutBounce);
                            }

                            OnCompleteCallback?.Invoke();
                        });

        this.prop = setProp;
        
        return moveTween;
    }
}