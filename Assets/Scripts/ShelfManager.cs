using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShelfManager : MonoBehaviour
{
    public List<ShelfGrid> shelfGrids;

    List<Prop> remainingProps;

    private void Awake()
    {
        shelfGrids = GetComponentsInChildren<ShelfGrid>().ToList();
    }

    public void StockShelfs(List<Prop> origPropList)
    {
        remainingProps = new List<Prop>(origPropList);

        List<List<Prop>> propPairs = new List<List<Prop>>();
        for (int i = 0; i < remainingProps.Count; i += 3)
        {
            List<Prop> pair = new List<Prop> { remainingProps[i] };
            if (i + 1 < remainingProps.Count) pair.Add(remainingProps[i + 1]);
            if (i + 2 < remainingProps.Count) pair.Add(remainingProps[i + 2]);

            propPairs.Add(pair);
        }
        
        Utils.ShuffleList(ref propPairs);
        
        remainingProps = propPairs.SelectMany(p => p).ToList();

        while (remainingProps.Count > 0)
        {
            foreach (var shelf in shelfGrids)
            {
                StartCoroutine(shelf.SetProps(remainingProps,
                    (receivedProps) => remainingProps = receivedProps));
                
                if (remainingProps.Count == 0) break;
            }
        }
    }

    public bool IsLevelDone()
    {
        if (GetPropCount() <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetPropCount()
    {
        int propCount = 0;

        foreach (var shelfGrid in shelfGrids)
        {
            foreach (var shelfLayer in shelfGrid.shelfPropList)
            {
                propCount += shelfLayer.Count;
            }
        }

        return propCount;
    }

    public bool ClearGrids(bool reset = false)
    {
        foreach (var shelfGrid in shelfGrids)
        {
            var shelfPropsList = new List<List<Prop>>(shelfGrid.shelfPropList);

            foreach (var shelfLayers in shelfPropsList)
            {
                foreach (var prop in shelfLayers)
                {
                    if (prop == null) continue;

                    if (reset)
                    {
                        prop.ResetProp();
                    }
                    else
                    {
                        Destroy(prop.gameObject);
                    }
                }
            }

            shelfGrid.shelfPropList.Clear();
            if (reset) shelfGrid.Awake();
        }

        remainingProps.Clear();

        return true;
    }
}