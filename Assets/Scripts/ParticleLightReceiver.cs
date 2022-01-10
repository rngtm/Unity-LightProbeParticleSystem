using System;
using UnityEngine.Rendering;

namespace ParticleLighting
{
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// SHLatticeを利用して、ライト情報をマテリアルへ渡す
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    [ExecuteAlways]
    public class ParticleLightReceiver : MonoBehaviour
    {
        [SerializeField] private SHLattice shLattice;
        [SerializeField] private bool updateEveryFrame = false;

        [Header("=== Cache ===")]
        [SerializeField] private Texture3D shTexture3D;
        [SerializeField] private Color32[] textureColors;
        private ParticleSystem _particleSystem;
        private ParticleSystemRenderer _particleSystemRenderer;
        private MaterialPropertyBlock _materialPropertyBlock;
        private bool _isInitialize = false;

        private void Awake()
        {
            ApplySH();
        }

        private void Start()
        {
            ApplySH();
        }

        private void OnValidate()
        {
            ApplySH();
        }

        private void Update()
        {
            if (updateEveryFrame)
            {
                ApplySH();
            }
        }

        private void OnDestroy()
        {
            DestroyTexture3D(shTexture3D);
            shTexture3D = null;
        }

        private void Setup()
        {
            if (_isInitialize) return;

            if (shLattice == null) return;

            if (_particleSystem == null)
                _particleSystem = GetComponent<ParticleSystem>();
            if (_particleSystemRenderer == null)
                _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
            if (_materialPropertyBlock == null)
                _materialPropertyBlock = new MaterialPropertyBlock();

            _isInitialize = true;
        }

        private void TryCreateTexture(Vector3Int size)
        {
            if (shTexture3D == null)
            {
                shTexture3D = new Texture3D(size.x, size.y, size.z, TextureFormat.RGB24, false);
            }
            else
            {
                if (shTexture3D.width != size.x || shTexture3D.height != size.y || shTexture3D.depth != size.z)
                {
                    DestroyTexture3D(shTexture3D);
                    shTexture3D = new Texture3D(size.x, size.y, size.z, TextureFormat.RGB24, false);
                }
            }
        }

        private void DestroyTexture3D(Texture3D texture3D)
        {
            if (texture3D == null) return;
            
            if (Application.isPlaying)
            {
                Texture3D.Destroy(texture3D);
            }
            else
            {
                Texture3D.DestroyImmediate(texture3D);
            }
        }

        private void ApplySH()
        {
            Setup();
            if (shLattice == null) return;
            if (_particleSystem == null) return;
            if (_particleSystemRenderer == null) return;
            if (_materialPropertyBlock == null) return;
            
            TryCreateTexture(shLattice.Grid);
            if (shTexture3D == null) return;

            SetTextureColor();
            _materialPropertyBlock.SetTexture(ShaderProperty._ShTex, shTexture3D);
            _materialPropertyBlock.SetVector(ShaderProperty._MinPosition, shLattice.MinPosition);
            _materialPropertyBlock.SetVector(ShaderProperty._MaxPosition, shLattice.MaxPosition);
            _particleSystemRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        /// <summary>
        /// テクスチャへ色反映
        /// </summary>
        private void SetTextureColor()
        {
            Vector3Int grid = shLattice.Grid;
            int count = grid.x * grid.y * grid.z;
            if (textureColors == null || textureColors.Length != count)
            {
                textureColors = new Color32[count];
            }

            for (int i = 0; i < count; i++)
            {
                var sh = shLattice.ShTable[i];
                var r = sh[rgb: 0, coefficient: 0];
                var g = sh[rgb: 1, coefficient: 0];
                var b = sh[rgb: 2, coefficient: 0];
                textureColors[i] = new Color(r, g, b);
            }

            shTexture3D.SetPixels32(textureColors);
            shTexture3D.Apply();
        }
    }
}