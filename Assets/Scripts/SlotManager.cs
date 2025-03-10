using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class SlotManager : MonoBehaviour
{
    public SlotManagerState currentSlotManagerState;

    public Slot extraSlot;
    public List<Slot> slots;
    private List<int> matchingSlotIndexs = new List<int>();
    private List<Prop> props = new List<Prop>();

    private Sequence shiftSequence;
    private Sequence rearrangeSequence;
    private Sequence matchingSequence;
    
    private Tween slotsScaleTween;

    private Queue<Func<IEnumerator>> propQueue = new Queue<Func<IEnumerator>>();
    private bool isProcessingQueue = false;

    private bool isComboActive = false;
    private int matchCount = 0;

    private int pickCount = 0;

    private void Awake()
    {
        currentSlotManagerState = SlotManagerState.Done;

        slots = GetComponentsInChildren<Slot>().ToList();
    }

    private void Start()
    {
        UIManager.instance.OnLevelChange.AddListener(SlotsSpawnTween);
        SlotsSpawnTween();
    }

    public void EnqueueProp(Prop prop, Action OnCompleteCallback = null)
    {
        props.Add(prop);

        propQueue.Enqueue(() => SetPropSlot(prop, OnCompleteCallback));
        ProcessPropQueue();
    }

    private void ProcessPropQueue()
    {
        if (isProcessingQueue || propQueue.Count == 0)
            return;

        isProcessingQueue = true;
        StartCoroutine(ProcessNextProp());
    }

    private IEnumerator ProcessNextProp()
    {
        if (propQueue.Count > 0)
        {
            var nextPropAction = propQueue.Dequeue();
            yield return StartCoroutine(nextPropAction());
        }

        isProcessingQueue = false;
        ProcessPropQueue();
    }

    private IEnumerator SetPropSlot(Prop prop, Action OnCompleteCallback)
    {
        int insertIndex = -1;
        bool willCauseMatch = false;

        pickCount++;

        insertIndex = slots.FindIndex(slot => slot.slotProp == null);

        if (insertIndex >= slots.Count - 3)
        {
            SlotsAlmostFullTween();
        }
        
        int sameColorIndex = slots.FindLastIndex(s => s.slotProp != null && s.slotProp.gameObject.name == prop.gameObject.name);

        if (sameColorIndex != -1)
        {
            insertIndex = sameColorIndex + 1;
            
            if (slots[insertIndex].slotProp != null)
            {
                yield return StartCoroutine(ShiftSlots(insertIndex));
            }

            willCauseMatch = CheckForMatch(insertIndex, prop);

            Tween propTween = slots[insertIndex].SetSlotPositionTween(prop);
            yield return propTween.WaitForCompletion();

            if (willCauseMatch)
            {
                currentSlotManagerState = SlotManagerState.Matching;
                
                BoosterManager.previousPickedProp = null;
                pickCount = 0;

                HandleMatchingSlotGroupsCallback();
                yield return StartCoroutine(RearrangeSlots());
            }
        }
        else
        {
            Tween propTween = slots[insertIndex].SetSlotPositionTween(prop);
            yield return propTween.WaitForCompletion();
        }

        if (insertIndex + 1 >= slots.Count)
        {
            if (!willCauseMatch)
            {
                UIManager.instance.OnGameOver.Invoke();
                Debug.Log("All slots filled");
            }
        }
        
        // if the slots scale tween is playing and we just had a match then
        // that means the slots must have been cleared
        if ((slotsScaleTween != null && slotsScaleTween.IsPlaying()) & willCauseMatch)
        {
            slotsScaleTween.Kill(true);
            slotsScaleTween = null;
            
            transform.localScale = Vector3.one;
        }

        if (pickCount == 3)
        {
            if (!willCauseMatch)
            {
                if (isComboActive)
                {
                    pickCount = 0;
                    matchCount = 0;
                    isComboActive = false;
                }

                UIManager.instance.bonusStarHandler.RemoveBonusStars();
            }
        }
        
        OnCompleteCallback?.Invoke();
    }

    private IEnumerator ShiftSlots(int insertIndex)
    {
        shiftSequence = DOTween.Sequence();

        List<KeyValuePair<Prop, int>> propsToShift = new List<KeyValuePair<Prop, int>>();

        for (int i = insertIndex; i < slots.Count - 1; i++)
        {
            if (slots[i].slotProp != null)
            {
                propsToShift.Add(new KeyValuePair<Prop, int>(slots[i].slotProp, i));
            }
        }

        slots[insertIndex].slotProp = null;

        for (int i = propsToShift.Count - 1; i >= 0; i--)
        {
            Tween shiftTween = slots[propsToShift[i].Value + 1].SetSlotPositionTween(propsToShift[i].Key, true);
            shiftSequence.Append(shiftTween);
        }

        yield return shiftSequence.WaitForCompletion();
    }

    private bool CheckForMatch(int insertIndex, Prop prop)
    {
        var originalProp = slots[insertIndex].slotProp;
        slots[insertIndex].slotProp = prop;

        for (int i = 0; i < slots.Count - 2; i++)
        {
            if (slots[i].slotProp != null &&
                slots[i + 1].slotProp != null &&
                slots[i + 2].slotProp != null &&
                slots[i].slotProp.gameObject.name == slots[i + 1].slotProp.gameObject.name &&
                slots[i].slotProp.gameObject.name == slots[i + 2].slotProp.gameObject.name)
            {
                matchingSlotIndexs.Add(i);
                matchingSlotIndexs.Add(i + 1);
                matchingSlotIndexs.Add(i + 2);

                return true;
            }
        }

        slots[insertIndex].slotProp = originalProp;
        return false;
    }

    private IEnumerator RearrangeSlots()
    {
        rearrangeSequence = DOTween.Sequence();

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].slotProp == null)
            {
                for (int j = i + 1; j < slots.Count; j++)
                {
                    if (slots[j].slotProp != null)
                    {
                        Tween rearrangeTween = slots[i].SetSlotPositionTween(slots[j].slotProp, true);
                        slots[j].slotProp = null;

                        rearrangeSequence.Append(rearrangeTween);
                        break;
                    }
                }
            }
        }

        yield return rearrangeSequence.WaitForCompletion();
    }

    private void SlotsSpawnTween(int level = 0)
    {
        foreach (var slot in slots)
        {
            Vector3 origSlotScale = slot.transform.localScale;
            
            slot.transform.localScale = Vector3.zero;
            slot.gameObject.transform.DOScale(Vector3.one * 0.95f, 0.2f).SetEase(Ease.InOutQuad)
                .OnComplete(() => { slot.transform.localScale = origSlotScale; });
        }
    }

    public void SlotsAlmostFullTween()
    {
        if (slotsScaleTween != null && slotsScaleTween.IsPlaying()) return;
        
        slotsScaleTween = transform.DOPunchScale(Vector3.one * 0.05f, 1.5f, 0, 0).SetEase(Ease.InOutElastic)
            .SetDelay(0.75f)
            .SetLoops(-1);
    }

    //private bool HandleMatchingSlotGroupsCallback()
    //{
    //    foreach (int matchingIndex in matchingSlotIndexs)
    //    {
    //        //Destroy(slots[matchingIndex].slotProp.gameObject);

    //        slots[matchingIndex].slotProp.gameObject.SetActive(false);
    //        slots[matchingIndex].slotProp = null;

    //        slots[matchingIndex].slotVFX.Play();
    //    }

    //    //SFXManager.Instance.PlayPropsMatchedSound();

    //    matchingSlotIndexs.Clear();

    //    return true;
    //}

    private void HandleMatchingSlotGroupsCallback()
    {
        matchingSequence = DOTween.Sequence();

        int middleIndex = matchingSlotIndexs[1];
        int leftIndex = matchingSlotIndexs[0];
        int rightIndex = matchingSlotIndexs[2];

        Transform middle = slots[middleIndex].slotProp.transform;
        Transform left = slots[leftIndex].slotProp.transform;
        Transform right = slots[rightIndex].slotProp.transform;

        Vector3 moveUpOffset = Vector3.up * 0.25f; // Upward movement amount
        Vector3 middleTarget = middle.position + moveUpOffset;
        Vector3 leftTarget = left.position + moveUpOffset;
        Vector3 rightTarget = right.position + moveUpOffset;

        float tiltAngle = 15f;  // Degrees to tilt the left and right props
        float moveDuration = 0.15f;
        float tiltDuration = 0.25f;
        float popScaleDuration = 0.05f;
        float mergeDuration = 0.1f; // Slightly fast bang effect

        // Move all three props up together
        Tween middleMoveUp = middle.DOMove(middleTarget, moveDuration).SetEase(Ease.OutQuad);
        Tween leftMoveUp = left.DOMove(leftTarget, moveDuration).SetEase(Ease.OutQuad);
        Tween rightMoveUp = right.DOMove(rightTarget, moveDuration).SetEase(Ease.OutQuad);

        // Left and right props tilt slightly toward the middle
        Tween leftTilt = left.DORotate(new Vector3(0, 0, tiltAngle), tiltDuration).SetEase(Ease.InOutQuad);
        Tween rightTilt = right.DORotate(new Vector3(0, 0, -tiltAngle), tiltDuration).SetEase(Ease.InOutQuad);
        
        // pop scale effect
        float targetScale = 1.1f;
        Vector3 scaleDifference = left.localScale * targetScale - left.localScale;
        
        Tween leftPopScale = left.DOPunchScale(scaleDifference, popScaleDuration, 5, 0);
        Tween rightPopScale = right.DOPunchScale(scaleDifference, popScaleDuration, 5, 0);

        // "Bang" effect: Left and Right move quickly into the middle prop
        Tween leftMerge = left.DOMove(middleTarget, mergeDuration).SetEase(Ease.InQuad);
        Tween rightMerge = right.DOMove(middleTarget, mergeDuration).SetEase(Ease.InQuad);

        // Play matched star tween
        UIManager.instance.starCountHandler.PlayStarAddTween();

        // Add bonus star
        UIManager.instance.bonusStarHandler.AddBonusStar();

        // Sequence setup
        matchingSequence.Append(middleMoveUp);
        matchingSequence.Join(leftMoveUp);
        matchingSequence.Join(rightMoveUp);
        matchingSequence.Append(leftTilt);
        matchingSequence.Join(rightTilt);
        matchingSequence.Append(leftPopScale);
        matchingSequence.Join(rightPopScale);
        matchingSequence.Append(leftMerge);
        matchingSequence.Join(rightMerge);
        matchingSequence.AppendCallback(() =>
        {
            Destroy(left.gameObject);
            Destroy(right.gameObject);
            Destroy(middle.gameObject);

            //slots[middleIndex].slotVFX.Play();
            SFXManager.instance.PlayPropMatchedSound();

            Instantiate(WorldManager.instance.matchVFX, middleTarget, Quaternion.identity)
                                        .TryGetComponent<ParticleSystem>(out ParticleSystem matchVFX);
            Destroy(matchVFX.gameObject, 1f);

            if (GameManager.instance.levelManager.shelfManager.IsLevelDone())
            {
                UIManager.instance.OnLevelDone.Invoke();
            }

            // Set Combo Text
            matchCount++;
            if (matchCount > 2)
            {
                isComboActive = true;
                UIManager.instance.comboTextHandler.SetComboText(middleTarget);
            }

            // Set Level Progress
            float currentProgress = (GameManager.instance.levelManager.shelfManager.GetPropCount() / (float)GameManager.instance.levelManager.numberOfProps) * 100;
            UIManager.instance.progressBar.SetProgress(100 - currentProgress);

            // Star animation
            GameObject star = Instantiate(WorldManager.instance.starPrefab, middleTarget, Quaternion.identity);
            AnimateStar(star, middleIndex);
            
            currentSlotManagerState = SlotManagerState.Done;
        });

        // Clear slot references
        slots[leftIndex].slotProp = null;
        slots[rightIndex].slotProp = null;
        slots[middleIndex].slotProp = null;
        matchingSlotIndexs.Clear();
    }

    public void AnimateStar(GameObject star, int midIndex)
    {
        Vector3 startPoint = star.transform.position;
        Vector3 endPoint = UIManager.instance.starCountHandler.starImage.transform.position;

        Vector3 screenTopRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        Vector3 screenBottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));

        int direction = -1;
        if (midIndex > 3) direction = 1;

        Vector3 midPoint = new Vector3(
            (startPoint.x + endPoint.x) / 2 + direction,
            (startPoint.y + endPoint.y) / 2 + (screenTopRight.y * 0.2f), 
            0
        );

        midPoint.x = Mathf.Clamp(midPoint.x, screenBottomLeft.x, screenTopRight.x);
        midPoint.y = Mathf.Clamp(midPoint.y, screenBottomLeft.y, screenTopRight.y);

        Vector3[] pathPoints = { startPoint, midPoint, endPoint };

        star.transform.DOPath(pathPoints, 1f, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                Destroy(star);
                UIManager.instance.starCountHandler.AddStar();
            });
    }

    public void EnableExtraSlot()
    {
        slots.Add(extraSlot);
        extraSlot.gameObject.SetActive(true);
        extraSlot.slotProp = null;

        transform.position = new Vector3(-0.5f, transform.position.y, transform.position.z);
    }

    public void DisableExtraSlot()
    {
        slots.Remove(extraSlot);
        extraSlot.gameObject.SetActive(false);

        transform.position = new Vector3(0, transform.position.y, transform.position.z);
    }

    public bool ClearAllSlots()
    {
        pickCount = 0;
        matchCount = 0;
        isComboActive = false;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].slotProp != null)
            {
                Destroy(slots[i].slotProp.gameObject);
                slots[i].slotProp = null;

                if (props.Contains(slots[i].slotProp))
                {
                    props.Remove(slots[i].slotProp);
                }
            }
        }

        // Clear any remaining prop references that didn't make into slots
        foreach (var prop in props)
        {
            try
            {
                Destroy(prop.gameObject);
            }
            catch
            {
                Debug.Log("prop ref no longer exists!!");
            }
        }
        props.Clear();

        return true;
    }

    public void ResetSlot(Prop prop)
    {
        foreach (var slot in slots)
        {
            if (slot.slotProp == prop)
            {
                slot.slotProp = null;
                break;
            }
        }
    }
}