using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingStation : MonoBehaviour
{
    [SerializeField]ParticleSystem m_ElectricParticle, mSparkParticle;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Charge()
    {
        if (m_ElectricParticle.isPlaying)
        {
            return;
        }
        m_ElectricParticle.Play();

        if (!mSparkParticle.isPlaying)
        {
            return;
        }
        mSparkParticle.Stop();
    }

    public void StopCharge()
    {
        if (!m_ElectricParticle.isPlaying)
        {
            return;
        }
        m_ElectricParticle.Stop();

        if (mSparkParticle.isPlaying)
        {
            return;
        }
        mSparkParticle.Play();
    }
}
