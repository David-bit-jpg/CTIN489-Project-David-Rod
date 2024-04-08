using Unity.Collections;
using UnityEngine;

namespace FPSController
{
    public class Leaning : MonoBehaviour
    {
        private FirstPersonController firstPersonController;
        private CameraController cameraController;

        private bool rightHasHit;
        private bool leftHasHit;
        private bool upHasHit;

        [Range(1f, 50f)] [SerializeField] public float leanAmount = 20f;
        [Range(1f, 10f)] [SerializeField] public float leanSpeed = 2f;

        [Space(10), Header("Public for Other Scripts")]
        [ReadOnly] public float leanValue;
        
        public bool isLeaningLeft;
        public bool isLeaningRight;
        public bool isLeaningUp;

        private Transform m_CameraTransform;
        private Vector3 m_InitialPosition;
        private Quaternion m_InitialRotation;

        void Start()
        {
            firstPersonController = GetComponent<FirstPersonController>();
            cameraController = GetComponentInChildren<CameraController>();
            m_CameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;

            m_InitialPosition = m_CameraTransform.localPosition;
            m_InitialRotation = m_CameraTransform.localRotation;
        }

        void Update()
        {
            #region Lean Control
            //If player is moving, cancels all leaning.
            if (firstPersonController.m_inputVector.x != 0 || firstPersonController.m_inputVector.y != 0)
            {
                isLeaningLeft = false;
                isLeaningRight = false;
                isLeaningUp = false;
                leanValue = Mathf.Lerp(leanValue, 0, leanSpeed * Time.deltaTime);
            }

            //Resets lean rotation to zero if not leaning to either side.
            if (!isLeaningLeft && !isLeaningRight)
                leanValue = Mathf.Lerp(leanValue, 0, leanSpeed * Time.deltaTime);
            #endregion

            CheckCanLeanLeft();
            CheckCanLeanRight();
            CheckCanLeanUp();
            CheckLeaning();
        }

        //New input system checks if lean left key is held down.
        public void LeftLeanInput()
        {
            if (firstPersonController.m_isGrounded && !isLeaningRight && !isLeaningUp && !leftHasHit)
                isLeaningLeft = true;
            else 
                isLeaningLeft = false;
        }

        //New input system checks if lean right key is held down.
        public void RightLeanInput()
        {
            if (firstPersonController.m_isGrounded && !isLeaningLeft && !isLeaningUp && !rightHasHit)
                isLeaningRight = true;                    
            else 
                isLeaningRight = false;
        }

        //New input system checks if lean up key is held down.
        public void UpLeanInput()
        {
            if (firstPersonController.m_isGrounded && !isLeaningLeft && !isLeaningRight &&
                    cameraController.m_desiredPitch > -30 && cameraController.m_desiredPitch < 30 && !upHasHit)
                if (firstPersonController.movementInputData.IsCrouching)
                    isLeaningUp = true;
                else { isLeaningUp = false; }
            else { isLeaningUp = false; }
        }

        //Raycast check to make sure nothing is blocking left.
        void CheckCanLeanLeft()
        {
            RaycastHit hit;
            if (Physics.Raycast(m_CameraTransform.position, m_CameraTransform.TransformDirection(Vector3.left * 0.5f), out hit, 0.5f))
            {
                leftHasHit = true;
            }
            else
                leftHasHit = false;
        }

        //Raycast check to make sure nothing is blocking right.
        void CheckCanLeanRight()
        {
            RaycastHit hit;
            if (Physics.Raycast(m_CameraTransform.position, m_CameraTransform.TransformDirection(Vector3.right * 0.5f), out hit, 0.5f))
            {
                rightHasHit = true;
            }
            else
                rightHasHit = false;
        }

        //Raycast check to make sure nothing is blocking above.
        void CheckCanLeanUp()
        {
            RaycastHit hit;
            if (Physics.Raycast(m_CameraTransform.position, m_CameraTransform.TransformDirection(Vector3.up * 0.5f), out hit, 0.5f))
            {
                upHasHit = true;
            }
            else
                upHasHit = false;
        }

        //Moves camera based on lean direction.
        void CheckLeaning()
        {
            //Moves the camera to the new rotation, counter-clockwise.
            if (isLeaningLeft)
            {
                Vector3 newPosition = new Vector3(m_InitialPosition.x - 0.5f, m_InitialPosition.y, m_InitialPosition.z);
                leanValue = Mathf.Lerp(leanValue, leanAmount, leanSpeed * Time.deltaTime);
                m_CameraTransform.localPosition = Vector3.Lerp(m_CameraTransform.localPosition, newPosition, Time.deltaTime * leanSpeed);
            }
            //Moves the camera to the new rotation, clockwise.
            else if (isLeaningRight)
            {
                Vector3 newPosition = new Vector3(m_InitialPosition.x + 0.5f, m_InitialPosition.y, m_InitialPosition.z);
                leanValue = Mathf.Lerp(leanValue, -leanAmount, leanSpeed * Time.deltaTime);
                m_CameraTransform.localPosition = Vector3.Lerp(m_CameraTransform.localPosition, newPosition, Time.deltaTime * leanSpeed);
            }
            //Moves the camera to the new rotation, upwards.
            else if (isLeaningUp)
            {
                Vector3 newPosition = new Vector3(m_InitialPosition.x, m_InitialPosition.y + 0.3f, m_InitialPosition.z);
                m_CameraTransform.localPosition = Vector3.Lerp(m_CameraTransform.localPosition, newPosition, Time.deltaTime * leanSpeed);
            }
            //If not leaning, resets the rotation and position to original.
            else
            {
                m_CameraTransform.localRotation = Quaternion.Lerp(m_CameraTransform.localRotation, m_InitialRotation, Time.deltaTime * leanSpeed);
                m_CameraTransform.localPosition = Vector3.Lerp(m_CameraTransform.localPosition, m_InitialPosition, Time.deltaTime * leanSpeed);
            }
        }
    }
}
