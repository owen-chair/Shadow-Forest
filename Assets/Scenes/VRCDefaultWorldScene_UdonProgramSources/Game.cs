using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using Miner28.UdonUtils.Network;
using VRC.Udon.Common.Exceptions;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;

public enum GameStatus
{
    Waiting,
    Generating,
    InProgress,
    Finished
}

public class Game : NetworkInterface
{
    public LevelGenerator m_LevelGenerator;
    public Narrator m_Narrator;
    public bool m_IsLocalPlayerFollower = false;
    public DataDictionary m_PlayerFollowerStatus;
    public int m_PlayerFollowerCount = 0;
    public PlayerCapsuleColliderManager m_PlayerCapsuleColliderManager;

    public readonly string[] GameStatusStringLookup = new string[]
    {
        "Waiting",
        "Generating",
        "InProgress",
        "Finished"
    };
    public Transform m_BlackBoxTransform;
    public bool m_HasBeenInDistance = false;


    public Canvas m_DebugCanvas;
    public float m_LoadingPercentage = 0.0f;
    private string m_PlayerGenStatusText = "";
    public TMPro.TMP_Text m_PlayerGenStatusTexts;
    public TMPro.TMP_Text m_LoadingPCTextMesh;
    private DataDictionary m_PlayerLoadingPercentages;
    private DataDictionary m_PlayerFPS;
    private float m_LastUpdatedLoadingPercentageTimeText = 0.0f;
    public TMPro.TMP_Text m_GameMasterText;

    public Melee_Weapon[] m_MeleeWeapons;
    public CeremonialWine[] m_CeremonialWines;

    public GameObject m_Main_Spawn_StartGameBtnForest;

    public GameObject m_Main_Spawn_SpawnerReferenceCube;
    public GameObject m_Game_Spawn_SpawnerReferenceCube;
    public GameObject m_Exit_Spawn_SpawnerReferenceCube;

    public Stairs m_TempleStairs;
    public DesktopStairsExit m_DesktopStairsExit;

    public bool m_IsLocalPlayerMaster = false;
    public GameStatus m_GameStatus = GameStatus.Waiting;
    public bool m_IsGameStarting = false;

    public float m_TimeWaiting = 0.0f;

    public int m_PlayersExpectedInCurrentGame = 0;
    public int m_PlayersInCurrentGame = 0;
    public DataDictionary m_PlayerMazeGenerationStatus;


    public bool m_LocalPlayerFinishedGeneratingMaze = false;
    public bool m_MasterFinishedGeneratingMaze = false;
    public float m_TimeMasterGeneratedMaze = 0.0f;

    public Lord m_Lord;
    public EnemyAI_Animated[] m_EnemyAIs;
    public DataDictionary m_PlayerAliveStatus;
    public bool m_LocalPlayerInGame = false;
    public float m_OptimisationDelayKek = 0.45f;
    public float m_LocalPlayerStartedGameTime = 0.0f;
    public float m_WaitForSlowPlayerGenerationTimeout = 20.0f;

    public float m_RoundTimeSeconds = 300.0f;
    public Slider m_RoundTimeSlider;
    public TMPro.TMP_Text m_RoundTimeSliderValueText;
    public bool m_RoundTimeSliderValueChanged = false;

    public bool m_AbandonLaggers = true;
    public Toggle m_AbandonLaggersToggle;

    public bool m_EasyModeOn = true;
    public Toggle m_EasyModeToggle;

    public bool m_HardModeOn = false;
    public Toggle m_HardModeToggle;

    public int[][] m_Maze;
    private readonly string[] NumberLookup = new string[] 
    {
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
        "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
        "30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
        "40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
        "50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
        "60", "61", "62", "63", "64", "65", "66", "67", "68", "69",
        "70", "71", "72", "73", "74", "75", "76", "77", "78", "79",
        "80", "81", "82", "83", "84", "85", "86", "87", "88", "89",
        "90", "91", "92", "93", "94", "95", "96", "97", "98", "99"
    };

    public int m_MazeTheme = -1; 
    public Vector2Int m_MazeExit = new Vector2Int(-1, -1);

    public DataToken m_ReceivedMazeData;
    private string[] m_ReceivedMazeDataParts = new string[10];
    private int m_ReceivedMazeDataPartsCount = 0;

    public float m_LastReceivedMazeDataTime = 0;
    public readonly float m_MazeDataReceiveCooldown = 30.0f;

    public int[][] m_InitialMazeData = new int[80][];
    public Vector2Int m_InitialMazeDataExit = new Vector2Int(-1, -1);
    private DataToken m_InitialMazeDataToken;
    private string[] m_InitialMazeDataSplit = new string[10];
    private int m_CurrentDataPartIndex = 0;

    public int m_Theme = 0;
    public int m_FailedGenerationAttempts = 0;
    public int m_MaxFailedGenerationAttempts = 50;

    public AudioSource m_Ambience_Cave;
    public AudioSource m_Ambience_Forest;
    public GameObject m_Firefly_ParticleSystem_Transform;
    public ParticleSystem m_Firefly_ParticleSystem;

    public shield m_Sheild;
    public club m_Club;
    public Potion m_Potion;
    public Exit m_Exit;

    private float m_RoundStartedTime = 0.0f;

    public Scoreboard1 m_SpawnAreaScoreboard;

    public int m_Deaths = 0;
    public int m_TotalDeaths = 0;
    public int m_ForestsAttempted = 0;
    public int m_ForestsCompleted = 0;

    public void On_MasterUI_RoundTimeSlider_ValueChanged()
    {
        if(!Networking.IsMaster) return;
        if (this.m_GameStatus != GameStatus.Waiting) return;

        if (this.m_RoundTimeSlider != null)
        {
            if(this.m_RoundTimeSlider.value != Mathf.Round(this.m_RoundTimeSeconds * 0.01666666666666666666666666666667f))
            {
                this.m_RoundTimeSliderValueChanged = true;
            }
        }
    }

    public void On_MasterUI_EasyMode_Toggled()
    {
        if (!Networking.IsMaster) return;

        if (this.m_EasyModeToggle != null)
        {
            this.m_EasyModeOn = this.m_EasyModeToggle.isOn;
            if (this.m_HardModeToggle != null)
            {
                this.m_HardModeToggle.isOn = !this.m_EasyModeOn;
                this.m_HardModeOn = !this.m_EasyModeOn;
            }

            SendMethodNetworked(
                nameof(this.On_EasyModeValueChanged),
                SyncTarget.All,
                new DataToken(Networking.LocalPlayer),
                new DataToken(this.m_EasyModeOn)
            );
        }
    }

    public void On_MasterUI_HardMode_Toggled()
    {
        if (!Networking.IsMaster) return;

        if (this.m_HardModeToggle != null)
        {
            this.m_HardModeOn = this.m_HardModeToggle.isOn;
            if (this.m_EasyModeToggle != null)
            {
                this.m_EasyModeToggle.isOn = !this.m_HardModeOn;
                this.m_EasyModeOn = !this.m_HardModeOn;
            }

            SendMethodNetworked(
                nameof(this.On_HardModeValueChanged),
                SyncTarget.All,
                new DataToken(Networking.LocalPlayer),
                new DataToken(this.m_HardModeOn)
            );
        }
    }

    [NetworkedMethod]
    public void On_EasyModeValueChanged(VRCPlayerApi requestingPlayer, bool state)
    {
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!requestingPlayer.isMaster) return;
        if (this.m_EasyModeToggle == null) return;

        this.m_EasyModeToggle.isOn = state;
        this.m_EasyModeOn = state;

