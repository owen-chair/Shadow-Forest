
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Exit : UdonSharpBehaviour
{
    public Game m_Game;
    public GameObject m_Exit_Spawn_SpawnerReferenceCube;
    public Lord m_Lord;

    private bool m_HasEnteredAlready = false;

    void Start()
    {
        if (this.m_Game == null) return;

        this.m_Game.m_Exit = this;
    }

    public override void Interact()
    {
        if (this.m_Game == null) return;
        base.Interact();

        this.m_Game.StopCaveAmbience();
        this.m_Game.StopForestAmbience();

        this.SendPlayerToExitCave();

        if (this.m_HasEnteredAlready) return;

        this.m_Game.OnLocalPlayerForestCompleted();

        this.m_HasEnteredAlready = true;

        SendCustomEventDelayedSeconds(
            nameof(On_Entered_Cave),
            1.0f
        );
    }

    public void On_Entered_Cave()
    {
        this.m_Lord.On_PlayerEntered();
    }

    public void SendPlayerToExitCave()
    {
        if(this.m_Exit_Spawn_SpawnerReferenceCube == null)
        {
            Debug.LogError("[Game.cs] On_StartGameReceived: m_Game_Spawn_SpawnerReferenceCube is null");
            return;
        }

        Networking.LocalPlayer.TeleportTo(
            this.m_Exit_Spawn_SpawnerReferenceCube.transform.position,
            this.m_Exit_Spawn_SpawnerReferenceCube.transform.rotation
        );
    }

    public void Reset()
    {
        this.m_HasEnteredAlready = false;
    }
}
