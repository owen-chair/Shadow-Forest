
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SkeletonSpellBall : UdonSharpBehaviour
{
    public VRCPlayerApi m_Target;

    public GameObject m_Hand;
    private bool m_IsThrown = false;
    private float m_TimeCast = 0.0f;
    private bool m_Instantiated = false;

    public AudioSource m_FlyingAudioSource;
    public AudioSource m_HitAudioSourceLocal;
    public AudioSource m_HitAudioSourceSpatial;

    public MeshRenderer m_MeshRenderer;
    public Bloodsplat_Object_Pool_Manager m_Bloodsplat_Object_Pool_Manager;

    private bool m_Exploded = false;
    private bool m_SpellTimedOut = false;

    public readonly float m_MoveSpeed = 11.5f; // Units per second
    private float m_RotationAngle = 0f;
    public readonly float m_RotationSpeed = 720f; // Degrees per second (2 full rotations)
    
    public readonly float m_SpellTimeout = 3.0f;
    

    void Start()
    {
        
    }

    public static void Instantiate(GameObject prefab, GameObject hand, VRCPlayerApi target, Bloodsplat_Object_Pool_Manager bloodpool)
    {
        if (hand == null) return;
        if (target == null) return;
        if (!target.IsValid()) return;
        if (prefab == null) return;
        if (hand.transform == null) return;

        GameObject spell = UnityEngine.Object.Instantiate(
            prefab,
            hand.transform.position,
            hand.transform.rotation
        );

        spell.SetActive(true);

        SkeletonSpellBall spellBall = spell.GetComponent<SkeletonSpellBall>();

        spellBall.m_Hand = hand;
        spellBall.m_Target = target;
        spellBall.m_IsThrown = false;
        spellBall.m_TimeCast = 0.0f;
        spellBall.m_Instantiated = true;
        spellBall.m_Exploded = false;
        spellBall.m_Bloodsplat_Object_Pool_Manager = bloodpool;

        AudioSource[] audioSources = spell.GetComponents<AudioSource>();

        if (audioSources.Length < 3) return;

        // Assign by index (assuming flying is first, hit is second)
        spellBall.m_FlyingAudioSource = audioSources[0];
        spellBall.m_HitAudioSourceLocal = audioSources[1];
        spellBall.m_HitAudioSourceSpatial = audioSources[2];

        MeshRenderer meshRenderer = spell.GetComponent<MeshRenderer>();
        spellBall.m_MeshRenderer = meshRenderer;

        LevelGenerator levelGenerator = GameObject.Find("LevelGenerator").GetComponent<LevelGenerator>();
        if (levelGenerator == null) return;
        if (levelGenerator.transform == null) return;

        spellBall.transform.SetParent(levelGenerator.transform);

        if(spellBall.m_Target.isLocal)
        {
            spellBall.m_Target.Immobilize(true);
        }
    }

    void Update()
    {
        if (!this.m_Instantiated) return;

        if (this.m_Target == null || !this.m_Target.IsValid())
        {
            Destroy(this.gameObject);
            Destroy(this);
            return;
        }

        if (this.m_Exploded || this.m_SpellTimedOut)
        {
            return;
        }

        this.m_TimeCast += Time.deltaTime;
        this.m_IsThrown = this.m_TimeCast > 0.11f;
        this.m_SpellTimedOut = this.m_TimeCast > this.m_SpellTimeout;

        if (!this.m_IsThrown && !this.m_SpellTimedOut)
        {
            this.transform.position = this.m_Hand.transform.position;
        }
        else if (this.m_IsThrown && !m_SpellTimedOut)
        {
            Vector3 targetsFace = this.m_Target.GetBonePosition(HumanBodyBones.Head);
            Vector3 direction = (targetsFace - this.transform.position).normalized;
            
            // Spinning rotation around forward axis
            this.m_RotationAngle += this.m_RotationSpeed * Time.deltaTime;
            Quaternion newRot = Quaternion.Euler(0f, 0f, this.m_RotationAngle);

            // Constant movement speed
            Vector3 newPos = this.transform.position + direction * this.m_MoveSpeed * Time.deltaTime;
            
            this.transform.SetPositionAndRotation(newPos, newRot);

            if (Vector3.Distance(this.transform.position, targetsFace) < 0.5f)
            {
                this.On_Explode(true);
            }
        }
        else if (this.m_SpellTimedOut)
        {
            this.On_Explode(false);
        }
    }

    public void On_Explode(bool didHit)
    {   
        if (this.m_Target == null) return;
        if (!this.m_Target.IsValid()) return;
        if (this.m_Exploded) return;
        if (this.m_FlyingAudioSource == null) return;
        if (this.m_HitAudioSourceLocal == null) return;
        if (this.m_HitAudioSourceSpatial == null) return;

        if (this.m_FlyingAudioSource != null && this.m_FlyingAudioSource.isPlaying)
        {
            this.m_FlyingAudioSource.Stop();
        }

        if (didHit)
        {
            if (this.m_Target.isLocal)
            {
                if (this.m_HitAudioSourceLocal != null && !this.m_HitAudioSourceLocal.isPlaying)
                {
                    this.m_HitAudioSourceLocal.Play();
                }
            }
            else
            {
                if (this.m_HitAudioSourceSpatial != null && !this.m_HitAudioSourceSpatial.isPlaying)
                {
                    this.m_HitAudioSourceSpatial.Play();
                }
            }

            this.OnHit();
        }

        if(this.m_Target.isLocal)
        {
            this.m_Target.Immobilize(false);
        }

        this.m_Exploded = true;
        this.m_MeshRenderer.enabled = false;

        Destroy(this.gameObject, 0.75f);
    }

    public Vector3 GetBloodsplatPosition(VRCPlayerApi player)
    {
        Vector3 playerPosition = player.GetPosition();
        Ray ray = new Ray(playerPosition, Vector3.down);
        RaycastHit hit;
        LayerMask floorMask = LayerMask.GetMask("Environment", "Default"); // Adjust the layers as needed

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorMask))
        {
            return hit.point;
        }

        // If the raycast doesn't hit anything, return the player's position
        return playerPosition;
    }

    public void OnHit()
    {
        if (!this.m_Target.IsValid()) return;

        Vector3 bloodPosition = GetBloodsplatPosition(this.m_Target);
        this.m_Bloodsplat_Object_Pool_Manager.CreateBloodsplat(
            bloodPosition,
            this.m_Target.GetRotation()
        );

        if (this.m_Target.isLocal)
        {
            SendCustomEventDelayedSeconds(nameof(this.RespawnOnHit), 0.5f);
        }
    }

    public void RespawnOnHit()
    {
        GameObject game_gameObject = GameObject.Find("Game");
        if (game_gameObject == null) return;

        Game game = game_gameObject.GetComponent<Game>();
        if (game == null) return;
        game.OnLocalPlayerKilledBySkeleton();
        game.SendPlayerToGameStart(this.m_Target);
    }
}
