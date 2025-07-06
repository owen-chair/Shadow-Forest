using UdonSharp;
using UnityEngine;
using Unity.Mathematics;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using System;

public class EnemyAI : UdonSharpBehaviour
{
    public int m_ID = -1;

    private Vector3[] m_Nodes;
    private int m_CurrentNode = 0;
    private int m_CurrentTargetNode = 0;
    private bool m_IsMoving = false;

    public static void InstantiateAI(GameObject prefab, Transform parent, Vector2Int[] nodes, Color color, int id)
    {
        Vector2Int node0 = nodes[0];
        Vector3 position = new Vector3(node0.y, 0, node0.x);

        GameObject newAIObject = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);

        newAIObject.name = $"EnemyAI_{node0.x}_{node0.y}";
        newAIObject.GetComponent<Renderer>().material.color = color;
        newAIObject.transform.parent = parent;

        EnemyAI newAI = newAIObject.GetComponent<EnemyAI>();
        newAI.m_ID = id;
        newAI.SetVector3NodesFromVector2Nodes(nodes);
        newAI.m_CurrentNode = 0;
        newAI.m_CurrentTargetNode = 0;
        newAI.m_IsMoving = false;

        newAI.On_MoveTo(id, newAI.m_Nodes.Length - 1);
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
            update = 0.0f;
            //Debug.Log("Update: Elapsed");
            if (this.m_IsMoving)
            {
                this.HandleMove(Time.deltaTime);
            }

            this.HandleRotation(Time.deltaTime);

            this.CheckVision();
        }
    }
    // Add to class members
    private Vector3 m_LookTarget;

    private void HandleRotation(float delta)
    {
        Vector3 lookDirection = (m_LookTarget - transform.position).normalized;
        lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            float rotationSpeed = 10f;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * delta);
        }
    }

    private void HandleMove(float delta)
    {
        int closestNextNode = GetNextClosest(
            this.m_CurrentNode,
            this.m_CurrentTargetNode,
            this.m_Nodes.Length
        );

        Vector3 nextNode = this.m_Nodes[closestNextNode];
        Vector3 currentPos = transform.position;

        float meshHeight = this.GetComponent<MeshFilter>().mesh.bounds.size.y;
        float verticalOffset = meshHeight / 2f;

        float distanceToNext = Vector3.Distance(
            new Vector3(currentPos.x, 0, currentPos.z),
            new Vector3(nextNode.x, 0, nextNode.z)
        );

        if (distanceToNext < 0.01f)
        {
            transform.position = new Vector3(nextNode.x, verticalOffset, nextNode.z);
            this.m_CurrentNode = closestNextNode;

            if (this.m_CurrentNode == this.m_CurrentTargetNode)
            {
                this.m_IsMoving = false;
                this.On_MoveTo(this.m_ID, UnityEngine.Random.Range(0, this.m_Nodes.Length));
            }
            return;
        }

        float moveSpeed = 1f;

        // Calculate movement direction and normalize
        Vector3 direction = new Vector3(
            nextNode.x - currentPos.x,
            0,
            nextNode.z - currentPos.z
        ).normalized;

        // Apply constant speed movement
        Vector3 newPos = new Vector3(
            currentPos.x + direction.x * moveSpeed * delta,
            verticalOffset,
            currentPos.z + direction.z * moveSpeed * delta
        );

        transform.position = newPos;
        this.m_LookTarget = this.m_Nodes[closestNextNode];
        Debug.DrawLine(currentPos, nextNode, Color.red, 0.1f);
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
    private void CheckVision()
    {

        float cosHalfFOV = Mathf.Cos(90f * 0.5f * Mathf.Deg2Rad);
        LayerMask obstaclesMask = LayerMask.GetMask("Environment", "Water", "Default");

        Vector3 origin = transform.position + (transform.forward * 0.1f);
        origin.y = GetComponent<MeshFilter>().mesh.bounds.size.y;

        VRCPlayerApi player = Networking.LocalPlayer;

        if (!player.IsValid()) return;

        float meshHeight = player.GetBonePosition(HumanBodyBones.Chest).y;
        float verticalOffset = meshHeight;

        Vector3 playerPos = player.GetPosition();
        playerPos.y = verticalOffset;

        if (Vector3.Distance(origin, playerPos) > m_MaxDistance) return;

        Vector3 directionToPlayer = (playerPos - origin).normalized;
        float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);

        if (dotProduct > cosHalfFOV)
        {
            float distanceToPlayer = Vector3.Distance(origin, playerPos);

            // Only check if environment blocks line of sight
            RaycastHit hit;
            if (!Physics.Raycast(origin, directionToPlayer, out hit, distanceToPlayer, obstaclesMask))
            {
                // No obstacles between enemy and player
                Debug.DrawRay(origin, directionToPlayer * distanceToPlayer, Color.red);
                KillPlayer(player);
            }
            else
            {
                Debug.DrawRay(origin, directionToPlayer * hit.distance, Color.green);
            }

        }
    }

    private void KillPlayer(VRCPlayerApi player)
    {
        GameObject game_gameObject = GameObject.Find("Game");
        if (game_gameObject == null) return;

        Game game = game_gameObject.GetComponent<Game>();
        if (game == null) return;

        game.SendPlayerToGameStart(player);
    }
}
