using System.Collections.Generic;
using UnityEngine;

public class ShelfManager : MonoBehaviour
{
    ShelfGrid[] shelfGrids;

    private void Awake()
    {
        shelfGrids = GetComponentsInChildren<ShelfGrid>();
    }

    public void StockShelfs(List<Prop> origPropList)
    {
        List<Prop> remainingProps = new List<Prop>(origPropList);
        remainingProps.Sort((a, b) => b.propCollider.bounds.size.x.CompareTo(a.propCollider.bounds.size.x));

        foreach (var shelf in shelfGrids)
        {
            List<Prop> placedProps = new List<Prop>();

            foreach (var prop in remainingProps)
            {
                if (shelf.SetProp(prop))
                {
                    placedProps.Add(prop);
                }
            }

            foreach (var prop in placedProps)
            {
                remainingProps.Remove(prop);
            }
        }

        origPropList.Clear();
        origPropList.AddRange(remainingProps);
    }
}