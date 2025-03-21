using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PropNode
{
    public Prop prop;
    public List<PropNode> children;
 
    public PropNode(Prop prop)
    {
        this.prop = prop;
        children = new List<PropNode>();
    }
}

public class ShelfGrid : MonoBehaviour
{
    BoxCollider shelfCollider;

    private float shelfPaddingX = 0.025f;
    private float shelfPaddingZ = 0.55f;
    private float propOverlapBoxDepth = 0.55f;

    private float propOverlapBoxReduction = 1f - 0.025f;

    float shelfLeft;
    float shelfRight;
    float shelfY;
    float shelfZ;
    float currentShelfPosX;
    float currentShelfPosZ;

    int currentLayer;

    private List<KeyValuePair<Vector3, Prop>> propsToPlace = new List<KeyValuePair<Vector3, Prop>>();

    private List<Prop> propsMovedInPreviousPick = new List<Prop>();
    public List<List<Prop>> shelfPropList = new List<List<Prop>>();
    
    public List<PropNode> shelfTree = new List<PropNode>();

    public void Awake()
    {
        shelfCollider = GetComponent<BoxCollider>();

        shelfLeft = shelfCollider.bounds.min.x;
        shelfRight = shelfCollider.bounds.max.x;

        shelfY = shelfCollider.bounds.max.y;
        shelfZ = shelfCollider.bounds.max.z;

        currentShelfPosX = shelfLeft;
        currentShelfPosZ = shelfZ;

        currentLayer = -1;
    }

    public IEnumerator SetProps(List<Prop> allProps, Action<List<Prop>> sendRemainingProps)
    {
        var currentPropList = new List<Prop>(allProps);
        var currentPlacedProps = new List<Prop>();

        currentShelfPosX = shelfLeft;
        currentShelfPosZ = shelfZ;

        propsToPlace.Clear();

        currentLayer++;

        foreach (Prop prop in currentPropList)
        {
            if (prop.isPicked)
            {
                allProps.Remove(prop);
                continue;
            }
            
            float propWidth = prop.propSize.x;
            //float propY = shelfY + (prop.propCollider.size.y / 2) - prop.propCollider.center.y;
            float propY = shelfY + (prop.propCollider.size.y * prop.transform.localScale.y / 2) - (prop.propCollider.center.y * prop.transform.localScale.y);
            float propDepth = prop.propSize.z;

            if (currentShelfPosX + propWidth <= shelfRight)
            {
                currentShelfPosZ = shelfZ - (currentLayer * shelfPaddingZ) - (propDepth / 2);

                Vector3 propPos = new Vector3(currentShelfPosX + (propWidth / 2), propY, currentShelfPosZ);

                currentPlacedProps.Add(prop);
                allProps.Remove(prop);

                //SetProp(prop, propPos);
                propsToPlace.Add(new KeyValuePair<Vector3, Prop>(propPos, prop));

                currentShelfPosX += propWidth + shelfPaddingX;
            }
        }

        shelfPropList.Add(currentPlacedProps);

        currentPropList = allProps;
        currentPropList = TryFitInGaps(currentPropList);

        yield return StartCoroutine(SetPropsPos());

        sendRemainingProps(currentPropList);
    }

    public List<Prop> TryFitInGaps(List<Prop> propsToFit)
    {
        var currentLayerProps = shelfPropList[currentLayer];
        var remainingProps = new List<Prop>(propsToFit);

        for (int i = 0; i < currentLayerProps.Count - 1; i++)
        {
            float gapStart = currentLayerProps[i].transform.position.x + (currentLayerProps[i].propSize.x / 2);
            float gapEnd = currentLayerProps[i + 1].transform.position.x - (currentLayerProps[i + 1].propSize.x / 2);
            float availableSpace = gapEnd - gapStart;

            for (int j = 0; j < remainingProps.Count; j++)
            {
                Prop prop = remainingProps[j];

                if (prop.propSize.x <= availableSpace)
                {
                    //float propY = shelfY + (prop.propSize.y / 2) - prop.propCollider.center.y;
                    float propY = shelfY + (prop.propSize.y * prop.transform.localScale.y / 2) - (prop.propCollider.center.y * prop.transform.localScale.y);
                    float propZ = currentShelfPosZ;
                    float propX = gapStart + (prop.propSize.x / 2);

                    Vector3 propPos = new Vector3(propX, propY, propZ);

                    shelfPropList[currentLayer].Add(prop);
                    propsToFit.Remove(prop);
                    remainingProps.RemoveAt(j);

                    propsToPlace.Add(new KeyValuePair<Vector3, Prop>(propPos, prop));
                    //SetProp(prop, propPos);

                    break;
                }
            }
        }

        return propsToFit;
    }

