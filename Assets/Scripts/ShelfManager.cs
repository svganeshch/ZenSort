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
        StartCoroutine(StockShelfsRoutine(origPropList));
    }

    private IEnumerator StockShelfsRoutine(List<Prop> origPropList)
    {
        remainingProps = new List<Prop>(origPropList);
        remainingProps.Sort((a, b) => b.propCollider.bounds.size.x.CompareTo(a.propCollider.bounds.size.x));

        while (remainingProps.Count > 0)
        {
            foreach (var shelf in shelfGrids)
            {
                yield return StartCoroutine(shelf.SetProps(remainingProps,
                    (receivedProps) => remainingProps = receivedProps));
            }
        }

        // Ensure all props have been placed before generating the shelf tree
        //foreach (var shelf in shelfGrids)
        //{
        //    shelf.UpdatePropsState();
        //}
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

    public bool ClearGrids()
    {
        foreach (var shelfGrid in shelfGrids)
        {
            var shelfPropsList = new List<List<Prop>>(shelfGrid.shelfPropList);

            foreach (var shelfLayers in shelfPropsList)
            {
                foreach (var prop in shelfLayers)
                {
                    if (prop == null) continue;

                    Destroy(prop.gameObject);
                }
            }

            shelfGrid.shelfPropList.Clear();
        }

        remainingProps.Clear();

        return true;
    }
}