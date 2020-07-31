using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParSysMov : MonoBehaviour
{
    public ParticleSystem psys;

    ParticleSystem m_System;
    ParticleSystem.Particle[] m_Particles;
    public float m_Drift = 0.01f;

    private void LateUpdate()
    {
        InitializeIfNeeded();

        bool bWorld = (m_System.main.simulationSpace == ParticleSystemSimulationSpace.World);
        if (bWorld)
        {
            int numParticlesAlive = m_System.GetParticles(m_Particles);
            // Change only the particles that are alive
            for (int i = 0; i < numParticlesAlive; i++)
            {
                m_Particles[i].position += Vector3.up * m_Drift;
            }
            // Apply the particle changes to the Particle System
            m_System.SetParticles(m_Particles, numParticlesAlive);
        }
    }

    void InitializeIfNeeded()
    {
        if (m_System == null)
            m_System = GetComponent<ParticleSystem>();

        if (m_Particles == null || m_Particles.Length < m_System.main.maxParticles)
            m_Particles = new ParticleSystem.Particle[m_System.main.maxParticles];
    }
}
