using UnityEngine;

namespace SlatePOV
{
    public interface ICommonCameraAPI
    {
        void RegisterCustomCamera(OWCamera OWCamera);
        (OWCamera, Camera) CreateCustomCamera(string name);
    }
}
