using ChainAbstractions;
using UnityEngine;
using UnityEngine.UI;

namespace StacksForce
{
    public class NftSpriteProvider : MonoBehaviour
    {
        private string _currentNftUrl;
        private string _lastRequestNftUrl;

        public INFT NFT { get; set; }

        private Image _image;
        private SpriteRenderer _spriteRenderer;

        public Sprite Sprite { get; private set; }

        private void Awake()
        {
            _image = GetComponent<Image>();
            _spriteRenderer = GetComponent<SpriteRenderer>(); 
        }

        private void Update()
        {
            UpdateNftImage();
        }

        private async void UpdateNftImage()
        {
            _currentNftUrl = NFT != null ? NFT.ImageUrl : null;

            if (!string.IsNullOrEmpty(_currentNftUrl))
            {
                if (_lastRequestNftUrl == _currentNftUrl)
                    return;

                _lastRequestNftUrl = _currentNftUrl;
                Sprite = await NftMeta.GetImage(_lastRequestNftUrl);

                if (_lastRequestNftUrl == _currentNftUrl)
                {
                    if (_image)
                    {
                        _image.sprite = Sprite;
                        _image.enabled = Sprite != null;
                    }
                    if (_spriteRenderer)
                    {
                        _spriteRenderer.sprite = Sprite;
                        _spriteRenderer.enabled = Sprite != null;
                    }
                }
            }
            else
            {
                if (_image)
                    _image.enabled = false;

                if (_spriteRenderer)
                    _spriteRenderer.enabled = false;

                Sprite = null;
            }
        }
    }
}