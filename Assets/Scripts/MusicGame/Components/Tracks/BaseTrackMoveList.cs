﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 标识一个记录基本Track运动信息的列表
/// </summary>
public class BaseTrackMoveList : IMoveList, IEnumerable<(float time, float x, string curve)>
{
	// Serializable and Public
	public int Count => moveList.Count;

	public (float time, float x, string curve) this[int index]
	{
		get => moveList[index];
		set
		{
			moveList[index] = value;
			lastIndex = 0;
		}
	}

	public float TimeStart
	{
		get => moveList[0].time;
		set
		{
			if (value > moveList[1].time) return;
			moveList[0] = (value, moveList[0].x, moveList[0].curve);
			lastIndex = 0;
		}
	}

	public float TimeEnd
	{
		get => moveList[^1].time;
		set
		{
			if (value < moveList[^2].time) return;
			moveList[^1] = (value, moveList[^1].x, moveList[^1].curve);
			lastIndex = 0;
		}
	}

	// Private
	private readonly PriorityQueue<(float time, float x, string curve)> moveList;
	private int lastIndex = 0;

	// Defined Functions
	public BaseTrackMoveList(float x, float timeStart, float timeEnd)
	{
		moveList = new(Comparer<(float time, float x, string curve)>.Create((a, b) => a.time.CompareTo(b.time)));
		moveList.Add((timeStart, x, "u"));
		moveList.Add((timeEnd, x, "u"));
	}

	public BaseTrackMoveList(List<(float time, float x, string curve)> moveList, float timeStart, float timeEnd,
		float posEnd)
	{
		moveList.RemoveAll(item => item.time < timeStart || item.time > timeEnd);
		this.moveList = new(moveList,
			Comparer<(float time, float x, string curve)>.Create((x, y) => x.time.CompareTo(y.time)));
		if (moveList.Count == 0) this.moveList.Add((timeStart, posEnd, "u"));
		else this.moveList[0] = (timeStart, this.moveList[0].x, this.moveList[0].curve);
		this.moveList.Add((timeEnd, posEnd, "u"));
	}

	public Vector3 GetPos(float current)
	{
		if (current < TimeStart) return new(moveList[0].x, 0, 0);
		// 更新lastIndex的位置
		if (moveList[lastIndex].time > current) lastIndex = 0;
		while (lastIndex < moveList.Count - 2 && moveList[lastIndex + 1].time < current) lastIndex++;
		// 计算x值
		if (moveList[lastIndex + 1].time - moveList[lastIndex].time == 0) return new(moveList[lastIndex + 1].x, 0, 0);
		float departX = moveList[lastIndex].x, destX = moveList[lastIndex + 1].x;
		float t = (current - moveList[lastIndex].time) / (moveList[lastIndex + 1].time - moveList[lastIndex].time);
		return new(CurveCalculator.GetEaseByName(moveList[lastIndex].curve).CalcCoord(departX, destX, t), 0, 0);
	}

	/// <summary> 当插入时间在[timeStart, timeEnd]外时，不予插入，返回false </summary>
	public bool Insert((float time, float x, string curve) item)
	{
		if (item.time < TimeStart || item.time > TimeEnd) return false;
		moveList.Add(item);
		lastIndex = 0;
		return true;
	}

	/// <summary> 不允许删除开头和结尾的物件：此时返回false </summary>
	public bool Remove((float time, float x, string curve) item)
	{
		if (moveList[0] == item || moveList[^1] == item) return false;
		bool ret = moveList.Remove(item);
		lastIndex = 0;
		return ret;
	}

	public int IndexOf((float time, float x, string curve) item)
	{
		return moveList.IndexOf(item);
	}

	public BaseTrackMoveList Clone(float timeStart, float xStart)
	{
		// 计算时间和位置的偏移量
		float timeOffset = timeStart - moveList[0].time;
		float xOffset = xStart - moveList[0].x;

		// 创建新的列表并填充数据
		List<(float time, float x, string curve)> newMoveList = new();
		// 最后一项会在构造方法中添加
		for (int i = 0; i < moveList.Count - 1; i++)
		{
			var (time, x, curve) = moveList[i];
			newMoveList.Add((time + timeOffset, x + xOffset, curve));
		}

		return new(newMoveList, timeStart, TimeEnd + timeOffset, moveList[^1].x + xOffset);
	}

	/// <summary> 返回不带前缀的pos行内容（含括号和分号） </summary>
	public override string ToString()
	{
		StringBuilder sb = new("(");
		for (int i = 0; i < moveList.Count - 2; i++)
		{
			sb.Append($"{Mathf.RoundToInt(moveList[i].time * 1000f)}, {moveList[i].x:0.00}, {moveList[i].curve}, ");
		}

		sb.Append($"{Mathf.RoundToInt(moveList[^2].time * 1000f)}, {moveList[^2].x:0.00}, {moveList[^2].curve});");
		return sb.ToString();
	}

	public IEnumerator<(float time, float x, string curve)> GetEnumerator()
	{
		for (int i = 0; i < moveList.Count; i++)
		{
			yield return moveList[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}