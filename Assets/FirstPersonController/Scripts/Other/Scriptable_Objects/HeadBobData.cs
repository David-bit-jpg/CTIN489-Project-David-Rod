using UnityEngine;

namespace FPSController
{
    [CreateAssetMenu(fileName = "HeadBobData", menuName = "FirstPersonController/Data/HeadBobData", order = 3)]
    public class HeadBobData : ScriptableObject
    {
        #region Variables    
            public AnimationCurve xCurve;
            public AnimationCurve yCurve;

            [Space]
            //How much does head bob?
            public float xAmplitude;
            public float yAmplitude;

            [Space]
            //How often does head bob? Higher means more.
            public float xFrequency;
            public float yFrequency;

            [Space]
            //Determines how much running increases headbob.
            public float runAmplitudeMultiplier;
            public float runFrequencyMultiplier;

            [Space]
            //Determines how much crouching decreases headbob.
            public float crouchAmplitudeMultiplier;
            public float crouchFrequencyMultiplier;
        #endregion

        #region Properties
            //How does moving backwards change headbob?
            public float MoveBackwardsFrequencyMultiplier {get;set;}
            public float MoveSideFrequencyMultiplier {get;set;}
        #endregion
    }
}
