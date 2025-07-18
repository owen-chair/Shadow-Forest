﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Data;
using VRC.Udon;
using Miner28.UdonUtils.Network;
public class EnemyAI_Animated : UdonSharpBehaviour
{
    public Game m_Game;
    public GameObject m_SkeletonSpellBallPrefab;
    public GameObject m_SkeletonRightHand;
    public Bloodsplat_Object_Pool_Manager m_Bloodsplat_Object_Pool_Manager;
    public Transform m_ArmaturePositionRotationTransform; //name = MasterControl
    public SkinnedMeshRenderer m_SkinnedMeshRenderer;
    public SkinnedMeshRenderer m_SkinnedMeshRenderer_Head; 
    public CapsuleCollider m_CapsuleCollider;
    public GameObject m_Bounds_Min;
    public GameObject m_Bounds_Max;
    public Animator m_Animator; // Reference to the Animator component
    public shield m_Shield;

    private Vector3 m_TargetPosition;
    private bool m_IsMoving = false;
    private float currentWalkSpeed = 0.0f;
    public float walkSpeed;
    public float moveSpeed;
    public float rotationSpeed;

    public int m_ID = -1;

    public string[] m_PredefinedPath;
    public bool m_PredefinedPathInitialised = false;

    public Vector3[] m_Nodes;
    private int m_CurrentNode = 0;
    private int m_CurrentTargetNode = 0;

    public ParticleSystem m_StunEffectParticleSystem;
    public AudioSource m_StunnedAudioSource;
    public hehehe m_HeheheAudioSource;

    public bool m_IsStunned = false;
    public Transform m_SmokePotionParticleTransform;
    public Potion m_SmokePotion;

    void Start()
    {
        if (this.m_Animator == null)
        {
            this.m_Animator = this.GetComponent<Animator>();
        }

        this.m_PredefinedPathInitialised = false;
        this.m_IsStunned = false;

        this.m_CosHalfFOV = Mathf.Cos(90f * 0.5f * Mathf.Deg2Rad);
        this.m_ObstaclesMask = LayerMask.GetMask("Environment", "Water", "Default");
    }

    private void UpdateWalkSpeed(float delta)
    {
        float targetSpeed = this.m_IsMoving && !this.m_IsStunned ? walkSpeed : 0.0f;
        float t = 30.0f * delta;
        
        currentWalkSpeed = Mathf.Lerp(currentWalkSpeed, targetSpeed, t); 
        currentWalkSpeed = Mathf.Clamp(currentWalkSpeed, 0.0f, 1.0f);
        this.m_Animator.SetFloat("WalkSpeed", currentWalkSpeed);
    }

    public bool CheckMeleeHit(ref Collision collision, ref GameObject collisionObject)
    {
        if (collision == null) return false;
        if (collisionObject == null) return false;

        if (collisionObject == this.m_CapsuleCollider.gameObject)
        {
            return true;
        }

        return false;
    }

    private float m_NumberOfStunEffectStars = 4;
    public void OnStunnedByPlayer(VRCPlayerApi attackingPlayer)
    {
        if (attackingPlayer == null) return;
        if (!attackingPlayer.IsValid()) return;
        if (this.m_StunEffectParticleSystem == null) return;
        if (this.m_StunnedAudioSource == null) return;

        for (int i = 0; i < this.m_NumberOfStunEffectStars; i++)
        {
            float angleRad = (i * (Mathf.PI * 0.5f)); 

            Vector3 position = new Vector3(
                Mathf.Cos(angleRad) * this.m_StunEffectParticleSystem.shape.radius,
                Mathf.Sin(angleRad) * this.m_StunEffectParticleSystem.shape.radius,
                0f
            );

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();

            emitParams.position = position;

            this.m_StunEffectParticleSystem.Emit(emitParams, 1);
        }

        this.m_StunEffectParticleSystem.Play();
        this.m_IsStunned = true;

        if (!this.m_StunnedAudioSource.isPlaying)
        {
            this.m_StunnedAudioSource.Play();
        }

        this.DisableCapsuleCollider();

        SendCustomEventDelayedSeconds(nameof(this.OnStunExpired), 6.0f);
    }

    public void OnStunExpired()
    {
        this.m_IsStunned = false;
        this.EnableCapsuleCollider();
    }

