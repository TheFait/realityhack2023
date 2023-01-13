/******************************************************************************
 * File: XRControllerManager.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces.Samples
{
    public class XRControllerManager : MonoBehaviour
    {
        public GameObject HostController;
        public GameObject XRControllers;

        private XRControllerProfile _xrControllerProfile;
        
        public void ActivateController(XRControllerProfile xrControllerProfile) {
            _xrControllerProfile = xrControllerProfile;
            switch (xrControllerProfile) {
                case XRControllerProfile.XRControllers: {
                    HostController.SetActive(false);
                    XRControllers.SetActive(true);
                    break;
                }
                default: {
                    XRControllers.SetActive(false);
                    HostController.SetActive(true);
                    break;
                }
            }
        }

        public void ResetPositionAndRotation(Transform newTransform) {
            transform.rotation = newTransform.rotation;
            switch (_xrControllerProfile) {
                case XRControllerProfile.XRControllers: {
                    transform.position = newTransform.position;
                    break;
                }
            }
        }
    }
}
