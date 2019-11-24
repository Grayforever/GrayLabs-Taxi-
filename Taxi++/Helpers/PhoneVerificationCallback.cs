using Android.Widget;
using Firebase;
using Firebase.Auth;
using Taxi__.Activities;
using static Firebase.Auth.PhoneAuthProvider;

namespace Taxi__.Helpers
{
    class PhoneVerificationCallback : OnVerificationStateChangedCallbacks
    {
        readonly PhoneValidationActivity _instance;
        public string verificationID = "";
        public string smsCode = "";

        public PhoneVerificationCallback(PhoneValidationActivity Instance)
        {
            _instance = Instance;
        }

        public override void OnCodeSent(string verificationId, ForceResendingToken forceResendingToken)
        {
            verificationID = verificationId;
            base.OnCodeSent(verificationId, forceResendingToken);
        }

        public override void OnVerificationCompleted(PhoneAuthCredential credential)
        {
            string strCode = credential.SmsCode;

            if (strCode != null)
            {
                _instance.CodePinView.Value = strCode;
                _instance.VerificationCode(verificationID, strCode);
                _instance.ShowProgressDialog();
            }
        }

        public override void OnVerificationFailed(FirebaseException exception)
        {
            _instance.CloseProgressDialog();
            Toast.MakeText(_instance.ApplicationContext, exception.Message, ToastLength.Long).Show();
        }

    }
}