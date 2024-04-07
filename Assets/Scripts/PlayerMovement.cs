using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using Cinemachine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private GameObject glowStick;
    [SerializeField] private Text glowStickPickupText;
    [SerializeField] private Text doorMoveUpText;
    [SerializeField] private Text chargingText;
    [SerializeField] private Text pickUpText;
    [SerializeField] private Text dropText;
    [SerializeField] private Text balloonText;
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
    private bool featureAble = false;
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
    public CameraControl cameraControl;
    private float lastStepTime = 0f;
    public float stepInterval = 0f;

    [SerializeField] private int glowStickNumber = 3;
    [SerializeField] public float walkStepInterval = 0.5f;
    [SerializeField] public float runStepInterval = 0.3f;
    [SerializeField] private float rayCastDist = 10.0f;
    [SerializeField] private LayerMask IgnoreLayer;
    public bool isRunning { get; private set; }
    public bool isMoving { get; private set; }
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded = true;
    public float horizontal;
    public float vertical;
    [SerializeField] public int maxStamina = 100;
    public int currentStamina;
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

    public bool chased = false;

    public bool killed = false;

    public Vector3 fixPos = Vector3.zero;

    ChargingStation chargingStation = null;

    [SerializeField] Transform CharacterBodyTransform;
    [SerializeField] CinemachineVirtualCamera VirtualCam;

    Hashtable glowSticks = new Hashtable();

    [SerializeField] float darkKillTime = 10.0f;
    float darkTimer;

    private Vignette thisVignette;

    int glowStickID = 0;

    public bool isHoldingVase = false;
    public Vase heldVase = null;

    public int keyCount = 0;

    [SerializeField] public Material vhsMaterial;
    private void Awake()
    {
        currentStamina = maxStamina;
        glowStickPickupText.gameObject.SetActive(false);
        doorMoveUpText.gameObject.SetActive(false);
        pickUpText.gameObject.SetActive(false);
        chargingText.gameObject.SetActive(false);
        balloonText.gameObject.SetActive(false);
        vhsFeature = rendererData.rendererFeatures.Find(feature => feature.name == "FullScreenPassRendererFeature");
        rb = GetComponent<Rigidbody>();
        cameraControl = FindObjectOfType<CameraControl>();
        AudioSource = gameObject.AddComponent<AudioSource>();
        AudioSource.clip = Audio;
        AudioSource.volume = volume;
        animator = GetComponent<Animator>();
        glowStickTimer = 0.0f;
        darkTimer = 0.0f;
        DrainTime = BatteryLife;
        DisableVHSFeature();
        flashManager = flashTransform.GetComponent<FlashManager>();
        if (vhsEffectStatusText != null)
        {
            vhsEffectStatusText.gameObject.SetActive(false);
        }
        UpdateGlowStickNumberUI();

        
    }

    private void Start()
    {
        LevelManager.Instance.postVolume = FindObjectOfType<Volume>();
        UpdateVHSParameters(0.0f);
    }

    void Update()
    {
        if (canMove)
        {
            if (isDark())
            {
                if(darkTimer >= darkKillTime)
                {
                    killed = true;
                }
                darkTimer += Time.deltaTime;
            }
            else
            {
                darkTimer = 0.0f;
            }
            /*LevelManager.Instance.postVolume.profile.TryGet(out thisVignette);
            thisVignette.intensity.value = darkTimer / darkKillTime;*/

            /*float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            Vector3 forward = CharacterBodyTransform.forward * moveZ;
            Vector3 right = CharacterBodyTransform.right * moveX;

            moveDirection = (forward + right).normalized;*/

            /*if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }*/

            
            /*isMoving = moveDirection != Vector3.zero;*/
            

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
                DrainTime -= 0.3f * Time.deltaTime;
                UpdateBatteryBar();
                if (!startedRed)
                    StartCoroutine(ToggleStateCoroutine());
            }
            if (featureAble)
            {
                if (DrainTime <= 0)
                {
                    DisableVHSFeature();
                    UpdateVHSEffectStatus(false);
                    featureAble = false;
                }
            }


            glowStickTimer -= Time.deltaTime;
            RaycastHit hit;
            Ray ray = new Ray(CameraIntractPointer.position, CameraIntractPointer.forward);
            if (Physics.SphereCast(ray, sphereRadius, out hit, rayCastDist, ~IgnoreLayer))
            {
                UpdateInteractionUI(hit);
                UpdateGlowStickNumberUI();
            }


            bool hitChargingStation = Physics.SphereCast(CameraIntractPointer.position, sphereRadius, CameraIntractPointer.forward, out hit, rayCastDist, ~IgnoreLayer) && hit.collider.CompareTag("ChargingStation");

            if (hitChargingStation)
            {
                chargingStation = hit.collider.gameObject.GetComponent<ChargingStation>();
            }

            if (Input.GetKey(KeyCode.E) && hitChargingStation)
            {
                isCharging = true;
                ChargeBattery();
                flashManager.ChargeBattery();
                if (chargingStation)
                {
                    chargingStation.Charge();
                }

            }
            else if (Input.GetKeyUp(KeyCode.E) || !hitChargingStation)
            {
                isCharging = false;

                if (chargingStation)
                {
                    chargingStation.StopCharge();
                }
            }

            if (isHoldingVase)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    heldVase.Drop();
                    isHoldingVase = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (Physics.SphereCast(ray, sphereRadius, out hit, rayCastDist, ~IgnoreLayer))
                {
                    HandleInteraction(hit);
                }
            }

            Vector3 rayStart = CameraIntractPointer.position;
            Vector3 rayDirection = CameraIntractPointer.forward;
            Debug.DrawRay(CameraIntractPointer.position, CameraIntractPointer.forward * rayCastDist, Color.red);
            float sphereCastDistance = rayCastDist;
            Color debugColor = Color.red;

            DrawSphereCast(rayStart, rayDirection, sphereRadius, sphereCastDistance, debugColor);
            if (CameraTimerActive)
            {
                float t = elapsedTime + (Time.time - startTime);

                string minutes = ((int)t / 60).ToString();
                string seconds = (t % 60).ToString("f2");

                timerText.text = minutes + ":" + seconds;
            }

            //Update Virual Cam
            UpdateVirtualCamera();
        }
        if (killed)
        {
            transform.position = fixPos;

            LevelManager.Instance.ShowRestartText();

            //Restart Level
            if (Input.GetKeyDown(KeyCode.R))
            {
                LevelManager.Instance.RestartLevel();
            }
        }
    }

    public void UpdateStamina(bool isSprinting)
    {
        if (isSprinting)
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
                GlowStickManager gsm = hit.collider.GetComponent<GlowStickManager>();
                if (!gsm.isTaken)
                {
                    glowStickNumber++;
                    glowSticks.Remove(hit.collider.gameObject.name);
                    Destroy(hit.collider.gameObject);
                    UpdateGlowStickNumberUI();
                }
                break;
            case "Door":
                DoorController doorController = hit.collider.GetComponent<DoorController>();
                if (doorController != null)
                {
                    doorController.ToggleDoor();
                }
                break;
            case "Balloon":
                if (!chased)
                {
                    Break_Ghost break_Ghost = hit.collider.GetComponent<Break_Ghost>();
                    if (break_Ghost != null && !break_Ghost.Is_Breaked)
                        break_Ghost.Is_Breaked = true;
                }
                break;
            case "Lever":
                Lever lever = hit.collider.GetComponent<Lever>();
                if (lever != null)
                {
                    lever.Interact();
                }
                break;
            case "Vase":
                Vase vase = hit.collider.GetComponent<Vase>();
                if (vase)
                {
                    vase.PickUp();
                }
                break;
            case "Key":
                Key key = hit.collider.GetComponent<Key>();
                if (key)
                {
                    key.PickUp();
                    keyCount++;
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
        pickUpText.gameObject.SetActive(hit.collider.gameObject.CompareTag("Key") || hit.collider.gameObject.CompareTag("Vase") && !isHoldingVase);
        dropText.gameObject.SetActive(isHoldingVase);
        chargingText.gameObject.SetActive(hit.collider.gameObject.CompareTag("ChargingStation"));
        balloonText.gameObject.SetActive(hit.collider.gameObject.CompareTag("Balloon") && !chased);
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
        //rb.MovePosition(rb.position + moveDirection * currentSpeed * Time.fixedDeltaTime);

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

            if (Physics.Raycast(rayStart, rayDirection, out hit, maxDistance, ~IgnoreLayer))
            {
                dropPosition = hit.point - rayDirection * 0.1f;
            }
            else
            {
                dropPosition = rayStart + rayDirection * maxDistance;
            }
            Quaternion dropRotation = Quaternion.Euler(CameraIntractPointer.eulerAngles);
            GameObject thisGlowStick = Instantiate(glowStick, dropPosition, dropRotation);

            //glowSticks.Add(thisGlowStick.name + glowStickID, thisGlowStick);
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

    private void UpdateVirtualCamera()
    {
        CinemachineBasicMultiChannelPerlin VirtualCamNoise = VirtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (isRunning)
        {
            VirtualCamNoise.m_FrequencyGain = 5;
        }
        else
        {
            VirtualCamNoise.m_FrequencyGain = 2;
        }
    }

    private float FindNearestGlowStickDist()
    {
        float minDist = float.MaxValue;
        foreach(GameObject glowStick in glowSticks)
        {
            float curDist = Vector3.Distance(glowStick.transform.position, gameObject.transform.position);
            if (curDist < minDist)
            {
                minDist = curDist;
            }
        }

        return minDist;
    }

    private bool isDark()
    {
        if (LightmapSwitcher.Instance.isDay)
        {
            return false;
        }

        if(FindNearestGlowStickDist() < 5.0f || flashManager.GetIsLightOn())
        {
            return false;
        }

        return true;
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

    private void UpdateVHSParameters(float lerpFactor)
    {
        if (vhsMaterial != null)
        {
            AudioSource.volume = Mathf.Lerp(0.0f, 0.4f, lerpFactor);
            float strength = Mathf.Lerp(0.0f, 1.0f, lerpFactor);
            float strip = Mathf.Lerp(0.3f, 0.2f, lerpFactor);
            float pixelOffset = Mathf.Lerp(0.0f, 40.0f, lerpFactor);
            float shake = Mathf.Lerp(0.003f, 0.01f, lerpFactor);
            float speed = Mathf.Lerp(0.5f, 1.2f, lerpFactor);
            vhsMaterial.SetFloat("_Strength", strength);
            vhsMaterial.SetFloat("_StripSize", strip);
            vhsMaterial.SetFloat("_PixelOffset", pixelOffset);
            vhsMaterial.SetFloat("_Shake", shake);
            vhsMaterial.SetFloat("_Speed", speed);
        }
    }
}