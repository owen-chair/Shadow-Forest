
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class puff_mushroom : UdonSharpBehaviour
{
    public GameObject m_PuffMushroom;
    public ParticleSystem m_PuffParticleSystem;
    private bool m_HasPuffed = false;
    public AudioSource m_PuffAudioSource;

    void Start()
    {
        this.m_HasPuffed = false;
    }

    public override void Interact()
    {
        if (this.m_HasPuffed) return; // Prevent multiple interactions
        this.m_HasPuffed = true;
        if (this.m_PuffMushroom != null)
        {
            // destroy the puff, revealing the stalk
            Destroy(this.m_PuffMushroom);
        }

        if (this.m_PuffParticleSystem != null && !this.m_PuffParticleSystem.isPlaying)
        {
            // play the puff
            this.m_PuffParticleSystem.Play();
        }

        if (this.m_PuffAudioSource != null && !this.m_PuffAudioSource.isPlaying)
        {
            // play the puff sound
            this.m_PuffAudioSource.Play();
        }
    }
}
