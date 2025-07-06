
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Data;
using VRC.Udon;
using Miner28.UdonUtils.Network;

public class shield : NetworkInterface
{
    public Game m_Game;
    public VRC_Pickup m_Pickup;

    private Vector3 m_OriginalPosition;
    private Quaternion m_OriginalRotation;

    private Vector3 m_Gib1_OriginalPosition;
    private Vector3 m_Gib2_OriginalPosition;
    private Vector3 m_Gib3_OriginalPosition;
    private Vector3 m_Gib4_OriginalPosition;
    private Vector3 m_Gib5_OriginalPosition;
    private Vector3 m_Gib6_OriginalPosition;

    public GameObject m_Gib1;
    public GameObject m_Gib2;
    public GameObject m_Gib3;
    public GameObject m_Gib4;
    public GameObject m_Gib5;
    public GameObject m_Gib6;

    // hehehe could be named better but idc
    public hehehe m_BreakSound;
    public GameObject m_RespawnDroppedItemTrigger;
    
    void Start()
    {
        this.m_OriginalPosition = this.transform.position;
        this.m_OriginalRotation = this.transform.rotation;

        this.m_Pickup = this.GetComponent<VRC_Pickup>();

        this.InitOriginalPositions();
    }

    void InitOriginalPositions()
    {
        if(this.m_Gib1 != null && this.m_Gib1.transform != null)
        {
            this.m_Gib1_OriginalPosition = this.m_Gib1.transform.position;
        }
        if(this.m_Gib2 != null && this.m_Gib2.transform != null)
        {
            this.m_Gib2_OriginalPosition = this.m_Gib2.transform.position;
        }
        if(this.m_Gib3 != null && this.m_Gib3.transform != null)
        {
            this.m_Gib3_OriginalPosition = this.m_Gib3.transform.position;
        }
        if(this.m_Gib4 != null && this.m_Gib4.transform != null)
        {
            this.m_Gib4_OriginalPosition = this.m_Gib4.transform.position;
        }
        if(this.m_Gib5 != null && this.m_Gib5.transform != null)
        {
            this.m_Gib5_OriginalPosition = this.m_Gib5.transform.position;
        }
        if(this.m_Gib6 != null && this.m_Gib6.transform != null)
        {
            this.m_Gib6_OriginalPosition = this.m_Gib6.transform.position;
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
        
        if (this.m_Gib1 != null)
        {
            this.m_Gib1.transform.position = this.m_Gib1_OriginalPosition;
        }

        if (this.m_Gib2 != null)
        {
            this.m_Gib2.transform.position = this.m_Gib2_OriginalPosition;
        }

        if (this.m_Gib3 != null)
        {
            this.m_Gib3.transform.position = this.m_Gib3_OriginalPosition;
        }

        if (this.m_Gib4 != null)
        {
            this.m_Gib4.transform.position = this.m_Gib4_OriginalPosition;
        }

        if (this.m_Gib5 != null)
        {
            this.m_Gib5.transform.position = this.m_Gib5_OriginalPosition;
        }

        if (this.m_Gib6 != null)
        {
            this.m_Gib6.transform.position = this.m_Gib6_OriginalPosition;
        }
    }

    public void Break(VRCPlayerApi holdingPlayer)
    {
        if (this.m_Game == null) return;
        if (holdingPlayer == null) return;
        if (!holdingPlayer.IsValid()) return;
        if (!holdingPlayer.isLocal) return;

        Vector3 shieldPos = new Vector3(
            this.transform.position.x,
            this.transform.position.y + 0.5f,
            this.transform.position.z
        );

        SendMethodNetworked(
            nameof(this.OnBreak),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(shieldPos)
        );

        this.Reset();
    }

    [NetworkedMethod]
    public void OnBreak(VRCPlayerApi holdingPlayer, Vector3 shieldPos)
    {
        if (this.m_Game == null) return;
        if (holdingPlayer == null) return;
        if (!holdingPlayer.IsValid()) return;

        if(this.m_BreakSound != null)
        {
            this.m_BreakSound.Play();
        }

        if (this.m_Gib1 != null)
        {
            this.m_Gib1.transform.position = shieldPos;
        }

        if (this.m_Gib2 != null)
        {
            this.m_Gib2.transform.position = shieldPos;
        }
        
        if (this.m_Gib3 != null)
        {
            this.m_Gib3.transform.position = shieldPos;
        }

        if (this.m_Gib4 != null)
        {
            this.m_Gib4.transform.position = shieldPos;
        }

        if (this.m_Gib5 != null)
        {
            this.m_Gib5.transform.position = shieldPos;
        }

        if (this.m_Gib6 != null)
        {
            this.m_Gib6.transform.position = shieldPos;
        }
    }
}
