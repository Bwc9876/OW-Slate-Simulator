using System;
using OWML.ModHelper;
using OWML.Common;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace SlatePOV
{
    public class SlatePOV : ModBehaviour
    {
        private Camera _camera;
        private OWCamera _owCamera, _playerCamera;
        private bool _setupBuffer, _camera_initialized;
        private GameObject _playerHead, _playerHelmet, _slateObj, _camObj;

        private void Awake()
        {
            GlobalMessenger.AddListener("PutOnHelmet", UpdateHeadVisibility);
            GlobalMessenger.AddListener("RemoveHelmet", UpdateHeadVisibility);
        }

        private void Start()
        {
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem)
                {
                    _camera_initialized = false;
                    return;
                }
                _slateObj = GameObject.Find("Villager_HEA_Slate");
                _camObj = new GameObject("Slate Camera");
                _camObj.SetActive(false);
                _camera = _camObj.AddComponent<Camera>();
                _camera.enabled = false;
                _owCamera = _camObj.AddComponent<OWCamera>();
                // _camObj.AddComponent<BoxCollider>();
                // _camObj.AddComponent<OWCollider>();
                // SectorDetector anchor = _camObj.AddComponent<SectorDetector>();
                // GameObject.Find("Sector_StartingCamp").GetComponent<Sector>().TrackDetector(anchor);
                // _camObj.layer = LayerMask.NameToLayer("AdvancedDetector");
                _owCamera.renderSkybox = true;
                _setupBuffer = true;
            };
        }

        private void Update()
        {
            if (_camera_initialized)
            {
                YoinkMainCamera();
                try
                {
                    _camObj.transform.LookAt(_playerCamera.transform, _slateObj.transform.up);
                }
                catch (NullReferenceException)
                {
                    _camera_initialized = false;
                }
            }
            if (_setupBuffer)
            {
                _setupBuffer = false;
                SetupCamera();
                UpdateHeadVisibility();
            }
        }

        private void UpdateHeadVisibility()
        {
            try
            {
                if (Locator.GetPlayerSuit().IsWearingHelmet())
                {
                    if (_playerHelmet == null)
                        _playerHelmet = GameObject.Find("Traveller_Mesh_v01:PlayerSuit_Helmet").gameObject;
                    _playerHelmet.layer = 0;
                }
                else
                {
                    if (_playerHead == null) _playerHead = GameObject.Find("player_mesh_noSuit:Player_Head").gameObject;
                    _playerHead.layer = 0;
                }
                GameObject.Find("HelmetRoot").SetActive(false);
            }
            catch (NullReferenceException)
            {
                // Pass
            }
        }

        private void YoinkMainCamera()
        {
            // Switch Cameras
            GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _owCamera);  
            Locator.GetPlayerCamera().mainCamera.enabled = false;
            _camera.enabled = true;
        }

        private void SetupCamera()
        {
             _playerCamera = Locator.GetPlayerCamera();
                
            // Weird Shader Stuff
            FlashbackScreenGrabImageEffect temp = _camObj.AddComponent<FlashbackScreenGrabImageEffect>();
            temp._downsampleShader = _playerCamera.gameObject.GetComponent<FlashbackScreenGrabImageEffect>()._downsampleShader;
            
            PlanetaryFogImageEffect image = _camObj.AddComponent<PlanetaryFogImageEffect>();
            image.fogShader = _playerCamera.gameObject.GetComponent<PlanetaryFogImageEffect>().fogShader;
            
            PostProcessingBehaviour postProcessing = _camObj.AddComponent<PostProcessingBehaviour>();
            postProcessing.profile = _playerCamera.gameObject.GetAddComponent<PostProcessingBehaviour>().profile;
            
            // Setup Positions
            _camObj.SetActive(true);
            _camera.CopyFrom(_playerCamera.mainCamera);
            _camObj.transform.SetParent(_slateObj.transform);
            _camObj.transform.position = _slateObj.transform.position;
            _camObj.transform.rotation = _slateObj.transform.rotation;
            _camObj.transform.localPosition = new Vector3(0, 1.3f, -0.1f);
            
            // Hide Slate
            GameObject.Find("Slate_Skin_01:Slate_Mesh:Villager_HEA_Slate").SetActive(false);
            GameObject.Find("ConversationZone_RSci").SetActive(false);
            _camera_initialized = true;
        }
    }
}