    public void OnHitByPlayer(VRCPlayerApi attackingPlayer)
    {
        if (attackingPlayer == null) return;
        if (!attackingPlayer.IsValid()) return;

        Vector3 bloodPosition = this.GetBloodsplatPosition();

        this.m_Bloodsplat_Object_Pool_Manager.CreateBloodsplat(
            bloodPosition,
            this.m_ArmaturePositionRotationTransform.rotation
        );

        this.OnDestroy();
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

    public void OnDestroy()
    {
        this.gameObject.SetActive(false);
        Destroy(this.gameObject, 0.25f);
    }

    public void DisableCapsuleCollider()
    {
        if (this.m_CapsuleCollider != null && this.m_CapsuleCollider.enabled == true)
        {
            this.m_CapsuleCollider.enabled = false;
        }
    }

    public void EnableCapsuleCollider()
    {
        if (this.m_CapsuleCollider != null && this.m_CapsuleCollider.enabled == false)
        {
            this.m_CapsuleCollider.enabled = true;
        }
    }

    public static void InstantiateAI(GameObject prefab, Transform parent, Vector2Int[] nodes, Color color, int id, Game g)
    {
        Vector2Int node0 = nodes[0];
        Vector3 position = new Vector3(node0.y, 0, node0.x);

        GameObject newAIObject = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);

        newAIObject.name = $"EnemyAI_{node0.x}_{node0.y}";
        //newAIObject.GetComponent<Renderer>().material.color = color;
        newAIObject.transform.parent = parent;

        newAIObject.SetActive(true);

        EnemyAI_Animated newAI = newAIObject.GetComponent<EnemyAI_Animated>();
        
        newAI.m_ID = id;
        newAI.SetVector3NodesFromVector2Nodes(nodes);
        newAI.m_CurrentNode = 0;
        newAI.m_CurrentTargetNode = 0;
        newAI.m_IsMoving = false;
        newAI.m_PredefinedPathInitialised = false;
        newAI.m_IsStunned = false;

        CapsuleCollider capsuleCollider = newAIObject.GetComponent<CapsuleCollider>();
        newAI.m_CapsuleCollider = capsuleCollider;

        newAI.m_Game = g;
    }

    public string[] GeneratePredefinedPath()
    {
        if(!Networking.IsMaster) return null;
        if (this.m_PredefinedPath.Length > 0) return this.m_PredefinedPath;

        this.m_PredefinedPath = new string[15];

        for (int i = 0; i < 15; i++)
        {
            if(UnityEngine.Random.Range(0, 2) == 0)
            {
                this.m_PredefinedPath[i] = $"MOVE_{UnityEngine.Random.Range(0, this.m_Nodes.Length)}";
            }
            else
            {
                this.m_PredefinedPath[i] = $"WAIT_{UnityEngine.Random.Range(1, 5)}";
            }

            //Debug.Log($"[EnemyAI.cs] GeneratePredefinedPath: path[{i}] = {this.m_PredefinedPath[i]}");
        }

        return this.m_PredefinedPath;
    }

    public void InitialisePredefinedPath(string[] path, int aiID)
    {
        if (this.m_ID != aiID) return;
        if (this.m_PredefinedPathInitialised) return;

        this.m_PredefinedPath = path;

        if (this.m_PredefinedPath.Length == 0)
        {
            Debug.LogError("[EnemyAI.cs] On_PredefinedPathReceived: No path received");
            return;
        }
        else
        {
            this.m_PredefinedPathInitialised = true;
            this.m_CurrentPathIndex = 0;
            this.HandlePredefinedPath();
        }
    }

    public int m_CurrentPathIndex = -1;

    public void HandlePredefinedPath()
    {
       // Debug.Log($"[EnemyAI.cs] HandlePredefinedPath: {this.m_ID} CPI = {this.m_CurrentPathIndex}");
        if (this.m_PredefinedPath == null) return;
        if (this.m_PredefinedPath.Length == 0) return;

        if (this.m_CurrentPathIndex >= this.m_PredefinedPath.Length)
        {
            this.m_CurrentPathIndex = 0;
        }

        string[] parts = this.m_PredefinedPath[this.m_CurrentPathIndex].Split('_');
        string action = parts[0];
        int value = int.Parse(parts[1]);

     //   Debug.Log($"[EnemyAI.cs] HandlePredefinedPath: {this.m_ID} Action = {action} Value = {value}");

        if (action == "MOVE")
        {
            this.On_MoveTo(this.m_ID, value);
        }
        else if (action == "WAIT")
        {
            float waitTime = value;
            SendCustomEventDelayedSeconds(nameof(this.HandlePredefinedPath), waitTime);
        }

        this.m_CurrentPathIndex++;
    }

    private void SetVector3NodesFromVector2Nodes(Vector2Int[] v2_Nodes)
    {
        int totalNodes = 0;
        for (int i = 0; i < v2_Nodes.Length; i++)
        {
            if (v2_Nodes[i].x == -999 && v2_Nodes[i].y == -999) { break; }
            totalNodes++;
        }

        if (totalNodes == 0)
        {
            Debug.LogError("[EnemyAI.cs] SetVector3NodesFromVector2Nodes: No nodes in path");
            return;
        }

        this.m_Nodes = new Vector3[totalNodes];
        for (int i = 0; i < totalNodes; i++)
        {
            if (v2_Nodes[i].x == -999 && v2_Nodes[i].y == -999) { continue; }

            this.m_Nodes[i] = new Vector3(v2_Nodes[i].y - 0.5f, 0, v2_Nodes[i].x - 0.5f);
        }
    }

