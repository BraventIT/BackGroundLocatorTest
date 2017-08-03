using System;
using Android.App;
using Android.Runtime;
using Android.Content;
using System.Collections.Generic;
using Android.OS;

namespace BackgroundLocationTest.Droid
{
    [Application]
    public class DroidApplication : Application, Application.IActivityLifecycleCallbacks
    {

        public Activity current;

        public DroidApplication(IntPtr javaReference, JniHandleOwnership transfer) 
            : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            RegisterGCM();
            RegisterActivityLifecycleCallbacks(this);
        }

        public override void OnTerminate()
        {
            base.OnTerminate();

            UnregisterActivityLifecycleCallbacks(this);
        }


        public Boolean isAppForground(Context mContext)
        {

            ActivityManager am = (ActivityManager)mContext.GetSystemService(Context.ActivityService);
            IList<ActivityManager.RunningTaskInfo> tasks = am.GetRunningTasks(1);
            if (tasks.Count > 0)
            {
                ComponentName topActivity = tasks[0].TopActivity;
                if (!topActivity.PackageName.Equals(mContext.PackageName))
                {
                    return false;
                }
            }

            return true;
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            current = activity;
        }

        public void OnActivityDestroyed(Activity activity)
        {

        }

        public void OnActivityPaused(Activity activity)
        {

        }

        public void OnActivityResumed(Activity activity)
        {
            current = activity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {

        }

        public void OnActivityStarted(Activity activity)
        {
            current = activity;
        }

        public void OnActivityStopped(Activity activity)
        {

        }

        private void RegisterGCM()
        {
            var intent = new Intent(this, typeof(NotificationRegistrationService));
            StartService(intent);
        }
    }
}