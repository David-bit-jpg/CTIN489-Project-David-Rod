using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerMovement : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject glowStick;
    [SerializeField] private Text glowStickNumberText; 
    [Header("Config")]
    [SerializeField] public Transform CameraIntractPointer;
    [SerializeField] private float normalSpeed = 2.0f;
    [SerializeField] private float runningSpeed = 5.0f;
    private float speed;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float crouchSpeed = 2.5f;
    public AudioSource AudioSource;
    [SerializeField] public float volume = 0.2f;
    [SerializeField] private AudioClip Audio;
    private Animator animator;
    private CameraControl cameraControl;
    private float lastStepTime = 0f;
    private float stepInterval = 0f;

    [SerializeField] private int glowStickNumber = 5;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;
    public bool isRunning { get; private set; }
    public bool isMoving { get; private set; }
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
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

    private void Awake()
    {
        currentStamina = maxStamina;
        rb = GetComponent<Rigidbody>();
        cameraControl = FindObjectOfType<CameraControl>();
        AudioSource = gameObject.AddComponent<AudioSource>();
        AudioSource.clip = Audio;
        AudioSource.volume = volume;
        animator = GetComponent<Animator>();
        glowStickTimer = 0.0f;
    }

    void Update()
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
        // isRunning = isGrounded && Input.GetKey(KeyCode.LeftShift);
        if (isRunning&&isMoving)
        {
            currentStamina--;
            lastRunTime = Time.time;
            UpdateStaminaBar();
        }
        else if (Time.time - lastRunTime > staminaRecoveryDelay && currentStamina < maxStamina)
        {
            currentStamina += (int)(staminaRecoveryRate * Time.deltaTime);
            currentStamina = Mathf.Min(currentStamina, maxStamina);
            UpdateStaminaBar();
        }

        if (Input.GetKey(KeyCode.Q) && glowStickNumber > 0)
        {
            DropGlowStick();
        }

        glowStickTimer-= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;

            Ray ray = new Ray(CameraIntractPointer.position, CameraIntractPointer.forward);

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.gameObject.CompareTag("GlowStick"))
                {
                    glowStickNumber++;
                    Destroy(hit.collider.gameObject);
                }
            }
        }
        UpdateGlowStickNumberUI();

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

    private void DropGlowStick()
    {
        if(glowStickTimer <= 0.0f)
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

}
