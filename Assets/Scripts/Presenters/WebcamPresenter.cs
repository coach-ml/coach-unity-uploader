using Utilities;
using Controllers;
using Models;
using Presenters;
using UnityEngine;
using UnityEngine.UI;
using ReactUnity;
using System.Collections.Generic;

public class WebcamPresenter : Presenter<WebcamController, WebcamModel>
{
    public Webcam Webcam;
    public AudioSource FinishedSound;
    public GameObject FinsihedPanel;
    public Text CurrentItem;

    private string Subject { get; set; }

    private void Awake()
    {
    }

    private bool GameFinished = false;
    protected override void Render(WebcamModel model)
    {
        
    }
    
    private void Update()
    {
        if (!GameFinished)
            TakePhoto();
    }

    public void GoBack()
    {
        Controller._sceneService.GoBack();
    }

    public void TakePhoto()
    {

    }

    private void ChallengeFinished(string subject)
    {
        FinishedSound.Play();
        Webcam.Pause();

        Controller.ChallengeFinished(subject);
    }

    protected override void OnPresenterDestroy()
    {
        Webcam.Stop();
    }
}
