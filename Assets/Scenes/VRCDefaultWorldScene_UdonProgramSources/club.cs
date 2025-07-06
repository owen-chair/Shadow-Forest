using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Data;
using VRC.Udon;
using Miner28.UdonUtils.Network;

public class club : NetworkInterface
{
    public Game m_Game;
    private VRC_Pickup m_Pickup;

    private Vector3 m_OriginalPosition;
    private Quaternion m_OriginalRotation;

    private Vector3 m_Broken_Head_OriginalPosition;
    private Vector3 m_Broken_Handle_OriginalPosition;

    public GameObject m_Broken_Head;
    public GameObject m_Broken_Handle;
    public GameObject m_RespawnDroppedItemTrigger;

    void Start()
    {
        this.m_OriginalPosition = this.transform.position;
        this.m_OriginalRotation = this.transform.rotation;
        
        this.m_Pickup = this.GetComponent<VRC_Pickup>();

        if (this.m_Broken_Head != null)
        {
            this.m_Broken_Head_OriginalPosition = this.m_Broken_Head.transform.position;
        }

        if (this.m_Broken_Handle != null)
        {
            this.m_Broken_Handle_OriginalPosition = this.m_Broken_Handle.transform.position;
        }
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

        if (this.m_Game == null) return;
        if (this.m_Game.m_EnemyAIs == null) return;
        if (this.m_Game.m_EnemyAIs.Length == 0) return;
        foreach (EnemyAI_Animated enemyAI in this.m_Game.m_EnemyAIs)
        {
            if (enemyAI == null) continue;
            if (enemyAI.CheckMeleeHit(ref collision, ref collisionObject))
            {
                SendMethodNetworked(
                    nameof(this.OnEnemyAIStunnedByPlayer),
                    SyncTarget.All,
                    new DataToken(Networking.LocalPlayer),
                    new DataToken(enemyAI.m_ID),
                    new DataToken(this.transform.position),
                    new DataToken(this.transform.rotation)
                );

                this.Reset();
            }
        }
    }

    [NetworkedMethod]
    public void OnEnemyAIStunnedByPlayer(VRCPlayerApi attackingPlayer, int enemyAI_ID, Vector3 club_position, Quaternion club_rotation)
    {
        if (this.m_Game == null) return;
        if (attackingPlayer == null) return;
        if (!attackingPlayer.IsValid()) return;

        foreach (EnemyAI_Animated enemyAI in this.m_Game.m_EnemyAIs)
        {
            if (enemyAI == null) continue;
            if (enemyAI.m_ID != enemyAI_ID) continue;

            enemyAI.OnStunnedByPlayer(attackingPlayer);

            if (this.m_Broken_Head != null)
            {
                this.m_Broken_Head.transform.position = club_position;
                this.m_Broken_Head.transform.rotation = club_rotation;
            }

            if (this.m_Broken_Handle != null)
            {
                this.m_Broken_Handle.transform.position = club_position;
                this.m_Broken_Handle.transform.rotation = club_rotation;
            }

            return;
        }
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

        this.ResetVelocities();

        this.transform.position = this.m_OriginalPosition;
        this.transform.rotation = this.m_OriginalRotation;

        SendCustomEventDelayedFrames(nameof(this.ResetVelocities), 1);
    }

    public void ResetVelocities()
    {
        // Reset velocity if there's a rigidbody
        Rigidbody rb = this.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ResetAll()
    {
        this.Reset();

        if (this.m_Broken_Head != null)
        {
            this.m_Broken_Head.transform.position = this.m_Broken_Head_OriginalPosition;
        }

        if (this.m_Broken_Handle != null)
        {
            this.m_Broken_Handle.transform.position = this.m_Broken_Handle_OriginalPosition;
        }
    }
}
