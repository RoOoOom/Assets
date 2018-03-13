using UnityEngine;
public class LoopParticleSystem : MonoBehaviour
{
    public float duration = 1f;
    private ParticleSystem[] m_particles;

    void Awake()
    {
        m_particles = GetComponentsInChildren<ParticleSystem>(true);
    }

    void OnEnable()
    {
        InvokeRepeating("Loop", 0f, duration);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void Loop()
    {
        if (null != m_particles && m_particles.Length > 0)
        {
            for (int i = 0; i < m_particles.Length; i++)
            {
                m_particles[i].Play();
            }
        }
    }
}