    private IEnumerator SetPropsPos()
    {
        Sequence setPropsSeq = DOTween.Sequence();

        foreach (var propValPair in propsToPlace)
        {
            Prop prop = propValPair.Value;
            Vector3 propPos = propValPair.Key;

            prop.shelfGrid = this;
            prop.propLayer = currentLayer;

            prop.SetPosition(propPos);
            
            if (currentLayer > 0)
            {
                 prop.SetPropState(false);
            }
            
            if (currentLayer == 0) setPropsSeq.Join(prop.PlaySpawnTween());
        }

        yield return null;

        //UpdatePropsState();
    }
    
    private void GenerateRootNodes()
    {
        foreach (var prop in shelfPropList[0])
        {
            PropNode parentNode = new PropNode(prop);
            shelfTree.Add(parentNode);
        }
    }
    
    public void GenerateShelfTree()
    {
        shelfTree.Clear();
        GenerateRootNodes();
        
        // 2nd layer props are always blocked by parent nodes so just
        // make them as child of blocked parents
        if (shelfPropList.Count > 1)
        {
            foreach (var prop in shelfPropList[1])
            {
                var blockedProps = GetBlockedProps(prop);

                foreach (var bProp in blockedProps)
                {
                    if (bProp.gameObject == prop.gameObject) continue;

                    foreach (var parentNode in shelfTree)
                    {
                        if (bProp.gameObject == parentNode.prop.gameObject)
                        {
                            PropNode childNode = new PropNode(prop);
                            parentNode.children.Add(childNode);
                        }
                    }
                }
            }

            // From 3rd layer check each prop's blocked prop and see which parents node
            // child it belongs to while checking through the depth of all children
            for (int i = 2; i < shelfPropList.Count; i++)
            {
                var propsList = shelfPropList[i];

                foreach (var prop in propsList)
                {
                    var blockedProps = GetBlockedProps(prop);

                    foreach (var bProp in blockedProps)
                    {
                        if (bProp.gameObject == prop.gameObject) continue;

                        foreach (var parentNode in shelfTree)
                        {
                            // Recursively find the node containing bProp
                            PropNode foundNode = FindNodeRecursive(parentNode, bProp.gameObject);

                            if (foundNode != null)
                            {
                                PropNode newChildNode = new PropNode(prop);
                                foundNode.children.Add(newChildNode);
                            }
                        }
                    }
                }
            }
        }
    }
    
    private PropNode FindNodeRecursive(PropNode currentNode, GameObject targetProp)
    {
        // If current node matches, return it
        if (currentNode.prop.gameObject == targetProp)
            return currentNode;

        // Recursively search in child nodes
        foreach (var child in currentNode.children)
        {
            PropNode found = FindNodeRecursive(child, targetProp);
            if (found != null)
                return found;
        }

        return null; // Not found
    }

    private Collider[] GetBlockedProps(Prop prop)
    {
        Vector3 overlapBoxSize = new Vector3(prop.propSize.x * propOverlapBoxReduction, prop.propSize.y * propOverlapBoxReduction, propOverlapBoxDepth);
        Vector3 frontOffset = prop.transform.forward * (prop.propSize.z * 0.5f + overlapBoxSize.z * 0.5f);
        Vector3 overlapBoxPos = prop.propCollider.bounds.center + frontOffset;
 
        Collider[] colliders = Physics.OverlapBox(overlapBoxPos, overlapBoxSize / 2, prop.transform.rotation, WorldLayerMaskManager.instance.propLayerMask);
 
        return colliders;
    }

