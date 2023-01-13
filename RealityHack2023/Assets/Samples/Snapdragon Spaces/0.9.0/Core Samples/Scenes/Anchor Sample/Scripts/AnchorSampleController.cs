/******************************************************************************
 * File: AnchorSampleController.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Qualcomm.Snapdragon.Spaces.Samples
{
    public class AnchorSampleController : SampleController
    {
        public ARAnchorManager AnchorManager;

        public GameObject GizmoTransparent;
        public GameObject GizmoSurface;
        public GameObject GizmoTrackedAnchor;
        public GameObject GizmoUntrackedAnchor;
        public GameObject GizmoSavedAddition;
        public GameObject GizmoNotSavedAddition;
        public GameObject CreateButtonUI;

        public InputActionReference TriggerAction;
        public Toggle SaveNewAnchorsToggle;
        public Text NumberOfAnchorsStoredText;

        public float PlacementDistance = 1f;
        public bool RestrictRaycastDistance = false;
        
        private SpacesAnchorStore _anchorStore;
        private GazeInteractor _gazeInteractor;
        private bool _placeAnchorAtRaycastHit;
        private bool _saveAnchorsToStore;
        private bool _canPlaceAnchorGizmos = true;
        private Transform _cameraTransform;
        private GameObject _indicatorGizmo;
        private GameObject _transparentGizmo;
        private GameObject _surfaceGizmo;
        private List<GameObject> _anchorGizmos = new List<GameObject>();
        private List<GameObject> _sessionGizmos = new List<GameObject>();
        private ARRaycastManager _raycastManager;

        public override void Start() {
            base.Start();
            _raycastManager = FindObjectOfType<ARRaycastManager>();
            _cameraTransform = Camera.main.transform;
            _gazeInteractor = GazePointer.GetComponent<GazeInteractor>();

            _indicatorGizmo = new GameObject("IndicatorGizmo");
            _transparentGizmo = Instantiate(GizmoTransparent, _indicatorGizmo.transform.position, Quaternion.identity, _indicatorGizmo.transform);
            _surfaceGizmo = Instantiate(GizmoSurface, _indicatorGizmo.transform.position, Quaternion.identity, _indicatorGizmo.transform);
            _surfaceGizmo.SetActive(false);
            
            CreateButtonUI.SetActive(GazePointer.activeSelf);

            _anchorStore = FindObjectOfType<SpacesAnchorStore>();
            NumberOfAnchorsStoredText.text = _anchorStore.GetSavedAnchorNames().Length.ToString();

            SaveNewAnchorsToggle.onValueChanged.AddListener(value => _saveAnchorsToStore = value);
            _saveAnchorsToStore = SaveNewAnchorsToggle.isOn;
        }

        public override void OnEnable() {
            base.OnEnable();
            StartCoroutine(LateOnEnable());
        }

        private IEnumerator LateOnEnable() {
            yield return new WaitForSeconds(0.1f);
            AnchorManager.anchorsChanged += OnAnchorsChanged;
            SwitchInputAction.action.performed += UpdateCreateButtonUI;
            TriggerAction.action.performed += OnTriggerAction;
        }

        public override void OnDisable() {
            base.OnDisable();
            AnchorManager.anchorsChanged -= OnAnchorsChanged;
            SwitchInputAction.action.performed -= UpdateCreateButtonUI;
            TriggerAction.action.performed -= OnTriggerAction;
        }

        public void OnCreateButtonClicked() {
            InstantiateGizmos();
        }

        private void OnTriggerAction(InputAction.CallbackContext context) {
            if (!_canPlaceAnchorGizmos) {
                return;
            }
            InstantiateGizmos();
        }

        private void OnAnchorsChanged(ARAnchorsChangedEventArgs args) {
            foreach (var anchor in args.added) {
                _anchorGizmos.Add(anchor.gameObject);
            }

            foreach (var anchor in args.updated) {
                if (anchor.transform.childCount > 0) {
                    Destroy(anchor.transform.GetChild(0).gameObject);
                }
                var newGizmo = Instantiate(anchor.trackingState == TrackingState.None ? GizmoUntrackedAnchor : GizmoTrackedAnchor, anchor.transform);

                if (_anchorStore.GetSavedAnchorNameFromARAnchor(anchor) != string.Empty) {
                    if (newGizmo.transform.childCount > 0) {
                        Destroy(newGizmo.transform.GetChild(0).gameObject);
                    }
                    Instantiate(GizmoSavedAddition, newGizmo.transform);
                }
            }

            foreach (var anchor in args.removed) {
                _anchorGizmos.Remove(anchor.gameObject);
            }
        }

        public override void Update() {
            base.Update();

            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);

            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            if (_raycastManager.Raycast(ray, hitResults)) {
                _placeAnchorAtRaycastHit = !RestrictRaycastDistance ||
                                           (hitResults[0].pose.position - _cameraTransform.position).magnitude <
                                           PlacementDistance;
            }
            else {
                _placeAnchorAtRaycastHit = false;
            }

            if (_placeAnchorAtRaycastHit) {
                if (!_surfaceGizmo.activeSelf) {
                    _surfaceGizmo.SetActive(true);
                    _transparentGizmo.SetActive(false);
                }
                _indicatorGizmo.transform.position = hitResults[0].pose.position;
            }
            else {
                if (_surfaceGizmo.activeSelf) {
                    _surfaceGizmo.SetActive(false);
                    _transparentGizmo.SetActive(true);
                }
                _indicatorGizmo.transform.position = _cameraTransform.position + _cameraTransform.forward * PlacementDistance;
            }
            
            if (GazePointer.activeSelf) {
                _canPlaceAnchorGizmos = !_gazeInteractor.IsHovering;
            }
        }

        public void InstantiateGizmos() {
            var targetPosition = _indicatorGizmo.transform.position;
            var sessionGizmo = _placeAnchorAtRaycastHit ?
                Instantiate(GizmoSurface, targetPosition, Quaternion.identity) :
                Instantiate(GizmoTransparent, targetPosition, Quaternion.identity);
            _sessionGizmos.Add(sessionGizmo);

            var anchorGizmo = new GameObject { transform = { position = targetPosition, rotation = Quaternion.identity}};
            var anchor = anchorGizmo.AddComponent<ARAnchor>();

            if (_saveAnchorsToStore) {
                Instantiate(GizmoNotSavedAddition, anchor.transform);
                _anchorStore.SaveAnchor(anchor, success => {
                    Debug.Log("Save Anchor Success: " + success);
                    NumberOfAnchorsStoredText.text = _anchorStore.GetSavedAnchorNames().Length.ToString();
                });
            }
        }

        public void LoadAllSavedAnchors() {
            _anchorStore.LoadAllSavedAnchors(success => {
                Debug.Log("Load Anchor Success: " + success);
            });
        }

        public void ClearAnchorStore() {
            _anchorStore.ClearStore();
            NumberOfAnchorsStoredText.text = _anchorStore.GetSavedAnchorNames().Length.ToString();
        }

        public void DestroyGizmos() {
            foreach (var anchorGizmo in _anchorGizmos.ToList()) {
                Destroy(anchorGizmo);
            }
            foreach (var gizmo in _sessionGizmos.ToList()) {
                Destroy(gizmo);
            }
            _sessionGizmos.Clear();
        }

        private IEnumerator DestroyGizmosCoroutine() {
            yield return new WaitForEndOfFrame();
            foreach (var anchorGizmo in _anchorGizmos.ToList()) {
                Destroy(anchorGizmo);
            }
            foreach (var gizmo in _sessionGizmos.ToList()) {
                Destroy(gizmo);
            }
            _sessionGizmos.Clear();
        }

        private void UpdateCreateButtonUI(InputAction.CallbackContext ctx) {
            CreateButtonUI.SetActive(GazePointer.activeSelf);
        }

        public void OnPointerEnterEvent() {
            _canPlaceAnchorGizmos = false;
        }

        public void OnPointerExitEvent() {
            _canPlaceAnchorGizmos = true;
        }
    }
}