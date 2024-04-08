using UnityEngine;

namespace FPSController
{
    [CreateAssetMenu(fileName = "MovementInputData", menuName = "FirstPersonController/Data/MovementInputData", order = 1)]
    public class MovementInputData : ScriptableObject
    {
        #region Data
        Vector2 m_inputVector;

        bool m_isRunning;
        bool m_isCrouching;

        bool m_crouchClicked;
        bool m_crouchHeld;
        bool m_crouchReleased;

        bool m_jumpClicked;

        bool m_runClicked;
        bool m_runHeld;
        bool m_runReleased;
        #endregion

        #region Properties
        public Vector2 InputVector => m_inputVector;
        public bool HasInput => m_inputVector != Vector2.zero;
        //If there is value in the x axis it means the player is moving sideways.
        public float InputVectorX
        {
            set => m_inputVector.x = value;
        }
        //If there is value in the y axis it means the player is moving forward or back.
        public float InputVectorY
        {
            set => m_inputVector.y = value;
        }

        #region Running
        //Is the player running currently?
        public bool IsRunning
        {
            get => m_isRunning;
            set => m_isRunning = value;
        }

        //Run clicked and released bools so it can be toggle or hold control.
        public bool RunClicked
        {
            get => m_runClicked;
            set => m_runClicked = value;
        }
        public bool RunHeld
        {
            get => m_runHeld;
            set => m_runHeld = value;
        }
        public bool RunReleased
        {
            get => m_runReleased;
            set => m_runReleased = value;
        }
        #endregion

        #region Crouching
        //Is the player crouching currently?
        public bool IsCrouching
        {
            get => m_isCrouching;
            set => m_isCrouching = value;
        }

        //Crouch clicked and released bools.
        public bool CrouchClicked
        {
            get => m_crouchClicked;
            set => m_crouchClicked = value;
        }
        public bool CrouchHeld
        {
            get => m_crouchHeld;
            set => m_crouchHeld = value;
        }
        public bool CrouchReleased
        {
            get => m_crouchReleased;
            set => m_crouchReleased = value;
        }
        #endregion

        //Has jump button been pressed?
        public bool JumpClicked
        {
            get => m_jumpClicked;
            set => m_jumpClicked = value;
        }
        #endregion

        #region Custom Methods
        public void ResetInput()
        {
            m_inputVector = Vector2.zero;

            m_isRunning = false;
            m_isCrouching = false;

            m_crouchClicked = false;
            m_crouchHeld = false;
            m_crouchReleased = false;
            m_jumpClicked = false;
            m_runClicked = false;
            m_runHeld = false;
            m_runReleased = false;
        }
        #endregion
    }
}