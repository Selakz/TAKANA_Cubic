#nullable enable

using System;

namespace T3Framework.Static.Collections
{
	public class RandomIntervalGenerator
	{
		public int BufferSize { get; }
		public float MinEdge { get; set; } = 0;
		public float MaxEdge { get; set; } = 1;
		public float MinSize { get; set; } = 0;
		public float MaxSize { get; set; } = 1;

		private readonly float[] intervalLengthBuffer;
		private readonly float[] gapLengthBuffer;
		private readonly Interval[] intervalBuffer;
		private readonly Func<double> valueGetter;

		public RandomIntervalGenerator(int bufferSize, Func<double>? valueGetter = null)
		{
			BufferSize = bufferSize;
			intervalLengthBuffer = new float[bufferSize];
			gapLengthBuffer = new float[bufferSize + 1];
			intervalBuffer = new Interval[bufferSize];
			this.valueGetter = valueGetter ?? new Random().NextDouble;
		}

		public ReadOnlySpan<Interval> Generate(int maxCount)
		{
			maxCount = Math.Min(maxCount, BufferSize);

			float totalRange = MaxEdge - MinEdge;
			int actualCount = 0;
			float currentSum = 0;
			for (int i = 0; i < maxCount; i++)
			{
				float nextLen = (float)(valueGetter.Invoke() * (MaxSize - MinSize) + MinSize);
				if (currentSum + nextLen > totalRange) break;

				intervalLengthBuffer[i] = nextLen;
				currentSum += nextLen;
				actualCount++;
			}

			float remainingSpace = totalRange - currentSum;
			double weightSum = 0;
			for (int i = 0; i < actualCount + 1; i++)
			{
				gapLengthBuffer[i] = (float)valueGetter.Invoke();
				weightSum += gapLengthBuffer[i];
			}

			for (int i = 0; i < actualCount + 1; i++)
			{
				gapLengthBuffer[i] = (float)(remainingSpace * (gapLengthBuffer[i] / weightSum));
			}

			float currentPos = MinEdge;
			for (int i = 0; i < actualCount; i++)
			{
				currentPos += gapLengthBuffer[i];
				float start = currentPos;
				currentPos += intervalLengthBuffer[i];
				intervalBuffer[i] = new Interval { Left = start, Right = currentPos };
			}

			return intervalBuffer.AsSpan(0, actualCount);
		}
	}
}