    public void On_MoveTo(int id, int nodeTarget)
    {
        if (this.m_ID != id) return;

        this.m_CurrentTargetNode = nodeTarget;
        this.m_IsMoving = true;
        int closestNextNode = GetNextClosest(
            this.m_CurrentNode,
            this.m_CurrentTargetNode,
            this.m_Nodes.Length
        );
        m_LookTarget = this.m_Nodes[closestNextNode];
    }

    private float update;
    void Update()
    {
        update += Time.deltaTime;
        if (update > 0.05f)
        {
            if (!this.m_IsStunned)
            {
                if (this.m_IsMoving)
                {
                    this.HandleMove(update);
                }

                this.HandleRotation(update);

                this.CheckVision();
            }

            UpdateWalkSpeed(update);
            update = 0.0f;
        }
    }

    // Add to class members
    private Vector3 m_LookTarget;
    private Vector3 m_LookDirection;
    private float m_DistanceToLookTarget;
    private Quaternion m_TargetRotation;
    private void HandleRotation(float delta)
    {
        this.m_LookDirection = (this.m_LookTarget - this.transform.position).normalized;
        this.m_LookDirection.y = 0;

        this.m_DistanceToLookTarget = Vector3.Distance(this.m_LookTarget, this.transform.position);
        if (this.m_DistanceToLookTarget < 0.05f)
        {
            return;
        }

        this.m_TargetRotation = Quaternion.LookRotation(this.m_LookDirection);

        // Use Quaternion.Slerp with a damping factor to smooth out the rotation
        this.transform.rotation = Quaternion.Slerp(
            this.transform.rotation,
            this.m_TargetRotation,
            this.rotationSpeed * delta
        );
    }

    public float movementSpeed = 0.5f;
    private void HandleMove(float delta)
    {
        if(!this.m_IsMoving) return;
        if(this.m_Nodes == null) return;
        if(this.m_Nodes.Length == 0) return;

        int closestNextNode = GetNextClosest(
            this.m_CurrentNode,
            this.m_CurrentTargetNode,
            this.m_Nodes.Length
        );

        Vector3 nextNode = this.m_Nodes[closestNextNode];
        Vector3 currentPos = transform.position;

        //float meshHeight = this.GetComponent<MeshFilter>().mesh.bounds.size.y;
        float verticalOffset = 0.0f;//meshHeight / 2f;

        float distanceToNext = Vector3.Distance(
            new Vector3(currentPos.x, 0, currentPos.z),
            new Vector3(nextNode.x, 0, nextNode.z)
        );

   //     Debug.Log($"[EnemyAI.cs] HandleMove: {this.m_ID} Current pos {currentPos} Next node {nextNode}");
    //    Debug.Log($"[EnemyAI.cs] HandleMove: {this.m_ID} Distance to next node {distanceToNext}");

        if (distanceToNext < 0.05f)
        {
            transform.position = new Vector3(nextNode.x, verticalOffset, nextNode.z);
            this.m_CurrentNode = closestNextNode;

            if (this.m_CurrentNode == this.m_CurrentTargetNode)
            {
                this.m_IsMoving = false;
                this.HandlePredefinedPath();
            }
            else
            {
         //       Debug.LogError($"[EnemyAI.cs] HandleMove Err: {this.m_ID} Reached node {this.m_CurrentNode} but not at target {this.m_CurrentTargetNode}");
            }
            return;
        }

        // Calculate movement direction and normalize
        Vector3 direction = new Vector3(
            nextNode.x - currentPos.x,
            0,
            nextNode.z - currentPos.z
        ).normalized;

        //Debug.Log($"[EnemyAI.cs] HandleMove: {this.m_ID} Direction {direction}");
        //Debug.Log($"[EnemyAI.cs] HandleMove: {this.m_ID} Dir * m * delta = {direction} * {movementSpeed} * {delta} = {direction * movementSpeed * delta}");

        // Apply constant speed movement
        Vector3 newPos = new Vector3(
            currentPos.x + (direction.x * movementSpeed * delta),
            verticalOffset,
            currentPos.z + (direction.z * movementSpeed * delta)
        );

       // Debug.Log($"[EnemyAI.cs] HandleMove: {this.m_ID} Moving from {currentPos} to {newPos}");

        transform.position = newPos;
        this.m_LookTarget = this.m_Nodes[closestNextNode];
    }

    static int GetNextClosest(int start, int target, int range)
    {
        // Normalize inputs to the circular range
        start = Mod(start, range);
        target = Mod(target, range);

        // Calculate the forward and backward distances
        int forwardDistance = (target - start + range) % range;
        int backwardDistance = (start - target + range) % range;

        // Determine the next closest node
        return forwardDistance <= backwardDistance ? (start + 1) % range : (start - 1 + range) % range;
    }

