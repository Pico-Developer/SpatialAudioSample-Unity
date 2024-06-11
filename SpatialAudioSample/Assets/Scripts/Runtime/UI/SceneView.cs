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
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace PXR.Benchmark
{
    /// <summary>
    /// Manages the visual representation and behavior of scene selection in a user interface, including hover effects.
    /// </summary>
    public class SceneView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// Gets or sets the index of the scene that this view represents.
        /// </summary>
        public int SceneIndex { get; set; }
        
        /// <summary>
        /// Event triggered when the pointer enters the scene view area.
        /// </summary>
        public Action<int> OnHoverEnter { get; set; }
        
        /// <summary>
        /// Event triggered when the pointer exits the scene view area.
        /// </summary>
        public Action<int> OnHoverExit { get; set; }
        
        [SerializeField] private Image sceneImage;
        [SerializeField] private Image highlightImage;
        [SerializeField] private float normalScale = 0.8f;
        [SerializeField] private float highlightScale = 1f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float transitionTime = 0.1f;
        private Coroutine _transitionCoroutine;
        
        /// <summary>
        /// Handles the pointer enter event to trigger visual changes and events.
        /// </summary>
        /// <param name="eventData">Event data associated with the pointer enter event.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverEnter?.Invoke(SceneIndex);
        }

        /// <summary>
        /// Handles the pointer exit event to revert visual changes and events.
        /// </summary>
        /// <param name="eventData">Event data associated with the pointer exit event.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverExit?.Invoke(SceneIndex);
        }

        /// <summary>
        /// Sets the visual highlight state of the scene view.
        /// </summary>
        /// <param name="highlight">Whether to highlight the scene view.</param>
        public void SetHighlight(bool highlight)
        {
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }
            _transitionCoroutine = StartCoroutine(AnimateScaleAndColor(highlight));
        }

        /// <summary>
        /// Animates the scaling and color transition of the scene view.
        /// </summary>
        /// <param name="highlight">Whether the animation should highlight or normalize the view.</param>
        /// <returns>An enumerator needed for coroutine continuation.</returns>
        private IEnumerator AnimateScaleAndColor(bool highlight)
        {
            if (highlight == false)
                highlightImage.gameObject.SetActive(false);
            
            float time = 0;
            var startScale = transform.localScale;
            var endScale = Vector3.one * (highlight ? highlightScale : normalScale);
            var startColor = sceneImage.color;
            var endColor = highlight ? highlightColor : normalColor;

            while (time < transitionTime)
            {
                transform.localScale = Vector3.Lerp(startScale, endScale, time / transitionTime);
                sceneImage.color = Color.Lerp(startColor, endColor, time / transitionTime);
                time += Time.deltaTime;
                yield return null;
            }

            // Ensure the final state is exactly what we want
            transform.localScale = endScale;
            sceneImage.color = endColor;
            
            // Toggle highlight image at the end if necessary
            if (highlight)
                highlightImage.gameObject.SetActive(true);
        }
    }
}
