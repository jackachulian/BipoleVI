using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    public static Camera MainCamera => _singleton._camera;


    [SerializeField] private CinemachineVirtualCamera _followCamera, _aimingCamera;
    public static CinemachineVirtualCamera followCamera => _singleton._followCamera;
    public static CinemachineVirtualCamera aimingCamera => _singleton._aimingCamera;
    
    [SerializeField] private CinemachineBrain _cameraBrain;

    [SerializeField] private LayerMask _aimLayer;

    private static CameraManager _singleton = null;
    public static CameraManager singleton => _singleton;

    private bool _aiming;
    public bool aiming {
        get { return _aiming; }
        set { _aiming = value; }
    }

    private Vector3 _aimTargetPoint = Vector3.zero;
    public Vector3 aimTargetPoint => _aimTargetPoint;

    private void Awake() {
        _singleton = this;

        _cameraBrain.m_DefaultBlend.m_Time = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        _aimingCamera.gameObject.SetActive(_aiming);
        SetAimTarget();
    }

    private void SetAimTarget() {
        Ray ray = _camera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _aimLayer)) {
            _aimTargetPoint = hit.point;
        }
        else {
            _aimTargetPoint = ray.GetPoint(1000);
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_aimTargetPoint, 0.1f);
    }
    #endif
}
