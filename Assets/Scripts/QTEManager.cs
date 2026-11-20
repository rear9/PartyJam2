using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Spawns one QTE and configures its prompt and timing.
public class QTEManager : MonoBehaviour
{
    [Serializable]
    private class KeyPrompt // pairs prompt sprite with a keyboard key, serialized to have flexibility in adding a custom amount of QTE inputs
    {
        public Sprite sprite;
        public Key key;
    }

    [Header("Spawn References")]
    [SerializeField] private QTECore _qtePrefab;
    [SerializeField] private RectTransform _qteContainer;

    [Header("Prompt Options")]
    [SerializeField] private KeyPrompt[] _prompts;

    [Header("QTE Tuning")]
    [SerializeField, Min(0.1f)] private float _shrinkDuration = 2f;
    [SerializeField, Min(0f)] private float _timingWindow = 0.15f;

    [Header("Results")]
    [SerializeField] private UnityEvent _onSuccess;
    [SerializeField] private UnityEvent _onMiss;

    private QTECore _activeQTE;
    private Key _lastKey = Key.None;

    public void SpawnQTE() // public since I'm connecting a button in editor for a proof of concept
    {
        if (_activeQTE != null) return;
        if (_qtePrefab == null || _qteContainer == null) return;

        KeyPrompt prompt = GetRandomPrompt();
        if (prompt == null) return;

        _activeQTE = Instantiate(_qtePrefab, _qteContainer, false);
        RectTransform rect = _activeQTE.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }

        _lastKey = prompt.key;
        _activeQTE.Initialise(prompt.sprite, prompt.key, _shrinkDuration, _timingWindow, HandleQTEResult);
    }

    private KeyPrompt GetRandomPrompt() // selects random QTE key from KeyPrompt class (checks to prevent duplicate in a row)
    {
        if (_prompts == null || _prompts.Length == 0) return null;

        List<KeyPrompt> options = new();
        foreach (KeyPrompt prompt in _prompts)
        {
            if (prompt == null || prompt.sprite == null || prompt.key == Key.None || prompt.key == _lastKey) continue;
            options.Add(prompt);
        }

        if (options.Count == 0) return null;
        return options[UnityEngine.Random.Range(0, options.Count)];
    }

    private void HandleQTEResult(bool success) // hit or miss handler through event invoke
    {
        _activeQTE = null;
        if (success)
        {
            Debug.Log("[QTEManager] QTE Success", this);
            _onSuccess?.Invoke();
        }
        else
        {
            Debug.Log("[QTEManager] QTE Miss", this);
            _onMiss?.Invoke();
        }
    }
}
