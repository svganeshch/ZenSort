using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private Queue<Func<IEnumerator>> propQueue = new Queue<Func<IEnumerator>>();
    private bool isProcessingQueue = false;

    private void Awake()
    {
        currentSlotManagerState = SlotManagerState.Done;

        slots = GetComponentsInChildren<Slot>().ToList();
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
        currentSlotManagerState = SlotManagerState.Matching;

        int sameColorIndex = slots.FindLastIndex(s => s.slotProp != null && s.slotProp.gameObject.name == prop.gameObject.name);

        if (sameColorIndex != -1)
        {
            int insertIndex = sameColorIndex + 1;

            if (insertIndex >= slots.Count)
                goto SlotsCheck;

            if (slots[insertIndex].slotProp != null)
            {
                yield return StartCoroutine(ShiftSlots(insertIndex));
            }

            bool willCauseMatch = CheckForMatch(insertIndex, prop);

            Tween propTween = slots[insertIndex].SetSlotPositionTween(prop);
            yield return propTween.WaitForCompletion();

            if (willCauseMatch)
            {
                //BoosterManager.Instance.previousTile = null;

                yield return new WaitUntil(() => HandleMatchingSlotGroupsCallback());
                yield return StartCoroutine(RearrangeSlots());
            }
        }
        else
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].slotProp == null)
                {
                    Tween propTween = slots[i].SetSlotPositionTween(prop);
                    yield return propTween.WaitForCompletion();
                    break;
                }
            }
        }

    SlotsCheck:
        if (slots.All(s => s.slotProp != null))
        {
            //UIManager.Instance.gameOverEvent.Invoke();
            Debug.Log("All slots filled");
        }

        OnCompleteCallback?.Invoke();

        currentSlotManagerState = SlotManagerState.Done;
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

    private bool HandleMatchingSlotGroupsCallback()
    {
        foreach (int matchingIndex in matchingSlotIndexs)
        {
            Destroy(slots[matchingIndex].slotProp.gameObject);
            slots[matchingIndex].slotProp = null;

            //slots[matchingIndex].slotVFX.Play();
        }

        //SFXManager.Instance.PlayPropsMatchedSound();

        matchingSlotIndexs.Clear();

        return true;
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