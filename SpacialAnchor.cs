using UnityEngine;
using Oculus.Interaction;

public class GrabbablePositionMemory : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Enable position memory for this object")]
    [SerializeField] private bool _enableMemory = true;

    [Tooltip("Custom ID (defaults to object name)")]
    [SerializeField] private string _customID = "";

    private Grabbable _grabbable;
    private string _saveKey;
    private Transform _targetTransform;
    private bool _wasGrabbed = false;

    void Start()
    {
        _grabbable = GetComponent<Grabbable>();
        if (_grabbable == null)
        {
            Debug.LogError("Grabbable component missing!", this);
            return;
        }

        _targetTransform = _grabbable.transform;
        InitializeMemory();
        LoadPosition();
    }

    void Update()
    {
        if (_grabbable == null) return;

        bool isGrabbed = _grabbable.SelectingPointsCount > 0;

        // Save when released after being grabbed
        if (_wasGrabbed && !isGrabbed && _enableMemory)
        {
            SavePosition();
        }

        _wasGrabbed = isGrabbed;
    }

    void InitializeMemory()
    {
        _saveKey = string.IsNullOrEmpty(_customID) ?
            $"{gameObject.name}_Position" :
            _customID;
    }

    void SavePosition()
    {
        PlayerPrefs.SetFloat($"{_saveKey}_PosX", _targetTransform.position.x);
        PlayerPrefs.SetFloat($"{_saveKey}_PosY", _targetTransform.position.y);
        PlayerPrefs.SetFloat($"{_saveKey}_PosZ", _targetTransform.position.z);

        PlayerPrefs.SetFloat($"{_saveKey}_RotX", _targetTransform.rotation.x);
        PlayerPrefs.SetFloat($"{_saveKey}_RotY", _targetTransform.rotation.y);
        PlayerPrefs.SetFloat($"{_saveKey}_RotZ", _targetTransform.rotation.z);
        PlayerPrefs.SetFloat($"{_saveKey}_RotW", _targetTransform.rotation.w);

        PlayerPrefs.Save();
    }

    void LoadPosition()
    {
        if (!_enableMemory || !PlayerPrefs.HasKey($"{_saveKey}_PosX")) return;

        Vector3 position = new Vector3(
            PlayerPrefs.GetFloat($"{_saveKey}_PosX"),
            PlayerPrefs.GetFloat($"{_saveKey}_PosY"),
            PlayerPrefs.GetFloat($"{_saveKey}_PosZ")
        );

        Quaternion rotation = new Quaternion(
            PlayerPrefs.GetFloat($"{_saveKey}_RotX"),
            PlayerPrefs.GetFloat($"{_saveKey}_RotY"),
            PlayerPrefs.GetFloat($"{_saveKey}_RotZ"),
            PlayerPrefs.GetFloat($"{_saveKey}_RotW")
        );

        _targetTransform.SetPositionAndRotation(position, rotation);
    }

    public void ResetSavedPosition()
    {
        PlayerPrefs.DeleteKey($"{_saveKey}_PosX");
        PlayerPrefs.DeleteKey($"{_saveKey}_PosY");
        PlayerPrefs.DeleteKey($"{_saveKey}_PosZ");
        PlayerPrefs.DeleteKey($"{_saveKey}_RotX");
        PlayerPrefs.DeleteKey($"{_saveKey}_RotY");
        PlayerPrefs.DeleteKey($"{_saveKey}_RotZ");
        PlayerPrefs.DeleteKey($"{_saveKey}_RotW");
    }
}
