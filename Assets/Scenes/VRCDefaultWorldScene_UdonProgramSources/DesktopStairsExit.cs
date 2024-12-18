using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DesktopStairsExit : UdonSharpBehaviour
{
    public Game m_Game;
    private GameObject m_TeleportLocation;

    void Start()
    {
    }

    public void Initialise(ref GameObject loc)
    {
        if (Networking.LocalPlayer == null) return;
        if (!Networking.LocalPlayer.IsValid()) return;

        if (Networking.LocalPlayer.IsUserInVR())
        {
            this.gameObject.SetActive(false);
        }
        else if (loc != null)
        {
            this.SetTeleportLocation(ref loc);
            this.gameObject.SetActive(true);
        }
    }

    public void SetTeleportLocation(ref GameObject pos)
    {
        this.m_TeleportLocation = pos;
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == null) return;
        if (!player.IsValid()) return;
        if (!player.isLocal) return;
        if (this.m_Game == null) return;
        if (player.IsUserInVR()) return;

        this.m_Game.StopCaveAmbience();
        this.m_Game.BeginForestAmbience();

        if (this.m_TeleportLocation == null)
        {
            Debug.LogError("[DesktopStairsExit.cs] Interact: m_TeleportLocation is null");
            return;
        }

        Networking.LocalPlayer.TeleportTo(
            this.m_TeleportLocation.transform.position,
            this.m_TeleportLocation.transform.rotation
        );
    }
}