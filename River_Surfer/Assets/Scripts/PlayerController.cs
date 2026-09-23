using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 2.5f; 
    [SerializeField] private float laneSwitchTime = 0.15f; 

    [Header("Vertical Action Settings")]
    [SerializeField] private float verticalActionDuration = 0.6f; 
    [SerializeField] private float jumpHeight = 2.0f; 
    [SerializeField] private float diveDepth = -1.5f; 

    private int currentLane = 1; // 0 = Left, 1 = Center, 2 = Right
    private bool isSwitchingLane = false;
    private bool isVerticalMoving = false;

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // תנועה אופקית (נתיבים)
        if (!isSwitchingLane)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) SwitchLane(-1);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) SwitchLane(1);
        }

        // תנועה אנכית (קפיצה/צלילה)
        if (!isVerticalMoving)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) PerformVerticalAction(jumpHeight);
            else if (Input.GetKeyDown(KeyCode.DownArrow)) PerformVerticalAction(diveDepth);
        }
    }

    private void SwitchLane(int direction)
    {
        int targetLane = currentLane + direction;

        // חסימת יציאה מגבולות הנתיבים
        if (targetLane < 0 || targetLane > 2) return;

        currentLane = targetLane;
        isSwitchingLane = true;

        float targetX = (currentLane - 1) * laneDistance;

        // אנימציית מעבר נתיב עם DOTween
        transform.DOMoveX(targetX, laneSwitchTime)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => isSwitchingLane = false);
    }

    private void PerformVerticalAction(float targetY)
    {
        isVerticalMoving = true;

        // יצירת רצף (Sequence) שעולה/יורד, ואז חוזר ל-0
        Sequence verticalSequence = DOTween.Sequence();
        verticalSequence.Append(transform.DOMoveY(targetY, verticalActionDuration / 2).SetEase(Ease.OutQuad));
        verticalSequence.Append(transform.DOMoveY(0f, verticalActionDuration / 2).SetEase(Ease.InQuad));
        
        verticalSequence.OnComplete(() => isVerticalMoving = false);
    }
}