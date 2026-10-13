using System.Collections.Generic;
using MainProject.Animations;
using System.Collections;
using UnityEngine;

public class FrameAnimator : MonoBehaviour
{
    public event System.Action<string> Completed;
    public event System.Action<string> SpecialEvent;

    [SerializeField] private SpriteRenderer Renderer;
    [SerializeField] private List<AnimationEntry> _animations;
    [SerializeField] private bool _playOnAwake;

    private Dictionary<string, AnimationConfig> _animMap = new();
    private Coroutine _currentAnim;
    private string _currentAnimID;

    public AnimationConfig CurrentAnimation { get; private set; }

    private void Awake()
    {
        if (Renderer == null)
        {
            Debug.LogError($"[FrameAnimator] на объекте {gameObject.name} не нашел SpriteRenderer!");
            enabled = false;
            return;
        }

        InitDictionary();

        if (_playOnAwake)
            Play(_animations[0].ID);
    }

    public void Play(string id, bool ignoreRepeat = false)
    {
        if (!ignoreRepeat)
            if (_currentAnimID == id) return;

        if (!_animMap.TryGetValue(id, out AnimationConfig config)) return;

        if (_currentAnim != null) 
            StopCoroutine(_currentAnim);

        CurrentAnimation = config;

        _currentAnimID = id;
        _currentAnim = StartCoroutine(Animate(config, config.SpecialEventId));
    }

    public void Play(string id, float speed, bool ignoreRepeat = false)
    {
        if (!ignoreRepeat)
            if (_currentAnimID == id) return;

        if (!_animMap.TryGetValue(id, out AnimationConfig config)) return;

        if (_currentAnim != null)
            StopCoroutine(_currentAnim);

        _currentAnimID = id;
        _currentAnim = StartCoroutine(Animate(config, config.SpecialEventId, speed));
    }

    public void Stop()
    {
        if (_currentAnim != null)
            StopCoroutine(_currentAnim);

        _currentAnimID = "";
    }

    private IEnumerator Animate(AnimationConfig config, string speciaEventlId, float speed = 1)
    {
        int index = 0;
        while (true)
        {
            Renderer.sprite = config.Frames[index].Sprite;
            if (config.Frames[index].ActivateEvent && speciaEventlId != "")
                SpecialEvent?.Invoke(speciaEventlId);
            yield return new WaitForSeconds(config.Frames[index].Duration * speed);

            index++;
            if (index >= config.Frames.Length)
            {
                Completed?.Invoke(_currentAnimID);
                if (config.Loop)
                    index = 0;
                else
                {
                    _currentAnim = null;
                    _currentAnimID = "";
                    CurrentAnimation = null;

                    yield break;
                }
            }
        }
    }

    private void InitDictionary()
    {
        foreach (var anim in _animations)
            if (anim.Config != null && !_animMap.ContainsKey(anim.ID))
                _animMap.Add(anim.ID, anim.Config);
    }
}