using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LayerSorter : MonoBehaviour
{
    public int CurrentLayer {  get; private set; }

    public bool IsStatic => isStatic;

    [SerializeField] private bool isStatic = true;
    [SerializeField] private float pivotOffset = 0;

    private SpriteRenderer _renderer;

    private const float _multiplier = -100f;

    private void Start()
    {
        GetRenderer();

        UpdateLayer();

        if (isStatic)
            enabled = false;
    }

    private void Update()
    {
        if (isStatic) return;

        UpdateLayer();
    }

    public void UpdateLayer()
    {
        int layer = Mathf.RoundToInt((transform.position.y + pivotOffset) * _multiplier);

        CurrentLayer = layer;

        GetRenderer().sortingOrder = layer;
    }

    private SpriteRenderer GetRenderer()
    {
        if (_renderer != null) return _renderer;

        _renderer = GetComponent<SpriteRenderer>();

        return _renderer;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        pos.y = transform.position.y + pivotOffset;
        Gizmos.DrawSphere(pos, 0.02f);
    }
#endif
}