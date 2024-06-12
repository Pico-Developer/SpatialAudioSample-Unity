/*******************************************************************************
Copyright © 2015-2024 PICO Technology Co., Ltd.All rights reserved.

NOTICE：All information contained herein is, and remains the property of
PICO Technology Co., Ltd. The intellectual and technical concepts
contained herein are proprietary to PICO Technology Co., Ltd. and may be
covered by patents, patents in process, and are protected by trade secret or
copyright law. Dissemination of this information or reproduction of this
material is strictly forbidden unless prior written permission is obtained from
PICO Technology Co., Ltd.
*******************************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PicoSpatialAudioSample.Runtime.Item;

namespace PicoSpatialAudioSample.Runtime.UI
{
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance;
        
        [SerializeField] private Color textSelectedColor = Color.white;
        [SerializeField] private Color imageSelectedColor = Color.blue;
        [SerializeField] private Color textNormalColor = Color.black;
        [SerializeField] private Color imageNormalColor = Color.gray;
        
        public GameObject globalSettingPanel;
        public GameObject spatialSettingPanel;
        public Button globalSettingBtn;
        public Button spatialSettingBtn;

        public Button closeBtn;

        //GlobalSetting Panel
        private PXR_Audio_Spatializer_AudioSource _currentAudioSourceSetting;

        public Slider masterVolumeSlider;
        public TMP_InputField masterVolumeValue;
        public Slider bgmVolumeSlider;
        public TMP_InputField bgmVolumeValue;
        public Slider spatialAudioVolumeSlider;
        public TMP_InputField spatialAudioVolumeValue;
        public Slider ambisonicVolumeSlider;
        public TMP_InputField ambisonicVolumeValue;

        public TMP_Dropdown audioSourceList;

        public Slider sourceGainSlider;
        public TMP_InputField sourceGainValue;
        public Slider reflectionGainSlider;
        public TMP_InputField reflectionGainValue;
        public Slider sourceRadiusSlider;
        public TMP_InputField sourceRadiusValue;

        public Toggle doppler;
        public Toggle directivity;
        public GameObject directivityIcon;
        public GameObject directivityPanel;
        public Slider shapeSlider;
        public TMP_InputField shapeValue;
        public Slider orderSlider;
        public TMP_InputField orderValue;

        public Toggle visualizerDirectivity;
        
        public Toggle controllerGuideTips;

        public bool globaldirectivityShow;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        //Spatial Audio Setting
        void Start()
        {
            globalSettingBtn.onClick.AddListener(OnClickGlobalSetting);
            spatialSettingBtn.onClick.AddListener(OnClickSpatialSetting);
            closeBtn.onClick.AddListener(OnClickClose);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            masterVolumeValue.onEndEdit.AddListener(OnMasterVolumeInputChanged);
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            bgmVolumeValue.onEndEdit.AddListener(OnBGMVolumeInputChanged);
            spatialAudioVolumeSlider.onValueChanged.AddListener(OnSpatialAudioVolumeChanged);
            spatialAudioVolumeValue.onEndEdit.AddListener(OnSpatialAudioVolumeInputChanged);
            ambisonicVolumeSlider.onValueChanged.AddListener(OnAmbisonicVolumeChanged);
            ambisonicVolumeValue.onEndEdit.AddListener(OnAmbisonicVolumeInputChanged);
            audioSourceList.onValueChanged.AddListener(OnAudioSourceChanged);
            sourceGainSlider.onValueChanged.AddListener(OnSourceGainValueChanged);
            sourceGainValue.onEndEdit.AddListener(OnSourceGainInputChanged);
            reflectionGainSlider.onValueChanged.AddListener(OnReflectionGainValueChanged);
            reflectionGainValue.onEndEdit.AddListener(OnReflectionGainInputChanged);
            sourceRadiusSlider.onValueChanged.AddListener(OnSourceRadiusValueChanged);
            sourceRadiusValue.onEndEdit.AddListener(OnSourceRadiusInputChanged);
            doppler.onValueChanged.AddListener(OnDopplerToggleChanged);
            directivity.onValueChanged.AddListener(OnDirectivityToggleChanged);
            shapeSlider.onValueChanged.AddListener(OnShapeValueChanged);
            shapeValue.onEndEdit.AddListener(OnShapeValueInputChanged);
            orderSlider.onValueChanged.AddListener(OnOrderValueChanged);
            orderValue.onEndEdit.AddListener(OnOrderValueInputChanged);
            visualizerDirectivity.onValueChanged.AddListener(OnVisualizerDirectivityToggleChanged);
            controllerGuideTips.onValueChanged.AddListener(OnVisualizerControllerGuideTipsToggleChanged);
            InitSettingData();
        }

        void InitSettingData()
        {
            masterVolumeSlider.value = AudioManager.Instance.GetVolume("Master");
            masterVolumeValue.text = $"{masterVolumeSlider.value:0.00}";
            bgmVolumeSlider.value = AudioManager.Instance.GetVolume("BackgroundSound");
            bgmVolumeValue.text = $"{bgmVolumeSlider.value:0.00}";
            spatialAudioVolumeSlider.value = AudioManager.Instance.GetVolume("SpatialAudio");
            spatialAudioVolumeValue.text = $"{spatialAudioVolumeSlider.value:0.00}";
            ambisonicVolumeSlider.value = AudioManager.Instance.GetVolume("Ambisonic");
            ambisonicVolumeValue.text = $"{ambisonicVolumeSlider.value:0.00}";
            audioSourceList.options ??= new List<TMP_Dropdown.OptionData>();

            audioSourceList.options.Clear();
            foreach (var audioSourceData in AudioManager.Instance.soundSourceRoots)
            {
                audioSourceList.options.Add(new TMP_Dropdown.OptionData(audioSourceData.optionName));
            }

            globaldirectivityShow = true;
            audioSourceList.value = 0;
            RefreshAudioSourcePanel(0);
            OnClickGlobalSetting();
            this.gameObject.SetActive(false);
        }

        private void RefreshAudioSourcePanel(int index)
        {
            if (audioSourceList.options == null)
            {
                Debug.Log("audioSourceList.options is null");
                return;
            }

            if (audioSourceList.options[index] == null)
            {
                Debug.Log("audioSourceList.options[index] is null");
                return;
            }

            audioSourceList.captionText.text = audioSourceList.options[index].text;
            var gameObjectName = audioSourceList.options[index].text;
            var audioData = AudioManager.Instance.soundSourceRoots.Find(p => p.optionName == gameObjectName);
            if (audioData == null)
            {
                Debug.Log("AudioSource is null");
                return;
            }

            var audioRoot = audioData.soundSourceRoot;
            if (audioRoot == null)
            {
                Debug.Log("AudioSource is null");
                return;
            }
            _currentAudioSourceSetting = audioRoot.GetComponent<PXR_Audio_Spatializer_AudioSource>();
            if (_currentAudioSourceSetting == null)
            {
                Debug.Log("AudioSource setting is null");
                return;
            }

            sourceGainSlider.value = _currentAudioSourceSetting.GetGainDB();
            sourceGainValue.text = $"{sourceGainSlider.value:0.0}";
            reflectionGainSlider.value = _currentAudioSourceSetting.GetReflectionGainDB();
            reflectionGainValue.text = $"{reflectionGainSlider.value:0}";
            sourceRadiusSlider.value = _currentAudioSourceSetting.GetSize();
            sourceRadiusValue.text = $"{sourceRadiusSlider.value:0}";

            doppler.isOn = _currentAudioSourceSetting.GetDopplerStatus();
            directivity.isOn = globaldirectivityShow;

            shapeSlider.value = _currentAudioSourceSetting.GetDirectivityAlpha();
            shapeValue.text = $"{shapeSlider.value:0.000}";
            orderSlider.value = _currentAudioSourceSetting.GetDirectivityOrder();
            orderValue.text = $"{orderSlider.value:0}";

            visualizerDirectivity.isOn = _currentAudioSourceSetting.GetComponent<SoundControl>()
                .sourceDirectivityController.gameObject.activeSelf;

            directivityPanel.gameObject.SetActive(globaldirectivityShow);

        }

        private void SetTabSelectedVisuals(Component button, bool isSelected)
        {
            var tmpText = button.GetComponentInChildren<TMP_Text>();
            var image = button.GetComponent<Image>();

            if (tmpText == null || image == null)
            {
                Debug.LogError("Button does not contain required Text or Image component.");
                return;
            }

            if (isSelected)
            {
                tmpText.color = textSelectedColor;
                image.color = imageSelectedColor;
            }
            else
            {
                tmpText.color = textNormalColor;
                image.color = imageNormalColor;
            }
        }
        
        private void OnClickGlobalSetting()
        {
            SetTabSelectedVisuals(globalSettingBtn, true);
            SetTabSelectedVisuals(spatialSettingBtn, false);
            
            globalSettingPanel.gameObject.SetActive(true);
            spatialSettingPanel.gameObject.SetActive(false);
        }

        private void OnClickSpatialSetting()
        {
            SetTabSelectedVisuals(globalSettingBtn, false);
            SetTabSelectedVisuals(spatialSettingBtn, true);
            
            globalSettingPanel.gameObject.SetActive(false);
            spatialSettingPanel.gameObject.SetActive(true);
        }

        public void DirectToSpatialSetting(string spatialSourceName)
        {
            globalSettingPanel.gameObject.SetActive(false);
            spatialSettingPanel.gameObject.SetActive(true);
            SetTabSelectedVisuals(globalSettingBtn, false);
            SetTabSelectedVisuals(spatialSettingBtn, true);
            var spatialOption =
                AudioManager.Instance.soundSourceRoots.Find(p => p.soundSourceRoot.name == spatialSourceName);
            var index = audioSourceList.options.FindIndex(p => p.text == spatialOption.optionName);
            audioSourceList.value = index;
        }

        private void OnClickClose()
        {
            CloseMenu();
        }

        private void CloseMenu()
        {
            this.gameObject.SetActive(false);
        }

        private void OnMasterVolumeChanged(float value)
        {
            AudioManager.Instance.SetVolume("Master", (float)System.Math.Round(value, 2));
            masterVolumeValue.text = $"{value:0.00}";
        }

        private void OnMasterVolumeInputChanged(string text)
        {
            if (text == "")
            {
                masterVolumeValue.text = $"{masterVolumeSlider.value:0.00}";
            }
            else
            {
                float.TryParse(text,out var outvalue);
                if (outvalue > masterVolumeSlider.maxValue)
                {
                    AudioManager.Instance.SetVolume("Master", (float)System.Math.Round(masterVolumeSlider.maxValue, 2));
                    masterVolumeValue.text = $"{masterVolumeSlider.maxValue:0.00}";
                    masterVolumeSlider.value = masterVolumeSlider.maxValue;
                }
                else if (outvalue < masterVolumeSlider.minValue)
                {
                    AudioManager.Instance.SetVolume("Master", (float)System.Math.Round(masterVolumeSlider.minValue, 2));
                    masterVolumeValue.text = $"{masterVolumeSlider.minValue:0.00}";
                    masterVolumeSlider.value = masterVolumeSlider.minValue;
                }
                else
                {
                    AudioManager.Instance.SetVolume("Master", (float)System.Math.Round(outvalue, 2));
                    masterVolumeValue.text = $"{outvalue:0.00}";
                    masterVolumeSlider.value = outvalue;
                }
                
            }
        }

        private void OnBGMVolumeChanged(float value)
        {
            AudioManager.Instance.SetVolume("BackgroundSound", (float)System.Math.Round(value, 2));
            bgmVolumeValue.text = $"{value:0.00}";
        }
        
        private void OnBGMVolumeInputChanged(string text)
        {
            if (text == "")
            {
                bgmVolumeValue.text = $"{bgmVolumeSlider.value:0.00}";
            }
            else
            {
                float.TryParse(text,out var outvalue);
                if (outvalue > bgmVolumeSlider.maxValue)
                {
                    AudioManager.Instance.SetVolume("BackgroundSound", (float)System.Math.Round(bgmVolumeSlider.maxValue, 2));
                    bgmVolumeValue.text = $"{bgmVolumeSlider.maxValue:0.00}";
                    bgmVolumeSlider.value = bgmVolumeSlider.maxValue;
                }
                else if (outvalue < bgmVolumeSlider.minValue)
                {
                    AudioManager.Instance.SetVolume("BackgroundSound", (float)System.Math.Round(bgmVolumeSlider.minValue, 2));
                    bgmVolumeValue.text = $"{bgmVolumeSlider.minValue:0.00}";
                    bgmVolumeSlider.value = bgmVolumeSlider.minValue;
                }
                else
                {
                    AudioManager.Instance.SetVolume("BackgroundSound", (float)System.Math.Round(outvalue, 2));
                    bgmVolumeValue.text = $"{outvalue:0.00}";
                    bgmVolumeSlider.value = outvalue;
                }
            }
        }

        private void OnSpatialAudioVolumeChanged(float value)
        {
            AudioManager.Instance.SetVolume("SpatialAudio", (float)System.Math.Round(value, 2));
            spatialAudioVolumeValue.text = $"{value:0.00}";
        }
        
        private void OnSpatialAudioVolumeInputChanged(string text)
        {
            if (text == "")
            {
                spatialAudioVolumeValue.text = $"{spatialAudioVolumeSlider.value:0.00}";
            }
            else
            {
                float.TryParse(text,out var outvalue);
                if (outvalue > spatialAudioVolumeSlider.maxValue)
                {
                    AudioManager.Instance.SetVolume("SpatialAudio", (float)System.Math.Round(spatialAudioVolumeSlider.maxValue, 2));
                    spatialAudioVolumeValue.text = $"{spatialAudioVolumeSlider.maxValue:0.00}";
                    spatialAudioVolumeSlider.value = spatialAudioVolumeSlider.maxValue;
                }
                else if (outvalue < spatialAudioVolumeSlider.minValue)
                {
                    AudioManager.Instance.SetVolume("SpatialAudio", (float)System.Math.Round(spatialAudioVolumeSlider.minValue, 2));
                    spatialAudioVolumeValue.text = $"{spatialAudioVolumeSlider.minValue:0.00}";
                    spatialAudioVolumeSlider.value = spatialAudioVolumeSlider.minValue;
                }
                else
                {
                    AudioManager.Instance.SetVolume("SpatialAudio", (float)System.Math.Round(outvalue, 2));
                    spatialAudioVolumeValue.text = $"{outvalue:0.00}";
                    spatialAudioVolumeSlider.value = outvalue;
                }
            }
        }

        private void OnAmbisonicVolumeChanged(float value)
        {
            AudioManager.Instance.SetVolume("Ambisonic", (float)System.Math.Round(value, 2));
            ambisonicVolumeValue.text = $"{value:0.00}";
        }
        
        private void OnAmbisonicVolumeInputChanged(string text)
        {
            if (text == "")
            {
                ambisonicVolumeValue.text = $"{ambisonicVolumeSlider.value:0.00}";
            }
            else
            {
                float.TryParse(text,out var outvalue);
                if (outvalue > ambisonicVolumeSlider.maxValue)
                {
                    AudioManager.Instance.SetVolume("Ambisonic", (float)System.Math.Round(ambisonicVolumeSlider.maxValue, 2));
                    ambisonicVolumeValue.text = $"{ambisonicVolumeSlider.maxValue:0.00}";
                    ambisonicVolumeSlider.value = ambisonicVolumeSlider.maxValue;
                }
                else if (outvalue < ambisonicVolumeSlider.minValue)
                {
                    AudioManager.Instance.SetVolume("Ambisonic", (float)System.Math.Round(ambisonicVolumeSlider.minValue, 2));
                    ambisonicVolumeValue.text = $"{ambisonicVolumeSlider.minValue:0.00}";
                    ambisonicVolumeSlider.value = ambisonicVolumeSlider.minValue;
                }
                else
                {
                    AudioManager.Instance.SetVolume("Ambisonic", (float)System.Math.Round(outvalue, 2));
                    ambisonicVolumeValue.text = $"{outvalue:0.00}";
                    ambisonicVolumeSlider.value = outvalue;
                }
            }
        }

        private void OnAudioSourceChanged(int index)
        {
            RefreshAudioSourcePanel(index);
        }

        private void OnSourceGainValueChanged(float value)
        {
            if (_currentAudioSourceSetting == null)
            {
                Debug.Log("_currentAudioSourceSetting is null");
                return;
            }

            _currentAudioSourceSetting.SetGainDB((float)System.Math.Round(value, 2));
            sourceGainValue.text = $"{value:0.00}";
        }
        
        private void OnSourceGainInputChanged(string text)
        {
            if (_currentAudioSourceSetting == null)
            {
                Debug.Log("_currentAudioSourceSetting is null");
                return;
            }
            if (text == "")
            {
                sourceGainValue.text = $"{sourceGainSlider.value:0.00}";
            }
            else
            {
                float.TryParse(text,out var outvalue);
                if (outvalue > sourceGainSlider.maxValue)
                {
                    _currentAudioSourceSetting.SetGainDB((float)System.Math.Round(sourceGainSlider.maxValue, 2));
                    sourceGainValue.text = $"{sourceGainSlider.maxValue:0.00}";
                    sourceGainSlider.value = sourceGainSlider.maxValue;
                }
                else if (outvalue < sourceGainSlider.minValue)
                {
                    _currentAudioSourceSetting.SetGainDB((float)System.Math.Round(sourceGainSlider.minValue, 2));
                    sourceGainValue.text = $"{sourceGainSlider.minValue:0.00}";
                    sourceGainSlider.value = sourceGainSlider.minValue;
                }
                else
                {
                    _currentAudioSourceSetting.SetGainDB((float)System.Math.Round(outvalue, 2));
                    sourceGainValue.text = $"{outvalue:0.00}";
                    sourceGainSlider.value = outvalue;
                }
            }
        }

        private void OnReflectionGainValueChanged(float value)
        {
            if (_currentAudioSourceSetting == null)
            {
                Debug.Log("_currentAudioSourceSetting is null");
                return;
            }

            _currentAudioSourceSetting.SetReflectionGainDB((float)System.Math.Round(value, 1));
            reflectionGainValue.text = $"{reflectionGainSlider.value:0.0}";
        }
        
        private void OnReflectionGainInputChanged(string text)
        {
            if (_currentAudioSourceSetting == null)
            {
                Debug.Log("_currentAudioSourceSetting is null");
                return;
            }
            if (text == "")
            {
                reflectionGainValue.text = $"{reflectionGainSlider.value:0.0}";
            }
            else
            {
                float.TryParse(text,out var outvalue);
                if (outvalue > reflectionGainSlider.maxValue)
                {
                    _currentAudioSourceSetting.SetReflectionGainDB((float)System.Math.Round(reflectionGainSlider.maxValue, 1));
                    reflectionGainValue.text = $"{reflectionGainSlider.maxValue:0.0}";
                    reflectionGainSlider.value = reflectionGainSlider.maxValue;
                }
                else if (outvalue < reflectionGainSlider.minValue)
                {
                    _currentAudioSourceSetting.SetReflectionGainDB((float)System.Math.Round(reflectionGainSlider.minValue, 1));
                    reflectionGainValue.text = $"{reflectionGainSlider.minValue:0.0}";
                    reflectionGainSlider.value = reflectionGainSlider.minValue;
                }
                else
                {
                    _currentAudioSourceSetting.SetReflectionGainDB((float)System.Math.Round(outvalue, 1));
                    reflectionGainValue.text = $"{outvalue:0.0}";
                    reflectionGainSlider.value = outvalue;
                }
            }
        }

        private void OnSourceRadiusValueChanged(float value)
        {
            if (_currentAudioSourceSetting == null)
            {
                Debug.Log("_currentAudioSourceSetting is null");
                return;
            }

            _currentAudioSourceSetting.SetSize((int)value);
            sourceRadiusValue.text = $"{value:0}";
        }
        
        private void OnSourceRadiusInputChanged(string text)
        {
            if (_currentAudioSourceSetting == null)
            {
                Debug.Log("_currentAudioSourceSetting is null");
                return;
            }
            if (text == "")
            {
                sourceRadiusValue.text = $"{sourceRadiusSlider.value:0}";
            }
            else
            {
                int.TryParse(text,out var outvalue);
                if (outvalue > sourceRadiusSlider.maxValue)
                {
                    _currentAudioSourceSetting.SetReflectionGainDB((int)sourceRadiusSlider.maxValue);
                    sourceRadiusValue.text = $"{sourceRadiusSlider.maxValue:0}";
                    sourceRadiusSlider.value = sourceRadiusSlider.maxValue;
                }
                else if (outvalue < sourceRadiusSlider.minValue)
                {
                    _currentAudioSourceSetting.SetReflectionGainDB((int)sourceRadiusSlider.minValue);
                    sourceRadiusValue.text = $"{sourceRadiusSlider.minValue:0}";
                    sourceRadiusSlider.value = sourceRadiusSlider.minValue;
                }
                else
                {
                    _currentAudioSourceSetting.SetReflectionGainDB(outvalue);
                    sourceRadiusValue.text = $"{outvalue:0}";
                    sourceRadiusSlider.value = outvalue;
                }
            }
        }

        private void OnShapeValueChanged(float value)
        {
            if (_currentAudioSourceSetting == null)
            {
                Debug.Log("_currentAudioSourceSetting is null");
                return;
            }

            _currentAudioSourceSetting.SetDirectivity((float)System.Math.Round(value, 3),
                _currentAudioSourceSetting.GetDirectivityOrder());
            _currentAudioSourceSetting.GetComponent<SoundControl>().sourceDirectivityController
                .SetAlpha((float)System.Math.Round(value, 3));
            shapeValue.text = $"{value:0.000}";
        }
        
        private void OnShapeValueInputChanged(string text)
        {
            if (_currentAudioSourceSetting == null)
            {
                Debug.Log("_currentAudioSourceSetting is null");
                return;
            }
            if (text == "")
            {
                shapeValue.text = $"{shapeSlider.value:0.000}";
            }
            else
            {
                float.TryParse(text,out var outvalue);
                if (outvalue > shapeSlider.maxValue)
                {
                    _currentAudioSourceSetting.SetDirectivity((float)System.Math.Round(shapeSlider.maxValue, 3),
                        _currentAudioSourceSetting.GetDirectivityOrder());
                    _currentAudioSourceSetting.GetComponent<SoundControl>().sourceDirectivityController
                        .SetAlpha((float)System.Math.Round(shapeSlider.maxValue, 3));
                    shapeValue.text = $"{shapeSlider.maxValue:0.000}";
                    shapeSlider.value = shapeSlider.maxValue;
                }
                else if (outvalue < shapeSlider.minValue)
                {
                    _currentAudioSourceSetting.SetDirectivity((float)System.Math.Round(shapeSlider.minValue, 3),
                        _currentAudioSourceSetting.GetDirectivityOrder());
                    _currentAudioSourceSetting.GetComponent<SoundControl>().sourceDirectivityController
                        .SetAlpha((float)System.Math.Round(shapeSlider.minValue, 3));
                    shapeValue.text = $"{shapeSlider.minValue:0.000}";
                    shapeSlider.value = shapeSlider.minValue;
                }
                else
                {
                    _currentAudioSourceSetting.SetDirectivity((float)System.Math.Round(outvalue, 3),
                        _currentAudioSourceSetting.GetDirectivityOrder());
                    _currentAudioSourceSetting.GetComponent<SoundControl>().sourceDirectivityController
                        .SetAlpha((float)System.Math.Round(outvalue, 3));
                    shapeValue.text = $"{outvalue:0.000}";
                    shapeSlider.value = outvalue;
                }
            }
        }

        private void OnOrderValueChanged(float value)
        {
            if (_currentAudioSourceSetting == null)
            {
                Debug.Log("_currentAudioSourceSetting is null");
                return;
            }

            _currentAudioSourceSetting.SetDirectivity(_currentAudioSourceSetting.GetDirectivityAlpha(), (int)value);
            _currentAudioSourceSetting.GetComponent<SoundControl>().sourceDirectivityController.SetOrder((int)value);
            orderValue.text = $"{value:0}";
        }
        
        private void OnOrderValueInputChanged(string text)
        {
            if (_currentAudioSourceSetting == null)
            {
                Debug.Log("_currentAudioSourceSetting is null");
                return;
            }
            if (text == "")
            {
                orderValue.text = $"{orderSlider.value:0}";
            }
            else
            {
                int.TryParse(text,out var outvalue);
                if (outvalue > orderSlider.maxValue)
                {
                    _currentAudioSourceSetting.SetDirectivity(_currentAudioSourceSetting.GetDirectivityAlpha(), (int)orderSlider.maxValue);
                    _currentAudioSourceSetting.GetComponent<SoundControl>().sourceDirectivityController.SetOrder((int)orderSlider.maxValue);
                    orderValue.text = $"{orderSlider.maxValue:0}";
                    orderSlider.value = orderSlider.maxValue;
                }
                else if (outvalue < orderSlider.minValue)
                {
                    _currentAudioSourceSetting.SetDirectivity(_currentAudioSourceSetting.GetDirectivityAlpha(), (int)orderSlider.minValue);
                    _currentAudioSourceSetting.GetComponent<SoundControl>().sourceDirectivityController.SetOrder((int)orderSlider.minValue);
                    orderValue.text = $"{orderSlider.minValue:0}";
                    orderSlider.value = orderSlider.minValue;
                }
                else
                {
                    _currentAudioSourceSetting.SetDirectivity(_currentAudioSourceSetting.GetDirectivityAlpha(), outvalue);
                    _currentAudioSourceSetting.GetComponent<SoundControl>().sourceDirectivityController.SetOrder(outvalue);
                    orderValue.text = $"{outvalue:0}";
                    orderSlider.value = outvalue;
                }
            }
        }

        private void OnDopplerToggleChanged(bool isOn)
        {
            _currentAudioSourceSetting.SetDopplerStatus(isOn);
            doppler.isOn = isOn;
        }

        private void OnDirectivityToggleChanged(bool isOn)
        {
            globaldirectivityShow = isOn;
            directivityPanel.gameObject.SetActive(globaldirectivityShow);
            if (isOn)
            {
                directivityIcon.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                directivityIcon.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }

        private void OnVisualizerDirectivityToggleChanged(bool isOn)
        {
            _currentAudioSourceSetting.GetComponent<SoundControl>().sourceDirectivityController.gameObject.SetActive(isOn);
        }
        
        private void OnVisualizerControllerGuideTipsToggleChanged(bool isOn)
        {
            App.Instance.showControllerGuide = isOn;
        }
        

    }
}
