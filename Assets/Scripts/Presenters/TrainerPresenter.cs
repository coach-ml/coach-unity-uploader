using Controllers;
using Models;
using UnityEngine;
using System;
using Utilities;
using UnityEngine.UI;
using ReactUnity;
using System.Net;

namespace Presenters
{
    class TrainerPresenter : Presenter<TrainerController, TrainerModel>
    {
        public Webcam Webcam;

        public InputField NameField;
        public Text ScanHeading;

        public Text Dismiss;

        private Outline Outline;
        public GameObject Tutorial;

        private bool shutterDown { get; set; }

        protected override void Render(TrainerModel state)
        {
            if (state.Subject != "")
            {
                ScanHeading.enabled = true;
                ScanHeading.text = $"{state.Subject}: {state.SampleCount}";
            }
        }

        private void Awake()
        {
            NameField.onValueChanged.AddListener((s) =>
            {
                Dismiss.gameObject.SetActive(s != "");
            });
            ScanHeading.enabled = false;
        }

        public void ShutterUp()
        {
            shutterDown = false;
        }

        public void ShutterDown()
        {
            shutterDown = true;
        }

        public void Update()
        {
            if (shutterDown)
                TakePhoto();
        }

        public void TakePhoto()
        {
            if (NameField.text != "")
            {
                var photo = Webcam.GetPhoto();
                byte[] bytes = photo.EncodeToJPG();
                UnityEngine.Object.Destroy(photo);

                Controller.TakePhoto(bytes);
                // await Controller._staticDataService.UploadImage(Controller.WebClient, NameField.text, bytes);

            }
        }

        public void GoBack()
        {
            Controller.FinishedCapture();
        }

        protected override void OnPresenterDestroy()
        {
            Webcam.Stop();
        }

        public void HideTutorial()
        {
            if (NameField.text == "")
                Outline.effectColor = new Color(255, 0, 0, 255);
            else
            {
                Tutorial.SetActive(false);
                Controller.SetSubject(NameField.text);
            }
        }
    }
}
