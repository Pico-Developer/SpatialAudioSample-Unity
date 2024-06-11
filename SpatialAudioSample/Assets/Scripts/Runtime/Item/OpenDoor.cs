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
using System;
using UnityEngine;

namespace PicoSpatialAudioSample.Runtime.Item
{

    enum DoorState
    {
        Stop,
        Open,
        Opened,
        Close,
        Closed
    }

    public class OpenDoor : MonoBehaviour
    {
        public GameObject point;

        [Range(0, 360)] public float minAngle;
        [Range(0, 360)] public float maxAngle = 90;


        [SerializeField] private DoorState state = DoorState.Stop;

        [Range(0, 360)] public float speed = 60;

        private float _angle;

        /// <summary>
        /// Switch door state
        /// </summary>
        public void Switch()
        {
            switch (state)
            {
                case DoorState.Stop:
                    if (this.gameObject.GetComponent<AudioSource>() != null)
                    {
                        this.gameObject.GetComponent<AudioSource>().Stop();
                        this.gameObject.GetComponent<AudioSource>().clip = AudioManager.Instance.openDoorClip;
                        this.gameObject.GetComponent<AudioSource>().Play();
                    }

                    state = DoorState.Open;
                    break;
                case DoorState.Open:
                    if (this.gameObject.GetComponent<AudioSource>() != null)
                    {
                        this.gameObject.GetComponent<AudioSource>().Stop();
                        this.gameObject.GetComponent<AudioSource>().clip = AudioManager.Instance.closeDoorClip;
                        this.gameObject.GetComponent<AudioSource>().Play();
                    }

                    state = DoorState.Close;
                    break;
                case DoorState.Close:
                    if (this.gameObject.GetComponent<AudioSource>() != null)
                    {
                        this.gameObject.GetComponent<AudioSource>().Stop();
                        this.gameObject.GetComponent<AudioSource>().clip = AudioManager.Instance.openDoorClip;
                        this.gameObject.GetComponent<AudioSource>().Play();
                    }

                    state = DoorState.Open;
                    break;
                case DoorState.Closed:
                    if (this.gameObject.GetComponent<AudioSource>() != null)
                    {
                        this.gameObject.GetComponent<AudioSource>().Stop();
                        this.gameObject.GetComponent<AudioSource>().clip = AudioManager.Instance.openDoorClip;
                        this.gameObject.GetComponent<AudioSource>().Play();
                    }

                    state = DoorState.Open;
                    break;
                case DoorState.Opened:
                    if (this.gameObject.GetComponent<AudioSource>() != null)
                    {
                        this.gameObject.GetComponent<AudioSource>().Stop();
                        this.gameObject.GetComponent<AudioSource>().clip = AudioManager.Instance.closeDoorClip;
                        this.gameObject.GetComponent<AudioSource>().Play();
                    }

                    state = DoorState.Close;
                    break;
            }
        }

        private void OnValidate()
        {
            minAngle = Math.Min(minAngle, maxAngle);
            maxAngle = Math.Max(minAngle, maxAngle);
            speed = Math.Clamp(speed, 0, 360);
        }


        // Update is called once per frame
        void Update()
        {
            if (!point)
            {
                return;
            }

            float thisFrameAngle = speed * Time.deltaTime;
            switch (state)
            {
                case DoorState.Stop:
                case DoorState.Opened:
                case DoorState.Closed:
                    return;
                case DoorState.Open:
                    if (_angle + thisFrameAngle > maxAngle)
                    {
                        thisFrameAngle = maxAngle - _angle;
                        state = DoorState.Opened;
                    }

                    break;
                case DoorState.Close:
                    thisFrameAngle *= -1;
                    if (_angle + thisFrameAngle < minAngle)
                    {
                        thisFrameAngle = -(_angle - minAngle);
                        state = DoorState.Closed;
                    }

                    break;
            }


            _angle += thisFrameAngle;

            gameObject.transform.RotateAround(point.transform.position, Vector3.up, thisFrameAngle);
        }
    }
}