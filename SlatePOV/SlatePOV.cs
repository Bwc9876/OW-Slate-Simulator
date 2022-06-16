using System;
using OWML.ModHelper;
using OWML.Common;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace SlatePOV
{
    public class SlatePOV : ModBehaviour
    {
        private Camera _camera;
        private OWCamera _owCamera, _playerCamera;
        private bool _cameraActive, _switchNextFrame;
        private GameObject _slateObj, _camObj;

        private ICommonCameraAPI _commonCameraAPI;

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            try
            {
                _commonCameraAPI = ModHelper.Interaction.GetModApi<ICommonCameraAPI>("xen.CommonCameraUtility");
            }
            catch (Exception e)
            {
                WriteLine($"CommonCameraAPI was not found. {nameof(SlatePOV)} will not run. {e.Message}, {e.StackTrace}", MessageType.Error);
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "SolarSystem")
            {
                _slateObj = GameObject.Find("Villager_HEA_Slate");

                (_owCamera, _camera) = _commonCameraAPI.CreateCustomCamera("SlateCamera");
                _camObj = _camera.gameObject;

                _camObj.gameObject.transform.parent = _slateObj.transform;
                _camObj.transform.rotation = _slateObj.transform.rotation;
                _camObj.transform.localPosition = new Vector3(0, 1.3f, -0.1f);

                // Hide Slate
                GameObject.Find("Slate_Skin_01:Slate_Mesh:Villager_HEA_Slate").SetActive(false);
                GameObject.Find("ConversationZone_RSci").SetActive(false);

                // Hide helmet
                FindObjectOfType<HUDHelmetAnimator>().gameObject.SetActive(false);

                _switchNextFrame = true;
            }
            else
            {
                _cameraActive = false;
            }
        }

        private void SwitchCamera()
        {
            _playerCamera = Locator.GetPlayerCamera();

            _playerCamera.enabled = false;
            _camera.enabled = true;
            GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _owCamera);

            _cameraActive = true;
        }

        private void Update()
        {
            if (_switchNextFrame)
            {
                SwitchCamera();
                _switchNextFrame = false;
            }

            if (_cameraActive)
            {
                try
                {
                    _camObj.transform.LookAt(_playerCamera.transform, _slateObj.transform.up);
                }
                catch { }
            }
        }

        private void WriteLine(string line, MessageType messageType = MessageType.Message)
        {
            ModHelper.Console.WriteLine($"{messageType} : {line}", messageType);
        }

        private void FireOnNextUpdate(Action action)
        {
            ModHelper.Events.Unity.FireOnNextUpdate(action);
        }
    }
}
