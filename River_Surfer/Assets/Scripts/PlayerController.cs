using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [Header("Forward Speed")]
    public float forwardSpeed = 20f;

    [Header("Lane Movement")]
    public float laneSwitchTime = 0.35f; 
    [SerializeField] private float rollAngle = 14f; 

    [Header("Jump Physics")]
    [Tooltip("גובה השיא של הקפיצה - הותאם לקשת ארוכה יותר")]
    [SerializeField] private float jumpHeight = 3.8f; 
    [Tooltip("משך כל הקפיצה בשניות - 1.4 שניות מכסות 14 מטר קדימה")]
    [SerializeField] private float jumpDuration = 1.4f;
    [Tooltip("זווית נטיית החרטום בקפיצה")]
    [SerializeField] private float jumpPitchAngle = 22f;

    [Header("Dive Physics")]
    [Tooltip("עומק הצלילה מתחת למים")]
    [SerializeField] private float diveDepth = -2.8f; 
    [Tooltip("משך הצלילה בשניות - 1.3 שניות מכסות 13 מטר קדימה")]
    [SerializeField] private float diveDuration = 1.3f;
    [Tooltip("זווית נטיית החרטום בצלילה")]
    [SerializeField] private float divePitchAngle = 20f;

    [Header("Visual Anchor (Optional)")]
    [SerializeField] private Transform visualModel;

    private int currentLane = 1; 
    private bool isSwitchingLane = false;
    private bool isVerticalMoving = false;
    private float initialY = 0f;

    private Transform ModelTransform => visualModel != null ? visualModel : transform;

    void Start()
    {
        initialY = transform.position.y;

        if (TrackManager.instance != null && TrackManager.instance.laneXPositions.Length > 0)
        {
            currentLane = TrackManager.instance.laneXPositions.Length / 2;
            
            Vector3 startPos = transform.position;
            startPos.x = TrackManager.instance.laneXPositions[currentLane];
            transform.position = startPos;
        }
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        HandleInput();
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
    }

    private void HandleInput()
    {
        if (!isSwitchingLane)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) SwitchLane(-1);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) SwitchLane(1);
        }

        if (!isVerticalMoving)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) PerformJump();
            else if (Input.GetKeyDown(KeyCode.DownArrow)) PerformDive();
        }
    }

    private void SwitchLane(int direction)
    {
        if (TrackManager.instance == null) return;

        int targetLane = currentLane + direction;
        int maxLaneIndex = TrackManager.instance.laneXPositions.Length - 1;

        if (targetLane < 0 || targetLane > maxLaneIndex) return;

        currentLane = targetLane;
        isSwitchingLane = true;

        float targetX = TrackManager.instance.laneXPositions[currentLane];

        transform.DOMoveX(targetX, laneSwitchTime)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => isSwitchingLane = false);

        float targetRoll = -direction * rollAngle;
        Sequence rollSequence = DOTween.Sequence();
        rollSequence.Append(ModelTransform.DOLocalRotate(new Vector3(0, 0, targetRoll), laneSwitchTime * 0.45f).SetEase(Ease.OutSine));
        rollSequence.Append(ModelTransform.DOLocalRotate(Vector3.zero, laneSwitchTime * 0.55f).SetEase(Ease.InSine));
    }

    private void PerformJump()
    {
        isVerticalMoving = true;
        Sequence jumpSeq = DOTween.Sequence();

        float halfDuration = jumpDuration * 0.5f;

        // 1. קשת גובה מלאה ומרווחת
        jumpSeq.Append(transform.DOMoveY(initialY + jumpHeight, halfDuration).SetEase(Ease.OutQuad));
        jumpSeq.Append(transform.DOMoveY(initialY, halfDuration).SetEase(Ease.InQuad));

        // 2. תנועת חרטום אורגנית המסונכרנת לפי מחצית משך הקשת
        Sequence pitchSeq = DOTween.Sequence();
        pitchSeq.Append(ModelTransform.DOLocalRotate(new Vector3(-jumpPitchAngle, 0, 0), halfDuration * 0.65f).SetEase(Ease.OutSine));
        pitchSeq.Append(ModelTransform.DOLocalRotate(new Vector3(jumpPitchAngle * 0.8f, 0, 0), halfDuration * 0.95f).SetEase(Ease.InOutSine));
        pitchSeq.Append(ModelTransform.DOLocalRotate(Vector3.zero, 0.18f).SetEase(Ease.OutSine));

        jumpSeq.Join(pitchSeq);

        // 3. שיכוך נחיתה במים וחזרה למפלס
        jumpSeq.Append(transform.DOMoveY(initialY - 0.25f, 0.12f).SetEase(Ease.OutSine));
        jumpSeq.Append(transform.DOMoveY(initialY, 0.15f).SetEase(Ease.InSine));

        jumpSeq.OnComplete(() => isVerticalMoving = false);
    }

    private void PerformDive()
    {
        isVerticalMoving = true;
        Sequence diveSeq = DOTween.Sequence();

        float halfDuration = diveDuration * 0.5f;

        // 1. קשת צלילה עמוקה וממושכת
        diveSeq.Append(transform.DOMoveY(initialY + diveDepth, halfDuration).SetEase(Ease.OutQuad));
        diveSeq.Append(transform.DOMoveY(initialY, halfDuration).SetEase(Ease.InQuad));

        // 2. תנועת חרטום מותאמת לצלילה
        Sequence pitchSeq = DOTween.Sequence();
        pitchSeq.Append(ModelTransform.DOLocalRotate(new Vector3(divePitchAngle, 0, 0), halfDuration * 0.6f).SetEase(Ease.OutSine));
        pitchSeq.Append(ModelTransform.DOLocalRotate(new Vector3(-divePitchAngle * 0.7f, 0, 0), halfDuration * 0.95f).SetEase(Ease.InOutSine));
        pitchSeq.Append(ModelTransform.DOLocalRotate(Vector3.zero, 0.18f).SetEase(Ease.OutSine));

        diveSeq.Join(pitchSeq);

        // 3. שבירת גלים ויישור עם המים
        diveSeq.Append(transform.DOMoveY(initialY + 0.18f, 0.12f).SetEase(Ease.OutSine));
        diveSeq.Append(transform.DOMoveY(initialY, 0.12f).SetEase(Ease.InSine));

        diveSeq.OnComplete(() => isVerticalMoving = false);
    }
}