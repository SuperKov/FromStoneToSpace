using Unity.Multiplayer.PlayMode;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LayerSorterFollower : MonoBehaviour
{
    [SerializeField] private LayerSorter followLayerSorter;
    [SerializeField] private int layerOffset;

    private SpriteRenderer _renderer;

    private void LateUpdate()
    {
        if (followLayerSorter.IsStatic) return;

        UpdateLayer();
    }

    private void UpdateLayer()
    {
        if (followLayerSorter == null)
        {
            Debug.LogError($"У [{name}] отсутсвует ссылка на отслеживаемый LayerSorter!!");
            return;
        }

        GetRenderer().sortingOrder = followLayerSorter.CurrentLayer + layerOffset;
    }

    private SpriteRenderer GetRenderer()
    {
        if (_renderer != null) return _renderer;

        _renderer = GetComponent<SpriteRenderer>();

        return _renderer;
    }
}