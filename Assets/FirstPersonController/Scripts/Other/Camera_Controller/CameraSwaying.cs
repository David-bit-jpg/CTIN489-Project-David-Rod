using UnityEngine;

namespace FPSController
{
    [System.Serializable]
    public class CameraSwaying
    {
        #region Variables
        [Space, Header("Sway Settings")]
        [SerializeField] private float swayAmount = 0f;
        [SerializeField] private float swaySpeed = 0f;
        [SerializeField] private float returnSpeed = 0f;
        [SerializeField] private float changeDirectionMultiplier = 0f;

        [SerializeField] private AnimationCurve swayCurve = new AnimationCurve();

        #region Private
        private Transform m_camTransform;
        private float _scrollSpeed;

        private float m_xAmountThisFrame;
        private float m_xAmountPreviousFrame;

        private bool m_diffrentDirection;
        #endregion
        #endregion

        #region Custom Methods
        public void Init(Transform _cam)
        {
            m_camTransform = _cam;
        }

        public void SwayPlayer(Vector3 _inputVector, float _rawXInput)
        {
            float _xAmount = _inputVector.x;

            m_xAmountThisFrame = _rawXInput;

            if (_rawXInput != 0f) //If there is some input.
            {
                //If previous dir is not equal to current one and the previous one was not idle.
                if (m_xAmountThisFrame != m_xAmountPreviousFrame && m_xAmountPreviousFrame != 0)
                    m_diffrentDirection = true;

                //Then multiply scroll so when changing direction it will sway to the other direction faster.
                float _speedMultiplier = m_diffrentDirection ? changeDirectionMultiplier : 1f;
                _scrollSpeed += (_xAmount * swaySpeed * Time.deltaTime * _speedMultiplier);
            }
            else //If we are not moving so there is no input.
            {
                if (m_xAmountThisFrame == m_xAmountPreviousFrame) //Check if previous direction equals current direction.
                    m_diffrentDirection = false; //If yes, reset this bool.

                _scrollSpeed = Mathf.Lerp(_scrollSpeed, 0f, Time.deltaTime * returnSpeed);
            }

            _scrollSpeed = Mathf.Clamp(_scrollSpeed, -1f, 1f);

            float _swayFinalAmount;

            //Determines negative or positive scroll value to apply to final sway vector.
            if (_scrollSpeed < 0f)
                _swayFinalAmount = -swayCurve.Evaluate(_scrollSpeed) * -swayAmount;
            else
                _swayFinalAmount = swayCurve.Evaluate(_scrollSpeed) * -swayAmount;

            Vector3 _swayVector;
            _swayVector.z = _swayFinalAmount;

            m_camTransform.localEulerAngles = new Vector3(m_camTransform.localEulerAngles.x, m_camTransform.localEulerAngles.y, _swayVector.z);

            m_xAmountPreviousFrame = m_xAmountThisFrame;
        }
        #endregion
    }
}
