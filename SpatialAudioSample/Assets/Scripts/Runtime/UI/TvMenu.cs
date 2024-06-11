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
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PicoSpatialAudioSample.Runtime.UI
{
    public class TvMainMenu : MonoBehaviour
    {
        public static TvMainMenu Instance;
        public Button prevBtn;
        public Button nextBtn;
        public Button playBtn;
        public GameObject playIcon;
        public GameObject pauseIcon;
        public GameObject voiceBoxLeft;
        public GameObject voiceBoxRight;
        public GameObject voiceBoxLeft1;
        public GameObject voiceBoxRight1;
        
        public ScrollRect audioListScrollRect;
        public GameObject itemPrefab;
        public TextMeshProUGUI currentPlaying;
        public bool isPlaying = true;
        private List<GameObject> _audioClipObjList;
        public GameObject musicBg;
        private int _currentPlayIndex;

        //Spatial Audio Setting
        // Start is called before the first frame update
        private void Start()
        {
            prevBtn.onClick.AddListener(OnPrevClick);
            nextBtn.onClick.AddListener(OnNextClick);
            playBtn.onClick.AddListener(OnPlayClick);
            InitSettingData();
        }

        private void InitSettingData()
        {
            _audioClipObjList ??= new List<GameObject>();
            foreach (var audioClip in AudioManager.Instance.musicSounds)
            {
                GameObject item = Instantiate(itemPrefab, audioListScrollRect.content);
                var audioName = item.GetComponentInChildren<TextMeshProUGUI>();
                audioName.text = audioClip.name;
                item.name = audioClip.name;
                item.SetActive(true);
                _audioClipObjList.Add(item);
            }

            _currentPlayIndex = 0;
            RefreshAudioClipPanel(_audioClipObjList[_currentPlayIndex]);
        }

        private void RefreshAudioClipPanel(GameObject item)
        {
            var audioData =
                AudioManager.Instance.musicSounds.FirstOrDefault(p => p.name == item.name);
            if (audioData == null)
            {
                Debug.Log("AudioClip is null");
                return;
            }
            currentPlaying.text = audioData.name;
            voiceBoxLeft.GetComponent<AudioSource>().Stop();
            voiceBoxRight.GetComponent<AudioSource>().Stop();
            voiceBoxLeft1.GetComponent<AudioSource>().Stop();
            voiceBoxRight1.GetComponent<AudioSource>().Stop();
            voiceBoxLeft.GetComponent<AudioSource>().clip = audioData.clip;
            voiceBoxRight.GetComponent<AudioSource>().clip = audioData.clip;
            voiceBoxLeft1.GetComponent<AudioSource>().clip = audioData.clip;
            voiceBoxRight1.GetComponent<AudioSource>().clip = audioData.clip;
            voiceBoxLeft.GetComponent<AudioSource>().Play();
            voiceBoxRight.GetComponent<AudioSource>().Play();
            voiceBoxLeft1.GetComponent<AudioSource>().Play();
            voiceBoxRight1.GetComponent<AudioSource>().Play();
            isPlaying = true;
            RefreshPlayIcon();
        }

        private void RefreshPlayIcon()
        {
            playIcon.SetActive(!isPlaying);
            pauseIcon.SetActive(isPlaying);
            if (isPlaying)
            {
                musicBg.transform.DOLocalRotate(new Vector3(0f, 0f, 360f), 2f, RotateMode.LocalAxisAdd).SetLoops(-1).SetEase(Ease.Linear);
            }
            else
            {
                musicBg.transform.DOKill();
            }
        }

        private void OnPrevClick()
        {
            Debug.Log("OnPrevClick");
            if (_currentPlayIndex - 1 < 0)
            {
                _currentPlayIndex = _audioClipObjList.Count - 1;
            }
            else
            {
                _currentPlayIndex--;
            }
            RefreshAudioClipPanel(_audioClipObjList[_currentPlayIndex]);
        }

        private void OnNextClick()
        {
            Debug.Log("OnNextClick");
            if (_currentPlayIndex + 1 >= _audioClipObjList.Count)
            {
                _currentPlayIndex = 0;
            }
            else
            {
                _currentPlayIndex++;
            }
            RefreshAudioClipPanel(_audioClipObjList[_currentPlayIndex]);
        }

        private void OnPlayClick()
        {
            Debug.Log("OnPlayClick");
            if (isPlaying)
            {
                voiceBoxLeft.GetComponent<AudioSource>().Pause();
                voiceBoxRight.GetComponent<AudioSource>().Pause();
                voiceBoxLeft1.GetComponent<AudioSource>().Pause();
                voiceBoxRight1.GetComponent<AudioSource>().Pause();
            }
            else
            {
                voiceBoxLeft.GetComponent<AudioSource>().UnPause();
                voiceBoxRight.GetComponent<AudioSource>().UnPause();
                voiceBoxLeft1.GetComponent<AudioSource>().UnPause();
                voiceBoxRight1.GetComponent<AudioSource>().UnPause();
            }

            isPlaying = !isPlaying;
            RefreshPlayIcon();
        }
    }
}
