using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public static Vector3 shakeOffset = Vector3.zero;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private float originalYPos = 0f;
    private PlayerMovement playerMovement;
    [SerializeField] private float bobbingAmount = 0.025f;
    [SerializeField] private float bobbingSpeed = 12f;
    private float timer = 0f;
    public bool canMove = true;
    CameraControl Instance;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerMovement = playerBody.GetComponent<PlayerMovement>();
        originalYPos = transform.localPosition.y;
    }

    void Update()
    {
        // if (canMove)
        // {
        //     float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        //     float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //     yRotation += mouseX;
        //     xRotation -= mouseY;
        //     xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //     UpdateCameraRotation();
        //     if (playerMovement.isMoving)
        //     {
        //         timer += Time.deltaTime * bobbingSpeed;
        //         float newY = originalYPos + Mathf.Sin(timer) * bobbingAmount;
        //         transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        //     }
        //     else
        //     {
        //         timer = 0;
        //         transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, originalYPos, Time.deltaTime * bobbingSpeed), transform.localPosition.z);
        //     }
        // }
        playerBody.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }

    private void UpdateCameraRotation()
    {

        transform.localRotation = Quaternion.Euler(xRotation + shakeOffset.x, 0f, 0f);
        playerBody.rotation = Quaternion.Euler(0f, yRotation + shakeOffset.y, 0f);
    }

}
