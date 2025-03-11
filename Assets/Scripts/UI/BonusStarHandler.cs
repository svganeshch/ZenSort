using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BonusStarSlot
{
    public GameObject slot;
    public GameObject star;
}

public class BonusStarHandler : MonoBehaviour
{
    public List<BonusStarSlot> bonusSlots = new();

    private void Start()
    {
        UIManager.instance.OnLevelChange.AddListener(RemoveBonusStars);
    }

    public void AddBonusStar()
    {
        foreach (var bonusSlot in bonusSlots)
        {
            if (!bonusSlot.star.activeSelf)
            {
                GameObject star = bonusSlot.star;

                star.SetActive(true);
                star.transform.DOPunchScale(Vector3.one * 0.01f, 0.1f, 0, 0).SetEase(Ease.InQuad);

                break;
            }
        }

        CheckSlots();
    }

    private void CheckSlots()
    {
        Sequence bonusStarJumpSeq = DOTween.Sequence();
        bool isSlotsFull = bonusSlots.All(bonusSlot => bonusSlot.star.activeSelf);

        if (isSlotsFull)
        {
            float delayIncrement = 0.15f;
            float currentDelay = 0f;

            foreach (var bonusSlot in bonusSlots)
            {
                var star = bonusSlot.star;

                Tween starPopTween = star.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 0, 0).SetEase(Ease.InQuad);
                Tween starJumpTween = star.transform.DOJump(UIManager.instance.starCountHandler.starImage.transform.position, -0.25f, 1, 0.5f)
                    .OnComplete(() =>
                    {
                        UIManager.instance.starCountHandler.PlayStarAddTween();
                        star.SetActive(false);
                    });

                bonusStarJumpSeq.Insert(currentDelay, starPopTween);
                bonusStarJumpSeq.Insert(currentDelay, starJumpTween);

                currentDelay += delayIncrement;
            }

            bonusStarJumpSeq.OnComplete(() =>
            {
                UIManager.instance.starCountHandler.AddStar(5);
                RemoveBonusStars();
            });

            SFXManager.instance.PlayStarBonusSound();
        }
    }

    public int GetBonusStarsCount()
    {
        int bonusStarsCount = 0;

        bonusStarsCount = bonusSlots.Count(slot => slot.star.activeSelf);
        
        return bonusStarsCount;
    }

    public void RemoveBonusStars(int level = 0)
    {
        foreach (var bonusSlot in bonusSlots)
        {
            bonusSlot.star.SetActive(false);
            bonusSlot.star.transform.position = bonusSlot.slot.transform.position;
        }
    }
}
