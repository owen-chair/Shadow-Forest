using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Data;
using VRC.Udon;
using Miner28.UdonUtils.Network;

public class Melee_Weapon : NetworkInterface
{
    public Game m_Game;
    private VRC_Pickup m_Pickup;

    public PlayerCapsuleColliderManager m_PlayerCapsuleColliderManager;
    public Bloodsplat_Object_Pool_Manager m_Bloodsplat_Object_Pool_Manager;

    public Lord m_Lord;

    private Vector3 m_OriginalPosition;
    private Quaternion m_OriginalRotation;
    public GameObject m_RespawnDroppedItemTrigger;
    void Start()
    {
        this.m_OriginalPosition = this.transform.position;
        this.m_OriginalRotation = this.transform.rotation;
        this.m_Pickup = this.GetComponent<VRC_Pickup>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.m_RespawnDroppedItemTrigger == null) return;
        if (other == null) return;
        if (other.gameObject != this.m_RespawnDroppedItemTrigger) return;

        if (this.m_Pickup == null) return;
        if (this.m_Pickup.IsHeld) return;

        this.Reset();
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (this.m_Game == null) return;
        if (collision == null) return;
        if (this.m_Pickup == null) return;
        if (!this.m_Pickup.IsHeld) return;
        if (this.m_Pickup.currentPlayer == null) return;
        if (!this.m_Pickup.currentPlayer.IsValid()) return;
        if (!this.m_Pickup.currentPlayer.isLocal) return;

        GameObject collisionObject = collision.gameObject;
        if (collisionObject == null) return;

        if(this.m_PlayerCapsuleColliderManager == null) return;

        VRCPlayerApi player = this.m_PlayerCapsuleColliderManager.GetPlayerByCapsuleCollider(
            collisionObject
        );
        
        if(player == null)
        {
            if(this.m_Game == null) return;
            if(!this.m_Game.m_IsLocalPlayerFollower)
            {   
                if (this.m_Lord != null)
                {
                    if (this.m_Lord.CheckMeleeHit(ref collision, ref collisionObject)) return;
                }
            }

            if(this.m_Game.m_EnemyAIs == null) return;
            if (this.m_Game.m_EnemyAIs.Length == 0) return;
            foreach (EnemyAI_Animated enemyAI in this.m_Game.m_EnemyAIs)
            {
                if (enemyAI == null) continue;
                if (enemyAI.CheckMeleeHit(ref collision, ref collisionObject))
                {
                    SendMethodNetworked(
                        nameof(this.OnEnemyAIHitByPlayer),
                        SyncTarget.All,
                        new DataToken(Networking.LocalPlayer),
                        new DataToken(enemyAI.m_ID)
                    );
                    return;
                }
            }

            return;
        }

        if(!player.IsValid()) return;
        if(player.isLocal) return;

        // successfully killed player so disable temporarily
        this.m_PlayerCapsuleColliderManager.DisableColliderTemporarily(
            collisionObject,
            5.0f
        );

        SendMethodNetworked(
            nameof(this.OnPlayerHit),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(player)
        );
    }

    [NetworkedMethod]
    public void OnEnemyAIHitByPlayer(VRCPlayerApi attackingPlayer, int enemyAI_ID)
    {
        if (this.m_Game == null) return;
        if (attackingPlayer == null) return;
        if (!attackingPlayer.IsValid()) return;

        foreach (EnemyAI_Animated enemyAI in this.m_Game.m_EnemyAIs)
        {
            if (enemyAI == null) continue;
            if (enemyAI.m_ID != enemyAI_ID) continue;

            enemyAI.OnHitByPlayer(attackingPlayer);
            return;
        }
    }

    [NetworkedMethod]
    public void OnPlayerHit(VRCPlayerApi attackingPlayer, VRCPlayerApi hitPlayer)
    {
        if(attackingPlayer == null || hitPlayer == null) return;
        if(!attackingPlayer.IsValid() || !hitPlayer.IsValid()) return;

        Vector3 bloodPosition = GetBloodsplatPosition(hitPlayer);
        this.m_Bloodsplat_Object_Pool_Manager.CreateBloodsplat(
            bloodPosition,
            hitPlayer.GetRotation()
        );

        if(hitPlayer.isLocal)
        {
            this.On_LocalPlayer_Hit(attackingPlayer);
        }

        if(Networking.IsMaster)
        {
            if(this.m_Game == null) return;
            this.m_Game.On_PlayerKilled(hitPlayer);
        }
    }

    public Vector3 GetBloodsplatPosition(VRCPlayerApi player)
    {
        if (player == null) return this.transform.position;
        if (!player.IsValid()) return this.transform.position;

        Vector3 playerPosition = player.GetPosition();
        Ray ray = new Ray(
            playerPosition + (Vector3.up * 0.5f),
            Vector3.down
        );

        RaycastHit hit;
        LayerMask floorMask = LayerMask.GetMask("Environment", "Default"); // Adjust the layers as needed

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorMask))
        {
            return hit.point;
        }

        // If the raycast doesn't hit anything, return the player's position
        return playerPosition;
    }

    public void On_LocalPlayer_Hit(VRCPlayerApi attackingPlayer)
    {
        Networking.LocalPlayer.Respawn();
        if(this.m_Game == null) return;

        this.m_Game.m_IsLocalPlayerFollower = false;
    }

    public void Reset()
    {
        if (this.m_Pickup == null) return;
        if (this.m_Pickup.currentPlayer == null) return;
        if (this.m_Pickup.IsHeld && this.m_Pickup.currentPlayer != null && this.m_Pickup.currentPlayer.IsValid() && this.m_Pickup.currentPlayer.isLocal)
        {
            this.m_Pickup.Drop();
        }

        if (this.transform == null) return;
        this.transform.position = this.m_OriginalPosition;
        this.transform.rotation = this.m_OriginalRotation;
    }
}