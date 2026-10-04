using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [Header("Debug")]
    public bool godMode = false;

    [Header("Forward Speed")]
    public float forwardSpeed = 50f;

    [Header("Lane Movement")]
    public float laneSwitchTime = 0.35f; 
    [SerializeField] private float rollAngle = 14f; 

    [Header("Jump Physics")]
    [SerializeField] private float jumpHeight = 3.8f; 
    [SerializeField] private float jumpDuration = 1.4f;
    [SerializeField] private float jumpPitchAngle = 22f;

    [Header("Dive Physics")]
    [SerializeField] private float diveDepth = -2.8f; 
    [SerializeField] private float diveDuration = 1.3f;
    [SerializeField] private float divePitchAngle = 20f;

    [Header("Visual Anchor (Optional)")]
    [SerializeField] private Transform visualModel;

    [Header("Effects")]
    public ParticleSystem wakeParticles;
    
    private int currentLane = 1; 
    private bool isSwitchingLane = false;
    private bool isVerticalMoving = false;
    private float initialY = 0f;
    
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
            bool isTouchingWater = Mathf.Abs(transform.position.y - initialY) < 0.2f;
            bool shouldHaveWake = (!isDead && forwardSpeed > 0f && isTouchingWater);

            var emission = wakeParticles.emission;
            if (emission.enabled != shouldHaveWake)
            {
                emission.enabled = shouldHaveWake;
            }
        }

        if (isDead || (GameManager.instance != null && GameManager.instance.isGameOver)) return;

        HandleInput();
        
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.World);
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
        
        if (AudioManager.Instance != null) 
            AudioManager.Instance.PlaySFX(AudioManager.Instance.laneSwitchSound);

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
        
        if (AudioManager.Instance != null) 
            AudioManager.Instance.PlaySFX(AudioManager.Instance.jumpSound);
            
        Sequence jumpSeq = DOTween.Sequence();
        float halfDuration = jumpDuration * 0.5f;

        jumpSeq.Append(transform.DOMoveY(initialY + jumpHeight, halfDuration).SetEase(Ease.OutQuad));
        jumpSeq.Append(transform.DOMoveY(initialY, halfDuration).SetEase(Ease.InQuad));

        Sequence pitchSeq = DOTween.Sequence();
        pitchSeq.Append(ModelTransform.DOLocalRotate(new Vector3(-jumpPitchAngle, 0, 0), halfDuration * 0.65f).SetEase(Ease.OutSine));
        pitchSeq.Append(ModelTransform.DOLocalRotate(new Vector3(jumpPitchAngle * 0.8f, 0, 0), halfDuration * 0.95f).SetEase(Ease.InOutSine));
        pitchSeq.Append(ModelTransform.DOLocalRotate(Vector3.zero, halfDuration * 0.4f).SetEase(Ease.OutSine)); 

        jumpSeq.Join(pitchSeq);
        jumpSeq.OnComplete(() => isVerticalMoving = false);
    }

    private void PerformDive()
    {
        isVerticalMoving = true;
        
        if (AudioManager.Instance != null) 
            AudioManager.Instance.PlaySFX(AudioManager.Instance.diveSound);
            
        Sequence diveSeq = DOTween.Sequence();
        float halfDuration = diveDuration * 0.5f;

        diveSeq.Append(transform.DOMoveY(initialY + diveDepth, halfDuration).SetEase(Ease.OutQuad));
        diveSeq.Append(transform.DOMoveY(initialY, halfDuration).SetEase(Ease.InQuad));

        Sequence pitchSeq = DOTween.Sequence();
        pitchSeq.Append(ModelTransform.DOLocalRotate(new Vector3(divePitchAngle, 0, 0), halfDuration * 0.6f).SetEase(Ease.OutSine));
        pitchSeq.Append(ModelTransform.DOLocalRotate(new Vector3(-divePitchAngle * 0.7f, 0, 0), halfDuration * 0.95f).SetEase(Ease.InOutSine));
        pitchSeq.Append(ModelTransform.DOLocalRotate(Vector3.zero, halfDuration * 0.45f).SetEase(Ease.OutSine)); 

        diveSeq.Join(pitchSeq);
        diveSeq.OnComplete(() => isVerticalMoving = false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.instance == null || GameManager.instance.isGameOver) return;

        if (other.CompareTag("Obstacle"))
        {
            if (godMode) return;
            
            if (AudioManager.Instance != null) 
                AudioManager.Instance.PlaySFX(AudioManager.Instance.crashSound);
            
            forwardSpeed = 0f;
            isDead = true;
            
            transform.DOKill();
            if (visualModel != null) visualModel.DOKill();

            GameManager.instance.TriggerGameOver(); 
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameManager.instance == null || GameManager.instance.isGameOver) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (godMode) return;
            
            if (AudioManager.Instance != null) 
                AudioManager.Instance.PlaySFX(AudioManager.Instance.crashSound);
            
            forwardSpeed = 0f;
            isDead = true;
            
            transform.DOKill();
            if (visualModel != null) visualModel.DOKill();

            GameManager.instance.TriggerGameOver(); 
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (visualModel != null)
        {
            visualModel.DOKill();
        }
    }
}