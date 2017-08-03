using System;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Firebase.Messaging;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using System.Collections.Generic;

namespace BackgroundLocationTest.Droid
{
    [Service(Exported = false), IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
	public class NotificationListenerService : FirebaseMessagingService, Android.Locations.ILocationListener
    {
        LocationManager locationManager;
        const int LOCATION_REQUEST_TIMEOUT_MILLIS = 60000;
        const int LOCATION_REQUEST_INTERVAL = 4000;

        bool LocationRequested;

        Handler Handler;
        Looper Looper;
        public NotificationListenerService()
        {
            Handler = new Handler();
            Looper = Handler.Looper;
        }

        void SendLocation(Location location)
        {
            LocationRequested = false;
            var title = "Localition request";
            var message = $"Current location: {location.Longitude} - {location.Latitude}";
            ShowAlert(title, message);
        }

        void RequesLocation()
        {
            LocationRequested = true;
            locationManager = (LocationManager)GetSystemService(Context.LocationService);
            locationManager.RequestLocationUpdates(LocationManager.NetworkProvider, LOCATION_REQUEST_INTERVAL, 0, this, Looper);
            locationManager.RequestLocationUpdates(LocationManager.GpsProvider, LOCATION_REQUEST_INTERVAL, 0, this, Looper);
        }

        string GetMessageFromRemote(RemoteMessage message)
        {
            string notificationMessage = "";
            if (message.GetNotification() != null)
            {
                var noti = message.GetNotification();
                notificationMessage = noti.Body;
            }
            else
            {
                if (message.Data != null)
                {
                    message.Data.TryGetValue("message", out notificationMessage);
                }
            }
            return notificationMessage;
        }

        string GetTitleFromRemote(RemoteMessage message)
        {
            string notificationTitle = "";
            if (message.GetNotification() != null)
            {
                var noti = message.GetNotification();
                notificationTitle = noti.Title;
            }
            else
            {
                if (message.Data != null)
                {
                    message.Data.TryGetValue("title", out notificationTitle);
                }
            }
            return notificationTitle;
        }

        void ShowAlert(string title, string message)
        {
            try
            {
                if (((DroidApplication)this.ApplicationContext).isAppForground(this))
                {
                    ((DroidApplication)this.ApplicationContext)?.current?.RunOnUiThread(() =>
                    {
                        AlertDialog.Builder alertbuilder;
                        Dialog dialog = null;
                        alertbuilder = new AlertDialog.Builder(((DroidApplication)this.ApplicationContext).current);
                        alertbuilder.SetTitle(title);
                        alertbuilder.SetMessage(message);
                        alertbuilder.SetCancelable(false);
                        alertbuilder.SetPositiveButton("OK", delegate { });
                        dialog = alertbuilder.Create();
                        dialog.Show();
                    });
                }
                else
                {
                    ShowPushNotification(this, message, title);
                }
            }
            catch (Exception ex)
            {
                ShowPushNotification(this, message, title);
            }

        }

        RemoteMessage LastRemoteMessage;

        public override void HandleIntent(Intent p0)
        {
            base.HandleIntent(p0);
			RequesLocation();
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            LastRemoteMessage = message;
            RequesLocation();
        }

        void ShowPushNotification(Context context, string notificationMessage, string notificationTitle)
        {
            try
            {
                Random random = new Random();
                int contentIntentRequestCode = random.Next();
                Intent activityIntent = this.PackageManager.GetLaunchIntentForPackage("net.bravent.backgroundlocationtest");
                activityIntent.PutExtra("push", notificationMessage);
                activityIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                PendingIntent pContentIntent = PendingIntent.GetActivity(context, contentIntentRequestCode, activityIntent, PendingIntentFlags.UpdateCurrent);

                NotificationCompat.Builder builder = new NotificationCompat.Builder(context)
                    .SetContentTitle(new Java.Lang.String(notificationTitle))
                    .SetContentText(new Java.Lang.String(notificationMessage))
                    .SetTicker(new Java.Lang.String(notificationMessage))
                    .SetLargeIcon(BitmapFactory.DecodeResource(context.Resources, Resource.Mipmap.ic_launcher))
                    .SetSmallIcon(Resource.Drawable.ic_notification_test)
                    .SetContentIntent(pContentIntent)
                    .SetAutoCancel(true)
                    .SetDefaults(Android.Support.V4.App.NotificationCompat.DefaultAll);

                Notification notification = builder.Build();
                NotificationManager manager = context.GetSystemService(Context.NotificationService) as NotificationManager;
                int notificationId = (int)DateTime.UtcNow.Ticks;
                manager.Notify(notificationId, notification);
            }
            catch (Exception ex)
            {

            }
        }

		#region  Android.Locations.ILocationListener implementation
		public void OnLocationChanged(Location location)
		{
			SendLocation(location);
			locationManager?.RemoveUpdates(this);
		}

		public void OnProviderDisabled(string provider)
		{
            System.Diagnostics.Debug.WriteLine($"PROVIDER DISABLED {provider}");
		}

		public void OnProviderEnabled(string provider)
		{
			System.Diagnostics.Debug.WriteLine($"PROVIDER ENABLED {provider}");
		}

		public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
		{
            if (status != Availability.Available)
                locationManager?.RemoveUpdates(this);
		}
		#endregion
	}
}