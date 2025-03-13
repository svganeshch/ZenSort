using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class InputHandler : MonoBehaviour
{
    PlayerInput playerInput;

    InputAction touchPositionAction;
    InputAction touchPressAction;
    
    public bool showMatch = false;
    private float matchCheckDelay = 3f;
    private float currentTime = 0f;
    private float previousTouchTime = 0f;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        touchPositionAction = playerInput.actions["TouchPosition"];
        touchPressAction = playerInput.actions["TouchPress"];
    }

    private void Start()
    {
        touchPressAction.performed += TouchPress;
        
        StartCoroutine(StartMatchFinderCounterCoroutine());
    }

    private void TouchPress(InputAction.CallbackContext ctx)
    {
        if (GameManager.currentGameState == GameState.Paused) return;
        
        ResetMatchCheck();
        
        Vector2 touchPosition = touchPositionAction.ReadValue<Vector2>();

        //Debug.Log("touch tap pos : " + touchPosition);

        Ray ray = Camera.main.ScreenPointToRay(touchPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null)
            {
                //Debug.Log("hit : " + hit.collider.gameObject.name);

                if (hit.collider.gameObject.TryGetComponent<Prop>(out Prop propObj))
                {
                    propObj.OnPicked();
                }
            }
        }
    }

    private void ResetMatchCheck()
    {
        previousTouchTime = Time.time;
        currentTime = 0;
        showMatch = false;

        if (MatchFinder.instance.matchesPropSeq != null)
        {
            if (MatchFinder.instance.matchesPropSeq.IsPlaying())
            {
                MatchFinder.instance.matchesPropSeq.Kill(true);
                MatchFinder.instance.ResetMatchProps();
            }
        }
    }

    private IEnumerator StartMatchFinderCounterCoroutine()
    {
        while (true)
        {
            while (showMatch) yield return null;
            
            currentTime = Time.time - previousTouchTime;

            if (currentTime >= matchCheckDelay)
            {
                Debug.Log("Match check counter triggered");
                showMatch = true;
                MatchFinder.instance.ShowMatches();
                
                currentTime = 0;
                previousTouchTime = Time.time;
            }
            
            yield return null;
        }
    }
}