using System;
using DG.Tweening;
using UnityEngine;

public class ZenPopUp : MonoBehaviour
{
    private void OnEnable()
    {
        transform.DOPunchScale(Vector3.one * 0.5f, 0.25f, 1, 0);
    }
}
