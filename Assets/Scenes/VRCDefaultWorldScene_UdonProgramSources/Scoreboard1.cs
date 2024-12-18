using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Data;
using VRC.Udon;
using Miner28.UdonUtils.Network;

public class Scoreboard1 : NetworkInterface
{
    public TMPro.TMP_Text m_PlayerListText;
    private string m_PlayerListTextString = "";
    public TMPro.TMP_Text m_DeathsText;
    private string m_DeathsTextString = "";
    public TMPro.TMP_Text m_TotalDeathsText;
    private string m_TotalDeathsTextString = "";
    public TMPro.TMP_Text m_ForestsAttemptedText;
    private string m_ForestsAttemptedTextString = "";
    public TMPro.TMP_Text m_ForestsCompletedText;
    private string m_ForestsCompletedTextString = "";

    private DataDictionary m_PlayerDeaths = new DataDictionary();
    private DataDictionary m_PlayerTotalDeaths = new DataDictionary();
    private DataDictionary m_PlayerForestsAttempted = new DataDictionary();
    private DataDictionary m_PlayerForestsCompleted = new DataDictionary();

    public Scoreboard1 m_Replication;

    void Start()
    {
        
    }

    public void ResetDeaths()
    {
        this.m_PlayerDeaths.Clear();

        this.Redraw();
    }

    public void AnnounceScoreChange(int deaths, int totalDeaths, int forestsAttempted, int forestsCompleted)
    {
        SendMethodNetworked(
            nameof(this.OnScoreChangeReceived),
            SyncTarget.All,
            new DataToken(Networking.LocalPlayer),
            new DataToken(deaths),
            new DataToken(totalDeaths),
            new DataToken(forestsAttempted),
            new DataToken(forestsCompleted)
        );
    }

    [NetworkedMethod]
    public void OnScoreChangeReceived(VRCPlayerApi requestingPlayer, int deaths, int totalDeaths, int forestsAttempted, int forestsCompleted)
    {
        if (requestingPlayer == null) return;
        if (!requestingPlayer.IsValid()) return;

        if(this.m_PlayerDeaths == null)
        {
            this.m_PlayerDeaths = new DataDictionary();
        }

        if (this.m_PlayerTotalDeaths == null)
        {
            this.m_PlayerTotalDeaths = new DataDictionary();
        }

        if (this.m_PlayerForestsAttempted == null)
        {
            this.m_PlayerForestsAttempted = new DataDictionary();
        }

        if (this.m_PlayerForestsCompleted == null)
        {
            this.m_PlayerForestsCompleted = new DataDictionary();
        }

        string playerKeyName = VRCPlayerApiObjectToUniqueNameString(requestingPlayer);

        this.m_PlayerDeaths[playerKeyName] = deaths;
        this.m_PlayerTotalDeaths[playerKeyName] = totalDeaths;
        this.m_PlayerForestsAttempted[playerKeyName] = forestsAttempted;
        this.m_PlayerForestsCompleted[playerKeyName] = forestsCompleted;

        this.Redraw();
    }

    private void Redraw()
    {
        if (this.m_PlayerListText == null) return;
        if (this.m_DeathsText == null) return;
        if (this.m_TotalDeathsText == null) return;
        if (this.m_PlayerDeaths == null) return;
        if (this.m_PlayerTotalDeaths == null) return;

        VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        allPlayers = VRCPlayerApi.GetPlayers(allPlayers);

        this.m_PlayerListTextString = "";
        this.m_DeathsTextString = "";
        this.m_TotalDeathsTextString = "";
        this.m_ForestsAttemptedTextString = "";
        this.m_ForestsCompletedTextString = "";

        foreach (VRCPlayerApi player in allPlayers)
        {
            if (player == null) continue;
            if (!player.IsValid()) continue;

            string playerKeyName = VRCPlayerApiObjectToUniqueNameString(player);

            this.m_PlayerListTextString += $"{player.displayName}\n";

            if (this.m_PlayerDeaths.ContainsKey(playerKeyName))
            {
                this.m_DeathsTextString += $"{this.m_PlayerDeaths[playerKeyName]}\n";
            }
            else
            {
                this.m_DeathsTextString += "0\n";
            }

            if (this.m_PlayerTotalDeaths.ContainsKey(playerKeyName))
            {
                this.m_TotalDeathsTextString += $"{this.m_PlayerTotalDeaths[playerKeyName]}\n";
            }
            else
            {
                this.m_TotalDeathsTextString += "0\n";
            }

            if (this.m_PlayerForestsAttempted.ContainsKey(playerKeyName))
            {
                this.m_ForestsAttemptedTextString += $"{this.m_PlayerForestsAttempted[playerKeyName]}\n";
            }
            else
            {
                this.m_ForestsAttemptedTextString += "0\n";
            }

            if (this.m_PlayerForestsCompleted.ContainsKey(playerKeyName))
            {
                this.m_ForestsCompletedTextString += $"{this.m_PlayerForestsCompleted[playerKeyName]}\n";
            }
            else
            {
                this.m_ForestsCompletedTextString += "0\n";
            }
        }

        this.m_PlayerListText.text = this.m_PlayerListTextString;
        this.m_DeathsText.text = this.m_DeathsTextString;
        this.m_TotalDeathsText.text = this.m_TotalDeathsTextString;
        this.m_ForestsAttemptedText.text = this.m_ForestsAttemptedTextString;
        this.m_ForestsCompletedText.text = this.m_ForestsCompletedTextString;

        if (this.m_Replication != null)
        {
            this.m_Replication.OnReplicationDataReceived(
                this.m_PlayerListTextString,
                this.m_DeathsTextString,
                this.m_TotalDeathsTextString,
                this.m_ForestsAttemptedTextString,
                this.m_ForestsCompletedTextString
            );
        }
    }

    public void OnReplicationDataReceived(string playerListText, string deathsText, string totalDeathsText, string forestsAttemptedText, string forestsCompletedText)
    {
        if (this.m_PlayerListText == null) return;
        if (this.m_DeathsText == null) return;
        if (this.m_TotalDeathsText == null) return;

        this.m_PlayerListText.text = playerListText;
        this.m_DeathsText.text = deathsText;
        this.m_TotalDeathsText.text = totalDeathsText;
        this.m_ForestsAttemptedText.text = forestsAttemptedText;
        this.m_ForestsCompletedText.text = forestsCompletedText;
    }

    public string VRCPlayerApiObjectToUniqueNameString(VRCPlayerApi player)
    {
        if (player == null) return null;
        if (!player.IsValid()) return null;

        return $"{player.displayName}#{player.playerId.ToString()}";
    }
}
