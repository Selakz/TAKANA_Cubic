using System;
using UnityEngine;
using static Takana3.MusicGame.Values;

public class TrackController : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private Highlight2D highlight;
    [SerializeField] private Transform scalePart;
    [SerializeField] private Transform sprite;
    [SerializeField] private Transform leftLine;
    [SerializeField] private Transform rightLine;
    [SerializeField] private Transform laneBeam;
    [SerializeField] private Animator laneBeamAnimator;
    [SerializeField] private BoxCollider2D boxCollider;

    public Track Info => _track;
    public float GameWidth
    {
        get => Camera.main.W2GPosX(sprite.localScale.x);
        private set
        {
            sprite.localScale = new(Camera.main.G2WPosX(value), sprite.localScale.y);
            leftLine.localPosition = new(Camera.main.G2WPosX(-value / 2), 0);
            rightLine.localPosition = new(Camera.main.G2WPosX(value / 2), 0);
            boxCollider.size = new(Mathf.Max(Camera.main.G2WPosX(value), Camera.main.G2WPosX(0.5f)), boxCollider.size.y);
        }
    }
    public float GamePos
    {
        get => Camera.main.W2GPosX(transform.localPosition.x);
        private set
        {
            transform.localPosition = new(Camera.main.G2WPosX(value), 0);
        }
    }

    // Private
    private float Current => TimeProvider.Instance.ChartTime;

    private Track _track;

    // Defined Functions
    public void InfoInit(Track track)
    {
        _track = track;
    }

    private void SpriteInit()
    {
        sprite.localScale = new(Camera.main.G2WPosX(1f), Camera.main.G2WPosY(30f));
        laneBeam.localScale = new(1, 0.1f);
        leftLine.localScale = new(Camera.main.G2WPosX(0.015f), Camera.main.G2WPosY(30f));
        rightLine.localScale = new(Camera.main.G2WPosX(0.015f), Camera.main.G2WPosY(30f));
        boxCollider.size = new(Camera.main.G2WPosX(1f), Camera.main.G2WPosY(30f));

        if (!_track.IsVisible || Current > _track.TimeEnd)
        {
            sprite.gameObject.SetActive(false);
            leftLine.gameObject.SetActive(false);
            rightLine.gameObject.SetActive(false);
        }
    }

    private void UpdatePosAndWidth()
    {
        float lx = _track.GetX(Current, true), rx = _track.GetX(Current, false);
        GameWidth = Math.Abs(lx - rx);
        GamePos = (lx + rx) / 2;
    }

    public void LaneHitEffect(JudgeResult judgeResult)
    {
        laneBeam.gameObject.SetActive(true);
        if (laneBeam.gameObject.activeInHierarchy) laneBeamAnimator.Play(0);
    }

    public void Highlight()
    {
        highlight.Highlight();
    }

    public void Dehighlight()
    {
        highlight.Dehighlight();
    }

    // System Functions
    void Start()
    {
        SpriteInit();
        UpdatePosAndWidth();
    }

    void Update()
    {
        UpdatePosAndWidth();

        if (Current > _track.TimeEnd + TimeAfterEnd)
        {
            Destroy(gameObject);
            return;
        }
        else if (Current > _track.TimeEnd)
        {
            sprite.gameObject.SetActive(false);
            leftLine.gameObject.SetActive(false);
            rightLine.gameObject.SetActive(false);
            boxCollider.enabled = false;
            return;
        }
    }
}
