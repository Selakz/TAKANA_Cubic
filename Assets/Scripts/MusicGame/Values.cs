using System;

namespace Takana3.MusicGame
{
    /// <summary>
    /// 该程序集会用到的一些常量定义
    /// </summary>
    public static class Values
    {
        /// <summary> 谱面总分 </summary>
        public const double MaxScore = 1000000f;

        /// <summary> 游戏x轴的正向长度 </summary>
        public const float GameWidthSize = 5.0f;
        /// <summary> 游戏y轴的正向长度 </summary>
        public const float GameHeightSize = 5.0f;
        /// <summary> 正速BaseNote的初始生成高度 </summary>
        public const float UpHeightLimit = 11.0f;
        /// <summary> 负速BaseNote的初始生成高度 </summary>
        public const float LowHeightLimit = -11.0f;

        /// <summary> Tap和Hold的宽度比Track小的固定值 </summary>
        public const float TapTrackGap = 0.2f;
        /// <summary> Slide的宽度比Track小的固定值 </summary>
        public const float SlideTrackGap = 0.4f;

        /// <summary> 元件停止运动后到销毁前的时间，方便响应其他组合逻辑 </summary>
        public const float TimeAfterEnd = 2.000f;

        /// <summary> 关卡中开始播放音乐前的等待时间 </summary>
        public const float TimePreAnimation = 3.000f;

        /// <summary> Hold中间可以松开的最长时间 </summary>
        public const float TimeHoldInterval = 0.250f;

        public const float MinSpeed = 1.0f, MaxSpeed = 10.0f;

        /// <summary>
        /// 获取“真实速度”，即在该speed下，1倍速的note每秒在y轴上移动的游戏距离。speed取值范围为[1,10]
        /// </summary>
        public static float ActualSpeed(float speed)
        {
            float advance = (float)(1.0 * Math.Sin((0.85 + 0.056 * speed) * Math.PI) + 1.0);
            return GameHeightSize / advance;
        }

        /// <summary>
        /// 获得以y = 0为基准的某一游戏高度对应的时间
        /// </summary>
        public static float GameYToTime(float current, float speed, float gameY)
        {
            float actualSpeed = ActualSpeed(speed);
            return gameY / actualSpeed + current;
        }

        /// <summary>
        /// 获得以y = 0为基准的某一游戏时间对应的高度
        /// </summary>
        public static float GameTimeToY(float current, float speed, float time)
        {
            float actualSpeed = ActualSpeed(speed);
            return (time - current) * actualSpeed;
        }
    }
}