    // Helper method for modular arithmetic
    static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    private float m_MaxDistance = 10.0f;
    private float m_ObserveDistance = 14.0f;
    private float m_LastKilledPlayerTime = 0.0f;
    private RaycastHit _RaycastHit_Unused;
    private VRCPlayerApi m_LocalPlayer;
    private float m_CosHalfFOV;
    private LayerMask m_ObstaclesMask;
    private Vector3 m_Origin;
    private float m_MeshHeight;
    private float m_VerticalOffset;
    private Vector3 m_PlayerPos;
    private Vector3 m_DirectionToPlayer;
    private float m_DotProduct;
    private float m_DistanceToPlayer;
    private Vector3 m_SmokePosition;
    private Vector3 m_SmokeDirection;
    private float m_SmokeDistance;
    private void CheckVision()
    {
        if (Time.time - this.m_LastKilledPlayerTime < 5.0f) return;

        this.m_Origin = this.transform.position + (this.transform.forward * 0.01f);
        if (this.m_CapsuleCollider == null) return;
        this.m_Origin.y = this.m_CapsuleCollider.bounds.size.y;

        this.m_LocalPlayer = Networking.LocalPlayer;
        if (this.m_LocalPlayer == null || !this.m_LocalPlayer.IsValid()) return;

        this.m_MeshHeight = this.m_LocalPlayer.GetBonePosition(HumanBodyBones.Chest).y;
        if (this.m_MeshHeight == 0.0f)
        {
            // Fallback in case their avatar has no chest bone
            this.m_MeshHeight = this.m_LocalPlayer.GetTrackingData(
                VRCPlayerApi.TrackingDataType.Head
            ).position.y;
        }
        this.m_VerticalOffset = this.m_MeshHeight;

        this.m_PlayerPos = this.m_LocalPlayer.GetPosition();
        this.m_PlayerPos.y = this.m_VerticalOffset;
        if (this.m_Game.m_HasBeenInDistance == false)
        {
            if (Vector3.Distance(this.m_Origin, this.m_PlayerPos) < this.m_ObserveDistance)
            {
                this.m_Game.m_HasBeenInDistance = true;
                this.m_Game.m_Narrator.PlayThroughTheTrees();
            }
        }
        if (Vector3.Distance(this.m_Origin, this.m_PlayerPos) > this.m_MaxDistance) return;

        this.m_DirectionToPlayer = (this.m_PlayerPos - this.m_Origin).normalized;
        this.m_DotProduct = Vector3.Dot(this.transform.forward, this.m_DirectionToPlayer);

        if (this.m_DotProduct < this.m_CosHalfFOV) return;

        this.m_DistanceToPlayer = Vector3.Distance(this.m_Origin, this.m_PlayerPos);
        // Check if there is an active smoke grenade particle transform between the origin and player
        if (this.m_SmokePotion != null && (Time.time - this.m_SmokePotion.m_LastUse) < 14.0f && this.m_SmokePotionParticleTransform != null)
        {
            this.m_SmokePosition = this.m_SmokePotionParticleTransform.position;
            this.m_SmokeDirection = (this.m_SmokePosition - this.m_Origin).normalized;
            this.m_SmokeDistance = Vector3.Distance(this.m_Origin, this.m_SmokePosition);

            // if the origin is < 2m from the smoke, vision is blocked
            if (this.m_SmokeDistance < 2.0f) return;
            // if the player is in the smoke, they can't be seen
            if (Vector3.Distance(this.m_SmokePosition, this.m_PlayerPos) < 2.0f) return;
        }

        // Only check if environment blocks line of sight
        if (!Physics.Raycast(this.m_Origin, this.m_DirectionToPlayer, out this._RaycastHit_Unused, this.m_DistanceToPlayer, this.m_ObstaclesMask))
        {
            if (Time.time - this.m_LastKilledPlayerTime > 5.0f)
            {
                this.m_LastKilledPlayerTime = Time.time;

                if (this.m_Game == null) return;
                this.m_Game.Announce_Self_Attacked_By_Enemy_AI(this.m_ID);
            }
        }
    }

    public void KillPlayer(VRCPlayerApi player)
    {
        if (player == null || !player.IsValid()) return;

        if (this.m_HeheheAudioSource != null)
        {
            this.m_HeheheAudioSource.Play();
        }

        SkeletonSpellBall.Instantiate(
            this.m_SkeletonSpellBallPrefab,
            this.m_SkeletonRightHand,
            player,
            this.m_Bloodsplat_Object_Pool_Manager,
            this.m_Shield
        );

        this.m_Animator.SetTrigger("ThrowSpellTrigger");
    }
}