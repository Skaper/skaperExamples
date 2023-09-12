using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ExampleApp.Core.Behaviours.ConstructorLib
{
    public class VisualizationBehaviourHelper : ExampleAppBehaviourHelper
    {
        private static readonly string[] RequiredMaterialProperties = 
        {
            "_Color",
            "_MainTex",
            "_Glossiness",
            "_Metallic"
        };

        public override bool CanAddBehaviour(GameObject gameObject, Type behaviourType)
        {
            if (!base.CanAddBehaviour(gameObject, behaviourType))
            {
                return false;
            }

            var renderers = gameObject.GetComponentsInChildren<Renderer>();

            if (renderers.Length <= 0)
            {
                return false;
            }

            var materials = renderers.Select(x => x.material).Where(HasRequiredProperties);
            Material firstMaterial = materials.FirstOrDefault();
            
            if (!firstMaterial)
            {
                return false;
            }

            Color firstColor = firstMaterial.color;
                
            return materials.All(x => x.color == firstColor);
        }

        private bool HasRequiredProperties(Material material)
        {
            return RequiredMaterialProperties.All(material.HasProperty);
        }
    }
    
    [ExampleAppComponent(English: "Visualization", Russian: "Визуализация")]
    [RequireComponentInChildren(typeof(Renderer))]
    public class VisualizationBehaviour : ConstructorExampleAppBehaviour
    {
        public enum State
        {
            [Item("Stop",Russian:"завершить")]
            Stop,
            [Item("Pause",Russian:"приостановить")]
            Pause,
            [Item("Continue",Russian:"продолжить")]
            Continue
        }
    
        public enum ShadowCastingMode
        {
            [Item(English: "Off", Russian: "Отключено")] Off,
            [Item(English: "On", Russian: "Включено")] On,
            [Item(English: "Two sided", Russian: "Двухстороннее")] TwoSided,
            [Item(English: "Shadows only", Russian: "Только тени")] ShadowsOnly
        }

        private State _currentState;
        
        private Renderer[] _renderers;
        private Renderer[] Renderers => _renderers ?? (_renderers = GetComponentsInChildren<Renderer>());

        private Material[] _unlitMaterials;
        private Material[] _litMaterials;
        
        private bool _receiveShadows;
        private Texture _materialTexture;
        private bool _transparent;
        private Color _mainColor = Color.white;
        private ShadowCastingMode _castShadows;
        private bool _unlit;
        private float _glossiness;
        private float _tilingX;
        private float _tilingY;
        private float _offsetX;
        private float _offsetY;
        private float _metallic;
        
        protected override void AwakeOverride()
        {
            InitializeMaterials();
            InitializeProperties();
        }

        #region ExampleAppInspector

        [ExampleAppInspector(English: "Texture", Russian: "Текстура")]
        public Texture MaterialTexture
        {
            get => _materialTexture;
            set
            {
                if (_materialTexture == value)
                {
                    return;
                }

                _materialTexture = value;
                
                foreach (var meshRenderer in Renderers)
                {
                    meshRenderer.material.mainTexture = _materialTexture;
                }
            }
        }
        
        [ExampleAppInspector(English: "Color", Russian: "Цвет")]
        public Color MainColor
        {
            get => _mainColor;
            set
            {
                if (_mainColor == value)
                {
                    return;
                }

                if (value.a < 1 != _transparent)
                {
                    _transparent = value.a < 1;
                    if (!_unlit)
                    {
                        SetMaterial();
                    }
                }

                _mainColor = value;
                SetColor();
            }
        }

        [ExampleAppInspector("Cast shadows", Russian: "Отбрасывание теней")]
        public ShadowCastingMode CastShadows
        {
            get => _castShadows;
            set
            {
                if (_castShadows == value)
                {
                    return;
                }

                _castShadows = value;
                Renderers[0].shadowCastingMode = (UnityEngine.Rendering.ShadowCastingMode) value;
            }
        }
        
        [ExampleAppInspector("Receive shadows", Russian: "Отображать тени других объектов")]
        public bool ReceiveShadows
        {
            get => _receiveShadows;
            set
            {
                if (_receiveShadows == value)
                {
                    return;
                }

                _receiveShadows = value;
                Renderers[0].receiveShadows = _receiveShadows;
            }
        }
        
        [ExampleAppInspector("Unlit", Russian: "Неосвещенный материал")]
        public bool Unlit
        {
            get => _unlit;
            set
            {
                if (_unlit == value)
                {
                    return;
                }

                _unlit = value;
                SetMaterial();

                if (Renderers[0].material.color == _mainColor)
                {
                    SetColor();
                }
            }
        }
        
        [ExampleAppInspector(English: "Metalness", Russian: "Металличность")]
        public float Metallic
        {
            get => _metallic;
            set
            {
                if (_metallic.ApproximatelyEquals(value))
                {
                    return;
                }

                _metallic = value;
                
                foreach (var meshRenderer in Renderers)
                {
                    if (meshRenderer.material.HasProperty("_Metallic"))
                    {
                        meshRenderer.material.SetFloat("_Metallic", value);
                    }
                }
            }
        }
        
        [ExampleAppInspector(English: "Smoothness", Russian: "Гладкость")]
        public float Smoothness
        {
            get => _glossiness;
            set
            {
                if (_glossiness.ApproximatelyEquals(value))
                {
                    return;
                }

                _glossiness = value;
                
                foreach (var meshRenderer in Renderers)
                {
                    if (meshRenderer.material.HasProperty("_Glossiness"))
                    {
                        meshRenderer.material.SetFloat("_Glossiness", value);
                    }
                }
            }
        }
        
        [ExampleAppInspector(English: "Texture tiling X", Russian: "Тайлинг текстуры по X")]
        public float TilingX
        {
            get => _tilingX;
            set
            {
                if (_tilingX.ApproximatelyEquals(value))
                {
                    return;
                }

                _tilingX = value;
                
                foreach (var meshRenderer in Renderers)
                {
                    meshRenderer.material.mainTextureScale = new Vector2(value, meshRenderer.material.mainTextureScale.y);
                }
            }
        }
        
        [ExampleAppInspector(English: "Texture tiling Y", Russian: "Тайлинг текстуры по Y")]
        public float TilingY
        {
            get => _tilingY;
            set
            {
                if (_tilingY.ApproximatelyEquals(value))
                {
                    return;
                }

                _tilingY = value;
                
                foreach (var meshRenderer in Renderers)
                {
                    meshRenderer.material.mainTextureScale = new Vector2(meshRenderer.material.mainTextureScale.x, value);
                }
            }
        }
        
        [ExampleAppInspector(English: "Texture offset X", Russian: "Смещение текстуры по X")]
        public float OffsetX
        {
            get => _offsetX;
            set
            {
                if (_offsetX.ApproximatelyEquals(value))
                {
                    return;
                }

                _offsetX = value;
                
                foreach (var meshRenderer in Renderers)
                {
                    meshRenderer.material.mainTextureOffset = new Vector2(value, OffsetY);
                }
            }
        }
        
        [ExampleAppInspector(English: "Texture offset Y", Russian: "Смещение текстуры по Y")]
        public float OffsetY
        {
            get => _offsetY;
            set
            {
                if (_offsetY.ApproximatelyEquals(value))
                {
                    return;
                }

                _offsetY = value;
                
                foreach (var meshRenderer in Renderers)
                {
                    meshRenderer.material.mainTextureOffset = new Vector2(OffsetX, value);
                }
            }
        }
        
        #endregion

        #region Actions

        [LogicGroup("Visualization", Russian: "Визуализация")]
        [LogicTooltip(English: "Instantly changes the object's color to the selected color. If you use the block with an object that has textures, the block will give it the selected hue.", Russian: "Мгновенно меняет цвет объекта на выбранный. При использовании блока с объектом, имеющим текстуры, блок придаст ему выбранный оттенок.")]
        [Action(English: "change color to", Russian: "изменить цвет")]
        public void ChangeObjectColor(Color color)
        {
            MainColor = color;
        }

        [LogicGroup("Visualization", Russian: "Визуализация")]
        [LogicTooltip(English: "Starts changing the object's color to the selected color for the specified amount of time. If you use the block with an object that has textures, the block will give it the selected hue.", Russian: "Запускает изменение цвета объекта на выбранный в течение заданного времени. При использовании блока с объектом, имеющим текстуры, блок придаст ему выбранный оттенок.")]
        [Action(English: "", Russian: "")]
        [ArgsFormat("change color up to {%} for {%} s", Russian: "изменить цвет до {%} в течении {%} с")]
        public IEnumerator SmoothlyChangeColor(Color color, float duration)
        {
            var time = 0f;
            _currentState = State.Continue;
            var startColors = _mainColor;
            while (time < 1.0f) 
            {
                if (_currentState == State.Pause)
                {
                    yield return null;
                }
                else if(_currentState == State.Stop)
                {
                    break;
                }
                else
                {
                    time += Time.deltaTime / duration;
                    if (time > 1) time = 1;

                    var nextColor = Color.Lerp(startColors, color, time);
                    MainColor = nextColor;
                    
                    yield return null;
                }
            }

            _currentState = State.Stop;
        }
        
        [LogicGroup("Visualization", Russian: "Визуализация")]
        [LogicTooltip(English: "Controls any color changing. A paused color changing can be continued with the corresponding block.", Russian: "Управляет любым изменением цвета. Приостановленное изменение цвета можно продолжить соответствующим блоком.")]
        [Action(English: "", Russian: "")]
        [ArgsFormat("{%} color changing", Russian: "{%} изменение цвета")]
        public void SetGhangingColorState(State targetState)
        {
            _currentState = targetState;
        }

        #endregion

        #region Checkers

        [LogicGroup("Visualization", Russian: "Визуализация")]
        [LogicTooltip(English: "Returns true if the specified object is currently changing color. Otherwise it returns false", Russian: "Возвращает “истину”, если указанный объект изменяет цвет в данный момент. В противном случае возвращает “ложь”.")]
        [Checker(English: "is changing color at the moment", Russian: "изменяет цвет в данный момент")]
        public bool IsColorChangingNow()
        {
            return _currentState == State.Continue;
        }

        #endregion
        
        #region Variables

        [LogicGroup("Visualization", Russian: "Визуализация")]
        [LogicTooltip(English: "Returns the color of the specified object as a color block.", Russian: "Возвращает цвет указанного объекта в виде блока цвета.")]
        [Variable(English: "color", Russian: "цвет")]
        public Color CurrentColor
        {
            get => MainColor;
        }
        #endregion

        #region Private Helpers
        
        private void InitializeMaterials()
        {
            Shader _unlitShader = Shader.Find("Unlit/TransparentColor");
            Shader _litShader = Shader.Find("Standard");

            _unlitMaterials = new Material[Renderers.Length];
            _litMaterials = new Material[Renderers.Length];
            for (int i = 0; i < Renderers.Length; i++)
            {
                _unlitMaterials[i] = new Material(_unlitShader);
                _litMaterials[i] = new Material(_litShader);
            }
        }
        private void InitializeProperties()
        {
            MaterialTexture = Renderers[0].material.mainTexture;
            MainColor = Renderers[0].material.HasProperty("_Color") ? Renderers[0].material.color : _mainColor;
            
            Metallic = Renderers[0].material.HasProperty("_Metallic") ? Renderers[0].material.GetFloat("_Metallic") : 0;

            Smoothness = Renderers[0].material.HasProperty("_Glossiness") ? Renderers[0].material.GetFloat("_Glossiness") : 0;

            TilingX = Renderers[0].material.mainTextureScale.x;
            TilingY = Renderers[0].material.mainTextureScale.y;
            OffsetX = Renderers[0].material.mainTextureOffset.x;
            OffsetY = Renderers[0].material.mainTextureOffset.y;

            CastShadows = (ShadowCastingMode) Renderers[0].shadowCastingMode;
            ReceiveShadows = Renderers[0].receiveShadows;
        }
        private void SetMaterial()
        {
            if (_unlit)
            {
                for (int i = 0; i < Renderers.Length; i++)
                {
                    _unlitMaterials[i].CopyPropertiesFromMaterial(Renderers[i].material);
                    Renderers[i].material = _unlitMaterials[i];
                }
            }
            else
            {
                for (int i = 0; i < Renderers.Length; i++)
                {
                    _litMaterials[i].CopyPropertiesFromMaterial(Renderers[i].material);
                    
                    if (_transparent)
                    {
                        _litMaterials[i].ToTransparentMode();
                    }
                    else
                    {
                        _litMaterials[i].ToOpaqueMode();
                    }
                    
                    Renderers[i].material = _litMaterials[i];
                }
            }
        }
        private void SetColor()
        {
            foreach (var meshRenderer in Renderers)
            {
                meshRenderer.material.color = _mainColor;
            }
        }

        #endregion
        
        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
    
}