using UnityEngine;
using static Takana3.MusicGame.Values;

public static class CameraExtension
{
    // G2W和W2G都不考虑相机自身的位置，仅作比例转换，因为游戏中Note、Track等考虑的也是相对于判定线的位置
    /// <summary> G2W = Game2World </summary>
    public static float G2WPosX(this Camera camera, float input)
    {
        float size = camera.orthographicSize;
        float aspect = camera.aspect;
        return input / GameWidthSize * size * aspect;
    }

    /// <summary> G2W = Game2World </summary>
    public static float G2WPosY(this Camera camera, float input)
    {
        float size = camera.orthographicSize;
        return input / GameHeightSize * size;
    }

    /// <summary> W2G = World2Game </summary>
    public static float W2GPosX(this Camera camera, float input)
    {
        float size = camera.orthographicSize;
        float aspect = camera.aspect;
        return input * GameWidthSize / size / aspect;
    }

    /// <summary> W2G = World2Game </summary>
    public static float W2GPosY(this Camera camera, float input)
    {
        float size = camera.orthographicSize;
        return input * GameHeightSize / size;
    }

    /// <summary> 检测某个屏幕点对应的世界位置是否在游戏视口范围内。一般传入Input.mousePosition </summary>
    public static bool ContainsScreenPoint(this Camera camera, Vector3 screenPoint)
    {
        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPoint);
        // 检测鼠标点击是否在相机视口的边界内
        return (worldPosition.x <= camera.G2WPosX(GameWidthSize) + camera.transform.position.x
             && worldPosition.x >= camera.G2WPosX(-GameWidthSize) + camera.transform.position.x
             && worldPosition.y <= camera.G2WPosY(GameHeightSize) + +camera.transform.position.y
             && worldPosition.y >= camera.G2WPosY(-GameHeightSize) + +camera.transform.position.y);
    }

    /// <summary> 缺乏通用性的Takana3编辑器中使用的基于判定线在(0,0)计算的屏幕点对应的相对于判定线的游戏点 </summary>
    public static Vector2 T3ScreenToGamePoint(this Camera camera, Vector3 screenPoint)
    {
        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPoint);
        float x = camera.W2GPosX(worldPosition.x);
        float y = camera.W2GPosY(worldPosition.y);
        return new(x, y);
    }
}
