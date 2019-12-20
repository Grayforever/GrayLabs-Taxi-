using Android.Gms.Tasks;
using System;

namespace Taxi__.EventListeners
{
    public class TaskCompletionStatusListener : Java.Lang.Object, IOnSuccessListener, IOnFailureListener
    {
        public event EventHandler Success;
        public event EventHandler Failure;

        public void OnFailure(Java.Lang.Exception e)
        {
            Failure?.Invoke(this, new EventArgs());
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            Success?.Invoke(this, new EventArgs());
        }
    }
}