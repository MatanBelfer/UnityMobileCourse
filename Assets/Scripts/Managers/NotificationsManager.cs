using System;
using UnityEngine;
using UnityEngine.Android;

/// <summary>
/// Handles notifications
/// </summary>
public class NotificationsManager : BaseManager
{
    private const string androidPermission = "android.permission.POST_NOTIFICATIONS";
    
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
    }

    private void EnsureNotificationsPermission()
    {
        if (!Permission.HasUserAuthorizedPermission(androidPermission))
        {
            Permission.RequestUserPermission(androidPermission);
        }
    }
}
