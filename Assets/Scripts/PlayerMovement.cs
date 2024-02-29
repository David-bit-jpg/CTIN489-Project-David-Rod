using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
public class PlayerMovement : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private GameObject glowStick;
    [SerializeField] private Text glowStickPickupText;
    [SerializeField] private Text doorMoveUpText;
    [SerializeField] private Text chargingText;
    [SerializeField] private Image redDot;
    [SerializeField] private GameObject vhsEffectStatusText;
    private bool rKeyPressed = false;
    private bool isCharging = false;
    public Text timerText;

    bool startedRed = false;

    private float startTime = 0.0f;
    private bool CameraTimerActive = false;

    private float elapsedTime = 0f;
    [SerializeField] private Text glowStickNumberText;

    float sphereRadius = 0.1f;

    [Header("Config")]
    private float countdownTime = 300f;

    [SerializeField] public Transform CameraIntractPointer;
    private ScriptableRendererFeature vhsFeature;
    public bool featureAble = false;
    [SerializeField] public UniversalRendererData rendererData;
    [SerializeField] private float normalSpeed = 2.0f;
    [SerializeField] private float runningSpeed = 5.0f;
    private float speed;
    public float moveSpeed = 1f;
    private bool canMove = true;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float crouchSpeed = 2.5f;
    public AudioSource AudioSource;
    [SerializeField] public float volume = 0.2f;
    [SerializeField] private AudioClip Audio;
    private Animator animator;
    private CameraControl cameraControl;
    private float lastStepTime = 0f;
    private float stepInterval = 0f;

    [SerializeField] private int glowStickNumber = 3;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;
    public bool isRunning { get; private set; }
    public bool isMoving { get; private set; }
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded = true;
    public float horizontal;
    public float vertical;
    [SerializeField] public int maxStamina = 100;
    private int currentStamina;
    public Image staminaBar;
    private float lastRunTime = 0f;
    [SerializeField] private float staminaRecoveryDelay = 3.0f;
    [SerializeField] private float staminaRecoveryRate = 1.0f;
    [SerializeField] private float glowStickCoolDown = 1.0f;
    private float glowStickTimer;
    [SerializeField] public float DrainTime;
    public Image BatterySlider;
    FlashManager flashManager;
    [SerializeField] private Transform flashTransform;
    [SerializeField] public int BatteryLife = 20;

    private void Awake()
    {
        currentStamina = maxStamina;
        glowStickPickupText.gameObject.SetActive(false);
        doorMoveUpText.gameObject.SetActive(false);
        chargingText.gameObject.SetActive(false);
        vhsFeature = rendererData.rendererFeatures.Find(feature => feature.name == "FullScreenPassRendererFeature");
        rb = GetComponent<Rigidbody>();
        cameraControl = FindObjectOfType<CameraControl>();
        AudioSource = gameObject.AddComponent<AudioSource>();
        AudioSource.clip = Audio;
        AudioSource.volume = volume;
        animator = GetComponent<Animator>();
        glowStickTimer = 0.0f;
        DrainTime = BatteryLife;
        DisableVHSFeature();
        flashManager = flashTransform.GetComponent<FlashManager>();
        if (vhsEffectStatusText != null)
        {
            vhsEffectStatusText.gameObject.SetActive(false);
        }
        UpdateGlowStickNumberUI();
    }

    void Update()
    {
        if (canMove)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            Vector3 forward = transform.forward * moveZ;
            Vector3 right = transform.right * moveX;

            moveDirection = (forward + right).normalized;

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            if (currentStamina >= 0 && Input.GetKey(KeyCode.LeftShift))
            {
                isRunning = true;
                speed = runningSpeed;
                stepInterval = runStepInterval;
            }
            else
            {
                isRunning = false;
                speed = normalSpeed;
                stepInterval = walkStepInterval;
            }
            isMoving = moveDirection != Vector3.zero;
            if (isRunning && isMoving)
            {
                currentStamina--;
                lastRunTime = Time.time;
                speed = runningSpeed;
                UpdateStaminaBar();
            }
            else if (Time.time - lastRunTime > staminaRecoveryDelay && currentStamina < maxStamina)
            {
                currentStamina += (int)(staminaRecoveryRate * Time.deltaTime);
                currentStamina = Mathf.Min(currentStamina, maxStamina);
                speed = normalSpeed;
                UpdateStaminaBar();
            }

            if (Input.GetKey(KeyCode.Q) && glowStickNumber > 0)
            {
                DropGlowStick();
            }
            if (Input.GetKey(KeyCode.R) && !rKeyPressed)
            {
                rKeyPressed = true;

                if (featureAble)
                {
                    DisableVHSFeature();
                    featureAble = false;
                    UpdateVHSEffectStatus(false);
                }
                else if (!featureAble && DrainTime >= 0)
                {
                    EnableVHSFeature();
                    featureAble = true;
                    UpdateVHSEffectStatus(true);
                }
                ToggleTimer();
            }
            else if (!Input.GetKey(KeyCode.R))
            {
                rKeyPressed = false;
            }
            if (featureAble)
            {
                DrainTime -= 0.1f * Time.deltaTime;
                UpdateBatteryBar();
                if(!startedRed)
                StartCoroutine(ToggleStateCoroutine());
            }
            if(featureAble)
            {
                if(DrainTime <= 0)
                {
                    DisableVHSFeature();
                    UpdateVHSEffectStatus(false);
                    featureAble = false;
                }
            }


            glowStickTimer -= Time.deltaTime;
            RaycastHit hit;
            Ray ray = new Ray(CameraIntractPointer.position, CameraIntractPointer.forward);
            if (Physics.SphereCast(ray, sphereRadius, out hit, 2.5f))
            {
                UpdateInteractionUI(hit);
                UpdateGlowStickNumberUI();
            }
            bool hitChargingStation = Physics.SphereCast(CameraIntractPointer.position, sphereRadius, CameraIntractPointer.forward, out hit, 2.5f) && hit.collider.CompareTag("ChargingStation");
            if (Input.GetKey(KeyCode.E) && hitChargingStation)
            {
                isCharging = true;
                ChargeBattery();
                flashManager.ChargeBattery();
            }
            else if (Input.GetKeyUp(KeyCode.E) || !hitChargingStation)
            {
                isCharging = false;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (Physics.SphereCast(ray, sphereRadius, out hit, 2.5f))
                {
                    HandleInteraction(hit);
                }
            }

            Vector3 rayStart = CameraIntractPointer.position;
            Vector3 rayDirection = CameraIntractPointer.forward;
            Debug.DrawRay(CameraIntractPointer.position, CameraIntractPointer.forward * 2.5f, Color.red);
            float sphereCastDistance = 2.5f;
            Color debugColor = Color.red;

            DrawSphereCast(rayStart, rayDirection, sphereRadius, sphereCastDistance, debugColor);
            if (CameraTimerActive)
            {
                float t = elapsedTime + (Time.time - startTime);

                string minutes = ((int)t / 60).ToString();
                string seconds = (t % 60).ToString("f2");

                timerText.text = minutes + ":" + seconds;
            }
        }
    }
    IEnumerator ToggleStateCoroutine()
    {
        while (true)
        {
            startedRed = true;
            yield return new WaitForSeconds(0.7f);
            redDot.gameObject.SetActive(!redDot.gameObject.activeSelf);
        }
        startedRed = false;
    }
    public void ToggleTimer()
    {
        if (CameraTimerActive)
        {
            elapsedTime += Time.time - startTime;
        }
        else
        {
            startTime = Time.time;
        }

        CameraTimerActive = !CameraTimerActive;
    }
    private void ChargeBattery()
    {
        float chargingRate = 4.0f;
        if (DrainTime < BatteryLife)
        {
            DrainTime += Time.deltaTime * chargingRate;
            DrainTime = Mathf.Min(DrainTime, BatteryLife);
            UpdateBatteryBar();
        }
    }
    void HandleInteraction(RaycastHit hit)
    {
        switch (hit.collider.gameObject.tag)
        {
            case "GlowStick":
                glowStickNumber++;
                Destroy(hit.collider.gameObject);
                UpdateGlowStickNumberUI();
                break;
            case "Door":
                DoorController doorController = hit.collider.GetComponent<DoorController>();
                if (doorController != null)
                {
                    doorController.ToggleDoor();
                    //doorController.ToggleDoor();
                    //doorController.isProcessing = false;
                }
                break;
            default:
                break;
        }
        UpdateInteractionUI(hit);
    }
    void UpdateInteractionUI(RaycastHit hit)
    {
        glowStickPickupText.gameObject.SetActive(hit.collider.gameObject.CompareTag("GlowStick"));
        doorMoveUpText.gameObject.SetActive(hit.collider.gameObject.CompareTag("Door"));
        chargingText.gameObject.SetActive(hit.collider.gameObject.CompareTag("ChargingStation"));
    }
    private void UpdateGlowStickNumberUI()
    {
        if (glowStickNumberText != null)
        {
            glowStickNumberText.text = "Glow Sticks: " + glowStickNumber.ToString();
        }
    }

    private void FixedUpdate()
    {
        float currentSpeed = isGrounded ? speed : crouchSpeed;

        if (isGrounded && moveDirection != Vector3.zero)
        {
            if (Time.time - lastStepTime > stepInterval)
            {
                AudioSource.Play();
                lastStepTime = Time.time;
            }
        }
        rb.MovePosition(rb.position + moveDirection * currentSpeed * Time.fixedDeltaTime);

    }
    private void UpdateStaminaBar()
    {
        if (staminaBar != null)
        {
            staminaBar.fillAmount = (float)currentStamina / maxStamina;
        }
    }
    public void UpdateBatteryBar()
    {
        if (BatterySlider != null)
        {
            BatterySlider.fillAmount = (float)DrainTime / BatteryLife;
        }
    }
    public void SetCanMove(bool c)
    {
        canMove = c;
    }

    private void DropGlowStick()
    {
        if (glowStickTimer <= 0.0f)
        {
            glowStickNumber--;
            glowStickTimer = glowStickCoolDown;
            Vector3 rayStart = CameraIntractPointer.position;
            Vector3 rayDirection = CameraIntractPointer.forward;
            float maxDistance = 2.0f;
            RaycastHit hit;
            Vector3 dropPosition;

            if (Physics.Raycast(rayStart, rayDirection, out hit, maxDistance))
            {
                dropPosition = hit.point - rayDirection * 0.1f;
            }
            else
            {
                dropPosition = rayStart + rayDirection * maxDistance;
            }
            Quaternion dropRotation = Quaternion.Euler(CameraIntractPointer.eulerAngles);
            Instantiate(glowStick, dropPosition, dropRotation);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    public void EnableVHSFeature()
    {
        if (vhsFeature != null)
        {
            vhsFeature.SetActive(true);
        }
    }

    public void DisableVHSFeature()
    {
        if (vhsFeature != null)
        {
            vhsFeature.SetActive(false);
        }
    }
    private void UpdateVHSEffectStatus(bool isActive)
    {
        if (vhsEffectStatusText != null)
        {
            vhsEffectStatusText.gameObject.SetActive(isActive);
        }
    }

    //Debug
    void DrawSphereCast(Vector3 origin, Vector3 direction, float radius, float distance, Color color)
    {
        Debug.DrawRay(origin, direction * distance, color);

        DrawWireSphere(origin, radius, color);

        Vector3 endPosition = origin + direction * distance;
        DrawWireSphere(endPosition, radius, color);
    }
    void DrawWireSphere(Vector3 center, float radius, Color color)
    {
        float angleStep = 10.0f;
        Vector3 prevPoint = center + Quaternion.Euler(0, 0, 0) * Vector3.up * radius;
        for (float angle = angleStep; angle <= 360.0f; angle += angleStep)
        {
            Vector3 point = center + Quaternion.Euler(0, angle, 0) * Vector3.up * radius;
            Debug.DrawLine(prevPoint, point, color);
            prevPoint = point;
        }

        prevPoint = center + Quaternion.Euler(0, 0, 0) * Vector3.forward * radius;
        for (float angle = angleStep; angle <= 360.0f; angle += angleStep)
        {
            Vector3 point = center + Quaternion.Euler(angle, 0, 0) * Vector3.forward * radius;
            Debug.DrawLine(prevPoint, point, color);
            prevPoint = point;
        }

        prevPoint = center + Quaternion.Euler(0, 0, 0) * Vector3.right * radius;
        for (float angle = angleStep; angle <= 360.0f; angle += angleStep)
        {
            Vector3 point = center + Quaternion.Euler(0, 0, angle) * Vector3.right * radius;
            Debug.DrawLine(prevPoint, point, color);
            prevPoint = point;
        }
    }
}