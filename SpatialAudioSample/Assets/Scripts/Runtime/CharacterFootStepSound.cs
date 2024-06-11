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
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace PicoSpatialAudioSample.Runtime
{
    public class CharacterFootStepSound : MonoBehaviour
    {
        public CharacterController characterController;
        public RaycastHit hit; // RayCastHit Result
        public GameObject rayGo; //RayCast Root
        public AudioClip[] clipsCarpet, clipsCeramic, clipsFloor; // different material's SoundClips
        public float dist = 20f; // RayCast Length
        public float stepInterval = 0.1f;
        private float _nextStepTime;
        private string _prevtag = "Untagged";
        public AudioSource audioSource;
        public InputActionReference pushUpAction;
        public InputActionReference pushDownAction;
        public InputActionReference pushLeftAction;
        public InputActionReference pushRightAction;
        private bool _pushUpState;
        private bool _pushDownState;
        private bool _pushLeftState;
        private bool _pushRightState;

        private void Start()
        {
            _nextStepTime = Time.time;
        }

        void Update()
        {
            if (pushUpAction.action.WasPerformedThisFrame())
            {
                _pushUpState = true;
            }

            if (pushUpAction.action.WasReleasedThisFrame())
            {
                _pushUpState = false;
            }

            if (pushDownAction.action.WasPerformedThisFrame())
            {
                _pushDownState = true;
            }

            if (pushDownAction.action.WasReleasedThisFrame())
            {
                _pushDownState = false;
            }

            if (pushLeftAction.action.WasPerformedThisFrame())
            {
                _pushLeftState = true;
            }

            if (pushLeftAction.action.WasReleasedThisFrame())
            {
                _pushLeftState = false;
            }

            if (pushRightAction.action.WasPerformedThisFrame())
            {
                _pushRightState = true;
            }

            if (pushRightAction.action.WasReleasedThisFrame())
            {
                _pushRightState = false;
            }

            if (_pushUpState || _pushDownState || _pushLeftState || _pushRightState)
            {
                App.Instance.CloseMenu();
                if (Physics.Raycast(rayGo.transform.position, Vector3.down, out hit, dist)) // 向下发射射线检测地面
                {

                    if ((Time.time > _nextStepTime && audioSource.isPlaying == false) ||
                        (hit.collider && !hit.collider.CompareTag(_prevtag)))
                    {
                        _nextStepTime = Time.time + stepInterval;
                        if (!hit.collider.CompareTag(_prevtag))
                        {
                            audioSource.Stop();
                        }

                        if (hit.collider)
                        {
                            _prevtag = hit.collider.tag;
                            switch (_prevtag)
                            {
                                case "Carpet":
                                    PlayRandomSoundOneShot(clipsCarpet);
                                    break;
                                case "Ceramic":
                                    PlayRandomSoundOneShot(clipsCeramic);
                                    break;
                                case "Floor":
                                    PlayRandomSoundOneShot(clipsFloor);
                                    break;
                                default:
                                    audioSource.Stop();
                                    break;
                            }
                        }
                    }
                }
            }
            else if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        /// <summary>
        /// Play a random sound clip
        /// </summary>
        private void PlayRandomSoundOneShot(AudioClip[] clips)
        {
            var clip = clips[Random.Range(0, clips.Length)];
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}


