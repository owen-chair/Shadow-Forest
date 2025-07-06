
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Narrator : UdonSharpBehaviour
{
    public AudioSource m_Dice_AudioSource;
    public AudioSource m_Intro_AudioSource;
    public AudioSource m_CaveIntro_AudioSource;
    public AudioSource m_ThroughTheTrees_AudioSource;
    public AudioSource m_BoneHit1_AudioSource;
    public AudioSource m_TimeRunsItsCourse_AudioSource;
    public AudioSource m_SomeoneElseKilledFurry_AudioSource;
    public AudioSource m_YouFailedToProtectFurry_AudioSource;
    public AudioSource m_YouKilledFurry_AudioSource;
    public AudioSource m_YouJoinedAsFollower_AudioSource;
    public AudioSource m_YouJoinedAlone_AudioSource;
    public AudioSource m_EveryoneDied_AudioSource;

    private bool m_HasPlayedIntro = false;
    private bool m_HasPlayedCaveIntro = false;
    private bool m_HasPlayedThroughTheTrees = false;
    void Start()
    {
        this.m_HasPlayedIntro = false;
        this.m_HasPlayedCaveIntro = false;
        this.m_HasPlayedThroughTheTrees = false;
    }

    public void PlayDice()
    {
        if (this.m_Dice_AudioSource == null) return;
        if (this.m_Dice_AudioSource.isPlaying) return;

        this.m_Dice_AudioSource.Play();
    }

    public void PlayIntro()
    {
        if (this.m_HasPlayedIntro) return; // Prevent playing intro multiple times
        if (this.m_Intro_AudioSource == null) return;
        if (this.m_Intro_AudioSource.isPlaying) return;

        this.m_HasPlayedIntro = true;
        this.m_Intro_AudioSource.Play();
    }

    public void PlayCaveIntro()
    {
        if (this.m_HasPlayedCaveIntro) return; // Prevent playing cave intro multiple times
        if (this.m_CaveIntro_AudioSource == null) return;
        if (this.m_CaveIntro_AudioSource.isPlaying) return;

        this.m_HasPlayedCaveIntro = true;
        this.m_CaveIntro_AudioSource.Play();
    }

    public void StopCaveIntro()
    {
        if (this.m_CaveIntro_AudioSource == null) return;
        if (!this.m_CaveIntro_AudioSource.isPlaying) return;

        this.m_CaveIntro_AudioSource.Stop();
    }

    public void PlayThroughTheTrees()
    {
        if (this.m_HasPlayedThroughTheTrees) return; // Prevent playing through the trees multiple times
        if (this.m_ThroughTheTrees_AudioSource == null) return;
        if (this.m_ThroughTheTrees_AudioSource.isPlaying) return;

        this.StopCaveIntro();
        this.m_HasPlayedThroughTheTrees = true;
        this.m_ThroughTheTrees_AudioSource.Play();
    }

    public void StopThroughTheTrees()
    {
        if (this.m_ThroughTheTrees_AudioSource == null) return;
        if (!this.m_ThroughTheTrees_AudioSource.isPlaying) return;

        this.m_ThroughTheTrees_AudioSource.Stop();
    }

    public void PlayBoneHit1()
    {
        if (this.m_BoneHit1_AudioSource == null) return;
        if (this.m_BoneHit1_AudioSource.isPlaying) return;

        this.StopThroughTheTrees();
        this.StopCaveIntro();
        this.m_BoneHit1_AudioSource.Play();
    }

    public void PlayTimeRunsItsCourse()
    {
        if (this.m_TimeRunsItsCourse_AudioSource == null) return;
        if (this.m_TimeRunsItsCourse_AudioSource.isPlaying) return;

        this.m_TimeRunsItsCourse_AudioSource.Play();
    }

    public void PlaySomeoneElseKilledFurry()
    {
        if (this.m_SomeoneElseKilledFurry_AudioSource == null) return;
        if (this.m_SomeoneElseKilledFurry_AudioSource.isPlaying) return;

        this.m_SomeoneElseKilledFurry_AudioSource.Play();
    }

    public void PlayYouFailedToProtectFurry()
    {
        if (this.m_YouFailedToProtectFurry_AudioSource == null) return;
        if (this.m_YouFailedToProtectFurry_AudioSource.isPlaying) return;

        this.m_YouFailedToProtectFurry_AudioSource.Play();
    }

    public void PlayYouKilledFurry()
    {
        if (this.m_YouKilledFurry_AudioSource == null) return;
        if (this.m_YouKilledFurry_AudioSource.isPlaying) return;

        this.m_YouKilledFurry_AudioSource.Play();
    }

    public void PlayYouJoinedAsFollower()
    {
        if (this.m_YouJoinedAsFollower_AudioSource == null) return;
        if (this.m_YouJoinedAsFollower_AudioSource.isPlaying) return;

        this.m_YouJoinedAsFollower_AudioSource.Play();
    }

    public void PlayYouJoinedAlone()
    {
        if (this.m_YouJoinedAlone_AudioSource == null) return;
        if (this.m_YouJoinedAlone_AudioSource.isPlaying) return;

        this.m_YouJoinedAlone_AudioSource.Play();
    }

    public void PlayEveryoneDied()
    {
        if (this.m_EveryoneDied_AudioSource == null) return;
        if (this.m_EveryoneDied_AudioSource.isPlaying) return;

        this.m_EveryoneDied_AudioSource.Play();
    }
}
