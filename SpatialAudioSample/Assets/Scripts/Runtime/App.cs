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
using UnityEngine;
using UnityEngine.InputSystem;
using PicoSpatialAudioSample.Runtime.UI;
using Unity.XR.PXR;

namespace PicoSpatialAudioSample.Runtime
{
    public class App : MonoBehaviour
    {
        public static App Instance;

        public InputActionReference inputActionReferenceLeft;
        public InputActionReference inputActionReferenceRight;
        public GameObject mainMenu;
        public GameObject deviceSimulator;
        public bool showControllerGuide = true;
        private int _controllerType = -1;
        public GameObject controllerGuideLeftRoot;
        public GameObject controllerGuideLeftNeo3Root;
        public GameObject controllerGuideLeftPico4Root;
        public GameObject controllerGuideRightRoot;
        public GameObject controllerGuideRightNeo3Root;
        public GameObject controllerGuideRightPico4Root;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            if (inputActionReferenceLeft != null)
            {
                inputActionReferenceLeft.action.Enable();
                inputActionReferenceLeft.action.performed += OpenMenu;
            }

            if (inputActionReferenceRight != null)
            {
                inputActionReferenceRight.action.Enable();
                inputActionReferenceRight.action.performed += OpenMenu;
            }
            _controllerType = PXR_Plugin.Controller.UPxr_GetControllerType();
#if UNITY_EDITOR
            Instantiate(deviceSimulator);
#endif

        }

        void Update()
        {
            if (showControllerGuide)
            {
#if UNITY_EDITOR
                controllerGuideLeftRoot.SetActive(true);
                controllerGuideRightRoot.SetActive(true);
                //controllerGuideLeftPico4Root.SetActive(true);
                //controllerGuideRightPico4Root.SetActive(true);
                //controllerGuideLeftNeo3Root.SetActive(true);
                //controllerGuideRightNeo3Root.SetActive(true);
#else

                if (_controllerType == (int)PXR_Input.ControllerDevice.Neo3)
                {
                    if (PXR_Input.IsControllerConnected(PXR_Input.Controller.LeftController))
                    {
                        controllerGuideLeftRoot.SetActive(true);
                        controllerGuideLeftNeo3Root.SetActive(true);
                    }
                    else
                    {
                        controllerGuideLeftNeo3Root.SetActive(false);
                        controllerGuideLeftPico4Root.SetActive(false);
                    }
                    if (PXR_Input.IsControllerConnected(PXR_Input.Controller.RightController))
                    {
                        controllerGuideRightRoot.SetActive(true);
                        controllerGuideRightNeo3Root.SetActive(true);
                    }
                    else
                    {
                        controllerGuideRightNeo3Root.SetActive(false);
                        controllerGuideRightPico4Root.SetActive(false);
                    }

                }
                else if (_controllerType == (int)PXR_Input.ControllerDevice.PICO_4)
                {
                    if (PXR_Input.IsControllerConnected(PXR_Input.Controller.LeftController))
                    {
                        controllerGuideLeftRoot.SetActive(true);
                        controllerGuideLeftPico4Root.SetActive(true);
                    }
                    else
                    {
                        controllerGuideLeftNeo3Root.SetActive(false);
                        controllerGuideLeftPico4Root.SetActive(false);
                    }
                    if (PXR_Input.IsControllerConnected(PXR_Input.Controller.RightController))
                    {
                        controllerGuideRightRoot.SetActive(true);
                        controllerGuideRightPico4Root.SetActive(true);
                    }
                    else
                    {
                        controllerGuideRightNeo3Root.SetActive(false);
                        controllerGuideRightPico4Root.SetActive(false);
                    }
                }
                else
                {
                    controllerGuideLeftRoot.SetActive(false);
                    controllerGuideRightRoot.SetActive(false);
                }
#endif
            }
            else
            {
                controllerGuideLeftRoot.SetActive(false);
                controllerGuideRightRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Open GlobalSetting Menus
        /// </summary>
        private void OpenMenu(InputAction.CallbackContext content)
        {
            if (mainMenu.activeInHierarchy)
            {
                mainMenu.SetActive(false);
            }
            else
            {
                mainMenu.SetActive(true);
                mainMenu.GetComponent<FollowEyeTracker>().followLayout = FollowEyeTracker.ELayout.Center;
            }
        }

        public void OpenAudioSettingMenu(string spatialSourceName)
        {
            mainMenu.SetActive(true);
            mainMenu.GetComponent<FollowEyeTracker>().followLayout = FollowEyeTracker.ELayout.Center;
            MainMenu.Instance.DirectToSpatialSetting(spatialSourceName);
        }
        public void CloseMenu()
        {
            mainMenu.SetActive(false);
        }
    }
}
