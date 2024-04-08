using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSController
{
    public class CameraShake : MonoBehaviour
    {
        FirstPersonController fpsController;
        HeadBobData headBobData;
        WallClimb wallClimb;
        WallRun wallRun;
        Slide slide;
        Dodge dodge;

        private Vector2 defaultAmplitude;
        private Vector2 defaultFrequency;
        [SerializeField] public Vector2 wallClimbShakeAmplitude = new Vector2(.1f, 0f);
        [SerializeField] public Vector2 wallClimbShakeFrequency = new Vector2(5f, 1f);
        [SerializeField] public Vector2 mantleShakeAmplitude = new Vector2(0f, -1.5f);
        [SerializeField] public Vector2 mantleShakeFrequency = new Vector2(0f, 1f);
        [SerializeField] public Vector2 wallRunShakeAmplitude = new Vector2(.1f, .3f);
        [SerializeField] public Vector2 wallRunShakeFrequency = new Vector2(2f, 2f);
        [SerializeField] public Vector2 slideShakeAmplitude = new Vector2(2f, 1f);
        [SerializeField] public Vector2 slideShakeFrequency = new Vector2(2f, 1f);
        [SerializeField] public Vector2 dodgeShakeFrequency = new Vector2(2f, 1f);
        [SerializeField] public Vector2 dodgeShakeAmplitude = new Vector2(2f, 1f);

        [Space(10), Header("Enable Camera Shake For:")]
        public bool wallClimbActive = true;
        public bool mantleActive = true;
        public bool wallRunActive = true;
        public bool slideActive = true;
        public bool dodgeActive = true;

        void Start()
        {
            fpsController = GetComponentInParent<FirstPersonController>();
            headBobData = fpsController.headBobData;
            wallClimb = GetComponentInParent<WallClimb>();
            wallRun = GetComponentInParent<WallRun>();
            slide = GetComponentInParent<Slide>();
            dodge = GetComponentInParent<Dodge>();

            defaultAmplitude = new Vector2(headBobData.xAmplitude, headBobData.yAmplitude);
            defaultFrequency = new Vector2(headBobData.xFrequency, headBobData.yFrequency);
        }

        void Update()
        {
            //If the player is wall climbing, applies the wall climb values to the headbob.
            if (wallClimb.isClimbing && wallClimbActive)
            {
                headBobData.xAmplitude = wallClimbShakeAmplitude.x;
                headBobData.yAmplitude = wallClimbShakeAmplitude.y;
                headBobData.xFrequency = wallClimbShakeFrequency.x;
                headBobData.yFrequency = wallClimbShakeFrequency.y;
            }
            //If the player is mantling, applies the mantling values to the headbob.
            if (wallClimb.isMantling && mantleActive)
            {
                headBobData.xAmplitude = mantleShakeAmplitude.x;
                headBobData.yAmplitude = mantleShakeAmplitude.y;
                headBobData.xFrequency = mantleShakeFrequency.x;
                headBobData.yFrequency = mantleShakeFrequency.y;
            }
            //If the player is wall running, applies the wall run values to the headbob.
            if (wallRun.isWallRunning && wallRunActive)
            {
                headBobData.xAmplitude = wallRunShakeAmplitude.x;
                headBobData.yAmplitude = wallRunShakeAmplitude.y;
                headBobData.xFrequency = wallRunShakeFrequency.x;
                headBobData.yFrequency = wallRunShakeFrequency.y;
            }
            //If the player is sliding, applies the slide values to the headbob.
            if (slide.isSliding && slideActive)
            {
                headBobData.xAmplitude = slideShakeAmplitude.x;
                headBobData.yAmplitude = slideShakeAmplitude.y;
                headBobData.xFrequency = slideShakeFrequency.x;
                headBobData.yFrequency = slideShakeFrequency.y;
            }
            //If the player is dodging, applies the dodge values to the headbob.
            if (dodge.isDodging && dodgeActive)
            {
                headBobData.xAmplitude = dodgeShakeAmplitude.x;
                headBobData.yAmplitude = dodgeShakeAmplitude.y;
                headBobData.xFrequency = dodgeShakeFrequency.x;
                headBobData.yFrequency = dodgeShakeFrequency.y;
            }
            //Defaults headbob values if non of the above are active.
            if (!wallClimb.isClimbing && !wallClimb.isMantling && !wallRun.isWallRunning || fpsController.m_isGrounded && !slide.isSliding)
            {
                headBobData.xAmplitude = defaultAmplitude.x;
                headBobData.yAmplitude = defaultAmplitude.y;
                headBobData.xFrequency = defaultFrequency.x;
                headBobData.yFrequency = defaultFrequency.y;
            }
        }
    }
}
