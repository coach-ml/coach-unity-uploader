using System.Collections.Generic;
using ReactUnity;
using UnityEngine;
using ReactUnity.Services;
using Newtonsoft.Json;
using UnityEngine.UI;

class UploadPresenter : Presenter<UploadController, UploadModel>
{
    public RectTransform Container;
    public GameObject ModelStatusPrefab;
    public Text PreviewText;

    protected override void Render(UploadModel viewModel)
    {
        PreviewText.enabled = viewModel.models.Count == 0;

        foreach (Transform child in Container.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var model in viewModel.models)
        {
            // Instantiate a loader
            GameObject entry = Instantiate(ModelStatusPrefab);
            if (model.maxSamples > 0)
                entry.GetComponentInChildren<Text>().text = $"{model.modelName}\n{model.sampleUploadProgress}/{model.maxSamples}";
            entry.transform.SetParent(Container, false);
        }
    }
}

class UploadController : Controller<UploadModel>
{
    IFirebaseService _firebaseService;
    public UploadController(IFirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public override void Start()
    {
        _firebaseService.WatchModels((sender, args) =>
        {
            if (args.DatabaseError == null)
            {
                var models = new List<UploadStruct>();
                var js = JsonConvert.DeserializeObject<Dictionary<string, UploadStruct>>(args.Snapshot.GetRawJsonValue());
                foreach (var key in js.Keys)
                {
                    models.Add(new UploadStruct()
                    {
                        maxSamples = js[key].maxSamples,
                        sampleUploadProgress = js[key].sampleUploadProgress,
                        modelName = js[key].modelName
                    });
                }

                SetState(new UploadModel() { models = models });
            }
        });
    }
}

class UploadModel : IModel
{
    public List<UploadStruct> models;
}