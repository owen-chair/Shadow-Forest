using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Data;
using VRC.Udon;
using Miner28.UdonUtils.Network;

public class Lord : NetworkInterface
{
    public Game m_Game;
    public Bloodsplat_Object_Pool_Manager m_Bloodsplat_Object_Pool_Manager;
    public Transform m_ArmaturePositionRotationTransform; //name = MasterControl
    public SkinnedMeshRenderer m_SkinnedMeshRenderer;
    public SkinnedMeshRenderer m_SkinnedMeshRenderer_Head;
    public CapsuleCollider m_CapsuleCollider;
    public GameObject m_Bounds_Min;
    public GameObject m_Bounds_Max;
    public Animator m_Animator; // Reference to the Animator component
    public Transform m_HeadTransform;
    public readonly Quaternion m_HeadTransformRotationOffset = Quaternion.Euler(0, 270, 270);

    private Vector3 m_TargetPosition;
    private float m_TargetStopDistance = 0.1f;
    private bool m_IsMoving = false;
    private bool m_IsStopping = false;

    private float m_ConversationStartTime = 0.0f;
    private float m_ConversationEndTime = 0.0f;
    private bool m_IsConversingWith = false;
    private VRCPlayerApi m_ConversingPlayer;
    private float m_LastRandomMoveTime = 0.0f;

    private float currentWalkSpeed = 0.0f;
    public float walkSpeed;
    public float moveSpeed;
    public float rotationSpeed;


    public float m_PlayerEnteredCaveTime = 0.0f;
    public AudioSource m_JoinMeAudioSource;


    private Ray m_MovementRay;
    private RaycastHit m_MovementRayHit;
    LayerMask m_LayerMask;

    void Start()
    {
        if (this.m_Animator == null)
        {
            this.m_Animator = this.GetComponent<Animator>();
        }

        AudioSource[] audioSources = this.GetComponents<AudioSource>();
        this.m_JoinMeAudioSource = audioSources[0];

        this.m_PlayerEnteredCaveTime = Time.time - 10.0f;
        this.m_IsConversingWith = false;
        this.m_ConversingPlayer = null;
        this.m_TargetPosition = this.transform.position;
        this.m_ConversationStartTime = Time.time - 10.0f;
        this.m_ConversationEndTime = Time.time - 10.0f;
        this.m_LastRandomMoveTime = Time.time;

        this.m_LayerMask = LayerMask.GetMask("Environment", "Default");
        this.m_MovementRay = new Ray();
    }

    private float m_LastUpdate = 0.0f;
    private readonly float UPDATE_RATE = 1.0f/30.0f;
    void Update()
    {
        this.m_LastUpdate += Time.deltaTime;
        if (!(this.m_LastUpdate > UPDATE_RATE)) return;


        if (this.m_IsMoving)
        {
            this.HandleMove(this.m_LastUpdate);
            this.HandleRotation(this.m_LastUpdate);
        }

        this.UpdateWalkSpeed(this.m_LastUpdate);
        this.m_LastUpdate = 0.0f;

        if (!Networking.IsMaster) return;

        if(!this.m_IsConversingWith && !this.m_IsMoving && Time.time - this.m_LastRandomMoveTime > 10.0f)
        {
            this.PickRandomTargetPosition();
        }
    }
    
    void LateUpdate()
    {
        if (this.m_IsConversingWith)
        {
            this.LookAtPlayer(this.m_ConversingPlayer);
        }
    }

