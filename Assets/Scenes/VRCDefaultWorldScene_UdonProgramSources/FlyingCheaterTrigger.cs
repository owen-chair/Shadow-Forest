
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FlyingCheaterTrigger : UdonSharpBehaviour
{
    public Game m_Game;

    void Start()
    {
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == null) return;
        if (!player.IsValid()) return;
        if (!player.isLocal) return;
        if (this.m_Game == null) return;

        this.m_Game.SendPlayerToGameStart(player);
    }
}