using Models;
using ReactUnity;
using ReactUnity.Services;

namespace Controllers
{
    public class WebcamController : Controller<WebcamModel>
    {
        public ISceneService _sceneService;

        WebcamController(ISceneService sceneService)
        {
            _sceneService = sceneService;
        }

        public override void Start()
        {
            var state = _sceneService.GetState<WebcamModel>();
            SetState(state);
        }

        public void ChallengeFinished(string subject)
        {
            SetState(new WebcamModel {
                GameFinished = true,
                SuccessText = string.Format("Nice work!\nYou found the {0}!", subject.ToLower())
            });
        }
    }
}
