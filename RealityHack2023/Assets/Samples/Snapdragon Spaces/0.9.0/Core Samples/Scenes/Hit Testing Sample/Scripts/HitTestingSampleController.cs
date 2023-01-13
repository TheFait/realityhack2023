/******************************************************************************
 * File: PlaneDetectionSampleController.cs
 * Copyright (c) 2021-2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Qualcomm.Snapdragon.Spaces.Samples
{
    public class HitTestingSampleController : SampleController
    {
        public GameObject HitIndicator;
        public GameObject NoHitIndicator;

        private ARRaycastManager _raycastManager;
        private GameObject _activeIndicator;
        private Transform _cameraTransform;
        private bool _isHit = false;

        private Vector3 _desiredPosition;

        public override void Start() {
            base.Start();
            _raycastManager = FindObjectOfType<ARRaycastManager>();
            _cameraTransform = Camera.main.transform;
            _activeIndicator = NoHitIndicator;
            _activeIndicator.SetActive(true);
        }

        public override void Update() {
            base.Update();
            CastRay();
            _activeIndicator.transform.position = _desiredPosition;
        }

        public void CastRay() {
            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);

            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            if (_raycastManager.Raycast(ray, hitResults)) {
                _desiredPosition = hitResults[0].pose.position;

                if (!_isHit) {
                    _activeIndicator.SetActive(false);
                    _activeIndicator = HitIndicator;
                    _activeIndicator.SetActive(true);
                    _isHit = true;
                }
            }
            else {
                _desiredPosition = _cameraTransform.position + _cameraTransform.forward;

                if (_isHit) {
                    _activeIndicator.SetActive(false);
                    _activeIndicator = NoHitIndicator;
                    _activeIndicator.SetActive(true);
                    _isHit = false;
                }
            }
        }
    }
}
