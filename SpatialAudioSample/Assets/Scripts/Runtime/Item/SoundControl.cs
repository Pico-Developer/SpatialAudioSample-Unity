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
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.GraphicsTools;

namespace PicoSpatialAudioSample.Runtime.Item
{
    /// <summary>
    /// Option of the form of the background
    /// </summary>
    public enum ExtraControlType
    {
        /// <summary>
        /// No extra action to control play
        /// </summary>
        None = 0,

        /// <summary>
        /// Turn off after played sound
        /// </summary>
        CloseAfterPlay = 1,

        /// <summary>
        /// Delay x seconds replayed audio clip
        /// </summary>
        TimeDelayPlay = 2
    }

    public class SoundControl : MonoBehaviour
    {
        private AudioSource _audioSound;
        public Image playStateIcon;
        public MeshOutlineHierarchy meshOutline;
        public SourceDirectivityController sourceDirectivityController;
        public ExtraControlType controlType = ExtraControlType.None;
        public float delayPlaySeconds;
        public Animator animator;
        private Coroutine _playFinishCoroutine;
        private Coroutine _playDelayCoroutine;
        private readonly Color _colorButton = new Color(61f/255f, 139f/255f, 255f/255f);
        private bool _isOn;
        private static readonly int IsOn = Animator.StringToHash("isOn");

        // Start is called before the first frame update
        void Start()
        {
            _audioSound = GetComponent<AudioSource>();
            if (_audioSound != null)
            {
                _isOn = _audioSound.isPlaying;
                if (_audioSound.isPlaying)
                {
                    if (meshOutline)
                    {
                        if (meshOutline.UseStencilOutline)
                        {
                            meshOutline.StencilWriteMaterial = AudioManager.Instance.highlightStencilOn;
                        }

                        meshOutline.OutlineMaterial = AudioManager.Instance.highlightOn;
                        
                        
                    }

                    if (playStateIcon)
                    {
                        playStateIcon.color = _colorButton;
                    }
                    
                }
                else
                {
                    if (meshOutline)
                    {
                        if (meshOutline.UseStencilOutline)
                        {
                            meshOutline.StencilWriteMaterial = AudioManager.Instance.highlightStencilOff;
                        }

                        meshOutline.OutlineMaterial = AudioManager.Instance.highlightOff;
                        
                    }

                    if (playStateIcon)
                    {
                        playStateIcon.color = _colorButton;
                    }
                }
            }
        }

        /// <summary>
        /// Switch the machine state
        /// </summary>
        public void SwitchSoundOpen()
        {
            if (_audioSound != null)
            {
                if (_isOn)
                {

                    _audioSound.Stop();
                    if (playStateIcon)
                    {
                        playStateIcon.color = _colorButton;
                    }

                    if (meshOutline)
                    {
                        if (meshOutline.UseStencilOutline)
                        {
                            meshOutline.StencilWriteMaterial = AudioManager.Instance.highlightStencilOff;
                        }

                        meshOutline.OutlineMaterial = AudioManager.Instance.highlightOff;
                        
                    }

                    if (_playFinishCoroutine != null)
                    {
                        StopCoroutine(_playFinishCoroutine);
                    }

                    if (_playDelayCoroutine != null)
                    {
                        StopCoroutine(_playDelayCoroutine);
                    }
                }
                else
                {
                    if (controlType == ExtraControlType.CloseAfterPlay || controlType == ExtraControlType.TimeDelayPlay)
                    {
                        _playFinishCoroutine = StartCoroutine(AudioPlayFinished(_audioSound.clip.length));
                    }

                    _audioSound.Play();

                    if (playStateIcon)
                    {
                        playStateIcon.color = _colorButton;
                    }
                    if (meshOutline)
                    {
                        if (meshOutline.UseStencilOutline)
                        {
                            meshOutline.StencilWriteMaterial = AudioManager.Instance.highlightStencilOn;
                        }

                        meshOutline.OutlineMaterial = AudioManager.Instance.highlightOn;
                        
                    }
                }

                if (animator)
                {
                    animator.SetBool(IsOn, !_isOn);
                }

                _isOn = !_isOn;
            }
        }

        public void OpenAudioSetting()
        {
            App.Instance.OpenAudioSettingMenu(this.gameObject.name);
        }

        private IEnumerator AudioPlayFinished(float time)
        {
            yield return new WaitForSeconds(time);
            if (controlType == ExtraControlType.CloseAfterPlay)
            {
                _isOn = false;
                if (animator)
                {
                    animator.SetBool(IsOn, false);
                }

                if (playStateIcon)
                {
                    playStateIcon.color = _colorButton;
                }
                if (meshOutline)
                {
                    if (meshOutline.UseStencilOutline)
                    {
                        meshOutline.StencilWriteMaterial = AudioManager.Instance.highlightStencilOff;
                    }
                    meshOutline.OutlineMaterial = AudioManager.Instance.highlightOff;
                }
            }

            if (controlType == ExtraControlType.TimeDelayPlay)
            {
                if (animator)
                {
                    animator.SetBool(IsOn, false);
                }

                _playDelayCoroutine = StartCoroutine(WaitForNextAudioPlay(delayPlaySeconds));
            }
        }

        private IEnumerator WaitForNextAudioPlay(float time)
        {
            yield return new WaitForSeconds(time);
            _audioSound.Play();
            if (animator)
            {
                animator.SetBool(IsOn, true);
            }

            _playFinishCoroutine = StartCoroutine(AudioPlayFinished(_audioSound.clip.length));
        }
    }
}
