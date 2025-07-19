using System;
using System.IO;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class AnalyticsManager : BaseManager
{
    private PermissionSettings permissionSettings;
    private string permissionPath;

    /// <summary>
    /// This should be called by the permission popup and settings menu 
    /// </summary>
    /// <param name="permission"></param>
    public void SetPermission(bool permission)
    {
        if (permission)
        {
            AnalyticsService.Instance.StartDataCollection();
        }
        else
        {
            AnalyticsService.Instance.StopDataCollection();
        }
    }

    /// <summary>
    /// This should be called by the permission popup and settings menu 
    /// </summary>
    public void RequestDataDeletion()
    {
        SetPermission(false);
        AnalyticsService.Instance.RequestDataDeletion();
    }
    
    protected override void OnInitialize()
    {
        permissionPath = Application.persistentDataPath + "/permission.json";
        ReadPermission();
        print($"Permission set to {permissionSettings.hasPermission}");
        
        //Subscribe to events to log them
        ManagersLoader.Game.OnHitBySpike += () => RecordEvent("playerLostToSpike");
        ManagersLoader.Game.OnPinFellOffScreen += () => RecordEvent("playerLostToScroll");

        InitializeServicesAndStartCollection();
    }

    private void RecordEvent(string eventName)
    {
        if (permissionSettings.hasPermission)
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
        if (permissionSettings.hasPermission)
        {
            AnalyticsService.Instance.StartDataCollection();
        }
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
        string file;
        try
        {
            file = File.ReadAllText(permissionPath);
            permissionSettings = JsonUtility.FromJson<PermissionSettings>(file);
        }
        catch (FileNotFoundException)
        {
            permissionSettings = new PermissionSettings();
            permissionSettings.hasPermission = false;
            permissionSettings.askPermission = true;
        }
        catch (Exception)
        {
            Debug.LogWarning("Unexpected exception when trying to read file");
            throw;       
        }
    }

    private void SavePermission()
    {
        string json = JsonUtility.ToJson(permissionSettings);
        File.WriteAllText(permissionPath, json);
    }

    private void AskForPermission()
    {
        ManagersLoader.UI.AnalyticsPermissionPopup();
    }

    class PermissionSettings
    {
        public bool hasPermission;
        public bool askPermission;
    }
}
