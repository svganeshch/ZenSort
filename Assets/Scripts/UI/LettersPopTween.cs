using System;
using DG.Tweening;
using UnityEngine;

public class LettersPopTween : MonoBehaviour
{
    public GameObject[] popLetters;
    public GameObject fillerObj;
    Sequence letterPopSequence;

    private void OnEnable()
    {
        letterPopSequence = DOTween.Sequence();
        
        foreach (var letter in popLetters)
        {
            Tween letterPopTween = letter.transform.DOPunchScale(Vector3.one * 0.5f, 0.15f, 1, 0);
            letterPopTween.OnStart(() =>
            {
                letter.SetActive(true);
            });
            
            letterPopSequence.Append(letterPopTween);
        }

        letterPopSequence.OnComplete((() =>
        {
            fillerObj.SetActive(true);
        }));
    }

    private void OnDisable()
    {
        foreach (var letter in popLetters)
        {
            letter.gameObject.SetActive(false);
        }
        
        fillerObj.SetActive(false);
    }
}
