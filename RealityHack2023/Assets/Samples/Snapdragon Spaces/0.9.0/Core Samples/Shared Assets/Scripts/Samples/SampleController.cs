/******************************************************************************
 * File: SampleController.cs
 * Copyright (c) 2021-2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using InputDevice = UnityEngine.XR.InputDevice;

namespace Qualcomm.Snapdragon.Spaces.Samples
{
    public enum PointerType {
        GazePointer = 0,
        ControllerPointer = 1
    }

    public enum XRControllerProfile {
        HostController = 0,
        XRControllers = 1
    }

    public class SampleController : MonoBehaviour
    {
        public GameObject GazePointer;
        public GameObject DevicePointer;
        public InputActionReference SwitchInputAction;

        protected virtual bool ResetSessionOriginOnStart => true;

        private XRControllerProfile _xrControllerProfile;
        private XRControllerManager _xrControllerManager;
        private bool _isSessionOriginMoved = false;
        private Transform _camera;
        private const string _controllerTypePrefsKey = "Qualcomm.Snapdragon.Spaces.Samples.Prefs.ControllerType";

        public virtual void Start() {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            _camera = Camera.main.transform;

            int controllerType = PlayerPrefs.GetInt(_controllerTypePrefsKey, 0);
            GazePointer.SetActive(controllerType == (int) PointerType.GazePointer);
            DevicePointer.SetActive(controllerType == (int) PointerType.ControllerPointer);

            RegisterXRProfiles();
            _xrControllerManager = DevicePointer.GetComponent<XRControllerManager>();
            SendControllerProfileToManager(_xrControllerManager);
        }

        public virtual void Update() {
            if (ResetSessionOriginOnStart && !_isSessionOriginMoved && _camera.position != Vector3.zero) {
                OffsetSessionOrigin();
                _isSessionOriginMoved = true;
            }
        }

        public void Quit() {
            Application.Quit();
        }

        protected void OffsetSessionOrigin() {
             ARSessionOrigin sessionOrigin = FindObjectOfType<ARSessionOrigin>();
             sessionOrigin.transform.Rotate(0.0f, -_camera.rotation.eulerAngles.y, 0.0f, Space.World);
             sessionOrigin.transform.position = -_camera.position;
            /* Also rotate the device pointers parent so that it has the same origin as the AR Camera. */
            if (_xrControllerManager != null) {
                _xrControllerManager.ResetPositionAndRotation(sessionOrigin.transform);
            }
        }

        public virtual void OnEnable() {
            SwitchInputAction.action.performed += OnSwitchInput;
            InputDevices.deviceConnected += RegisterConnectedDevice;
            RegisterXRProfiles();
        }

        public virtual void OnDisable() {
            SwitchInputAction.action.performed -= OnSwitchInput;
            InputDevices.deviceDisconnected -= RegisterConnectedDevice;
        }

        private void OnSwitchInput(InputAction.CallbackContext ctx) {
            if (ctx.interaction is TapInteraction) {
                GazePointer.SetActive(!GazePointer.activeSelf);
                DevicePointer.SetActive(!DevicePointer.activeSelf);
            }

            if (ctx.interaction is HoldInteraction) {
                Quit();
            }

            int controllerType = GazePointer.activeSelf ? (int) PointerType.GazePointer :
                                 DevicePointer.activeSelf ? (int) PointerType.ControllerPointer : 0;

            PlayerPrefs.SetInt(_controllerTypePrefsKey, controllerType);
        }
        
        private void RegisterXRProfiles() {
            List<InputDevice> inputDevices = new List<InputDevice>();
            InputDevices.GetDevices(inputDevices);
            foreach (var inputDevice in inputDevices) {
                RegisterConnectedDevice(inputDevice);
            }
        }

        private void RegisterConnectedDevice(InputDevice inputDevice) {
            if (inputDevice.name.Contains("Oculus")) {
                _xrControllerProfile = XRControllerProfile.XRControllers;
            }
            else {
                _xrControllerProfile = XRControllerProfile.HostController;
            }
        }

        private void SendControllerProfileToManager(XRControllerManager xrControllerManager) {
            if (xrControllerManager != null) {
                xrControllerManager.ActivateController(_xrControllerProfile);
            }
        }
    }
}