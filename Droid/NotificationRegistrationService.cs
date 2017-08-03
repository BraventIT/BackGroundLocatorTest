using System;
using Android.App;
using Android.Util;
using Firebase.Iid;
using WindowsAzure.Messaging;
using Android.Content;
using Android.Preferences;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BackgroundLocationTest.Droid
{
	[Service(Exported = false)]
	public class NotificationRegistrationService : IntentService
	{
		static object locker = new object();

		public NotificationRegistrationService() : base("RegistrationIntentService") { }

		protected override void OnHandleIntent(Intent intent)
		{
			var sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
			try
			{
				Log.Info("RegistrationIntentService", "Calling InstanceID.GetToken");
				lock (locker)
				{
					var FCMtoken = FirebaseInstanceId.Instance.Token;

					if ((sharedPreferences.GetString("FCMtoken", "")) != FCMtoken)
					{
						sharedPreferences.Edit().PutString("FCMtoken", FCMtoken).Apply();
					}
					String json = sharedPreferences.GetString("HUDChannels", "[]");
					List<string> channels = JsonConvert.DeserializeObject<List<string>>(json);
					if (channels.Count > 0)
					{
						try
						{
							NotificationHub hub = new NotificationHub(
								"HUBNAME",
								"CONNECTION STRING",
							 	Application.Context);
							//hub.UnregisterAll(token);
							String registrationId = hub.Register(FCMtoken, channels.ToArray()).RegistrationId;
							sharedPreferences.Edit().PutString("HUDRegistrationID", registrationId).Apply();
						}
						catch (Exception ex)
						{
							var msg = ex.Message;
						}

					}
				}
			}
			catch (Exception e)
			{
				Log.Debug("RegistrationIntentService", "Failed to get a registration token");
				return;
			}
		}
	}
}