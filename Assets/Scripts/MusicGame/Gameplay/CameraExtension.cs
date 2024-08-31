using UnityEngine;
using static Takana3.MusicGame.Values;

public static class CameraExtension
{
    // G2W��W2G����������������λ�ã���������ת������Ϊ��Ϸ��Note��Track�ȿ��ǵ�Ҳ��������ж��ߵ�λ��
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

    /// <summary> ���ĳ����Ļ���Ӧ������λ���Ƿ�����Ϸ�ӿڷ�Χ�ڡ�һ�㴫��Input.mousePosition </summary>
    public static bool ContainsScreenPoint(this Camera camera, Vector3 screenPoint)
    {
        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPoint);
        // ���������Ƿ�������ӿڵı߽���
        return (worldPosition.x <= camera.G2WPosX(GameWidthSize) + camera.transform.position.x
             && worldPosition.x >= camera.G2WPosX(-GameWidthSize) + camera.transform.position.x
             && worldPosition.y <= camera.G2WPosY(GameHeightSize) + +camera.transform.position.y
             && worldPosition.y >= camera.G2WPosY(-GameHeightSize) + +camera.transform.position.y);
    }

    /// <summary> ȱ��ͨ���Ե�Takana3�༭����ʹ�õĻ����ж�����(0,0)�������Ļ���Ӧ��������ж��ߵ���Ϸ�� </summary>
    public static Vector2 T3ScreenToGamePoint(this Camera camera, Vector3 screenPoint)
    {
        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPoint);
        float x = camera.W2GPosX(worldPosition.x);
        float y = camera.W2GPosY(worldPosition.y);
        return new(x, y);
    }
}
