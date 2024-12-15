using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Data;
using VRC.Udon;
using Miner28.UdonUtils.Network;

public class CeremonialWine : NetworkInterface
{
    public Game m_Game;
    public Lord m_Lord;
    private VRC_Pickup m_Pickup;
    public PlayerCapsuleColliderManager m_PlayerCapsuleColliderManager;

    public AudioSource m_SwallowSoundSource;
    public AudioSource m_LessIsMoreSoundSource;
    public AudioSource m_HeartBeat_AudioSource;

    private Vector3 m_OriginalPosition;

    void Start()
    {
        this.m_OriginalPosition = this.transform.position;
        this.m_Pickup = GetComponent<VRC_Pickup>();
    }

    public override void OnPickup()
    {
        base.OnPickup();

        Debug.Log("[CeremonialWine.cs] Item picked up");
    }

    public override void OnPickupUseDown()
    {
        base.OnPickupUseDown();

        if (Networking.LocalPlayer == null)
        {
            Debug.LogError("[CeremonialWine.cs] Use: Networking.LocalPlayer is null");
            return;
        }

        if (this.m_Game == null)
        {
            Debug.LogError("[CeremonialWine.cs] Use: m_Game is null");
            return;
        }

        if (this.m_Pickup == null)
        {
            Debug.LogError("[CeremonialWine.cs] Use: m_Pickup is null");
            return;
        }
        if (!this.m_Pickup.IsHeld)
        {
            Debug.LogError("[CeremonialWine.cs] Use: m_Pickup is not held");
            return;
        }

        if (this.m_Game.m_IsLocalPlayerFollower) return;
        
        if (this.m_SwallowSoundSource != null && !this.m_SwallowSoundSource.isPlaying)
        {
            this.m_SwallowSoundSource.Play();
        }

        this.BloodEffect_Play();
        
        SendMethodNetworked(
            nameof(this.On_RequestBecomeFollower),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer)
        );
    }

    public void BloodEffect_Play()
    {
        SendCustomEventDelayedSeconds(nameof(this.PlayHeartbeat), 0.1f);
    }

    public void PlayHeartbeat()
    {
        if (this.m_HeartBeat_AudioSource != null && !this.m_HeartBeat_AudioSource.isPlaying)
        {
            this.m_HeartBeat_AudioSource.Play();
        }
    }

    private float m_OnRequestBecomeFollower_LastInvokeTime = 0;

    [NetworkedMethod]
    public void On_RequestBecomeFollower(VRCPlayerApi requestingPlayer)
    {
        if(!Networking.IsMaster) return;
        if(Time.time - this.m_OnRequestBecomeFollower_LastInvokeTime < 10.0f) return;

        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;

        if (this.m_Game == null) return;
        if (!this.m_Game.m_GameStatus.Equals(GameStatus.InProgress)) return;

        if (this.m_Game.m_PlayerFollowerStatus == null) return;
        if (this.m_Game.m_PlayerFollowerCount > 0) return;

        this.m_OnRequestBecomeFollower_LastInvokeTime = Time.time;

        this.m_Game.m_PlayerFollowerCount = 1;
        this.m_Game.m_PlayerFollowerStatus[this.m_Game.VRCPlayerApiObjectToUniqueNameString(requestingPlayer)] = true;

        this.m_PlayerCapsuleColliderManager.SetColliderStatesFromDataDictionary(
            this.m_Game.m_PlayerFollowerStatus
        );

        SendMethodNetworked(
            nameof(this.On_FollowerAnnouncement),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(requestingPlayer)
        );
    }

    [NetworkedMethod]
    public void On_FollowerAnnouncement(VRCPlayerApi requestingPlayer, VRCPlayerApi player)
    {
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!requestingPlayer.isMaster) return;
        if (player == null) return;
        if (!player.IsValid()) return;
        if(this.m_Game == null) return;
        if(!this.m_Game.m_GameStatus.Equals(GameStatus.InProgress)) return;

        if (player.isLocal)
        {
            this.m_Game.m_IsLocalPlayerFollower = true;

            if (this.m_Lord == null) return;
            this.m_Lord.DisableCapsuleCollider();

            if (this.m_LessIsMoreSoundSource != null)
            {
                this.m_LessIsMoreSoundSource.Play();
            }
        }
        
        if(this.m_Game.m_PlayerFollowerStatus == null) return;
        string playerNameId = this.m_Game.VRCPlayerApiObjectToUniqueNameString(player);
        if(!this.m_Game.m_PlayerFollowerStatus.ContainsKey(playerNameId)) return;

        this.m_Game.m_PlayerFollowerStatus[playerNameId] = true;
        this.m_Game.m_PlayerFollowerCount = 1;

        this.m_PlayerCapsuleColliderManager.SetColliderStatesFromDataDictionary(
            this.m_Game.m_PlayerFollowerStatus
        );

        if(Networking.IsMaster && this.m_Game.AreAllNonFollowersDead())
        {
            this.m_Game.AnnounceGameFinish();
        }
    }

    public void Reset()
    {
        // Stop all sounds
        if (this.m_SwallowSoundSource != null)
        {
            this.m_SwallowSoundSource.Stop();
        }

        if (this.m_LessIsMoreSoundSource != null)
        {
            this.m_LessIsMoreSoundSource.Stop();
        }

        if (this.m_HeartBeat_AudioSource != null)
        {
            this.m_HeartBeat_AudioSource.Stop();
        }

        if (this.m_Pickup == null) return;
        if (this.m_Pickup.IsHeld && this.m_Pickup.currentPlayer != null && this.m_Pickup.currentPlayer.IsValid() && this.m_Pickup.currentPlayer.isLocal)
        {
            this.m_Pickup.Drop();
        }

        // Reset position
        if (this.transform == null) return;
        this.transform.position = this.m_OriginalPosition;
    }

}

// Attributions:
// - Miner28: NetworkEventCaller [MIT] via GitHub
// - Lesiakower: Less Is More.mp3 [CC-BY] via Pixabay
// - grass mix by Steve B [CC-BY] via Poly Pizza
// - Heartbeat Sound by BRVHRTZ [CC-BY] via Pixabay
// - Night Ambience Sound by brunobosell [CC-BY] via Pixabay
// - Cave Ambience Sound by yottasounds [CC-BY] via Pixabay
// - Plants by Marbles studio [CC-BY] via Sketchfab
// - Rocks by StevenTB [CC-BY] via Sketchfab
// - Temple Entrance by MMandali [CC-BY] via Sketchfab
// - Skeleton Model by Stanley Creative [CC-BY] via Sketchfab
// - Furry Model by Vloosh [CC-BY] via Sketchfab
// - Sword Model by David Zapata [CC-BY] via Sketchfab
// - Dagger Model by pau.raurellgomis [CC-BY] via Sketchfab
// - blood splat model by adolfochs [CC-BY] via Sketchfab
// - Bone pile model by giga [CC-BY] via Sketchfab
// - Textures by 3dtextures.me [CC0]
// - Stab sound by u_xjrmmgxfru [CC-BY] via Pixabay
// - Stunned sound by floraphonic [CC-BY] via Pixabay
// - Wood block sound by freesound_community [CC-BY] via Pixabay
// - Wood break sound by u_xjrmmgxfru [CC-BY] via Pixabay