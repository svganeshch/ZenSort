using System;
using DG.Tweening;
using UnityEngine;

public class SpeakerTween : MonoBehaviour
{
    public Tween speakerPopTween;
    private void OnEnable()
    {
        speakerPopTween = transform.DOPunchScale(Vector3.one * 0.75f, 6f, 2, 0.05f)
            .OnComplete((() =>
            {
                transform.localScale = Vector3.one;
            }));
    }

    private void OnDisable()
    {
        speakerPopTween.Complete();
        transform.localScale = Vector3.one;
    }
}
