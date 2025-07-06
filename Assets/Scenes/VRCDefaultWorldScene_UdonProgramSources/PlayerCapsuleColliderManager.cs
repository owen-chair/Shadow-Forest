using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerCapsuleColliderManager : UdonSharpBehaviour
{
    public Game m_Game;
    public bool m_IsDebugging = false;
    public GameObject capsuleColliderPrefab;

    private GameObject[] m_PlayerColliders = new GameObject[100];
    private VRCPlayerApi[] m_Players = new VRCPlayerApi[100];
    private string[] m_PlayerNames = new string[100];
    private bool[] m_ColliderDisabled = new bool[100];
    private float[] m_DisableTimers = new float[100];

    void Start()
    {
        // Initialize if needed
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (this.m_Game == null) return;
        if (player == null || !player.IsValid()) return;
        if (player.isLocal) return;

        string playerNameId = this.m_Game.VRCPlayerApiObjectToUniqueNameString(player);
        int index = this.GetPlayerIndex(playerNameId);

        this.m_Players[index] = player;

        GameObject capsuleCollider = UnityEngine.Object.Instantiate(capsuleColliderPrefab);
        if (capsuleCollider == null) return;

        capsuleCollider.name = $"PlayerCapsuleCollider_{playerNameId}_{player.playerId.ToString()}";
        capsuleCollider.transform.position = player.GetPosition();
        this.m_PlayerColliders[index] = capsuleCollider;
        this.m_PlayerNames[index] = playerNameId;
        this.m_ColliderDisabled[index] = false;
        this.m_DisableTimers[index] = 0f;

        Debug.Log($"Player {playerNameId} joined and capsule collider added.");
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (this.m_Game == null) return;
        if (player == null || !player.IsValid()) return;
        if(player.isLocal) return;

        string playerNameId = this.m_Game.VRCPlayerApiObjectToUniqueNameString(player);
        int index = this.GetPlayerIndex(playerNameId);

        if (this.m_PlayerColliders[index] != null)
        {
            Destroy(this.m_PlayerColliders[index]);
            this.m_PlayerColliders[index] = null;
            this.m_PlayerNames[index] = null;
            this.m_Players[index] = null;
            this.m_ColliderDisabled[index] = false;
            this.m_DisableTimers[index] = 0f;
        }

        Debug.Log($"Player {playerNameId} left and capsule collider removed.");
    }

    private int GetPlayerIndex(string playerNameId)
    {
        if (playerNameId == null) return -1;
        if (this.m_PlayerNames == null) return -1;
        for (int i = 0; i < this.m_PlayerNames.Length; i++)
        {
            if (this.m_PlayerNames[i] == playerNameId)
            {
                return i;
            }
        }

        for (int i = 0; i < this.m_PlayerNames.Length; i++)
        {
            if (this.m_PlayerNames[i] == null)
            {
                return i;
            }
        }

        return -1; // Should not happen if array size is sufficient
    }

    public VRCPlayerApi GetPlayerByCapsuleCollider(GameObject capsuleCollider)
    {
        if (capsuleCollider == null) return null;
        if (this.m_PlayerColliders == null) return null;

        for (int i = 0; i < this.m_PlayerColliders.Length; i++)
        {
            if(this.m_PlayerColliders[i] == null) continue;

            if (this.m_PlayerColliders[i].name == capsuleCollider.name)
            {
                return this.m_Players[i];
            }
        }

        return null;
    }
    
    private int GetColliderIndex(GameObject capsuleCollider)
    {
        if (capsuleCollider == null) return -1;

        if (this.m_PlayerColliders == null) return -1;
        for (int i = 0; i < this.m_PlayerColliders.Length; i++)
        {
            if(this.m_PlayerColliders[i] == null) continue;
            if (this.m_PlayerColliders[i].name == capsuleCollider.name)
            {
                return i;
            }
        }
        return -1;
    }

    public void DisableColliderTemporarily(GameObject capsuleCollider, float duration)
    {
        if(capsuleCollider == null) return;
        int index = this.GetColliderIndex(capsuleCollider);
        if (index != -1)
        {
            this.m_ColliderDisabled[index] = true;
            this.m_DisableTimers[index] = duration;
        }
    }

    public void EnableCollider(GameObject capsuleCollider)
    {
        if (capsuleCollider == null) return;
        int index = this.GetColliderIndex(capsuleCollider);
        if (index != -1)
        {
            this.m_ColliderDisabled[index] = false;
            Collider c = capsuleCollider.GetComponent<Collider>();
            if (c != null)
            {
                c.enabled = true;
            }
        }
    }

    public void EnableColliderByPlayerNameId(string playerNameId)
    {
        int index = this.GetPlayerIndex(playerNameId);
        if (index != -1)
        {
            this.m_ColliderDisabled[index] = false;
            Collider c = this.m_PlayerColliders[index].GetComponent<Collider>();
            if (c != null)
            {
                c.enabled = true;
            }
        }
    }

    public void DisableColliderByPlayerName(string playerNameId, float duration)
    {
        int index = this.GetPlayerIndex(playerNameId);
        if (index != -1)
        {
            this.m_ColliderDisabled[index] = true;
            this.m_DisableTimers[index] = duration;
        }
    }

    public void SetColliderStatesFromDataDictionary(DataDictionary playerStates)
    {
        if (this.m_Game == null) { Debug.LogError("[PlayerCapsuleColliderManager.cs] SetColliderStatesFromDataDictionary: m_Game is null"); return; }
        if (playerStates == null) { Debug.LogError("[PlayerCapsuleColliderManager.cs] SetColliderStatesFromDataDictionary: playerStates is null"); return; }
        VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        allPlayers = VRCPlayerApi.GetPlayers(allPlayers);

        bool isLocalPlayerFollower = false;
        string localPlayerNameId = this.m_Game.VRCPlayerApiObjectToUniqueNameString(Networking.LocalPlayer);

        if (playerStates.ContainsKey(localPlayerNameId))
        {
            isLocalPlayerFollower = playerStates[localPlayerNameId].Boolean;
        }

        foreach (VRCPlayerApi player in allPlayers)
        {
            if (player == null || !player.IsValid() || player.isLocal) continue;

            string playerNameId = this.m_Game.VRCPlayerApiObjectToUniqueNameString(player);
            if (!playerStates.ContainsKey(playerNameId)) continue;

            bool isFollower = playerStates[playerNameId].Boolean;

            if (isLocalPlayerFollower)
            {
                if (isFollower && !player.isLocal)
                {
                    this.EnableColliderByPlayerNameId(playerNameId);
                }
                else
                {
                    this.EnableColliderByPlayerNameId(playerNameId);
                }
            }
            else
            {
                if (!isFollower && !player.isLocal)
                {
                    this.DisableColliderByPlayerName(playerNameId, 99999.0f);
                }
                else
                {
                    this.EnableColliderByPlayerNameId(playerNameId);
                }
            }
        }
    }

    Collider currentCollider = null;
    void Update()
    {
        if (this.m_ColliderDisabled == null) return;
        if (this.m_PlayerColliders == null) return;
        if (this.m_PlayerNames == null) return;
        if (this.m_Players == null) return;

        for (int i = 0; i < this.m_PlayerNames.Length; i++)
        {
            if (this.m_PlayerNames[i] != null)
            {
                VRCPlayerApi player = this.m_Players[i];
                if (player != null && player.IsValid())
                {
                    Vector3 playerPosition = player.GetPosition();
                    float eyeHeight = player.GetAvatarEyeHeightAsMeters();

                    if (this.m_PlayerColliders[i] != null)
                    {
                        if (this.m_PlayerColliders[i].transform == null) continue;

                        this.m_PlayerColliders[i].transform.position = playerPosition + new Vector3(
                            0,
                            eyeHeight * 0.5f,
                            0
                        );

                        this.m_PlayerColliders[i].transform.localScale = new Vector3(1, eyeHeight, 1);
                        
                        if (this.m_ColliderDisabled[i])
                        {
                            this.m_DisableTimers[i] -= Time.deltaTime;
                            currentCollider = null;
                            if (this.m_DisableTimers[i] <= 0)
                            {
                                this.m_ColliderDisabled[i] = false;
                                currentCollider = this.m_PlayerColliders[i].GetComponent<Collider>();
                                if (currentCollider != null)
                                {
                                    currentCollider.enabled = true;
                                }
                            }
                            else
                            {
                                currentCollider = this.m_PlayerColliders[i].GetComponent<Collider>();
                                if (currentCollider != null)
                                {
                                    currentCollider.enabled = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}