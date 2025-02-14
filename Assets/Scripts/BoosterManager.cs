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

    }

    public void HandleShuffleBooster()
    {

    }
}
