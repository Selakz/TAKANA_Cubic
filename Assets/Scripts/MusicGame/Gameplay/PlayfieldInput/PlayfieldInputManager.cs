using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class PlayfieldInputManager : MonoBehaviour
{
    // Serializable and Public
    public PlayfieldInputManager Instance => _instance;

    // Private
    private float Current => TimeProvider.Instance.ChartTime;

    // Static
    private static PlayfieldInputManager _instance;

    // Defined Functions
    public void NewTouchRaycast(Vector2 screenPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue, SelectTarget.Track.GetMask());
        foreach (var raycastHit in raycastHits)
        {
            var track = raycastHit.transform.GetComponent<TrackController>().Info;
            if (track.Notes.Count == 0) continue;
            var timeInput = Current;
            // �ҵ�������ʱ�������note
            float timeDiff = float.MaxValue;
            for (int i = 0; i < track.Notes.Count; i++)
            {
                if (Math.Abs(track.Notes["Judge", i].TimeJudge - timeInput) < timeDiff)
                {
                    timeDiff = Math.Abs(track.Notes["Judge", i].TimeJudge - timeInput);
                    if (i == track.Notes.Count - 1)
                    {
                        track.Notes["Judge", i].HandleInput(timeInput);
                    }
                }
                else
                {
                    track.Notes["Judge", i - 1].HandleInput(timeInput);
                    break;
                }
            }
        }
    }

    // System Functions
    void Awake()
    {
        _instance = this;
    }

    void OnEnable()
    {
        // ���� EnhancedTouch ֧��
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        // ���� EnhancedTouch ֧��
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        // �������е�ǰ����
        foreach (var touch in Touch.activeTouches)
        {
            // ����µĴ���
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                NewTouchRaycast(touch.screenPosition);
            }
        }
    }
}
