using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class MatchFinder : MonoBehaviour
{
    public static MatchFinder instance;
    private static readonly int emissionColorProperty = Shader.PropertyToID("_EmissionColor");

    public Sequence matchesPropSeq;

    List<Prop> matchingProps = new List<Prop>();
    List<List<Prop>> shelfsFirstLayerProps = new List<List<Prop>>();
    
    private void Awake()
    {
        instance = this;
    }
    
    public void ShowMatches()
    {
        matchesPropSeq = DOTween.Sequence();
        var shelfGrids = GameManager.instance.levelManager.shelfManager.shelfGrids;
        shelfsFirstLayerProps = shelfGrids.Select(shelfGrid => shelfGrid.shelfPropList[0]).ToList();

        var allProps = shelfsFirstLayerProps.SelectMany(x => x).ToList();
        if (allProps.Count == 0) return;

        var slotManager = GameManager.instance.slotManager;
        var slotProp = slotManager.slots[0].slotProp;
        var nextSlotProp = slotManager.slots[1].slotProp;

        Prop propToMatch = null;
        int numberOfPropsToMatch = 0;

        if (slotProp != null)
        {
            propToMatch = slotProp;
            numberOfPropsToMatch = (nextSlotProp != null && slotProp.name == nextSlotProp.name) ? 1 : 2;
        }
        else
        {
            var propGroups = allProps
                .GroupBy(p => p.name)
                .Where(group => group.Count() >= 3)
                .Select(group => group.First())
                .ToList();

            if (propGroups.Count > 0)
            {
                propToMatch = propGroups[Random.Range(0, propGroups.Count)];
                numberOfPropsToMatch = 3;
            }
        }

        if (propToMatch == null)
        {
            Debug.Log("Not enough matching props found.");
            return;
        }

        matchingProps = allProps.Where(p => p.name == propToMatch.name).ToList();

        if (matchingProps.Count < numberOfPropsToMatch)
        {
            Debug.Log("Not enough matching props found.");
            return;
        }

        matchingProps = matchingProps.OrderBy(_ => Random.value).Take(numberOfPropsToMatch).ToList();

        foreach (Prop matchingProp in matchingProps)
        {
            Tween propGlowTween = matchingProp.materials[0]
                .DOColor(Color.gray * 0.5f, emissionColorProperty, 0.75f);

            float targetScale = 1.1f;
            Vector3 scaleDifference = matchingProp.transform.localScale * targetScale - matchingProp.transform.localScale;
            Tween propScaleTween = matchingProp.transform.DOPunchScale(scaleDifference, 0.75f, 0, 0);

            matchesPropSeq.Join(propGlowTween);
            matchesPropSeq.Join(propScaleTween);
            matchesPropSeq.SetLoops(-1, LoopType.Yoyo);
        }

        Debug.Log($"Found matching props: {propToMatch.name}");
    }
    
    public void ResetMatchProps()
    {
        foreach (var matchingProp in matchingProps)
        {
            matchingProp.transform.localScale = matchingProp.origPropScale;
            matchingProp.materials[0].SetColor(emissionColorProperty, Color.clear);
        }
    }
}