using Models;
using ReactUnity;
using ReactUnity.Services;
using UnityEngine;

namespace Controllers
{
    public class SplashController : Controller<SplashModel>
    {
        private ISceneService _sceneService;
        private IStaticDataService _staticDataService;
        private IFirebaseService _firebaseService;

        public SplashController(ISceneService sceneService, IFirebaseService firebaseService, IStaticDataService staticDataService)
        {
            _sceneService = sceneService;
            _firebaseService = firebaseService;
            _staticDataService = staticDataService;
        }

        public override void Start()
        {
            _firebaseService.Initialize(AuthStateChanged);
        }

        public void SignIn(string email, string password)
        {
            _firebaseService.SignIn(email, password);
        }

        void AuthStateChanged(object sender, System.EventArgs eventArgs)
        {
            if (_firebaseService.User != null)
            {
                _sceneService.LoadScene<Scenes.ListScene>(new SplashModel() { IsReady = true });
            }
        }
    }
}