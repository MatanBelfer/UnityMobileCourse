using System;
using System.IO;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using Random = UnityEngine.Random;

public class AnalyticsManager : BaseManager
{
    private bool hasPermission;
    private bool canAskPermission;
    private string playerPrefs_hasPermission = "analyticsPermission";
    private string playerPrefs_askPermission = "analyticsAskPermission";
    private string permissionPath;

    /// <summary>
    /// Reads permission from playerprefs and starts/stops data collection accordingly. When permission is denied, requests data deletion. 
    /// </summary>
    public void UpdatePermission()
    {
        ReadPermission();
        if (hasPermission)
        {
            AnalyticsService.Instance.StartDataCollection();
        }
        else
        {
            AnalyticsService.Instance.StopDataCollection();
            AnalyticsService.Instance.RequestDataDeletion();
        }
    }
    
    protected override void OnInitialize()
    {
        permissionPath = Application.persistentDataPath + "/permission.json";
        ReadPermission();
        print($"Permission set to {hasPermission}");
        
        //Subscribe to events to log them
        ManagersLoader.Game.OnHitBySpike += () => RecordEvent("playerLostToSpike");
        ManagersLoader.Game.OnPinFellOffScreen += () => RecordEvent("playerLostToScroll");

        InitializeServicesAndStartCollection();
    }

    private void RecordEvent(string eventName)
    {
        if (hasPermission)
        {
            AnalyticsService.Instance.RecordEvent(eventName);
            print($"Recorded {eventName}");
        }
        else
        {
            print($"Couldn't record {eventName} because I don't have permission");
        }
    }

    private async Task InitializeServicesAndStartCollection()
    {
        await UnityServices.InitializeAsync();
        UpdatePermission();
    }

    protected override void OnReset()
    {
        //
    }

    protected override void OnCleanup()
    {
        SavePermission();
    }

    /// <summary>
    /// Reads permission from json and sets to hasPermission
    /// </summary>
    private void ReadPermission()
    {
        bool haskey = PlayerPrefs.HasKey(playerPrefs_hasPermission);
        if (haskey)
        {
            hasPermission = PlayerPrefs.GetInt(playerPrefs_hasPermission) == 1;
        }
        else
        {
            hasPermission = false;
            PlayerPrefs.SetInt(playerPrefs_hasPermission, 0);
        }

        if (PlayerPrefs.HasKey(playerPrefs_askPermission))
        {
            canAskPermission = PlayerPrefs.GetInt(playerPrefs_askPermission) == 1;
        }
        else
        {
            canAskPermission = true;
            PlayerPrefs.SetInt(playerPrefs_askPermission, 1);
        }

        // print($"permission read as {hasPermission}");
    }

    /// <summary>
    /// Test
    /// </summary>
    public void Update()
    {
        if (Random.value < 0.05f)
        {
            ReadPermission();
        }
    }

    private void SavePermission()
    {
        PlayerPrefs.SetInt(playerPrefs_hasPermission, hasPermission ? 1 : 0);
        PlayerPrefs.SetInt(playerPrefs_askPermission, canAskPermission ? 1 : 0);
    }

    private void AskForPermission()
    {
        if (!canAskPermission) return;
        ManagersLoader.UI.AnalyticsPermissionPopup();
        canAskPermission = false;
        SavePermission();
    }
}
