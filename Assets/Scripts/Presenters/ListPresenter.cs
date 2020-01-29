using Controllers;
using Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ReactUnity;

namespace Presenters
{
    // have to pass the ListModel to the presenter again.. TODO
    public class ListPresenter : Presenter<ListController, ListModel>
    {
        // Simple list text for displaying our items
        public GameObject buttonPrefab;
        public GameObject modalParent;
        public GameObject testPanel;
        public GameObject uploadPanel;
        public Text noModelsWarning;
        
        protected override void Render(ListModel state)
        {
            if (state.Models?.Count > 0)
                AddButtons(state.Models);
        }

        public void ShowUploadPanel()
        {
            this.uploadPanel.SetActive(true);
            this.testPanel.SetActive(false);
        }

        public void ShowTestPanel()
        {
            this.uploadPanel.SetActive(false);
            this.testPanel.SetActive(true);
        }
        
        private void AddButtons(List<ItemModel> itemList)
        {
            var contentPanel = testPanel.GetComponentInChildren<GameObject>().GetComponentInChildren<RectTransform>();
            foreach (Transform child in contentPanel.transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < itemList.Count; i++)
            {
                var model = itemList[i];

                GameObject newButton = Instantiate(buttonPrefab);
                newButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    DownloadAndPlay(model);
                });
                newButton.GetComponentInChildren<Text>().text = model.Name;
                
                newButton.transform.SetParent(contentPanel, false);
            }
        }
        
        public void DownloadAndPlay(ItemModel model)
        {
            if (!model.BinaryExists)
            {
                /*
                Controller._staticDataService.DownloadAssetToPath(model.Binary, model.Name, (sender, args) =>
                {
                    Play(model);
                });
                */
            }
            else
            {
                Play(model);
            }
        }

        public void Play(ItemModel model)
        {
            string path = ""; // Controller._staticDataService.FullBinaryPath(model.Name);
            Controller._sceneService.LoadScene<Scenes.WebcamScene>(new WebcamModel()
            {
                BinaryPath = path,
                Labels = model.Labels,
                Title = model.Name.ToUpper()[0] + model.Name.Substring(1) + " Recognizer"
            }, LoadSceneMode.Additive);
        }

        public void ShowTrainer()
        {
            Controller._sceneService
                .LoadScene<Scenes.TrainerScene>(new TrainerModel()
                {
                    SampleCount = 0,
                    Subject = ""
                }, LoadSceneMode.Additive);
        }

        private int Counter = 0;
        public void FixedUpdate()
        {
            Counter++;
            if (Counter % 200 == 0)
                Controller.GetModels();
        }
    }
}