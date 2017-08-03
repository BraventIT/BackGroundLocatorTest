using System;
using Android.App;
using Android.Content;
using Firebase.Iid;

namespace BackgroundLocationTest.Droid
{
	[Service]
	[IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
	public class MyFirebaseInstanceIdService : FirebaseInstanceIdService
	{
		/**
		 * Called if InstanceID token is updated. This may occur if the security of
		 * the previous token had been compromised. Note that this is called when the InstanceID token
		 * is initially generated so this is where you would retrieve the token.
		 */
		public override void OnTokenRefresh()
		{
			Intent intent = new Intent(this, typeof(NotificationRegistrationService));
			StartService(intent);
		}
	}
}

