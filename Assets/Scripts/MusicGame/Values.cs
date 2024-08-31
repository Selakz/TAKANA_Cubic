using System;

namespace Takana3.MusicGame
{
    /// <summary>
    /// �ó��򼯻��õ���һЩ��������
    /// </summary>
    public static class Values
    {
        /// <summary> �����ܷ� </summary>
        public const double MaxScore = 1000000f;

        /// <summary> ��Ϸx������򳤶� </summary>
        public const float GameWidthSize = 5.0f;
        /// <summary> ��Ϸy������򳤶� </summary>
        public const float GameHeightSize = 5.0f;
        /// <summary> ����BaseNote�ĳ�ʼ���ɸ߶� </summary>
        public const float UpHeightLimit = 11.0f;
        /// <summary> ����BaseNote�ĳ�ʼ���ɸ߶� </summary>
        public const float LowHeightLimit = -11.0f;

        /// <summary> Tap��Hold�Ŀ�ȱ�TrackС�Ĺ̶�ֵ </summary>
        public const float TapTrackGap = 0.2f;
        /// <summary> Slide�Ŀ�ȱ�TrackС�Ĺ̶�ֵ </summary>
        public const float SlideTrackGap = 0.4f;

        /// <summary> Ԫ��ֹͣ�˶�������ǰ��ʱ�䣬������Ӧ��������߼� </summary>
        public const float TimeAfterEnd = 2.000f;

        /// <summary> �ؿ��п�ʼ��������ǰ�ĵȴ�ʱ�� </summary>
        public const float TimePreAnimation = 3.000f;

        /// <summary> Hold�м�����ɿ����ʱ�� </summary>
        public const float TimeHoldInterval = 0.250f;

        public const float MinSpeed = 1.0f, MaxSpeed = 10.0f;

        /// <summary>
        /// ��ȡ����ʵ�ٶȡ������ڸ�speed�£�1���ٵ�noteÿ����y�����ƶ�����Ϸ���롣speedȡֵ��ΧΪ[1,10]
        /// </summary>
        public static float ActualSpeed(float speed)
        {
            float advance = (float)(1.0 * Math.Sin((0.85 + 0.056 * speed) * Math.PI) + 1.0);
            return GameHeightSize / advance;
        }

        /// <summary>
        /// �����y = 0Ϊ��׼��ĳһ��Ϸ�߶ȶ�Ӧ��ʱ��
        /// </summary>
        public static float GameYToTime(float current, float speed, float gameY)
        {
            float actualSpeed = ActualSpeed(speed);
            return gameY / actualSpeed + current;
        }

        /// <summary>
        /// �����y = 0Ϊ��׼��ĳһ��Ϸʱ���Ӧ�ĸ߶�
        /// </summary>
        public static float GameTimeToY(float current, float speed, float time)
        {
            float actualSpeed = ActualSpeed(speed);
            return (time - current) * actualSpeed;
        }
    }
}