    public void UpdatePropsState()
    {
        for (int i = 0; i < shelfPropList.Count; i++)
        {
            var propList = new List<Prop>(shelfPropList[i]);

            foreach (var prop in propList)
            {
                if (prop == null) continue;

                //if (prop.propLayer > 0) continue;

                prop.SetPropState(!IsPropBlocked(prop));
            }
        }
    }

    public void UpdateShelfTreeProps(Prop pickedProp)
    {
        PropNode foundNode = null;
        List<Prop> layerPropsToMove = new List<Prop>();

        foreach (var parentNode in shelfTree)
        {
            foundNode = FindNodeRecursive(parentNode, pickedProp.gameObject);

            if (foundNode != null)
            {
                foreach (var childNode in foundNode.children)
                {
                    if (CountParents(childNode.prop) == 1) // Move only if it has a single parent
                    {
                        CollectValidDescendants(childNode, layerPropsToMove);
                    }
                }
            }
        }

        StartCoroutine(MovePropForward(layerPropsToMove));
    }

    // Recursively collect all descendants, considering parent count
    private void CollectValidDescendants(PropNode node, List<Prop> propsToMove)
    {
        if (CountParents(node.prop) == 1) // Ensure it still has a single parent
        {
            propsToMove.Add(node.prop);

            foreach (var child in node.children)
            {
                CollectValidDescendants(child, propsToMove);
            }
        }
    }

    // Counts how many parents reference a specific prop
    private int CountParents(Prop prop)
    {
        int count = 0;

        foreach (var parentNode in shelfTree)
        {
            if (HasChildRecursive(parentNode, prop))
            {
                count++;
            }
        }

        return count;
    }

    // Recursively checks if a node has the target prop as a child
    private bool HasChildRecursive(PropNode parentNode, Prop targetProp)
    {
        if (parentNode.children.Any(child => child.prop == targetProp))
        {
            return true;
        }

        foreach (var child in parentNode.children)
        {
            if (HasChildRecursive(child, targetProp))
            {
                return true;
            }
        }

        return false;
    }

    public IEnumerator UpdateShelf(Prop pickedProp)
    {
        int startLayer = pickedProp != null ? pickedProp.propLayer + 1 : 0;

        propsMovedInPreviousPick.Clear();

        for (int i = startLayer; i < shelfPropList.Count; i++)
        {
            var propList = new List<Prop>(shelfPropList[i]);
            List<Prop> layerPropsToMove = new List<Prop>();

            foreach (var prop in propList)
            {
                if (prop == null) continue;

                if (!IsPropBlocked(prop))
                {
                    shelfPropList[i].Remove(prop);
                    
                    var currentPropLayer = Mathf.Max(0, i - 1);
                    prop.propLayer = currentPropLayer;
                    shelfPropList[currentPropLayer].Add(prop);

                    layerPropsToMove.Add(prop);
                }
            }

            yield return StartCoroutine(MovePropForward(layerPropsToMove));
        }
    }

    public IEnumerator MovePropForward(List<Prop> propsToMove)
    {
        Sequence moveFwdSeq = DOTween.Sequence();

        foreach (var propToMove in propsToMove)
        {
            if (propToMove.transform.position.z >= shelfZ) continue;

            Vector3 shiftPos = propToMove.transform.position;
            shiftPos.z = Mathf.Min(shelfZ, shiftPos.z + shelfPaddingZ);

            propToMove.previousPos = propToMove.transform.position;

            propsMovedInPreviousPick.Add(propToMove);

            Tween moveFwdTween = propToMove.transform.DOMove(shiftPos, 0.15f).SetEase(Ease.InOutSine);
            
            shelfPropList[propToMove.propLayer].Remove(propToMove);
            
            var currentPropLayer = propToMove.propLayer - 1;
            propToMove.propLayer = currentPropLayer;
            shelfPropList[currentPropLayer].Add(propToMove);

            moveFwdSeq.Join(moveFwdTween);
        }

        yield return moveFwdSeq.WaitForCompletion();

        GenerateShelfTree();
        UpdatePropsState();
    }

