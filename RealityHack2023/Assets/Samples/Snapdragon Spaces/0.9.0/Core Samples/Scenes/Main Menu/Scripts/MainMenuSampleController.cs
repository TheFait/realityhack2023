/******************************************************************************
 * File: MainMenuSampleController.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR;

namespace Qualcomm.Snapdragon.Spaces.Samples
{
    public class MainMenuSampleController : SampleController
    {
        public GameObject ContentGameObject;
        public GameObject ComponentVersionsGameObject;
        public Transform ComponentVersionContent;
        public GameObject ComponentVersionPrefab;

        public ScrollRect ScrollRect;
        public Scrollbar VerticalScrollbar;
        public GameObject GazeScrollButtons;
        public InputActionReference TouchpadInputAction;

        public override void Start() {
            base.Start();

            OnInputSwitch(new InputAction.CallbackContext());

            var baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();
            if (!baseRuntimeFeature) {
                Debug.LogWarning("Base Runtime Feature isn't available.");
                return;
            }

            var componentVersions = baseRuntimeFeature.ComponentVersions;
            for (int i = 0; i < componentVersions.Count; i++) {
                var componentVersionObject = Instantiate(ComponentVersionPrefab, ComponentVersionContent);

                var componentVersionDisplay = componentVersionObject.GetComponent<ComponentVersionDisplay>();
                componentVersionDisplay.ComponentNameText = componentVersions[i].ComponentName;
                componentVersionDisplay.BuildIdentifierText = componentVersions[i].BuildIdentifier;
                componentVersionDisplay.VersionIdentifierText = componentVersions[i].VersionIdentifier;
                componentVersionDisplay.BuildDateTimeText = componentVersions[i].BuildDateTime;
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            SwitchInputAction.action.performed += OnInputSwitch;
            TouchpadInputAction.action.performed += OnTouchpadInput;
        }

        public override void OnDisable() {
            base.OnDisable();
            SwitchInputAction.action.performed -= OnInputSwitch;
            TouchpadInputAction.action.performed -= OnTouchpadInput;
        }

        private void OnTouchpadInput(InputAction.CallbackContext context) {
            var touchpadValue = context.ReadValue<Vector2>();

            if (touchpadValue.y > 0f) {
                OnVerticalScrollViewChanged(0.44f);
            }
            else {
                OnVerticalScrollViewChanged(-0.44f);
            }
        }

        public void OnInfoButtonPress() {
            ContentGameObject.SetActive(!ContentGameObject.activeSelf);
            ComponentVersionsGameObject.SetActive(!ComponentVersionsGameObject.activeSelf);
        }

        public void OnVerticalScrollViewChanged(float value) {
            ScrollRect.verticalNormalizedPosition = Mathf.Clamp01(ScrollRect.verticalNormalizedPosition + value * Time.deltaTime);
            VerticalScrollbar.value = ScrollRect.verticalNormalizedPosition;
        }

        private void OnInputSwitch(InputAction.CallbackContext ctx) {
            if (GazePointer.activeSelf) {
                ScrollRect.verticalScrollbar = null;
                VerticalScrollbar.gameObject.SetActive(false);

                GazeScrollButtons.SetActive(true);
            }
            else if (DevicePointer.activeSelf) {
                ScrollRect.verticalScrollbar = VerticalScrollbar;
                VerticalScrollbar.gameObject.SetActive(true);

                GazeScrollButtons.SetActive(false);
            }
        }
    }
}