using System.Collections.Generic;
using System.Linq;
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
        if (GameManager.instance.slotManager.currentSlotManagerState != SlotManagerState.Done) return;
        if (previousPickedProp == null) return;

        previousPickedProp.PropUndo();
    }

    public void HandleMagnetBooster()
    {
        if (GameManager.instance.slotManager.currentSlotManagerState != SlotManagerState.Done) return;
        if (GameManager.instance.levelManager.shelfManager.GetPropCount() <= 0) return;

        List<ShelfGrid> shelfGrids = new List<ShelfGrid>(GameManager.instance.levelManager.shelfManager.shelfGrids);
        List<Prop> propsToPull = new List<Prop>();
        var slots = GameManager.instance.slotManager.slots;

        string propToPullName = "";
        int emptySlotCount = 0;
        int count = 0;
        int numberPropsToPull = 0;

        for (int i = 0; i < slots.Count - 1; i++)
        {
            var firstProp = slots[0].slotProp;
            var slotProp = slots[i].slotProp;
            var nextProp = slots[i + 1].slotProp;

            if (slotProp != null && nextProp != null)
            {
                if (slotProp.name == nextProp.name)
                {
                    propToPullName = slots[i].slotProp.name;
                    numberPropsToPull = 1;
                    break;
                }
            }

            if (firstProp != null)
            {
                propToPullName = firstProp.name;
                numberPropsToPull = 2;
            }

            foreach (var slot in slots)
            {
                if (slot.slotProp == null)
                {
                    emptySlotCount++;
                }
            }

            // if (emptySlotCount == slots.Count)
            // {
            //     bool foundProp = false;
            //
            //     while (!foundProp)
            //     {
            //         var validShelfGrids = shelfGrids
            //             .Where(grid => grid.shelfPropList != null && grid.shelfPropList.Count > 0)
            //             .ToList();
            //
            //         if (validShelfGrids.Count > 0)
            //         {
            //             var randomShelfGrid = validShelfGrids[Random.Range(0, validShelfGrids.Count)];
            //             var validLayers = randomShelfGrid.shelfPropList
            //                 .Where(layer => layer != null && layer.Count > 0)
            //                 .ToList();
            //
            //             if (validLayers.Count > 0)
            //             {
            //                 var randomLayer = validLayers[Random.Range(0, validLayers.Count)];
            //                 var validProps = randomLayer
            //                     .Where(prop => prop != null)
            //                     .ToList();
            //
            //                 if (validProps.Count > 0)
            //                 {
            //                     var randomProp = validProps[Random.Range(0, validProps.Count)];
            //                     propToPullName = randomProp.name;
            //                     numberPropsToPull = 3;
            //                     foundProp = true;
            //                 }
            //             }
            //         }
            //     }
            // }

            if (emptySlotCount < numberPropsToPull) return;
        }

        foreach (var shelfGrid in shelfGrids)
        {
            var shelfLayersList = new List<List<Prop>>(shelfGrid.shelfPropList);

            foreach (var shelfLayer in shelfLayersList)
            {
                var shelfLayerProps = new List<Prop>(shelfLayer);

                foreach (var prop in shelfLayerProps)
                {
                    if (count == numberPropsToPull)
                    {
                        break;
                    }

                    if (prop.name == propToPullName)
                    {
                        propsToPull.Add(prop);
                        count++;
                    }
                }
            }
        }

        foreach (var prop in propsToPull)
        {
            prop.SetPropState(true);
            prop.OnPicked();

            previousPickedProp = null;
        }
    }

    public void HandleShuffleBooster()
    {
        if (GameManager.instance.slotManager.currentSlotManagerState != SlotManagerState.Done) return;
        if (GameManager.instance.levelManager.shelfManager.GetPropCount() <= 0) return;

        previousPickedProp = null;
        StartCoroutine(GameManager.instance.levelManager.ShuffleLevel());
    }
}
