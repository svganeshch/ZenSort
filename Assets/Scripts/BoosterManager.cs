using System.Collections.Generic;
using UnityEngine;

public class BoosterManager : MonoBehaviour
{
    public static BoosterManager instance;

    public static Prop previousPickedProp;

    private void Awake()
    {
        instance = this;
    }

    public void HandleUndoBooster()
    {
        if (previousPickedProp == null) return;

        previousPickedProp.PropUndo();
    }

    public void HandleMagnetBooster()
    {
        if (GameManager.instance.slotManager.currentSlotManagerState == SlotManagerState.Matching) return;

        var shelfGrids = GameManager.instance.levelManager.shelfManager.shelfGrids;
        var slots = GameManager.instance.slotManager.slots;

        string propToPull = "";
        int emptySlotCount = 0;
        int count = 0;
        int propsToPull = 0;

        for (int i = 0; i < slots.Count - 1; i++)
        {
            var firstProp = slots[0].slotProp;
            var slotProp = slots[i].slotProp;
            var nextProp = slots[i + 1].slotProp;

            if (slotProp != null && nextProp != null)
            {
                if (slotProp.name == nextProp.name)
                {
                    propToPull = slots[i].slotProp.name;
                    propsToPull = 1;
                    break;
                }
            }

            if (firstProp != null)
            {
                propToPull = firstProp.name;
                propsToPull = 2;
            }

            foreach (var slot in slots)
            {
                if (slot.slotProp == null)
                {
                    emptySlotCount++;
                }
            }

            if (emptySlotCount == slots.Count)
            {
                var randomShelfGrid = shelfGrids[Random.Range(0, shelfGrids.Length - 1)].shelfPropList;
                var randomLayer = randomShelfGrid[Random.Range(0, randomShelfGrid.Count - 1)];
                var randomProp = randomLayer[Random.Range(0, randomLayer.Count - 1)];

                propToPull = randomProp.name;
                propsToPull = 3;
            }

            if (emptySlotCount < propsToPull) return;
        }

        foreach (var shelfGrid in shelfGrids)
        {
            var shelfLayersList = new List<List<Prop>>(shelfGrid.shelfPropList);

            foreach (var shelfLayer in shelfLayersList)
            {
                var shelfLayerProps = new List<Prop>(shelfLayer);

                foreach (var prop in shelfLayerProps)
                {
                    if (count == propsToPull)
                    {
                        return;
                    }

                    if (prop.name == propToPull)
                    {
                        prop.SetPropState(true);
                        prop.OnPicked();

                        count++;
                    }
                }
            }
        }
    }

    public void HandleShuffleBooster()
    {
        StartCoroutine(GameManager.instance.levelManager.ShuffleLevel());
    }
}
