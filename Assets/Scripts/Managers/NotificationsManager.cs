using System;
using System.IO;
using System.Text;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.Android;

/// <summary>
/// Handles notifications
/// </summary>
public class NotificationsManager : BaseManager
{
    [Header("Settings")] 
    [SerializeField] private string notificationTitle = "Rubber Climber";
    [SerializeField] private string notificationDescription = "Boing Boing...?";
    private TimeSpan bumpNotificationDelay = TimeSpan.FromHours(24); //should be 24 hours
    
    private const string _androidPermission = "android.permission.POST_NOTIFICATIONS";
    private const string _channelId = "default";
    private const string _channelName = "Default";
    private const string _futureNotificationPath = "/futureNotification.json";
    
    protected override void OnInitialize()
    {
        //
    }

    protected override void OnReset()
    {
        //
    }

    protected override void OnCleanup()
    {
        //
    }

    private void Start()
    {
        EnsureNotificationsPermission();
        EnsureNotificationChannel();
        CancelFutureNotification();
    }

    private void OnApplicationQuit()
    {
        SceduleNotification();
    }

    private void EnsureNotificationsPermission()
    {
        if (!Permission.HasUserAuthorizedPermission(_androidPermission))
        {
            Permission.RequestUserPermission(_androidPermission);
        }
    }

    private void EnsureNotificationChannel()
    {
        var channel = AndroidNotificationCenter.GetNotificationChannel(_channelId);
        if (channel.Id == null)
        {
            channel = new AndroidNotificationChannel(
                    _channelId,
                    _channelName,
                    "All Notifications",
                    Importance.Default
            );
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }
    }

    private void CancelFutureNotification()
    {
        int futureNotificationID = GetFutureNotificationID();
        if (futureNotificationID == -1) return;
        
        NotificationStatus status = AndroidNotificationCenter.CheckScheduledNotificationStatus(futureNotificationID);
        if (status <= 0) //Unavailable or unknown
        {
            Debug.LogWarning($"Unexpected notification status {status}");
        } 
        else if (status == NotificationStatus.Scheduled)
        {
            AndroidNotificationCenter.CancelScheduledNotification(futureNotificationID);
        }
    }

    private int GetFutureNotificationID()
    {
        string json;
        try
        {
            json =
                File.ReadAllText(Application.persistentDataPath + _futureNotificationPath, Encoding.UTF8);
        }
        catch (FileNotFoundException)
        {
            Debug.LogWarning($"File {_futureNotificationPath} not found");
            return -1;
        }
        catch (Exception)
        {
            Debug.LogWarning("Unexpected exception when trying to read file");
            throw;
        }
        JsonID idStruct = JsonUtility.FromJson<JsonID>(json);
        return idStruct.futureNotificationID;
    }

    private void SaveIDToJson(int ID)
    {
        string json = JsonUtility.ToJson(new JsonID(ID));
        string path = Application.persistentDataPath + _futureNotificationPath;
        System.IO.File.WriteAllText(path, json);
    }

    private class JsonID
    {
        public int futureNotificationID = -1;
        
        public JsonID(int ID)
        {
            futureNotificationID = ID;
        }
    }
    private void SceduleNotification()
    {
        var notification = new AndroidNotification(
            title: notificationTitle,
            text: notificationDescription,
            DateTime.Now + bumpNotificationDelay);

        int futureNotificationID = AndroidNotificationCenter.SendNotification(notification, _channelId);
        SaveIDToJson(futureNotificationID);
    }
}
