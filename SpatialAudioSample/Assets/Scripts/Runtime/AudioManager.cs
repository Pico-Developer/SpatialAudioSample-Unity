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
using UnityEngine.Audio;

namespace PicoSpatialAudioSample.Runtime
{
    [System.Serializable]
    public class SoundData
    {
        public string name;
        public AudioClip clip;
    }
    
    [System.Serializable]
    public class SoundSourceData
    {
        public string optionName;
        public GameObject soundSourceRoot;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        public SoundData[] musicSounds;
        public List<SoundSourceData> soundSourceRoots;

        public AudioMixer audioMixer;

        public AudioClip openDoorClip;
        public AudioClip closeDoorClip;
        public Material highlightOn;
        public Material highlightOff;
        public Material highlightStencilOn;
        public Material highlightStencilOff;

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
        /// Get Volume Setting By Mixer GroupName
        /// </summary>

        public float GetVolume(string mixerGroupName)
        {
            float outVolume = 0f;
            AudioMixerGroup mixerGroup = audioMixer.FindMatchingGroups(mixerGroupName)[0];
            if (mixerGroup == null || mixerGroup.audioMixer == null)
            {
                Debug.Log("mixer is null!!!");
                return outVolume;
            }

            mixerGroup.audioMixer.GetFloat(mixerGroupName + "Volume", out outVolume);
            return outVolume;
        }

        /// <summary>
        /// Set Volume Setting By Mixer GroupName
        /// </summary>
        public void SetVolume(string mixerGroupName, float value)
        {
            AudioMixerGroup mixerGroup = audioMixer.FindMatchingGroups(mixerGroupName)[0];
            if (mixerGroup == null || mixerGroup.audioMixer == null)
            {
                Debug.Log("mixer is null!!!");
                return;
            }

            mixerGroup.audioMixer.SetFloat(mixerGroupName + "Volume", value);
        }
    }
}
