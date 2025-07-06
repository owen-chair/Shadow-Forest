using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Data;
using Miner28.UdonUtils.Network;

public class Potion : NetworkInterface
{
    public VRC_Pickup m_Pickup;
    public ParticleSystem m_ParticleSystem;
    public Transform m_ParticleSystemTransform;
    public Vector3 m_ParticleSystemTransformOriginalPosition;
    public hehehe m_BreakSoundObject;

    private Vector3 m_OriginalPosition;
    private Quaternion m_OriginalRotation;

    // Store who last held this potion
    private VRCPlayerApi m_LastLocalHolder;
    private float m_LastDropTime = 0;
    public float m_LastUse = 0;
    public GameObject m_RespawnDroppedItemTrigger;

    void Start()
    {
        // Save original position/rotation for reset
        this.m_OriginalPosition = this.transform.position;
        this.m_OriginalRotation = this.transform.rotation;

        this.m_ParticleSystemTransformOriginalPosition = this.m_ParticleSystemTransform.position;
    }

    // Called when any player picks up this pickup
    public override void OnPickup()
    {
        // If the local player picks it up, remember that
        if (Networking.LocalPlayer != null && Networking.LocalPlayer.isLocal)
        {
            this.m_LastLocalHolder = Networking.LocalPlayer;
        }
    }

    // Called when the local player drops it
    public override void OnDrop()
    {
        // If the local player drops it, update holder info
        if (Networking.LocalPlayer != null && Networking.LocalPlayer.isLocal)
        {
            this.m_LastLocalHolder = Networking.LocalPlayer;
            this.m_LastDropTime = Time.time; // Store the time of the drop
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.m_RespawnDroppedItemTrigger == null) return;
        if (other == null) return;
        if (other.gameObject != this.m_RespawnDroppedItemTrigger) return;

        if (this.m_Pickup == null) return;
        if (this.m_Pickup.IsHeld) return;

        this.ResetPotionLocal();
    }

    void OnCollisionEnter(Collision collision)
    {
        // 1) Return if it's still being held; not actually thrown
        if (this.m_Pickup.IsHeld) return;

        // 2) The item was last held by someone else or nobody
        if (this.m_LastLocalHolder == null) return;
        if (!this.m_LastLocalHolder.IsValid()) return;
        if (!this.m_LastLocalHolder.isLocal) return;
        if (this.m_LastDropTime == 0) return;
        if (Time.time - this.m_LastDropTime > 3f) return; // only break if dropped recently
        if (Time.time - this.m_LastUse < 1f) return; // Prevent immediate re-break
        if (Vector3.Distance(this.transform.position, this.m_OriginalPosition) < 2f) return;
        if (collision == null || collision.gameObject == null) return;


        // -- At this point, we know the local player last dropped it, and it’s not still held. --

        // (Optional) Check if velocity is high enough to count as a throw:
        // if (GetComponent<Rigidbody>().velocity.magnitude < 2f) return;

        Vector3 breakPosition = new Vector3(
            this.transform.position.x,
            this.transform.position.y + 0.5f, // Slightly above ground
            this.transform.position.z
        );

        // Reset potion locally
        ResetPotionLocal();

        // Fire a network event to show the particle effect for everyone
        SendMethodNetworked(
            nameof(this.NW_SpawnBreakParticle),
            SyncTarget.All,
            new DataToken(breakPosition)
        );
    }

    [NetworkedMethod]
    public void NW_SpawnBreakParticle(Vector3 atPosition)
    {
        this.m_Pickup.pickupable = false; // Disable pickup while particle is active
        this.m_LastUse = Time.time; // Update last use time
        if (this.m_ParticleSystem != null && this.m_ParticleSystemTransform != null)
        {
            this.m_ParticleSystemTransform.position = atPosition;
            this.m_ParticleSystem.Play();

            // Reset the particle effect after 10 seconds if desired
            SendCustomEventDelayedSeconds(nameof(this.OnEffectEnded), 10f);
        }
        //on next frame
        SendCustomEventDelayedFrames(nameof(this.PlayBreakSound), 1);
    }

    public void PlayBreakSound()
    {
        if (this.m_BreakSoundObject != null)
        {
            this.m_BreakSoundObject.Play();
        }
    }

    public void StopBreakParticle()
    {
        this.m_Pickup.pickupable = true; // Re-enable pickup
        if (this.m_ParticleSystem != null)
        {
            this.m_ParticleSystem.Stop();
        }
    }

    public void OnEffectEnded()
    {
        this.StopBreakParticle();
        this.ResetPotionLocal();

        // let the particles clear before resetting position
        SendCustomEventDelayedSeconds(nameof(this.ResetSmokeTransform), 3f);
    }

    public void ResetSmokeTransform()
    {
        if (this.m_ParticleSystemTransform != null)
        {
            this.m_ParticleSystemTransform.position = this.m_ParticleSystemTransformOriginalPosition;
        }
    }

    public void ResetPotionLocal()
    {
        this.ResetVelocities();

        // Teleport the potion back to original location
        transform.position = this.m_OriginalPosition;
        transform.rotation = this.m_OriginalRotation;

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
}