using ReactUnity;
using Controllers;
using Models;
using UnityEngine;
using UnityEngine.UI;

namespace Presenters
{
    class ItemPresenter : Presenter<ItemController, ItemModel>
    {
        public Text ModelName;
        public Image ModelPreview;
        public Button DownloadPlay;

        protected override void Render(ItemModel state)
        {
            if (state != null)
            {
                ModelName.text = state.Name;

                if (state.PreviewTexture != null)
                {
                    Sprite sprite = Sprite.Create(
                        state.PreviewTexture,
                        new Rect(0, 0, state.PreviewTexture.width, state.PreviewTexture.height),
                        new Vector2()
                    );

                    ModelPreview.sprite = sprite;
                    ModelPreview.transform.Rotate(Vector3.back, 90f);
                }
                DownloadPlay.GetComponentInChildren<Text>().text = state.BinaryExists ? "Play" : "Download & Play";

                // Important to call this before adding a listener otherwise we can have multiple listeners on 1 button
                DownloadPlay.onClick.RemoveAllListeners();
                DownloadPlay.onClick.AddListener(() =>
                {
                    Controller.DownloadAndPlay(() => Close());
                });
            }
        }

        public void Close()
        {
            Destroy(this.gameObject);
        }
    }
}