    private bool IsPropBlocked(Prop prop)
    {
        //Vector3 propSize = prop.propCollider.bounds.size;
        Vector3 propCenter = prop.propCollider.bounds.center;

        Vector3 overlapBoxSize = new Vector3(prop.propSize.x * propOverlapBoxReduction, prop.propSize.y * propOverlapBoxReduction, propOverlapBoxDepth);
        Vector3 frontOffset = prop.transform.forward * (prop.propSize.z * 0.5f + overlapBoxSize.z * 0.5f);
        Vector3 overlapBoxPos = propCenter + frontOffset;

        Collider[] colliders = Physics.OverlapBox(overlapBoxPos, overlapBoxSize / 2, prop.transform.rotation, WorldLayerMaskManager.instance.propLayerMask);
        bool isBlocked = colliders.Any(c => c != prop.propCollider);

        return isBlocked;
    }
    
    private void ShiftPropTween(Prop prop, Vector3 shiftPos)
    {
        propsMovedInPreviousPick.Add(prop);

        shiftPos = new Vector3(prop.transform.position.x, prop.transform.position.y, prop.transform.position.z + shelfPaddingZ);

        prop.transform.DOMove(shiftPos, 0.25f).SetEase(Ease.InQuad);
    }

    public IEnumerator OnPropUndo(Prop prop)
    {
        Sequence propUndoMoveSeq = DOTween.Sequence();

        foreach (var previousProp in propsMovedInPreviousPick)
        {
            previousProp.SetPropState(false);
            
            Tween moveTween = previousProp.SetPositionTween(previousProp.previousPos);
            moveTween.OnComplete(() =>
            {
                shelfPropList[previousProp.propLayer].Remove(previousProp);
                shelfPropList[previousProp.propLayer + 1].Add(previousProp);
                previousProp.propLayer = previousProp.propLayer + 1;
            });

            propUndoMoveSeq.Join(moveTween);
        }

        yield return propUndoMoveSeq.WaitForCompletion();

        UpdatePropsState();
        propsMovedInPreviousPick.Clear();
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        if (shelfTree == null) return;

        foreach (var rootNode in shelfTree)
        {
            DrawNodeConnections(rootNode);
        }

        // for (int i = 0; i < shelfPropList.Count; i++)
        // {
        //     var currentLayerPropList = shelfPropList[i];
        //
        //     foreach (var prop in currentLayerPropList)
        //     {
        //         Gizmos.color = Color.blue;
        //
        //         if (prop == null) continue;
        //
        //         //Vector3 propSize = prop.propCollider.size;
        //         Vector3 propCenter = prop.propCollider.bounds.center;
        //
        //         Vector3 overlapBoxSize = new Vector3(prop.propSize.x * propOverlapBoxReduction, prop.propSize.y * propOverlapBoxReduction, propOverlapBoxDepth);
        //
        //         Vector3 frontOffset = prop.transform.forward * (prop.propSize.z * 0.5f + overlapBoxSize.z * 0.5f);
        //         Vector3 overlapBoxPos = propCenter + frontOffset;
        //
        //         Gizmos.DrawWireCube(overlapBoxPos, overlapBoxSize);
        //     }
        // }
    }
    
    private void DrawNodeConnections(PropNode node)
    {
        if (node == null || node.prop == null) return;

        Vector3 nodePos = node.prop.transform.position;

        foreach (var child in node.children)
        {
            if (child == null || child.prop == null) continue;

            Vector3 childPos = child.prop.transform.position;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(nodePos, childPos);

            DrawNodeConnections(child);
        }
    }
}