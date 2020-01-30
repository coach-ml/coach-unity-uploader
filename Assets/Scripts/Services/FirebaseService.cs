using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using Newtonsoft.Json;
using UnityEngine;

namespace ReactUnity.Services
{
    public interface IFirebaseService : IService
    {
        FirebaseUser User { get; set; }
        void Initialize(EventHandler authStateChanged);
        Task<FirebaseUser> SignIn(string email, string password);
        void NewModel(string modelName);
        Task UpdateModel(string modelName, int maxSamples, int sampleUploadProgress);
        Task UpdateModel(string modelName);
        // void UpdateModel(string modelName, int maxSamples, int sampleUploadProgress = 0);
        void Logout();

        Task<CoachUser> GetUserDetails();
        void WatchModels(EventHandler<ValueChangedEventArgs> onChange);
        Task<Dictionary<string, ModelStruct>> GetModels();
    }
    public class FirebaseService : IFirebaseService
    {
        private FirebaseAuth Auth;
        public FirebaseUser User { get; set; }
        private DatabaseReference BaseRef;

        private event EventHandler AuthStateChanged;

        public void Initialize(EventHandler authStateChanged)
        {
            AuthStateChanged = authStateChanged;

            FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://coachuploader-ac4d7.firebaseio.com/");
            BaseRef = FirebaseDatabase.DefaultInstance.RootReference;

            Auth = FirebaseAuth.DefaultInstance;
            Auth.StateChanged += _AuthStateChanged;
            _AuthStateChanged(this, null);
        }

        void _AuthStateChanged(object sender, EventArgs eventArgs)
        {
            if (Auth.CurrentUser != User)
            {
                bool signedIn = User != Auth.CurrentUser && Auth.CurrentUser != null;
                if (!signedIn && User != null)
                {
                    Debug.Log("Signed out " + User.UserId);
                }
                User = Auth.CurrentUser;
                if (signedIn)
                {
                    AuthStateChanged(sender, eventArgs);
                }
            }
        }

        public async Task<CoachUser> GetUserDetails()
        {
            CoachUser result = null;
            try
            {
                var snapshot = await BaseRef.Child(User.UserId).Child("user").GetValueAsync();
                result = JsonUtility.FromJson<CoachUser>(snapshot.GetRawJsonValue());
            }
            catch (Exception ex)
            {
                // Handle the error...
            }
            return result;
        }

        public System.Threading.Tasks.Task<FirebaseUser> SignIn(string email, string password)
        {
            return Auth.SignInWithEmailAndPasswordAsync(email, password);
        }

        public void NewModel(string modelName)
        {
            var json = JsonUtility.ToJson(new ModelStruct()
            {
                modelName = modelName
            });
            var reff = BaseRef.Child(User.UserId).Child("models").Child(modelName);
            reff.SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task.Exception.Message);
                }
            });

        }

        public async Task UpdateModel(string modelName, int maxSamples, int sampleUploadProgress)
        {
            var json = JsonUtility.ToJson(new ModelStruct() {
                modelName = modelName,
                maxSamples =  maxSamples,
                sampleUploadProgress = sampleUploadProgress
            });
            BaseRef.Child(User.UserId).Child("models").Child(modelName).SetRawJsonValueAsync(json);
        }

        public async Task UpdateModel(string modelName)
        {
            var currentModel = await GetModel(modelName);

            var json = JsonUtility.ToJson(new ModelStruct()
            {
                modelName = currentModel.modelName,
                maxSamples = currentModel.maxSamples,
                sampleUploadProgress = currentModel.sampleUploadProgress + 1
            });
            BaseRef.Child(User.UserId).Child("models").Child(modelName).SetRawJsonValueAsync(json);
        }

        public void WatchModels(EventHandler<ValueChangedEventArgs> onChange)
        {
            BaseRef.Child(User.UserId).Child("models").ValueChanged += onChange;
        }

        public async Task<ModelStruct> GetModel(string modelName)
        {
            ModelStruct result = null;
            try
            {
                var snapshot = await BaseRef.Child(User.UserId).Child("models").Child(modelName).GetValueAsync();
                result = JsonConvert.DeserializeObject<ModelStruct>(snapshot.GetRawJsonValue());
            }
            catch (Exception ex)
            {
                // Handle the error...
            }
            return result;
        }

        public async Task<Dictionary<string, ModelStruct>> GetModels()
        {
            Dictionary<string, ModelStruct> result = null;
            try
            {
                var snapshot = await BaseRef.Child(User.UserId).Child("models").GetValueAsync();
                result = JsonConvert.DeserializeObject<Dictionary<string, ModelStruct>>(snapshot.GetRawJsonValue());
            }
            catch (Exception ex)
            {
                // Handle the error...
            }
            return result;
        }

        public void Logout()
        {
            Auth.SignOut();
        }
    }

    public class ModelParent
    {
        public string modelName;
        public ModelStruct modelInfo;
    }

    public class ModelStruct
    {
        public string modelName;
        public int maxSamples;
        public int sampleUploadProgress;
    }

    public class CoachUser
    {
        public string s3Key;
        public string coachApi;
        public string s3Secret;
        public string bucket;
        public string clientName;
    }

}