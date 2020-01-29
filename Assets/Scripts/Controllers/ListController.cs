using Models;
using System;
using ReactUnity;
using ReactUnity.Services;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Controllers
{
    public class ListController : Controller<ListModel>
    {
        public IStaticDataService _staticDataService;
        public ISceneService _sceneService;
        public IS3Service _s3Service;
        public IFirebaseService _firebaseService;

        ListController(IStaticDataService staticDataService, ISceneService sceneService, IS3Service s3Service, IFirebaseService firebaseService)
        {
            _staticDataService = staticDataService;
            _sceneService = sceneService;
            _s3Service = s3Service;
            _firebaseService = firebaseService;
        }

        public override async void Start()
        {
            var userDetails = await _firebaseService.GetUserDetails();
            _s3Service.Initialize(userDetails);
            _s3Service.WatchRoot();

            ListModel existingModel = _sceneService.GetState<ListModel>();
            if (existingModel.Models != null)
                SetState(existingModel);
            else
            {
                GetModels();
            }
        }

        public void GetModels()
        {
            // Note, this gets called whenever our presenter loads, this should be kept somewhere so we don't need to get it again
            /*_staticDataService.GetModelList((sender, evt) =>
            {
                var model = new ListModel();
                JsonUtility.FromJsonOverwrite(evt.Result, model);

                // Set the state once we've got our data, this calls refresh in our presenter
                SetState(model);
            });*/

            SetState(new ListModel() { Models = new List<ItemModel>() });
        }
    }
}
