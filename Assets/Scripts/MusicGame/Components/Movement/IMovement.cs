using System;
using T3Framework.Runtime;

namespace MusicGame.Components.Movement
{
	/// <summary>
	/// 标识一个记录元件运动信息的列表
	/// </summary>
	public interface IMovement<TPosition> : ISerializable
	{
		public event Action OnMovementUpdated;

		public TPosition GetPos(T3Time time);

		public IMovement<TPosition> Clone(T3Time timeOffset, TPosition positionOffset);
	}
}