        if (this.m_HardModeToggle != null)
        {
            if (this.m_HardModeToggle.isOn == state)
            {
                this.m_HardModeToggle.isOn = !state;
                this.m_HardModeOn = !state;
            }
        }
    }

    [NetworkedMethod]
    public void On_HardModeValueChanged(VRCPlayerApi requestingPlayer, bool state)
    {
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!requestingPlayer.isMaster) return;
        if (this.m_HardModeToggle == null) return;

        this.m_HardModeToggle.isOn = state;
        this.m_HardModeOn = state;

        if (this.m_EasyModeToggle != null)
        {
            if (this.m_EasyModeToggle.isOn == state)
            {
                this.m_EasyModeToggle.isOn = !state;
                this.m_EasyModeOn = !state;
            }
        }
    }

    public void On_MasterUI_AbandonLaggers_Toggled()
    {
        if (!Networking.IsMaster) return;

        if (this.m_AbandonLaggersToggle != null)
        {
            this.m_AbandonLaggers = this.m_AbandonLaggersToggle.isOn;

            SendMethodNetworked(
                nameof(this.On_AbandonLaggersValueChanged),
                SyncTarget.All,
                new DataToken(Networking.LocalPlayer),
                new DataToken(this.m_AbandonLaggers)
            );
        }
    }

    [NetworkedMethod]
    public void On_AbandonLaggersValueChanged(VRCPlayerApi requestingPlayer, bool state)
    {
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!requestingPlayer.isMaster) return;
        if (this.m_AbandonLaggersToggle == null) return;
        if (this.m_AbandonLaggersToggle.isOn == state) return;
        
        this.m_AbandonLaggersToggle.isOn = state;
        this.m_AbandonLaggers = state;
    }

    public void MasterUIUpdate()
    {
        if(!Networking.IsMaster) return;
        if (this.m_RoundTimeSlider != null)
        {
            if (Mathf.Round(this.m_RoundTimeSlider.value) != Mathf.Round(this.m_RoundTimeSeconds * 0.01666666666666666666666666666667f))
            {
                SendMethodNetworked(
                    nameof(this.OnRoundTimeSliderValueChanged),
                    SyncTarget.All,
                    new DataToken(Networking.LocalPlayer),
                    new DataToken(Mathf.Round(this.m_RoundTimeSlider.value))
                );

                this.m_RoundTimeSeconds = Mathf.Round(this.m_RoundTimeSlider.value * 60.0f);
                this.m_RoundTimeSliderValueChanged = false;
            }
        }
    }

    [NetworkedMethod]
    public void OnRoundTimeSliderValueChanged(VRCPlayerApi requestingPlayer, float valueMinutes)
    {
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!requestingPlayer.isMaster) return;
        if (this.m_RoundTimeSlider == null) return;
        if (valueMinutes < this.m_RoundTimeSlider.minValue || valueMinutes > this.m_RoundTimeSlider.maxValue) return;

        this.m_RoundTimeSeconds = Mathf.Round(valueMinutes * 60.0f);
        if(this.m_RoundTimeSliderValueText != null)
        {
            this.m_RoundTimeSliderValueText.text = Mathf.Round(this.m_RoundTimeSeconds * 0.01666666666666666666666666666667f).ToString();
        }

        if (!Networking.IsMaster)
        {
            this.m_RoundTimeSlider.value = valueMinutes;
        }
    }

    void Start()
    {
        this.m_LastReceivedMazeDataTime = Time.time - (this.m_MazeDataReceiveCooldown * 0.9f);

        if (this.m_PlayerFollowerStatus == null)
        {
            this.m_PlayerFollowerStatus = new DataDictionary();
        }

        if (this.m_PlayerAliveStatus == null)
        {
            this.m_PlayerAliveStatus = new DataDictionary();
        }

        this.m_MaxFailedGenerationAttempts = 50;
        this.BeginForestAmbience();
        
        SendCustomEventDelayedSeconds(nameof(this.PlayIntro), 1.0f);
    }

    public void PlayIntro()
    {
        this.m_Narrator.PlayIntro();
    }

    public void RequestInitialGameStatus()
    {
        if (!Networking.IsMaster) return;

        SendMethodNetworked(
            nameof(this.On_InitialGameStatusRequest),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer)
        );
    }

    [NetworkedMethod]
    public void On_InitialGameStatusRequest(VRCPlayerApi requestingPlayer)
    {
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!Networking.IsMaster) return;

        SendMethodNetworked(
            nameof(this.On_InitialGameStatusResponse),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(requestingPlayer),
            new DataToken(this.GameStatusToString()),
            new DataToken(this.m_RoundTimeSeconds),
            new DataToken(this.m_AbandonLaggers)
        );
    }

    [NetworkedMethod]
    public void On_InitialGameStatusResponse(VRCPlayerApi respondingPlayer, VRCPlayerApi requestingPlayer, string gameStatus, float roundTimeSeconds, bool abandonLaggers)
    {
        if (respondingPlayer == null) return;
        if (!respondingPlayer.IsValid()) return;
        if (!respondingPlayer.isMaster) return;
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!requestingPlayer.isLocal) return;

        GameStatus receivedGameStatus = this.GameStatusFromString(gameStatus);
        if (this.m_GameStatus != receivedGameStatus)
        {
            this.m_GameStatus = receivedGameStatus;
        }

        if (this.m_RoundTimeSeconds != roundTimeSeconds)
        {
            this.m_RoundTimeSeconds = roundTimeSeconds;
            if (this.m_RoundTimeSlider != null)
            {
                this.m_RoundTimeSlider.value = Mathf.Round(
                    roundTimeSeconds * 0.01666666666666666666666666666667f
                );
            }
        }

        if (this.m_AbandonLaggers != abandonLaggers)
        {
            this.m_AbandonLaggers = abandonLaggers;
            if (this.m_AbandonLaggersToggle != null)
            {
                this.m_AbandonLaggersToggle.isOn = abandonLaggers;
            }
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player == null) return;
        if (!player.IsValid()) return;

        base.OnPlayerJoined(player);
        Debug.Log($"[Game.cs] OnPlayerJoined: {player.displayName}");
        if(Networking.IsMaster)
        {
            Debug.Log($"[Game.cs] OnPlayerJoined: Master is local player");

            if (this.m_EasyModeToggle != null)
            {
                this.m_EasyModeToggle.interactable = true;
            }

            if (this.m_HardModeToggle != null)
            {
                this.m_HardModeToggle.interactable = true;
            }

            if (this.m_AbandonLaggersToggle != null)
            {
                this.m_AbandonLaggersToggle.interactable = true;
            }

            if (this.m_RoundTimeSlider != null)
            {
                this.m_RoundTimeSlider.interactable = true;
            }
        }
        else
        {
            if (this.m_EasyModeToggle != null)
            {
                this.m_EasyModeToggle.interactable = false;
            }

            if (this.m_HardModeToggle != false)
            {
                this.m_HardModeToggle.interactable = false;
            }

            if (this.m_AbandonLaggersToggle != false)
            {
                this.m_AbandonLaggersToggle.interactable = false;
            }

            if (this.m_RoundTimeSlider != null)
            {
                this.m_RoundTimeSlider.interactable = false;
            }
        }

        if (player.isLocal && !Networking.IsMaster)
        {
            this.RequestInitialGameStatus();
        }

        if (player.isLocal)
        {
            if (this.m_GameMasterText != null)
            {
                if (Networking.Master != null && Networking.Master.IsValid())
                {
                    this.m_GameMasterText.text = Networking.Master.displayName;
                }
            }
        }
    }
    
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (player == null) return;
        if (!player.IsValid()) return;
        base.OnPlayerLeft(player);

        if (!Networking.IsMaster) return;

        if (this.m_GameStatus.Equals(GameStatus.Generating))
        {
            string playerNameId = this.VRCPlayerApiObjectToUniqueNameString(player);

            if (this.m_PlayerLoadingPercentages != null)
            {
                if (this.m_PlayerLoadingPercentages.ContainsKey(playerNameId))
                {
                    this.m_PlayerLoadingPercentages.Remove(playerNameId);
                }
            }

            if (this.m_PlayerFPS != null)
            {
                if (this.m_PlayerFPS.ContainsKey(playerNameId))
                {
                    this.m_PlayerFPS.Remove(playerNameId);
                }
            }

            if (this.m_PlayerAliveStatus != null)
            {
                if (this.m_PlayerAliveStatus.ContainsKey(playerNameId))
                {
                    this.m_PlayerAliveStatus.Remove(playerNameId);
                }
            }

            if (this.m_PlayerFollowerStatus != null)
            {
                if (this.m_PlayerFollowerStatus.ContainsKey(playerNameId))
                {
                    this.m_PlayerFollowerStatus.Remove(playerNameId);
                }
            }

            if (this.m_PlayerMazeGenerationStatus != null)
            {
                if (this.m_PlayerMazeGenerationStatus.ContainsKey(playerNameId))
                {
                    if (this.m_PlayerMazeGenerationStatus[playerNameId] == true)
                    {
                        this.m_PlayersInCurrentGame--;
                    }

                    this.m_PlayersExpectedInCurrentGame--;
                    this.m_PlayerMazeGenerationStatus.Remove(playerNameId);
                    if (this.m_PlayersInCurrentGame == this.m_PlayersExpectedInCurrentGame && !this.m_IsGameStarting)
                    {
                        this.On_StartGame();
                    }
                }
            }
        }

        if (this.m_GameStatus.Equals(GameStatus.InProgress))
        {
            string playerNameId = this.VRCPlayerApiObjectToUniqueNameString(player);
            if (this.m_PlayerAliveStatus != null)
            {
                if (this.m_PlayerAliveStatus.ContainsKey(playerNameId))
                {
                    this.m_PlayersInCurrentGame--;
                    this.m_PlayersExpectedInCurrentGame--;

                    this.m_PlayerAliveStatus.Remove(playerNameId);
                }
            }

            if (this.m_PlayerFollowerStatus != null)
            {
                if (this.m_PlayerFollowerStatus.ContainsKey(playerNameId))
                {
                    if (this.m_PlayerFollowerStatus[playerNameId] == true)
                    {
                        this.m_PlayerFollowerCount--;
                    }

                    this.m_PlayerFollowerStatus.Remove(playerNameId);
                }
            }
        }
    }

    public override void OnMasterTransferred(VRCPlayerApi newMaster)
    {
        if (newMaster == null) return;
        if (!newMaster.IsValid()) return;

        base.OnMasterTransferred(newMaster);
        Debug.Log($"[Game.cs] OnMasterTransferred: {newMaster.displayName}");

        if(Networking.IsMaster)
        {
            if(this.m_GameStatus == GameStatus.Waiting || this.m_GameStatus == GameStatus.Finished || this.m_GameStatus == GameStatus.Generating)
            {
                this.On_ResetGame();
            }

            if(this.m_EasyModeToggle != null)
            {
                this.m_EasyModeToggle.interactable = true;
            }

            if (this.m_HardModeToggle != null)
            {
                this.m_HardModeToggle.interactable = true;
            }

            if (this.m_AbandonLaggersToggle != null)
            {
                this.m_AbandonLaggersToggle.interactable = true;
            }

            if (this.m_RoundTimeSlider != null)
            {
                this.m_RoundTimeSlider.interactable = true;
            }
        }
        else
        {
            if (this.m_EasyModeToggle != null)
            {
                this.m_EasyModeToggle.interactable = false;
            }

            if (this.m_HardModeToggle != false)
            {
                this.m_HardModeToggle.interactable = false;
            }

            if (this.m_AbandonLaggersToggle != false)
            {
                this.m_AbandonLaggersToggle.interactable = false;
            }

            if (this.m_RoundTimeSlider != null)
            {
                this.m_RoundTimeSlider.interactable = false;
            }
        }

        if (this.m_GameMasterText != null)
        {
            this.m_GameMasterText.text = newMaster.displayName;
        }
    }

    private string GameStatusToString()
    {
        int statusIndex = (int)this.m_GameStatus;
        if (statusIndex < 0 || statusIndex >= this.GameStatusStringLookup.Length)
        {
            return "Unknown";
        }

        return this.GameStatusStringLookup[statusIndex];
    }
    private GameStatus GameStatusFromString(string statusString)
    {
        for (int i = 0; i < this.GameStatusStringLookup.Length; i++)
        {
            if (this.GameStatusStringLookup[i] == statusString)
            {
                return (GameStatus)i;
            }
        }

        return GameStatus.Waiting; // Default value or handle as needed
    }

    private float update;
    private readonly float m_MaxDIC_PC = 92.0f * 92.0f;
    private Vector3 m_PlayerPos = new Vector3(0, 0, 0);
    private Quaternion m_PlayerEyeQuat = new Quaternion(0, 0, 0, 0);
    private VRCPlayerApi.TrackingData m_PlayerTrackingData;
    void Update()
    {
        update += Time.deltaTime;
        if (update > 0.5f)
        {
            if(Networking.IsMaster)
            {
                this.MasterGameUpdate(update);
                this.MasterUIUpdate();
            }

            if(this.m_Firefly_ParticleSystem_Transform != null)
            {
                this.m_PlayerTrackingData = Networking.LocalPlayer.GetTrackingData(
                    VRCPlayerApi.TrackingDataType.Head
                );

                this.m_PlayerPos = m_PlayerTrackingData.position;
                this.m_PlayerPos.y = 0.0f;

                this.m_PlayerEyeQuat = Quaternion.Euler(
                    0,
                    m_PlayerTrackingData.rotation.eulerAngles.y,
                    0
                );

                if (this.m_Firefly_ParticleSystem != null)
                {
                    this.m_Firefly_ParticleSystem.transform.SetPositionAndRotation(
                        this.m_PlayerPos,
                        this.m_PlayerEyeQuat
                    );
                }
            }

            update = 0.0f;

            if (this.m_GameStatus.Equals(GameStatus.Generating))
            {
                if (!this.m_LocalPlayerFinishedGeneratingMaze)
                {
                    if (this.m_LevelGenerator == null) return;
                    if (this.m_LevelGenerator.m_DebugInstantiation_Counter > 0.0f)
                    {
                        this.m_LoadingPercentage = (this.m_LevelGenerator.m_DebugInstantiation_Counter / this.m_MaxDIC_PC) * 100.0f;
                    }

                    if (this.m_LoadingPCTextMesh == null) return;
                    this.m_LoadingPCTextMesh.text = $"{this.GameStatusToString()} ({Mathf.Round(this.m_LoadingPercentage)}%)";
                }
                else
                {
                    if (this.m_LoadingPCTextMesh == null) return;
                    this.m_LoadingPCTextMesh.text = $"{this.GameStatusToString()} ({this.m_LoadingPercentage}%)";
                }

                SendMethodNetworked(
                    nameof(this.On_ReceivedPlayerLoadingPercentage),
                    SyncTarget.All,
                    new DataToken(Networking.LocalPlayer),
                    new DataToken(this.m_LoadingPercentage),
                    new DataToken(this.VRCPlayerApiObjectToUniqueNameString(Networking.LocalPlayer)),
                    new DataToken(this.m_FPS)
                );
            }
            else
            {
                if (this.m_LoadingPCTextMesh == null) return;
                this.m_LoadingPCTextMesh.text = $"{this.GameStatusToString()}";
            }
        }
    }

    public float m_FPS = 0.0f;
    void LateUpdate()
    {
        this.m_FPS = Mathf.Round(1.0f / (Time.unscaledDeltaTime == 0 ? 1.0f : Time.unscaledDeltaTime));
    }

    [NetworkedMethod]
    public void On_ReceivedPlayerLoadingPercentage(VRCPlayerApi requestingPlayer, float percentage, string playerNameId, float fps)
    {
        if (!this.m_GameStatus.Equals(GameStatus.Generating)) return;
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (playerNameId == null) return;
        if (playerNameId == "") return;
        if (percentage < -100.0f || percentage > 200.0f) return;
        if (this.m_PlayerLoadingPercentages == null) return;
        if (this.m_PlayerFPS == null) return;
        if (this.m_PlayerGenStatusTexts == null) return;

        if (this.m_PlayerLoadingPercentages.ContainsKey(playerNameId))
        {
            this.m_PlayerLoadingPercentages[playerNameId] = percentage;
        }

        if (this.m_PlayerFPS.ContainsKey(playerNameId))
        {
            this.m_PlayerFPS[playerNameId] = fps;
        }

        if (Time.time - this.m_LastUpdatedLoadingPercentageTimeText > 1.0f)
        {
            if(this.m_PlayerMazeGenerationStatus == null) return;
            
            this.m_PlayerGenStatusText = "";
            this.m_LastUpdatedLoadingPercentageTimeText = Time.time;
            VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            allPlayers = VRCPlayerApi.GetPlayers(allPlayers);
            foreach (VRCPlayerApi player in allPlayers)
            {
                if (player == null) continue;
                if (!player.IsValid()) continue;

                string key = this.VRCPlayerApiObjectToUniqueNameString(player);

                if (!this.m_PlayerLoadingPercentages.ContainsKey(key)) continue;
                float thisFPS = this.m_PlayerFPS.ContainsKey(key) ? this.m_PlayerFPS[key].Float : 0.0f;
                float playerLoadingPC = Mathf.Round(this.m_PlayerLoadingPercentages[key].Float);

                this.m_PlayerGenStatusText += $"{player.displayName}: ({playerLoadingPC}%) FPS: {thisFPS}\n";

            }

            this.m_PlayerGenStatusTexts.text = this.m_PlayerGenStatusText;
        }
    }

    public void MasterGameUpdate(float delta)
    {
        if (this.m_GameStatus == GameStatus.Waiting)
        {
            this.m_TimeWaiting += delta;
            if (this.m_TimeWaiting > 5.0f)
            {
                this.EnableGameStartButton();
            }
        }
        else if (this.m_GameStatus == GameStatus.InProgress)
        {
            if(Time.time - this.m_RoundStartedTime > this.m_RoundTimeSeconds)
            {
                this.On_GameTimeExpired();
            }
        }
    }

    public void EnableGameStartButton()
    {
        if (Networking.IsMaster)
        {
            if (this.m_Main_Spawn_StartGameBtnForest == null)
            {
                Debug.LogError("[Game.cs] HandleGameStateWaiting: m_Main_Spawn_StartGameBtn is null");
                return;
            }
            if (!this.m_Main_Spawn_StartGameBtnForest.activeSelf)
            {
                this.m_Main_Spawn_StartGameBtnForest.SetActive(true);
            }
            
            this.m_AbandonLaggersToggle.interactable = true;
            this.m_EasyModeToggle.interactable = true;
            this.m_HardModeToggle.interactable = true;
            this.m_RoundTimeSlider.interactable = true;
        }
    }

    public void On_StartGameButtonPressed(int theme)
    {
        if (Networking.IsMaster && this.m_GameStatus == GameStatus.Waiting)
        {
            Debug.Log("[Game.cs] On_StartGameButtonPressed: Starting game");
            this.m_RoundStartedTime = 0.0f;
            this.m_MasterFinishedGeneratingMaze = false;
            this.m_Theme = theme;
            this.m_GameStatus = GameStatus.Generating;
            this.m_IsGameStarting = false;
            if (this.m_AbandonLaggersToggle != null)
            {
                this.m_AbandonLaggers = this.m_AbandonLaggersToggle.isOn;
            }

            this.m_AbandonLaggersToggle.interactable = false;
            this.m_EasyModeToggle.interactable = false;
            this.m_HardModeToggle.interactable = false;
            this.m_RoundTimeSlider.interactable = false;

            if(this.m_HardModeToggle.isOn)
            {
                this.m_HardModeOn = true;
                this.m_EasyModeOn = false;
            }
            else if(this.m_EasyModeToggle.isOn)
            {
                this.m_EasyModeOn = true;
                this.m_HardModeOn = false;
            }
            else
            {
                this.m_EasyModeOn = true;
                this.m_HardModeOn = false;
            }

            SendMethodNetworked(
                nameof(this.On_AnnounceStartGameButtonPressed),
                SyncTarget.All,
                new DataToken(Networking.LocalPlayer)
            );

            GenerateNewMaze();
        }
    }

    [NetworkedMethod]
    public void On_AnnounceStartGameButtonPressed(VRCPlayerApi requestingPlayer)
    {
        if(requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!requestingPlayer.isMaster) return;

        this.m_Narrator.PlayDice();
    }

    public void MazeGenerationFailed()
    {
        Debug.LogError("[Game.cs] MazeGenerationFailed: Failed to generate maze");
        this.m_GameStatus = GameStatus.Waiting;
        this.m_FailedGenerationAttempts = 0;
        this.m_TimeWaiting = 0.0f;
        
        this.m_AbandonLaggersToggle.interactable = true;
        this.m_EasyModeToggle.interactable = true;
        this.m_HardModeToggle.interactable = true;
        this.m_RoundTimeSlider.interactable = true;
    }

    public void MazeGenerationFailedAttempt()
    {
        this.m_FailedGenerationAttempts++;
        if(this.m_FailedGenerationAttempts > this.m_MaxFailedGenerationAttempts)
        {
            this.MazeGenerationFailed();
            return;
        }

        SendCustomEventDelayedSeconds(nameof(this.GenerateNewMaze), 0.5f);
    }

    public void GenerateNewMaze()
    {
        if (!Networking.IsMaster) return;
        if (this.m_LevelGenerator == null) return;
        Debug.Log("[Game.cs] GenerateNewMaze: Generating new maze");
        
        int success = this.m_LevelGenerator.GenerateMaze(80, 80);
        if(success == 0)
        {
            Debug.LogError("[Game.cs] GenerateNewMaze: Failed to generate maze");
            SendCustomEventDelayedSeconds(nameof(this.MazeGenerationFailedAttempt), 0.5f);
        }
        else if (success == -1)
        {
            Debug.LogError("[Game.cs] GenerateNewMaze: Failed to generate maze (null vars - shouldn't happen)");
        }
    }

    public void On_MazeGenerationComplete(int numEnemies)
    {
        int minEnemies = this.m_EasyModeOn ? 2 : 5;
        int maxEnemies = this.m_EasyModeOn ? 5 : 15;

        if (numEnemies < minEnemies)
        {
            Debug.LogError($"[Game.cs] On_MazeGenerationComplete: Invalid number of enemies {numEnemies}, resetting to default");
            SendCustomEventDelayedSeconds(nameof(this.MazeGenerationFailedAttempt), 0.5f);
            return;
        }

        SendCustomEventDelayedSeconds(nameof(this.SerializeMazeToB64Json), this.m_OptimisationDelayKek);
    }

    public void SerializeMazeToB64Json()
    {
        if (!Networking.IsMaster) return;

        if (VRCJson.TrySerializeToJson(MazeToDictionary(80, 80, this.m_Theme), JsonExportType.Minify, out this.m_InitialMazeDataToken))
        {
            SendCustomEventDelayedSeconds(nameof(this.On_Successfully_Serialized_To_JSON), this.m_OptimisationDelayKek);
        }
        else
        {
            Debug.LogError("[Game.cs] SerializeMazeToB64Json: Failed to serialize maze to JSON");
            SendCustomEventDelayedSeconds(nameof(this.MazeGenerationFailedAttempt), this.m_OptimisationDelayKek);
        }
    }



    public void On_Successfully_Serialized_To_JSON()
    {
        if (!Networking.IsMaster) return;

        Debug.Log("[Game.cs] On_Successfully_Serialized_To_JSON: Successfully serialized maze to JSON");

        VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        allPlayers = VRCPlayerApi.GetPlayers(allPlayers);

        this.m_PlayersExpectedInCurrentGame = allPlayers.Length;
        this.m_PlayersInCurrentGame = 0;
        this.m_PlayerMazeGenerationStatus = new DataDictionary();
        this.m_PlayerLoadingPercentages = new DataDictionary();
        this.m_PlayerFPS = new DataDictionary();
        this.m_LastUpdatedLoadingPercentageTimeText = Time.time;
        this.m_LoadingPercentage = 0.0f;

        this.m_PlayerGenStatusText = "";
        foreach (VRCPlayerApi player in allPlayers)
        {
            if (player == null) continue;
            if (!player.IsValid()) continue;

            string playerNameId = this.VRCPlayerApiObjectToUniqueNameString(player);
            this.m_PlayerMazeGenerationStatus[playerNameId] = false;
            this.m_PlayerLoadingPercentages[playerNameId] = 0.0f;
            this.m_PlayerFPS[playerNameId] = 0.0f;

            this.m_PlayerGenStatusText += $"{player.displayName}: (0%)\n";
        }

        if (this.m_PlayerGenStatusTexts != null)
        {
            this.m_PlayerGenStatusTexts.text = this.m_PlayerGenStatusText;
        }

        // Split the JSON string into 10 parts
        string jsonString = this.m_InitialMazeDataToken.String;
        if (jsonString == null || jsonString.Length < 10)
        {
            Debug.LogError("[Game.cs] On_Successfully_Serialized_To_JSON: bad JSON");
            SendCustomEventDelayedSeconds(nameof(this.MazeGenerationFailedAttempt), this.m_OptimisationDelayKek);
            return;
        }
         
        int partLength = jsonString.Length / 10;
        for (int i = 0; i < 10; i++)
        {
            int startIndex = i * partLength;
            int length = (i == 9) ? jsonString.Length - startIndex : partLength;
            this.m_InitialMazeDataSplit[i] = jsonString.Substring(startIndex, length);
        }

        SendCustomEventDelayedSeconds(nameof(this.SyncStateDictionaries), this.m_OptimisationDelayKek);
    }

    public void SyncStateDictionaries()
    {
        Debug.Log("[Game.cs] SyncStateDictionaries: Syncing state dictionaries");
        if (!Networking.IsMaster) return;

        DataToken generationStatusJson;
        if (!VRCJson.TrySerializeToJson(this.m_PlayerMazeGenerationStatus, JsonExportType.Minify, out generationStatusJson))
        {
            return;
        }

        DataToken loadingStatesJson;
        if (!VRCJson.TrySerializeToJson(this.m_PlayerLoadingPercentages, JsonExportType.Minify, out loadingStatesJson))
        {
            return;
        }

        if (generationStatusJson.String == null || loadingStatesJson.String == null
        || generationStatusJson.String.Length < 10 || loadingStatesJson.String.Length < 10)
        {
            Debug.LogError("[Game.cs] SyncStateDictionaries: bad JSON");
            SendCustomEventDelayedSeconds(nameof(this.MazeGenerationFailedAttempt), this.m_OptimisationDelayKek);
            return;
        }

        SendMethodNetworked(
            nameof(this.On_SyncStateDictionaries),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(generationStatusJson.String),
            new DataToken(loadingStatesJson.String),
            new DataToken(this.m_PlayersExpectedInCurrentGame)
        );
    }

    [NetworkedMethod]
    public void On_SyncStateDictionaries(VRCPlayerApi requestingPlayer, string generationStatusToken, string loadingStatesToken, int playersExpected)
    {
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!requestingPlayer.isMaster) return;

        if (Networking.IsMaster)
        {
            SendCustomEventDelayedSeconds(
                nameof(this.SendGeneratedMazeDataParts),
                this.m_OptimisationDelayKek * 2.0f
            );

            return;
        }

        DataToken generationStatusDeserialized;
        if (!VRCJson.TryDeserializeFromJson(generationStatusToken, out generationStatusDeserialized))
        {
            return;
        }
        DataDictionary generationStatus = generationStatusDeserialized.DataDictionary;

        DataToken loadingStatesDeserialized;
        if (!VRCJson.TryDeserializeFromJson(loadingStatesToken, out loadingStatesDeserialized))
        {
            return;
        }
        DataDictionary loadingStates = loadingStatesDeserialized.DataDictionary;

        Debug.Log($"[Game.cs] On_SyncStateDictionaries: Generation status and loading states are valid");

        this.m_PlayerMazeGenerationStatus = generationStatus;
        this.m_PlayerLoadingPercentages = loadingStates;

        if (this.m_PlayerMazeGenerationStatus == null || this.m_PlayerLoadingPercentages == null)
        {
            Debug.LogError("[Game.cs] On_SyncStateDictionaries: bad JSON recvd");
            return;
        }

        this.m_PlayerFPS = new DataDictionary();
        this.m_LastUpdatedLoadingPercentageTimeText = Time.time;
        this.m_LoadingPercentage = 0.0f;

        this.m_PlayersExpectedInCurrentGame = playersExpected;

        Debug.Log($"[Game.cs] On_SyncStateDictionaries: Synced state dictionaries");

        VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        allPlayers = VRCPlayerApi.GetPlayers(allPlayers);

        this.m_PlayerGenStatusText = "";
        foreach (VRCPlayerApi player in allPlayers)
        {
            if (player == null) continue;
            if (!player.IsValid()) continue;

            string playerNameId = this.VRCPlayerApiObjectToUniqueNameString(player);
            this.m_PlayerMazeGenerationStatus[playerNameId] = false;
            this.m_PlayerLoadingPercentages[playerNameId] = 0.0f;
            this.m_PlayerFPS[playerNameId] = 0.0f;

            this.m_PlayerGenStatusText += $"{player.displayName}: (0%)\n";
        }

        if (this.m_PlayerGenStatusTexts != null)
        {
            this.m_PlayerGenStatusTexts.text = this.m_PlayerGenStatusText;
        }
    }

    public void SendGeneratedMazeDataParts()
    {
        if (!Networking.IsMaster) return;

        // Check if the split data is valid
        if (this.m_InitialMazeDataSplit == null || this.m_InitialMazeDataSplit.Length != 10)
        {
            SendCustomEventDelayedSeconds(nameof(this.MazeGenerationFailedAttempt), 0.5f);
            return;
        }

        this.m_CurrentDataPartIndex = 0;

        SendCustomEventDelayedSeconds(
            nameof(this.SendGeneratedMazeDataPart),
            this.m_OptimisationDelayKek
        );
    }

    public void SendGeneratedMazeDataPart()
    {
        if (!Networking.IsMaster) return;

        // Check if the split data is valid
        if (this.m_InitialMazeDataSplit == null || this.m_InitialMazeDataSplit.Length != 10)
        {
            SendCustomEventDelayedSeconds(nameof(this.MazeGenerationFailedAttempt), 0.5f);
            return;
        }

        SendMethodNetworked(
            nameof(this.On_MazeDataPartReceived),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(this.m_InitialMazeDataSplit[this.m_CurrentDataPartIndex]),
            new DataToken(this.m_CurrentDataPartIndex)
        );

        this.m_CurrentDataPartIndex++;

        if (this.m_CurrentDataPartIndex < 10)
        {
            SendCustomEventDelayedSeconds(
                nameof(this.SendGeneratedMazeDataPart),
                this.m_OptimisationDelayKek
            );
        }
    }

    public DataDictionary MazeToDictionary(int width, int height, int theme)
    {
        DataDictionary d = new DataDictionary();
        
        d["theme"] = theme;
        d["width"] = width;
        d["height"] = height;
        d["exit_x"] = this.m_InitialMazeDataExit.x;
        d["exit_y"] = this.m_InitialMazeDataExit.y;

        DataDictionary mazeDictionary = new DataDictionary();
        for(int x = 0; x < width; x++)
        {
            DataDictionary xDictionary = new DataDictionary();
            for(int y = 0; y < height; y++)
            {
                xDictionary[NumberLookup[y]] = this.m_InitialMazeData[x][y];
            }
            mazeDictionary[NumberLookup[x]] = xDictionary;
        }

        d["maze"] = mazeDictionary;

        return d;
    }

    [NetworkedMethod]
    public void On_MazeDataPartReceived(VRCPlayerApi respondingPlayer, string dataPart, int partIndex)
    {
        if (respondingPlayer == null) return;
        if (!respondingPlayer.IsValid()) return;
        if (!respondingPlayer.isMaster) return;

        // Initialize the array if necessary
        if (this.m_ReceivedMazeDataParts == null || this.m_ReceivedMazeDataParts.Length != 10
           || this.m_ReceivedMazeDataPartsCount == 0) 
        {
            this.m_ReceivedMazeDataParts = new string[10];
            this.m_ReceivedMazeDataPartsCount = 0;
        }

        // Store the received part
        this.m_ReceivedMazeDataParts[partIndex] = dataPart;
        this.m_ReceivedMazeDataPartsCount++;

        // When all parts are received, reassemble and process the data
        if (this.m_ReceivedMazeDataPartsCount == 10)
        {
            string jsonString = string.Join("", this.m_ReceivedMazeDataParts);

            if (Time.time - this.m_LastReceivedMazeDataTime < this.m_MazeDataReceiveCooldown)
            {
                Debug.LogWarning("[Game.cs] On_MazeDataPartReceived: Received maze data too quickly");
                return;
            }

            this.m_GameStatus = GameStatus.Generating;
            this.m_LastReceivedMazeDataTime = Time.time;

            if (VRCJson.TryDeserializeFromJson(jsonString, out this.m_ReceivedMazeData))
            {
                if (this.m_ReceivedMazeData.TokenType == TokenType.DataDictionary)
                {
                    SendCustomEventDelayedSeconds(nameof(this.HandleMazeDictionary), this.m_OptimisationDelayKek);
                }
            }
        }
    }
    // Maze dimensions
    private int m_MazeWidth;
    private int m_MazeHeight;

    // Current row being processed
    private int m_CurrentMazeRow = 0;

    // Cached maze data dictionary
    private DataDictionary m_MazeDictionary;
    public void HandleMazeDictionary()
    {
        if (!this.m_GameStatus.Equals(GameStatus.Generating)) return;
        // Cache dictionary references
        DataDictionary dataDictionary = this.m_ReceivedMazeData.DataDictionary;

        if (!dataDictionary.ContainsKey("width") ||
            !dataDictionary.ContainsKey("height") ||
            !dataDictionary.ContainsKey("theme") ||
            !dataDictionary.ContainsKey("exit_x") ||
            !dataDictionary.ContainsKey("exit_y") ||
            !dataDictionary.ContainsKey("maze"))
        {
            Debug.LogError("[Game.cs] HandleMazeDictionary: Missing keys in dictionary");
            return;
        }

        // Get dimensions once
        this.m_MazeWidth = (int)dataDictionary["width"].Double;
        this.m_MazeHeight = (int)dataDictionary["height"].Double;
        this.m_MazeTheme = (int)dataDictionary["theme"].Double;
        this.m_MazeExit = new Vector2Int(
            (int)dataDictionary["exit_x"].Double,
            (int)dataDictionary["exit_y"].Double
        );

        // Cache maze dictionary
        this.m_MazeDictionary = dataDictionary["maze"].DataDictionary;

        // Pre-allocate maze array
        this.m_Maze = new int[this.m_MazeHeight][];

        // Reset current row index
        this.m_CurrentMazeRow = 0;

        // Start processing the first row with a delay
        SendCustomEventDelayedSeconds(nameof(ProcessMazeRow), this.m_OptimisationDelayKek);
    }

    public void ProcessMazeRow()
    {
        if (!this.m_GameStatus.Equals(GameStatus.Generating)) return;
        int x = this.m_CurrentMazeRow;

        // Initialize the row array
        this.m_Maze[x] = new int[this.m_MazeWidth];

        // Get the row dictionary for the current row
        DataDictionary rowDictionary = this.m_MazeDictionary[NumberLookup[x]].DataDictionary;

        // Process each column in the current row
        for (int y = 0; y < this.m_MazeWidth; y++)
        {
            this.m_Maze[x][y] = (int)rowDictionary[NumberLookup[y]].Double;
        }

        // Move to the next row
        this.m_CurrentMazeRow++;

        // If there are more rows to process, schedule the next row processing
        if (this.m_CurrentMazeRow < this.m_MazeHeight)
        {
            SendCustomEventDelayedSeconds(nameof(ProcessMazeRow), 0.05f);
        }
        else
        {
            // All rows processed, proceed to the next step
            SendCustomEventDelayedSeconds(nameof(this.BeginInstantiatingMaze), this.m_OptimisationDelayKek);
        }
    }

    private float m_BeginInstantiatingTime = 0.0f;
    public void BeginInstantiatingMaze()
    {
        if (!this.m_GameStatus.Equals(GameStatus.Generating)) return;
        if (this.m_LevelGenerator == null) return;
        this.m_LocalPlayerFinishedGeneratingMaze = false;

        this.m_LevelGenerator.DeleteAllDebugWalls();
        this.m_BeginInstantiatingTime = Time.time;
        Debug.Log("$[Game.cs] BeginInstantiatingMaze: Instantiating maze at " + this.m_BeginInstantiatingTime.ToString("F2"));
        this.m_LevelGenerator.DebugInstantiateMaze_Sliced(this.m_MazeTheme);
    }

    public void On_LocalPlayer_MazeGenerated(ref GameObject exitTileRef)
    {
        if(!this.m_GameStatus.Equals(GameStatus.Generating)) return;

        if (Networking.IsMaster)
        {
            this.m_MasterFinishedGeneratingMaze = true;
            this.m_TimeMasterGeneratedMaze = Time.time;
        }

        Debug.Log($"[Game.cs] On_LocalPlayer_MazeGenerated: Maze generated in {Time.time - this.m_BeginInstantiatingTime:F2} seconds");
        if (this.m_TempleStairs == null)
        {
            Debug.LogError("[Game.cs] On_LocalPlayer_MazeGenerated: m_TempleStairs is null");
            return;
        }

        this.m_TempleStairs.SetTeleportLocation(ref exitTileRef);

        if (this.m_DesktopStairsExit != null)
        {
            this.m_DesktopStairsExit.Initialise(ref exitTileRef);
        }

        this.m_LoadingPercentage = 100.0f;
        this.m_LocalPlayerFinishedGeneratingMaze = true;

        SendMethodNetworked(
            nameof(this.On_Received_FinishedGeneratingMaze),
            SyncTarget.Master,
            new DataToken(Networking.LocalPlayer)
        );
    }

    [NetworkedMethod]
    public void On_Received_FinishedGeneratingMaze(VRCPlayerApi player)
    {
        if (this.m_GameStatus.Equals(GameStatus.Generating) && !this.m_IsGameStarting)
        {
            if (!Networking.IsMaster) return;
            if (player == null) return;
            if (!player.IsValid()) return;
            if (this.m_PlayerMazeGenerationStatus == null) return;

            string playerNameId = this.VRCPlayerApiObjectToUniqueNameString(player);
            if (!this.m_PlayerMazeGenerationStatus.ContainsKey(playerNameId)) return;

            if (this.m_PlayerMazeGenerationStatus[playerNameId] == false)
            {
                this.m_PlayerMazeGenerationStatus[playerNameId] = true;
                this.m_PlayersInCurrentGame++;
            }

            if (this.m_PlayersInCurrentGame == this.m_PlayersExpectedInCurrentGame)
            {
                this.On_StartGame();
            }
            else if (this.m_AbandonLaggers && this.m_PlayersInCurrentGame > this.m_PlayersExpectedInCurrentGame * 0.75f)
            {
                SendCustomEventDelayedSeconds(
                    nameof(this.On_WaitingForAllPlayersToGenerateTimeout),
                    this.m_WaitForSlowPlayerGenerationTimeout
                );
            }
        }
        else if ((this.m_GameStatus.Equals(GameStatus.Generating) || this.m_GameStatus.Equals(GameStatus.InProgress)) && this.m_IsGameStarting)
        {
            this.HandleLateJoiner(player);
        }
    }

    public void HandleLateJoiner(VRCPlayerApi player)
    {
        if (player == null) return;
        if (!player.IsValid()) return;
        if (!Networking.IsMaster) return;
        if (this.m_PlayerMazeGenerationStatus == null) return;
        if (this.m_PlayerFollowerStatus == null) return;
        if (this.m_PlayerAliveStatus == null) return;
        if (this.m_IsGameStarting == false) return;
        if (!(this.m_GameStatus.Equals(GameStatus.Generating) || this.m_GameStatus.Equals(GameStatus.InProgress))) return;

        string playerNameId = this.VRCPlayerApiObjectToUniqueNameString(player);
        if (!this.m_PlayerMazeGenerationStatus.ContainsKey(playerNameId)) return;
        if (this.m_PlayerMazeGenerationStatus[playerNameId] == true) return;
        if (this.m_PlayerFollowerStatus.ContainsKey(playerNameId)) return;
        if (this.m_PlayerAliveStatus.ContainsKey(playerNameId)) return;

        this.m_PlayerMazeGenerationStatus[playerNameId] = true;
        this.m_PlayerAliveStatus[playerNameId] = true;
        this.m_PlayerFollowerStatus[playerNameId] = false;

        this.m_PlayersInCurrentGame++;

        DataToken playerFollowerStatuses;
        if (VRCJson.TrySerializeToJson(this.m_PlayerFollowerStatus, JsonExportType.Minify, out playerFollowerStatuses))
        {
            if (playerFollowerStatuses.String == null) { Debug.LogError("[Game.cs] On_StartGame: playerFollowerStatuses is null"); return; }

            SendMethodNetworked(
                nameof(this.On_StartGameReceived),
                SyncTarget.All,
                new DataToken(Networking.LocalPlayer),
                new DataToken(playerFollowerStatuses.String)
            );

            SendCustomEventDelayedSeconds(
                nameof(this.SetupAI),
                1.0f
            );

            SendMethodNetworked(
                nameof(this.On_LateJoiner),
                SyncTarget.All,
                new DataToken(Networking.LocalPlayer),
                new DataToken(player)
            );
        }
    }

    [NetworkedMethod]
    public void On_LateJoiner(VRCPlayerApi requestingPlayer, VRCPlayerApi latePlayer)
    {
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!requestingPlayer.isMaster) return;
        if (latePlayer == null) return;
        if (!latePlayer.IsValid()) return;

        if (this.m_PlayerFollowerStatus == null) return;
        if (this.m_PlayerAliveStatus == null) return;

        string playerNameId = this.VRCPlayerApiObjectToUniqueNameString(latePlayer);

        this.m_PlayerFollowerStatus[playerNameId] = false;
        this.m_PlayerAliveStatus[playerNameId] = true;
        this.m_PlayerCapsuleColliderManager.SetColliderStatesFromDataDictionary(this.m_PlayerFollowerStatus);
    }

    public void On_WaitingForAllPlayersToGenerateTimeout()
    {
        if (!Networking.IsMaster) return;
        if (!this.m_GameStatus.Equals(GameStatus.Generating)) return;

        this.On_StartGame();
    }

    public void On_StartGame()
    {
        if (!this.m_GameStatus.Equals(GameStatus.Generating)) return;
        if (!Networking.IsMaster) return;
        if (this.m_PlayerMazeGenerationStatus == null) return;
        if (this.m_PlayerFollowerStatus == null) { Debug.LogError("[Game.cs] On_StartGame: this.m_PlayerFollowerStatus is null"); return; }
        if (this.m_IsGameStarting == true) return;

        this.m_IsGameStarting = true;

        VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        allPlayers = VRCPlayerApi.GetPlayers(allPlayers);
        this.m_PlayerAliveStatus = new DataDictionary();
        this.m_PlayerFollowerStatus = new DataDictionary();
        this.m_PlayerFollowerCount = 0;
        foreach (VRCPlayerApi player in allPlayers)
        {
            if (player == null) continue;
            if (!player.IsValid()) continue;

            string playerNameId = this.VRCPlayerApiObjectToUniqueNameString(player);
            if(!this.m_PlayerMazeGenerationStatus.ContainsKey(playerNameId)) continue;
            if(this.m_PlayerMazeGenerationStatus[playerNameId] == false) continue;

            this.m_PlayerAliveStatus[playerNameId] = true;

            this.m_PlayerFollowerStatus[playerNameId] = false;
        }

        DataToken playerFollowerStatuses;
        if (VRCJson.TrySerializeToJson(this.m_PlayerFollowerStatus, JsonExportType.Minify, out playerFollowerStatuses))
        {
            if (playerFollowerStatuses.String == null) { Debug.LogError("[Game.cs] On_StartGame: playerFollowerStatuses is null"); return; }

            SendMethodNetworked(
                nameof(this.On_StartGameReceived),
                SyncTarget.All,
                new DataToken(Networking.LocalPlayer),
                new DataToken(playerFollowerStatuses.String)
            );

            SendCustomEventDelayedSeconds(
                nameof(this.SetupAI),
                1.0f
            );
        }
    }

    public string VRCPlayerApiObjectToUniqueNameString(VRCPlayerApi player)
    {
        if(player == null) return null;
        if(!player.IsValid()) return null;

        return $"{player.displayName}#{player.playerId.ToString()}";
    }

    [NetworkedMethod]
    public void On_StartGameReceived(VRCPlayerApi requestingPlayer, string playerFollowrStatusJsonString)
    {
        if (!this.m_GameStatus.Equals(GameStatus.Generating)) return;
        if (!this.m_LocalPlayerFinishedGeneratingMaze) return;
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!requestingPlayer.isMaster) return;
        if (playerFollowrStatusJsonString == null) { Debug.LogError("[Game.cs] On_StartGameReceived: playerFollowrStatus is null"); return; }
        if (playerFollowrStatusJsonString.Length == 0) { Debug.LogError("[Game.cs] On_StartGameReceived: playerFollowrStatus is empty"); return; }

        this.m_IsLocalPlayerFollower = false;

        DataToken playerFollowrStatusToken;
        if (!VRCJson.TryDeserializeFromJson(playerFollowrStatusJsonString, out playerFollowrStatusToken)) { Debug.LogError("[Game.cs] On_StartGameReceived: Failed to deserialize playerFollowrStatus"); return; }
        if (playerFollowrStatusToken.TokenType != TokenType.DataDictionary) { Debug.LogError("[Game.cs] On_StartGameReceived: playerFollowrStatus is not a DataDictionary"); return; }
        this.m_PlayerFollowerStatus = playerFollowrStatusToken.DataDictionary;

        this.m_PlayerCapsuleColliderManager.SetColliderStatesFromDataDictionary(this.m_PlayerFollowerStatus);
        string localPlayerNameId = this.VRCPlayerApiObjectToUniqueNameString(Networking.LocalPlayer);
        if (!this.m_PlayerFollowerStatus.ContainsKey(localPlayerNameId))
        {
            this.m_PlayerFollowerStatus[localPlayerNameId] = false;
            this.m_IsLocalPlayerFollower = false;
        }
        else
        {
            this.m_IsLocalPlayerFollower = this.m_PlayerFollowerStatus[localPlayerNameId].Boolean;
        }

        this.m_GameStatus = GameStatus.InProgress;

        this.m_ForestsAttempted++;

        if (this.m_SpawnAreaScoreboard != null)
        {
            this.m_SpawnAreaScoreboard.ResetDeaths();
        }

        this.m_Lord.Show_Lord();
        this.SendPlayerToGameStart(Networking.LocalPlayer);

        this.m_LocalPlayerStartedGameTime = Time.time;
        this.m_LocalPlayerInGame = true;

        this.m_RoundStartedTime = Time.time;
        this.m_Narrator.PlayCaveIntro();
    }

    public void InitialSpawnFreeze()
    {
        Networking.LocalPlayer.Immobilize(true);
    }

    public void InitialSpawnUnfreeze()
    {
        Networking.LocalPlayer.Immobilize(false);
    }

    public void SetupAI()
    {
        if (!(this.m_GameStatus.Equals(GameStatus.Generating) || this.m_GameStatus.Equals(GameStatus.InProgress))) return;
        if (!Networking.IsMaster) return;
        if (this.m_LevelGenerator == null) return;

        this.m_EnemyAIs = this.m_LevelGenerator.GetComponentsInChildren<EnemyAI_Animated>();

        foreach (EnemyAI_Animated enemyAI in this.m_EnemyAIs)
        {
            if (enemyAI == null) continue;

            string[] path = enemyAI.GeneratePredefinedPath();
            this.SendPredefinedPathToOtherClients(path, enemyAI.m_ID);
        }
    }

    public void SendPredefinedPathToOtherClients(string[] path, int aiID)
    {
        if (!(this.m_GameStatus.Equals(GameStatus.Generating) || this.m_GameStatus.Equals(GameStatus.InProgress))) return;
        if (!Networking.IsMaster) return;
        if (path == null) return;
        if (path.Length == 0) return;
        if (aiID == -1) return;

        string joined = string.Join(",", path);

        SendMethodNetworked(
            nameof(this.On_PredefinedPathReceived),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(joined),
            new DataToken(aiID)
        );
    }

    [NetworkedMethod]
    public void On_PredefinedPathReceived(VRCPlayerApi requestingPlayer, string path, int aiID)
    {
        if (!(this.m_GameStatus.Equals(GameStatus.Generating) || this.m_GameStatus.Equals(GameStatus.InProgress))) return;
        if (!this.m_LocalPlayerFinishedGeneratingMaze) return;
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;
        if (!requestingPlayer.isMaster) return;
        if (path == null) return;
        if (path.Length == 0) return;
        if (aiID == -1) return;
        if (this.m_LevelGenerator == null) return;


        this.m_EnemyAIs = this.m_LevelGenerator.GetComponentsInChildren<EnemyAI_Animated>();

        foreach(EnemyAI_Animated enemyAI in this.m_EnemyAIs)
        {
            if (enemyAI == null) continue;
            if (enemyAI.m_ID == aiID)
            {
                string[] pathArray = path.Split(',');
                enemyAI.InitialisePredefinedPath(pathArray, aiID);
                return;
            }
        }
    }

    public void SendPlayerToGameStart(VRCPlayerApi player)
    {
        if (!this.m_GameStatus.Equals(GameStatus.InProgress)) return;
        if (player == null) return;
        if (!player.IsValid()) return;

        if (this.m_IsLocalPlayerFollower)
        {
            if(this.m_Exit_Spawn_SpawnerReferenceCube == null)
            {
                Debug.LogError("[Game.cs] On_StartGameReceived: m_Exit_Spawn_SpawnerReferenceCube is null");
                return;
            }

            Networking.LocalPlayer.TeleportTo(
                this.m_Exit_Spawn_SpawnerReferenceCube.transform.position,
                this.m_Exit_Spawn_SpawnerReferenceCube.transform.rotation
            );
        }
        else
        {
            if(this.m_Game_Spawn_SpawnerReferenceCube == null)
            {
                Debug.LogError("[Game.cs] On_StartGameReceived: m_Game_Spawn_SpawnerReferenceCube is null");
                return;
            }

            Networking.LocalPlayer.TeleportTo(
                this.m_Game_Spawn_SpawnerReferenceCube.transform.position,
                this.m_Game_Spawn_SpawnerReferenceCube.transform.rotation
            );
        }
    }

    public void BeginForestAmbience()
    {
        if (this.m_Ambience_Forest != null && !this.m_Ambience_Forest.isPlaying)
        {
            this.m_Ambience_Forest.Play();
        }

        if (this.m_Firefly_ParticleSystem != null)
        {
            this.m_Firefly_ParticleSystem.Play();
        }
    }

    public void StopForestAmbience()
    {
        if(this.m_Ambience_Forest != null && this.m_Ambience_Forest.isPlaying)
        {
            this.m_Ambience_Forest.Stop();
        }

        if (this.m_Firefly_ParticleSystem != null)
        {
            this.m_Firefly_ParticleSystem.Stop();
        }
    }

    public void BeginCaveAmbience()
    {
        if (!this.m_GameStatus.Equals(GameStatus.InProgress)) return;

        if (this.m_Ambience_Cave != null && !this.m_Ambience_Cave.isPlaying)
        {
            this.m_Ambience_Cave.Play();
        }
    }

    public void StopCaveAmbience()
    {
        if (this.m_Ambience_Cave != null && this.m_Ambience_Cave.isPlaying)
        {
            this.m_Ambience_Cave.Stop();
        }
    }

    // When the masters' game timer expires and the game is over
    public void On_GameTimeExpired()
    {
        if(!Networking.IsMaster) return;
        if(this.m_GameStatus != GameStatus.InProgress) return;
        if(Time.time - this.m_LocalPlayerStartedGameTime < (this.m_RoundTimeSeconds - 5.0f)) return;

        this.m_GameStatus = GameStatus.Finished;
        SendMethodNetworked(
            nameof(this.On_GameFinished),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(false),
            new DataToken(false)
        );
    }

    // When either the Lord dies or all normal players die
    public void On_GameEndedLordDied(VRCPlayerApi attackingPlayer)
    {
        if(!Networking.IsMaster) return;
        if(this.m_GameStatus != GameStatus.InProgress) return;

        SendMethodNetworked(
            nameof(this.On_GameFinished),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(true),
            new DataToken(false),
            new DataToken(attackingPlayer)
        );
    }

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
        if(player == null) return;
        if (!player.IsValid()) return;
        if(!player.isLocal) return;

        base.OnPlayerRespawn(player);

        this.StopCaveAmbience();
        this.BeginForestAmbience();

        if (!this.m_GameStatus.Equals(GameStatus.InProgress)) return;

        SendMethodNetworked(
            nameof(this.On_AnnouncePlayerRespawned),
            SyncTarget.Master,
            new DataToken(player)
        );
    }

    [NetworkedMethod]
    public void On_AnnouncePlayerRespawned(VRCPlayerApi requestingPlayer)
    {
        if (!this.m_GameStatus.Equals(GameStatus.InProgress)) return;
        if (!Networking.IsMaster) return;
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;

        this.On_PlayerKilled(requestingPlayer);
    }

    public void On_PlayerKilled(VRCPlayerApi deadPlayer)
    {
        if (!this.m_GameStatus.Equals(GameStatus.InProgress)) return;
        if (!Networking.IsMaster) return;
        if (deadPlayer == null) return;
        if (!deadPlayer.IsValid()) return;

        if (this.m_PlayerAliveStatus == null) return;
        string deadPlayerNameId = this.VRCPlayerApiObjectToUniqueNameString(deadPlayer);
        if(!this.m_PlayerAliveStatus.ContainsKey(deadPlayerNameId)) return;

        this.m_PlayerAliveStatus[deadPlayerNameId] = false;

        // If they were a follower and they died, remove them from the follower list
        if (this.m_PlayerFollowerStatus == null) return;
        if (this.m_PlayerFollowerStatus.ContainsKey(deadPlayerNameId))
        {
            if (this.m_PlayerFollowerStatus[deadPlayerNameId] == true)
            {
                this.m_PlayerFollowerStatus[deadPlayerNameId] = false;
                this.m_PlayerFollowerCount--;
            }
        }

        // Check if all players who are not followers are dead
        if (!this.AreAllNonFollowersDead()) return;

        // if all players are dead or followers, end the game
        this.AnnounceGameFinish();
    }

    public bool AreAllNonFollowersDead()
    {
        if (!this.m_GameStatus.Equals(GameStatus.InProgress)) return false;
        if (this.m_PlayerAliveStatus == null) return false;
        if (this.m_PlayerFollowerStatus == null) return false;

        VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        allPlayers = VRCPlayerApi.GetPlayers(allPlayers);
        foreach (VRCPlayerApi player in allPlayers)
        {
            // check the player is valid and in the alive dictionary
            if (player == null || !player.IsValid()) continue;
            string playerNameId = this.VRCPlayerApiObjectToUniqueNameString(player);
            if (!this.m_PlayerAliveStatus.ContainsKey(playerNameId)) continue;

            // check the player is not a follower
            if (this.m_PlayerFollowerStatus.ContainsKey(playerNameId) && this.m_PlayerFollowerStatus[playerNameId] == true) continue;

            // check the player is alive
            if (this.m_PlayerAliveStatus[playerNameId] == true) return false;
        }

        return true;
    }

    public void AnnounceGameFinish()
    {
        if (!this.m_GameStatus.Equals(GameStatus.InProgress)) return;
        if (!Networking.IsMaster) return;

        SendMethodNetworked(
            nameof(this.On_GameFinished),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(false),
            new DataToken(true)
        );
    }

    [NetworkedMethod]
    public void On_GameFinished(VRCPlayerApi requestingPlayer, bool lordDied, bool allNonFollowersDied, VRCPlayerApi attackingPlayer)
    {
        if(requestingPlayer == null) return;
        if(!requestingPlayer.IsValid()) return;
        if(!requestingPlayer.isMaster) return;
        if(!this.m_LocalPlayerInGame) return;

        this.m_GameStatus = GameStatus.Finished;
        this.m_RoundStartedTime = 0.0f;
        this.m_LocalPlayerInGame = false;

        if (lordDied)
        {
            if (this.m_IsLocalPlayerFollower)
            {
                this.m_Narrator.PlayYouFailedToProtectFurry();
            }
            else if (attackingPlayer != null && attackingPlayer.IsValid() && attackingPlayer.isLocal)
            {
                this.m_Narrator.PlayYouKilledFurry();
            }
            else
            {
                this.m_Narrator.PlaySomeoneElseKilledFurry();
            }

            this.m_PlayerFollowerStatus.Clear();
            this.m_IsLocalPlayerFollower = false;
        }
        else if (allNonFollowersDied)
        {
            if (this.m_IsLocalPlayerFollower)
            {
                if (this.m_PlayersExpectedInCurrentGame == 1)
                {
                    this.m_Narrator.PlayYouJoinedAlone();
                }
                else
                {
                    this.m_Narrator.PlayYouJoinedAsFollower();
                }
            }
            else if (this.m_PlayersExpectedInCurrentGame > 1)
            {
                this.m_Narrator.PlayEveryoneDied();
            }
        }
        else
        {
            this.m_Narrator.PlayTimeRunsItsCourse();
        }

        foreach (Melee_Weapon melee_Weapon in this.m_MeleeWeapons)
            {
                if (melee_Weapon == null) continue;
                melee_Weapon.Reset();
            }

        foreach (CeremonialWine ceremonialWine in this.m_CeremonialWines)
        {
            if (ceremonialWine == null) continue;
            ceremonialWine.Reset();
        }

        this.Respawn();

        this.StopCaveAmbience();
        this.BeginForestAmbience();

        if (Networking.IsMaster)
        {
            SendCustomEventDelayedSeconds(nameof(this.On_ResetGame), 10);
        }
    }

    public void On_ResetGame()
    {
        if(!Networking.IsMaster) return;

        SendMethodNetworked(
            nameof(this.On_ResetGameReceived),
            SyncTarget.All
        );
    }

    [NetworkedMethod]
    public void On_ResetGameReceived()
    {
        this.m_RoundStartedTime = 0.0f;
        this.m_GameStatus = GameStatus.Waiting;
        this.m_IsGameStarting = false;
        this.m_ReceivedMazeDataPartsCount = 0;
        this.m_LocalPlayerInGame = false;
        this.m_LocalPlayerFinishedGeneratingMaze = false;
        this.m_MasterFinishedGeneratingMaze = false;
        this.m_TimeMasterGeneratedMaze = 0.0f;
        this.m_FailedGenerationAttempts = 0;

        this.m_PlayerMazeGenerationStatus = null;
        this.m_PlayerAliveStatus = null;

        this.m_PlayersExpectedInCurrentGame = 0;
        this.m_PlayersInCurrentGame = 0;

        this.m_LocalPlayerStartedGameTime = 0.0f;

        this.m_TimeWaiting = 0.0f;

        this.m_Maze = null;
        this.m_MazeTheme = -1;

        this.m_LoadingPCTextMesh.text = "";
        this.m_PlayerGenStatusTexts.text = "";
        this.m_LoadingPercentage = 0.0f;
        this.m_FPS = 0.0f;
        this.m_LastUpdatedLoadingPercentageTimeText = 0.0f;

        this.m_IsLocalPlayerFollower = false;
        this.m_PlayerFollowerCount = 0;
        this.m_LocalPlayerInGame = false;

        this.m_LastReceivedMazeDataTime = Time.time - (this.m_MazeDataReceiveCooldown * 0.9f);

        foreach (Melee_Weapon melee_Weapon in this.m_MeleeWeapons)
        {
            if (melee_Weapon == null) continue;
            melee_Weapon.Reset();
        }

        foreach (CeremonialWine ceremonialWine in this.m_CeremonialWines)
        {
            if (ceremonialWine == null) continue;
            ceremonialWine.Reset();
        }

        if (this.m_Exit != null)
        {
            this.m_Exit.Reset();
        }

        if (this.m_Club != null)
        {
            this.m_Club.ResetAll();
        }
        
        if(this.m_Sheild != null)
        {
            this.m_Sheild.ResetAll();
        }

        if (this.m_Potion != null)
        {
            this.m_Potion.ResetPotionLocal();
        }

        this.StopCaveAmbience();
        this.BeginForestAmbience();

        Networking.LocalPlayer.Immobilize(false);

        this.m_Deaths = 0;

        if (this.m_LevelGenerator == null) return;
        this.m_LevelGenerator.ResetAllStateVariables();
        this.m_LevelGenerator.DeleteAllDebugWalls();
    }

    public void Respawn()
    {
        if(this.m_Main_Spawn_SpawnerReferenceCube == null)
        {
            Debug.LogError("[Game.cs] On_StartGameReceived: m_Main_Spawn_SpawnerReferenceCube is null");
            return;
        }

        Networking.LocalPlayer.TeleportTo(
            this.m_Main_Spawn_SpawnerReferenceCube.transform.position,
            this.m_Main_Spawn_SpawnerReferenceCube.transform.rotation
        );
    }

    public void Announce_Self_Attacked_By_Enemy_AI(int aiID)
    {
        if (!this.m_GameStatus.Equals(GameStatus.InProgress)) return;
        if (aiID == -1) return;

        SendMethodNetworked(
            nameof(this.On_Player_Attacked_By_Enemy_AI),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(aiID)
        );
    }

    [NetworkedMethod]
    public void On_Player_Attacked_By_Enemy_AI(VRCPlayerApi deadPlayer, int aiID)
    {
        if (!this.m_GameStatus.Equals(GameStatus.InProgress)) return;
        if (deadPlayer == null) return;
        if(!deadPlayer.IsValid()) return;
        if(this.m_EnemyAIs == null) return;
        if(this.m_EnemyAIs.Length == 0) return;
        if (aiID == -1) return;

        foreach (EnemyAI_Animated enemyAI in this.m_EnemyAIs)
        {
            if (enemyAI == null) continue;
            if (enemyAI.m_ID == aiID)
            {
                enemyAI.KillPlayer(deadPlayer);
                return;
            }
        }
    }

    public void OnLocalPlayerKilledBySkeleton()
    {
        this.m_Deaths++;
        this.m_TotalDeaths++;

        if (this.m_SpawnAreaScoreboard == null) return;

        this.m_SpawnAreaScoreboard.AnnounceScoreChange(
            this.m_Deaths,
            this.m_TotalDeaths,
            this.m_ForestsAttempted,
            this.m_ForestsCompleted
        );

        if (this.m_Deaths == 1)
        {
            this.OnFirstSkeletonDeath();
        }
        else
        {
            this.SendPlayerToGameStart(Networking.LocalPlayer);
        }
    }

    public void OnFirstSkeletonDeath()
    {
        if (!this.m_GameStatus.Equals(GameStatus.InProgress)) return;

        Networking.LocalPlayer.Immobilize(true);

        Networking.LocalPlayer.TeleportTo(
            this.m_BlackBoxTransform.transform.position,
            this.m_BlackBoxTransform.transform.rotation
        );

        if (this.m_Narrator != null)
        {
            this.m_Narrator.PlayBoneHit1();
        }

        SendCustomEventDelayedSeconds(
            nameof(this.ReturnPlayerToGameAfterFirstSkeletonDeath),
            8.0f
        );
    }

    public void ReturnPlayerToGameAfterFirstSkeletonDeath()
    {
        Networking.LocalPlayer.Immobilize(false);
        if (!this.m_GameStatus.Equals(GameStatus.InProgress)) return;
        this.SendPlayerToGameStart(Networking.LocalPlayer);
    }

    public void OnLocalPlayerForestCompleted()
    {
        if (!this.m_GameStatus.Equals(GameStatus.InProgress)) return;

        this.m_ForestsCompleted++;

        if (this.m_SpawnAreaScoreboard == null) return;

        this.m_SpawnAreaScoreboard.AnnounceScoreChange(
            this.m_Deaths,
            this.m_TotalDeaths,
            this.m_ForestsAttempted,
            this.m_ForestsCompleted
        );
    }
}


















// notes
/*

    - gameplay loop
        - players respawn
            - players in the graveyard respawn in the main cave
            - followers who didnt die respawn in the temple
            - players who didnt die also respawn in the cave
        
        - 'good' players must navigate the maze to the exit
            - followers can defend Lord Kek and the temple by killing good players
        
        - players must find the exit before the time runs out
            - the forest exit is the entrance to the temple

        - players enter the temple

        - players find random loot in the temple
            - weapons
            - potions

        - players can either:
            - kill Lord Kek with a weapon
                - all players and followers respawn (maybe a game over sequence)
                - game over restart

            - join Lord Kek
                - players in the temple become a follower of Lord Kek
                - followerrs can then kill other players (including other followers)
                - there can be only 2 followers at a time
                    - if there are more than 2, a random follower will be killed by Lord Kek
                        at the end of the round

        - dead players go to the graveyard until the round ends















*/