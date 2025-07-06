
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class hehehe : UdonSharpBehaviour
{
    public AudioSource m_HeheheAudioSource;

    void Start()
    {
        
    }

    public void Play()
    {
        if (this.m_HeheheAudioSource != null && !this.m_HeheheAudioSource.isPlaying)
        {
            this.m_HeheheAudioSource.Play();
        }
    }
}