    public void LookAtPlayer(VRCPlayerApi player)
    {
        if (player == null) return;
        if (!player.IsValid()) return;
        if(this.m_HeadTransform == null) return;

        Vector3 playerHeadPosition = player.GetBonePosition(HumanBodyBones.Head);
        Vector3 lookDirection = (playerHeadPosition - this.m_HeadTransform.position).normalized;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            // Force override the head rotation
            this.m_HeadTransform.SetPositionAndRotation(
                this.m_HeadTransform.position,
                targetRotation * this.m_HeadTransformRotationOffset
            );
        }
    }

    public void PickRandomTargetPosition()
    {
        if (!Networking.IsMaster) return;

        this.m_LastRandomMoveTime = Time.time;
        Vector3 min = this.m_Bounds_Min.transform.position;
        Vector3 max = this.m_Bounds_Max.transform.position;

        float randomX = Random.Range(min.x, max.x);
        float randomZ = Random.Range(min.z, max.z);

        Vector3 newTargetPosition = new Vector3(randomX, this.m_Bounds_Max.transform.position.y, randomZ);
        if (Vector3.Distance(newTargetPosition, this.m_TargetPosition) < 2.0f) return;

        SendMethodNetworked(
            nameof(this.Notify_SetTargetPosition),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(newTargetPosition)
        );
    }

    [NetworkedMethod]
    public void Notify_SetTargetPosition(VRCPlayerApi requestingPlayer, Vector3 targetPosition)
    {
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!requestingPlayer.isMaster) return;

        if (this.m_IsConversingWith && Time.time - this.m_ConversationStartTime < 10.0f) return;

        this.m_IsConversingWith = false;

        this.SetTargetPosition(targetPosition);
    }

    public void SetTargetPosition(Vector3 targetPosition, float targetStopDistance = 0.1f)
    {
        if (Vector3.Distance(targetPosition, this.m_TargetPosition) > 15.0f) return;

        this.m_TargetPosition = targetPosition;
        this.m_TargetStopDistance = targetStopDistance;
        this.m_IsMoving = true;
        this.m_IsStopping = false;
    }

    public float GetHeight()
    {
        this.m_MovementRay.origin = this.transform.position + (Vector3.up * 2.0f);
        this.m_MovementRay.direction = Vector3.down;

        if (Physics.Raycast(this.m_MovementRay, out this.m_MovementRayHit, Mathf.Infinity, this.m_LayerMask))
        {
            return this.m_MovementRayHit.point.y;
        }

        return 1.0f;
    }

    private void HandleMove(float delta)
    {
        Vector3 currentPos = transform.position;
        float distanceToTarget = Vector3.Distance(currentPos, this.m_TargetPosition);

        if(distanceToTarget < this.m_TargetStopDistance + 0.15f)
        {
            if (!this.m_IsStopping)
            {
                this.m_IsStopping = true;
            }
        }
        if (distanceToTarget < this.m_TargetStopDistance)
        {
            this.m_IsMoving = false;

            this.On_WalkTargetReached();

            return;
        }

        float y = GetHeight();
        Vector3 direction = (this.m_TargetPosition - currentPos).normalized;
        Vector3 newPos = currentPos + (direction * moveSpeed) * delta;
        newPos.y = y;

        transform.position = newPos;
    }

    private void HandleRotation(float delta)
    {
        if (!this.m_IsMoving) return;
        if(this.m_IsStopping) return;

        Vector3 lookDirection = (this.m_TargetPosition - transform.position).normalized;
        lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            // Apply an additional rotation offset to correct the orientation
            Quaternion rotationOffset = Quaternion.Euler(0, 0, 0); // Adjust this based on your model's initial orientation
            targetRotation *= rotationOffset;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * delta
            );
        }
    }
    
    private void UpdateWalkSpeed(float delta)
    {
        float targetSpeed = (this.m_IsMoving && !this.m_IsStopping) ? walkSpeed : 0.0f;
        float t = (this.m_IsStopping ? 15.0f : 20.0f) * delta;

        currentWalkSpeed = Mathf.Lerp(currentWalkSpeed, targetSpeed, t);
        currentWalkSpeed = Mathf.Clamp(currentWalkSpeed, 0.0f, 1.0f);
        if (this.m_Animator != null)
        {
            this.m_Animator.SetFloat("WalkSpeed", currentWalkSpeed);
        }
    }

    public bool CheckMeleeHit(ref Collision collision, ref GameObject collisionObject)
    {
        if (collision == null) return false;
        if (collisionObject == null) return false;

        if (collisionObject == this.m_CapsuleCollider.gameObject)
        {
            this.DisableCapsuleCollider();
            SendMethodNetworked(
                nameof(this.OnHitByPlayer),
                SyncTarget.All,
                new DataToken(Networking.LocalPlayer)
            );

            return true;
        }

        return false;
    }

    [NetworkedMethod]
    public void OnHitByPlayer(VRCPlayerApi attackingPlayer)
    {
        if (attackingPlayer == null) return;
        if (!attackingPlayer.IsValid()) return;

        this.Hide_Lord();
        this.m_JoinMeAudioSource.Stop();

        Vector3 bloodPosition = this.GetBloodsplatPosition();

        this.m_Bloodsplat_Object_Pool_Manager.CreateBloodsplat(
            bloodPosition,
            this.m_ArmaturePositionRotationTransform.rotation
        );

        if(Networking.IsMaster)
        {
            if(this.m_Game == null) return;
            this.m_Game.On_GameEndedLordDied(attackingPlayer);
        }
    }

    public Vector3 GetBloodsplatPosition()
    {
        Ray ray = new Ray(
            this.gameObject.transform.position + (Vector3.up * 0.5f),
            Vector3.down
        );

        RaycastHit hit;
        LayerMask floorMask = LayerMask.GetMask("Environment", "Default"); // Adjust the layers as needed

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorMask))
        {
            return hit.point;
        }

        // If the raycast doesn't hit anything, return the player's position
        return this.gameObject.transform.position;
    }

    public void Hide_Lord()
    {
        if (this.m_SkinnedMeshRenderer != null)
        {
            this.m_SkinnedMeshRenderer.enabled = false;
        }
        
        if (this.m_SkinnedMeshRenderer_Head != null)
        {
            this.m_SkinnedMeshRenderer_Head.enabled = false;
        }

        if (this.m_CapsuleCollider != null)
        {
            this.m_CapsuleCollider.enabled = false;
        }
    }

    public void Show_Lord()
    {
        if (this.m_SkinnedMeshRenderer != null && this.m_SkinnedMeshRenderer.enabled == false)
        {
            this.m_SkinnedMeshRenderer.enabled = true;
        }
        
        if (this.m_SkinnedMeshRenderer_Head != null && this.m_SkinnedMeshRenderer_Head.enabled == false)
        {
            this.m_SkinnedMeshRenderer_Head.enabled = true;
        }

        if (this.m_CapsuleCollider != null && this.m_CapsuleCollider.enabled == false)
        {
            this.m_CapsuleCollider.enabled = true;
        }
    }

    public void EnableCapsuleCollider()
    {
        if (this.m_CapsuleCollider != null && this.m_CapsuleCollider.enabled == false)
        {
            this.m_CapsuleCollider.enabled = true;
        }
    }

    public void DisableCapsuleCollider()
    {
        if (this.m_CapsuleCollider != null && this.m_CapsuleCollider.enabled == true)
        {
            this.m_CapsuleCollider.enabled = false;
        }
    }

    public void On_WalkTargetReached()
    {
        if(this.m_IsConversingWith)
        {
            this.On_SayDrinkWine();
        }
    }

    public void On_SayDrinkWine()
    {
        if(this.m_JoinMeAudioSource != null && !this.m_JoinMeAudioSource.isPlaying)
        {
            this.m_JoinMeAudioSource.Play();
        }

        if (this.m_Animator != null)
        {
            this.m_Animator.SetTrigger("PointSelfTrigger");
        }

        SendCustomEventDelayedSeconds(nameof(this.On_DrinkWineEnd), 6.0f);
    }

    public void On_DrinkWineEnd()
    {
        this.m_IsConversingWith = false;
        this.m_ConversationEndTime = Time.time;
    }

    public void On_PlayerEntered()
    {
        if (Networking.LocalPlayer == null) return;
        if (!Networking.LocalPlayer.IsValid()) return;
        if (this.m_Game == null) return;
        if (this.m_Game.m_GameStatus != GameStatus.InProgress) return;
        if (this.m_Game.m_PlayerFollowerCount > 0) return;

        if (Time.time - this.m_PlayerEnteredCaveTime < 10.0f) return;

        this.m_PlayerEnteredCaveTime = Time.time;

        SendMethodNetworked(
            nameof(this.On_Notify_PlayerEntered),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer)
        );
    }

    [NetworkedMethod]
    public void On_Notify_PlayerEntered(VRCPlayerApi player)
    {
        bool rejected = false;

        if (player == null) rejected = true;
        if (!player.IsValid()) rejected = true;
        if (Time.time - this.m_ConversationStartTime < 10.0f) rejected = true;
        if (player.IsValid() && Vector3.Distance(player.GetPosition(), this.transform.position) > 20.0f) rejected = true;
        if (this.m_IsConversingWith) rejected = true;
        if (this.m_Game == null) rejected = true;
        if (this.m_Game.m_GameStatus != GameStatus.InProgress) rejected = true;
        if (this.m_Game.m_PlayerFollowerCount > 0) rejected = true;

        if (Networking.IsMaster && rejected)
        {
            SendMethodNetworked(
                nameof(this.Notify_PlayerEnterFailed),
                SyncTarget.All,
                new DataToken(player)
            );
            return;
        }
        else if (rejected)
        {
            if(player.isLocal)
            {
                this.Notify_PlayerEnterFailed(player);
            }
            return;
        }

        this.m_ConversationStartTime = Time.time;
        this.m_IsConversingWith = true;
        this.m_ConversingPlayer = player;

        this.UnfreezeLocalPlayer();
        SendCustomEventDelayedSeconds(nameof(this.On_DrinkWineEnd), 10.0f);

        this.SetTargetPosition(player.GetPosition(), 2.0f);
    }

    [NetworkedMethod]
    public void Notify_PlayerEnterFailed(VRCPlayerApi targetPlayer)
    {
        if (targetPlayer == null) return;
        if (!targetPlayer.IsValid()) return;

        if (targetPlayer.isLocal)
        {
            this.m_IsConversingWith = false;
            this.m_ConversingPlayer = null;
            
            this.UnfreezeLocalPlayer(); 
        }
    }

    public void UnfreezeLocalPlayer()
    {
        Networking.LocalPlayer.Immobilize(false);
    }
}