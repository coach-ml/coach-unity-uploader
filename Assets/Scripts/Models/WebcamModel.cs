using ReactUnity;

namespace Models
{
    public class WebcamModel : IModel
    {
        public string BinaryPath { get; set; }
        public string[] Labels { get; set; }
        public string Title { get; set; }

        public string SuccessText { get; set; }
        public bool GameFinished { get; set; }
    }
}