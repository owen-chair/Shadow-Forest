
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Stairs : UdonSharpBehaviour
{
    public Game m_Game;
    private GameObject m_TeleportLocation;

    void Start()
    {
    }

    public void SetTeleportLocation(ref GameObject pos)
    {
        this.m_TeleportLocation = pos;
    }

    public override void Interact()
    {
        if (this.m_Game == null) return;
        base.Interact();

        this.m_Game.StopCaveAmbience();
        this.m_Game.BeginForestAmbience();

        if (this.m_TeleportLocation == null)
        {
            Debug.LogError("[Stairs.cs] Interact: m_TeleportLocation is null");
            return;
        }

        Networking.LocalPlayer.TeleportTo(
            this.m_TeleportLocation.transform.position,
            this.m_TeleportLocation.transform.rotation
        );
    }
}
