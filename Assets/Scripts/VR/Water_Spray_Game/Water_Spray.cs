using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class Water_Spray : MonoBehaviour, Oculus.Interaction.HandGrab.IHandGrabUseDelegate
{
    public static Water_Spray instance;

    [Header("Input")]
    [SerializeField] private Transform _trigger;
    [SerializeField] private Transform _nozzle;
    [SerializeField] private AnimationCurve _triggerRotationCurve;
    [SerializeField] private SnapAxis _axis = SnapAxis.X;
    [SerializeField][Range(0f, 1f)] private float _releaseThresold = 0.3f;
    [SerializeField][Range(0f, 1f)] private float _fireThresold = 0.9f;
    [SerializeField] private float _triggerSpeed = 2f;
    [SerializeField] private AnimationCurve _strengthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private Plant_Progress plantProgress;

    [Header("Output")]
    [SerializeField, Tooltip("Masks the Raycast used to find objects to make wet")] private LayerMask _raycastLayerMask = ~0;
    [SerializeField, Tooltip("The spread angle when spraying, larger values will make a larger area wet")] private float _spraySpreadAngle = 40;
    [SerializeField] private float _sprayStrength = 1.5f;
    [SerializeField] private int _sprayHits = 6;
    [SerializeField] private float _sprayRandomness = 6f;
    [SerializeField, Tooltip("The max distance of the spray, controls the raycast and shader")] private float _maxDistance = 2;
    [SerializeField] private float _dryingSpeed = 0.1f;
    [SerializeField, Tooltip("Material for applying a stamp, should using the MeshBlitStamp shader or similar")] private Material _sprayStampMaterial = null;
    [SerializeField, Tooltip("When not null, will be set as the '_WetBumpMap' property on wet renderers")] private Texture _waterBumpOverride = null;
    [SerializeField] private UnityEvent WhenSpray;
    private static readonly int WET_MAP_PROPERTY = Shader.PropertyToID("_WetMap");
    private static readonly int STAMP_MULTIPLIER_PROPERTY = Shader.PropertyToID("_StampMultipler");
    private static readonly int SUBTRACT_PROPERTY = Shader.PropertyToID("_Subtract");
    private static readonly int WET_BUMPMAP_PROPERTY = Shader.PropertyToID("_WetBumpMap");
    private static readonly int STAMP_MATRIX_PROPERTY = Shader.PropertyToID("_StampMatrix");
    private static readonly WaitForSeconds WAIT_TIME = new WaitForSeconds(0.1f);
    private bool _wasFired = false;
    public bool hitPlant { get; private set; } = false;
    private float _dampedUseStrength = 0;
    private float _lastUseTime;


    private Vector3 Original_Pos;

    #region input
    void Start()
    {
        if (instance == null)
            instance = this;
        Original_Pos = transform.position;
    }
    void Update()
    {
        RaycastHit hit;
        hitPlant = false;
        if (Physics.Raycast(_nozzle.position, _nozzle.forward, out hit, _maxDistance, _raycastLayerMask))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                hitPlant = true;

            }
            else
            {
                hitPlant = false;

            }
        }
        if (GameStateManager.Instance.game_Status == GameStateManager.Game_Status.SprayGameStart)
        {
            SprayGameManager.Instance.Display_SprayGameTutorial(!hitPlant);
        }



    }

    private void UpdateTriggerRotation(float progress)
    {
        float value = _triggerRotationCurve.Evaluate(progress);
        Vector3 angles = _trigger.localEulerAngles;
        if ((_axis & SnapAxis.X) != 0)
        {
            angles.x = value;
        }
        if ((_axis & SnapAxis.Y) != 0)
        {
            angles.y = value;
        }
        if ((_axis & SnapAxis.Z) != 0)
        {
            angles.z = value;
        }
        _trigger.localEulerAngles = angles;
    }
    #endregion

    #region output
    private void Spray()
    {
        StartCoroutine(StampRoutine(_sprayHits, _sprayRandomness, _spraySpreadAngle, _sprayStrength));
    }


    private IEnumerator StampRoutine(int stampCount, float randomness, float spread, float strength)
    {

        // If player successes, change level.
        if (hitPlant && plantProgress.In_Range)
        {
            plantProgress.Change_Success_Dot(plantProgress.Level_index - 1, 1);
            plantProgress.Change_Level();
        }


        StartStamping();
        Pose originalPose = _nozzle.GetPose();
        for (int i = 0; i < stampCount; i++)
        {
            yield return WAIT_TIME;
            Pose randomPose = originalPose;
            randomPose.rotation =
                randomPose.rotation *
                Quaternion.Euler(
                    Random.Range(-randomness, randomness),
                    Random.Range(-randomness, randomness),
                    0f);

            Stamp(randomPose, _maxDistance, spread, strength);
        }
        StartDrying();
    }


    private void StartStamping()
    {
        _sprayStampMaterial.SetFloat(SUBTRACT_PROPERTY, 0);
    }

    private void StartDrying()
    {
        _sprayStampMaterial.SetMatrix(STAMP_MATRIX_PROPERTY, Matrix4x4.zero);
        _sprayStampMaterial.SetFloat(SUBTRACT_PROPERTY, _dryingSpeed);
    }

    private void Stamp(Pose pose, float maxDistance, float angle, float strength)
    {
        _sprayStampMaterial.SetMatrix(STAMP_MATRIX_PROPERTY, CreateStampMatrix(pose, angle));
        _sprayStampMaterial.SetFloat(STAMP_MULTIPLIER_PROPERTY, strength);

        float radius = Mathf.Tan(Mathf.Deg2Rad * angle / 2) * maxDistance;
        Vector3 startPoint = pose.position + pose.forward * radius;
        Vector3 endPoint = pose.position + pose.forward * maxDistance;
        int hitCount = Physics.OverlapCapsuleNonAlloc(startPoint, endPoint, radius, NonAlloc._overlapResults, _raycastLayerMask.value, QueryTriggerInteraction.Ignore);
        HashSet<Transform> roots = NonAlloc.GetRootsFromOverlapResults(hitCount);


        foreach (Transform rootObject in roots)
        {
            RenderSplash(rootObject);
        }
        roots.Clear();
    }


    private void RenderSplash(Transform rootObject)
    {
        List<MeshFilter> meshFilters = NonAlloc.GetMeshFiltersInChildren(rootObject);
        for (int i = 0; i < meshFilters.Count; i++)
        {
            int id = meshFilters[i].GetInstanceID();
            if (!NonAlloc._blits.ContainsKey(id)) { NonAlloc._blits[id] = CreateMeshBlit(meshFilters[i]); }
            NonAlloc._blits[id].Blit();
        }

    }


    private Mesh_Blit CreateMeshBlit(MeshFilter meshFilter)
    {
        Mesh_Blit newBlit = meshFilter.gameObject.AddComponent<Mesh_Blit>();
        newBlit.material = _sprayStampMaterial;
        newBlit.renderTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.RHalf);
        newBlit.BlitsPerSecond = 30;

        if (meshFilter.TryGetComponent(out Renderer renderer))
        {
            renderer.GetPropertyBlock(NonAlloc.PropertyBlock);

            NonAlloc.PropertyBlock.SetTexture(WET_MAP_PROPERTY, newBlit.renderTexture);
            if (_waterBumpOverride)
            {
                NonAlloc.PropertyBlock.SetTexture(WET_BUMPMAP_PROPERTY, _waterBumpOverride);
            }

            renderer.SetPropertyBlock(NonAlloc.PropertyBlock);
        }

        return newBlit;
    }

    private Matrix4x4 CreateStampMatrix(Pose pose, float angle)
    {
        Matrix4x4 viewMatrix = Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one).inverse;
        viewMatrix.m20 *= -1f;
        viewMatrix.m21 *= -1f;
        viewMatrix.m22 *= -1f;
        viewMatrix.m23 *= -1f;
        return GL.GetGPUProjectionMatrix(Matrix4x4.Perspective(angle, 1, 0, _maxDistance), true) * viewMatrix;
    }

    private void OnDestroy()
    {
        NonAlloc.CleanUpDestroyedBlits();
    }

    public void BeginUse()
    {
        _dampedUseStrength = 0f;
        _lastUseTime = Time.realtimeSinceStartup;
    }

    public void EndUse()
    {

    }

    public float ComputeUseStrength(float strength)
    {
        float delta = Time.realtimeSinceStartup - _lastUseTime;
        _lastUseTime = Time.realtimeSinceStartup;
        if (strength > _dampedUseStrength)
        {
            _dampedUseStrength = Mathf.Lerp(_dampedUseStrength, strength, _triggerSpeed * delta);
        }
        else
        {
            _dampedUseStrength = strength;
        }
        float progress = _strengthCurve.Evaluate(_dampedUseStrength);
        UpdateTriggerProgress(progress);
        return progress;
    }

    private void UpdateTriggerProgress(float progress)
    {
        UpdateTriggerRotation(progress);

        if (progress >= _fireThresold && !_wasFired)
        {
            _wasFired = true;
            Spray();
            WhenSpray?.Invoke();
        }
        else if (progress <= _releaseThresold)
        {
            _wasFired = false;
        }
    }

    #endregion
    static class NonAlloc
    {
        public static readonly Collider[] _overlapResults = new Collider[12];
        public static readonly Dictionary<int, Mesh_Blit> _blits = new Dictionary<int, Mesh_Blit>();
        public static MaterialPropertyBlock PropertyBlock => _block != null ? _block : _block = new MaterialPropertyBlock();

        private static readonly List<MeshFilter> _meshFilters = new List<MeshFilter>();
        private static readonly HashSet<Transform> _roots = new HashSet<Transform>();
        private static MaterialPropertyBlock _block;

        public static List<MeshFilter> GetMeshFiltersInChildren(Transform root)
        {
            root.GetComponentsInChildren(_meshFilters);
            return _meshFilters;
        }

        public static HashSet<Transform> GetRootsFromOverlapResults(int hitCount)
        {
            _roots.Clear();
            for (int i = 0; i < hitCount; i++)
            {
                Transform root = GetRoot(_overlapResults[i]);
                _roots.Add(root);
            }
            return _roots;
        }

        /// <summary>
        /// Returns the most likely 'root object' for the hit e.g. the rigidbody
        /// </summary>
        static Transform GetRoot(Collider hit)
        {
            return hit.attachedRigidbody ? hit.attachedRigidbody.transform :
                hit.transform.parent ? hit.transform.parent :
                hit.transform;
        }

        public static void CleanUpDestroyedBlits()
        {
            if (!_blits.ContainsValue(null)) { return; }

            foreach (int key in new List<int>(_blits.Keys))
            {
                if (_blits[key] == null) _blits.Remove(key);
            }
        }
    }
}

