using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// TODO: 学会Shader然后用Shader来描边和高亮。。。
// 目前是专为编辑界面适用的非常sb逻辑
public class Highlight2D : MonoBehaviour
{
    public Sprite highlightSprite;
    public int highlightLayer;

    //private GameObject edge;
    private SpriteRenderer spriteRenderer = null;
    private Sprite originSprite;
    private int originLayer;
    //private Material original;
    //private Material highlight;
    //private Material silhouette;

    public void Highlight()
    {
        // spriteRenderer.material = highlight;

        //if (edge == null)
        //{
        //    edge = new GameObject("Edge");
        //    edge.transform.SetParent(transform.parent);
        //    var sr = edge.AddComponent<SpriteRenderer>();
        //    sr.material = silhouette;
        //}
        //edge.SetActive(true);

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = highlightSprite;
            spriteRenderer.sortingOrder = highlightLayer;
        }
    }

    public void Dehighlight()
    {
        //spriteRenderer.material = original;
        //if (edge != null) edge.SetActive(false);
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = originSprite;
            spriteRenderer.sortingOrder = originLayer;
        }
    }

    // System Functions

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originSprite = spriteRenderer.sprite;
        originLayer = spriteRenderer.sortingOrder;
        //highlight = Resources.Load<Material>("Material/MT_Highlight");
        //original = spriteRenderer.material;
        //silhouette = Resources.Load<Material>("Material/MT_Silhouette");
    }

    //void Update()
    //{
    //    if (edge != null && edge.activeInHierarchy)
    //    {
    //        edge.transform.SetLocalPositionAndRotation(transform.localPosition, transform.localRotation);
    //        edge.transform.localScale = transform.localScale;
    //    }
    //}
}
