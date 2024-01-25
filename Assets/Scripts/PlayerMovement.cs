using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Config")]

    [SerializeField]private float normalSpeed = 2.0f;
    [SerializeField]private float runningSpeed = 5.0f;
    private float speed;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float crouchSpeed = 2.5f;
    public AudioSource AudioSource;
    [SerializeField] private AudioClip Audio;
    private CameraControl cameraControl;
    private float lastStepTime = 0f;
    private float stepInterval = 0f;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cameraControl = FindObjectOfType<CameraControl>();
        AudioSource = gameObject.AddComponent<AudioSource>();
        AudioSource.clip = Audio;
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 forward = transform.forward * moveZ;
        Vector3 right = transform.right * moveX;

        moveDirection = (forward + right).normalized;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded)
        {
            speed = runningSpeed;
            stepInterval = runStepInterval;
        }
        else
        {
            speed = normalSpeed;
            stepInterval = walkStepInterval;
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
