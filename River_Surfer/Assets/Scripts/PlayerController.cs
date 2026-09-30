using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("סמן ב-V כדי שהסירה לא תיפסל ממכשולים")]
    public bool godMode = false;

    [Header("Forward Speed")]
    public float forwardSpeed = 50f;

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

    [Header("Effects")]
    public ParticleSystem wakeParticles;
    
    private bool isWakePlaying = false; 
    private int currentLane = 1; 
    private bool isSwitchingLane = false;
    private bool isVerticalMoving = false;
    private float initialY = 0f;
    
    // משתנה שנוסף כדי למנוע קלט אחרי פסילה
    public bool isDead = false; 

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
        if (wakeParticles != null)
        {
            // בודק אם הסירה נמצאת בדיוק בגובה פני המים (עם סטייה קטנה למניעת ריצוד)
            bool isTouchingWater = Mathf.Abs(transform.position.y - initialY) < 0.2f;
            
            // השובל יעבוד רק אם השחקן חי, זז קדימה, ונוגע פיזית במים
            bool shouldHaveWake = (!isDead && forwardSpeed > 0f && isTouchingWater);

            // שליטה ישירה ומוחלטת בפליטת החלקיקים ברמת הפריים
            var emission = wakeParticles.emission;
            if (emission.enabled != shouldHaveWake)
            {
                emission.enabled = shouldHaveWake;
            }
        }

        if (isDead || (GameManager.instance != null && GameManager.instance.isGameOver)) return;

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

        jumpSeq.Append(transform.DOMoveY(initialY + jumpHeight, halfDuration).SetEase(Ease.OutQuad));
        jumpSeq.Append(transform.DOMoveY(initialY, halfDuration).SetEase(Ease.InQuad));

        Sequence pitchSeq = DOTween.Sequence();
        pitchSeq.Append(ModelTransform.DOLocalRotate(new Vector3(-jumpPitchAngle, 0, 0), halfDuration * 0.65f).SetEase(Ease.OutSine));
        pitchSeq.Append(ModelTransform.DOLocalRotate(new Vector3(jumpPitchAngle * 0.8f, 0, 0), halfDuration * 0.95f).SetEase(Ease.InOutSine));
        pitchSeq.Append(ModelTransform.DOLocalRotate(Vector3.zero, 0.18f).SetEase(Ease.OutSine));

        jumpSeq.Join(pitchSeq);

        jumpSeq.Append(transform.DOMoveY(initialY - 0.25f, 0.12f).SetEase(Ease.OutSine));
        jumpSeq.Append(transform.DOMoveY(initialY, 0.15f).SetEase(Ease.InSine));

        jumpSeq.OnComplete(() => isVerticalMoving = false);
    }

    private void PerformDive()
    {
        isVerticalMoving = true;
        Sequence diveSeq = DOTween.Sequence();

        float halfDuration = diveDuration * 0.5f;

        diveSeq.Append(transform.DOMoveY(initialY + diveDepth, halfDuration).SetEase(Ease.OutQuad));
        diveSeq.Append(transform.DOMoveY(initialY, halfDuration).SetEase(Ease.InQuad));

        Sequence pitchSeq = DOTween.Sequence();
        pitchSeq.Append(ModelTransform.DOLocalRotate(new Vector3(divePitchAngle, 0, 0), halfDuration * 0.6f).SetEase(Ease.OutSine));
        pitchSeq.Append(ModelTransform.DOLocalRotate(new Vector3(-divePitchAngle * 0.7f, 0, 0), halfDuration * 0.95f).SetEase(Ease.InOutSine));
        pitchSeq.Append(ModelTransform.DOLocalRotate(Vector3.zero, 0.18f).SetEase(Ease.OutSine));

        diveSeq.Join(pitchSeq);

        diveSeq.Append(transform.DOMoveY(initialY + 0.18f, 0.12f).SetEase(Ease.OutSine));
        diveSeq.Append(transform.DOMoveY(initialY, 0.12f).SetEase(Ease.InSine));

        diveSeq.OnComplete(() => isVerticalMoving = false);
    }

    // --- התוספת לטיפול בפסילה (מכסה את כל סוגי המכשולים) ---
    private void OnTriggerEnter(Collider other)
    {
        if (godMode) return; 

        if (other.CompareTag("Obstacle") && !isDead)
        {
            Die();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (godMode) return; 

        if (collision.gameObject.CompareTag("Obstacle") && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        forwardSpeed = 0f; // עוצר את המהירות, מה שיעצור גם את טקסטורת הים

        // עוצר את כל האנימציות של DOTween שרצות כרגע על הסירה
        transform.DOKill();
        if (visualModel != null) visualModel.DOKill();

        // קריאה ל-GameManager לעדכן את מצב המשחק (אם הפונקציה אצלך נקראת אחרת, עדכן אותה)
        if (GameManager.instance != null)
        {
            // GameManager.instance.GameOver(); 
        }
        
        Debug.Log("Player hit an obstacle! Game Over.");
    }
}