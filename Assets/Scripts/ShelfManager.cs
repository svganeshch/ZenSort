using System.Collections.Generic;
using UnityEngine;

public class ShelfManager : MonoBehaviour
{
    public ShelfGrid[] shelfGrids;

    List<Prop> remainingProps;

    private void Awake()
    {
        shelfGrids = GetComponentsInChildren<ShelfGrid>();
    }

    public void StockShelfs(List<Prop> origPropList)
    {
        remainingProps = new List<Prop>(origPropList);
        remainingProps.Sort((a, b) => b.propCollider.bounds.size.x.CompareTo(a.propCollider.bounds.size.x));

        while (remainingProps.Count > 0)
        {
            foreach (var shelf in shelfGrids)
            {
                remainingProps = shelf.SetProps(remainingProps);
            }
        }
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