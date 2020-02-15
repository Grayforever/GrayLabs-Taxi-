using Android.Widget;
using Firebase;
using Firebase.Auth;
using Taxi__.Activities;
using static Firebase.Auth.PhoneAuthProvider;

namespace Taxi__.Helpers
{
    public class PhoneVerificationCallback : OnVerificationStateChangedCallbacks
    {
        readonly PhoneValidationActivity _instance;
        public string smsCode = "";

        public PhoneVerificationCallback(PhoneValidationActivity Instance)
        {
            _instance = Instance;
        }

        public override void OnCodeSent(string verificationId, ForceResendingToken forceResendingToken)
        {
            //base.OnCodeSent(verificationId, forceResendingToken);
            
            if (!string.IsNullOrWhiteSpace(_instance.VerificationID))
            {
                _instance.VerificationID = "";
                _instance.VerificationID = verificationId;
            }
            else
            {
                _instance.VerificationID = verificationId;
            }
        }

        public override void OnVerificationCompleted(PhoneAuthCredential credential)
        {
            string strCode = credential.SmsCode;

            if (strCode != null)
            {
                _instance.CodePinView.Value = strCode;
                _instance.VerifyCode(strCode);
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