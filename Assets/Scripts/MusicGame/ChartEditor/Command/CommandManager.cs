// Modified from Arcade-Plus: https://github.com/yojohanshinwataikei/Arcade-plus

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CommandManager : MonoBehaviour
{
    // Serializable and Public
    public Button undoButton, redoButton;
    public uint bufferSize = 200;

    public static CommandManager Instance => _instance;

    // Private
    private bool _undoClickable, _redoClickable;
    private bool UndoClickable
    {
        get => _undoClickable;
        set
        {
            if (_undoClickable != value)
            {
                undoButton.interactable = value;
                _undoClickable = value;
            }
        }
    }
    private bool RedoClickable
    {
        get => _redoClickable;
        set
        {
            if (_redoClickable != value)
            {
                redoButton.interactable = value;
                _redoClickable = value;
            }
        }
    }

    private readonly LinkedList<ICommand> undoList = new();
    private readonly LinkedList<ICommand> redoList = new();
    private ICommand preparing = null;

    // Static
    private static CommandManager _instance;

    // Defined Functions
    public void Add(ICommand command)
    {
        if (preparing != null)
        {
            throw new Exception("有正在进行的命令，暂时不能执行新命令");
        }
        command.Do();
        Debug.Log($"执行命令: {command.Name}");
        undoList.AddLast(command);
        if (undoList.Count > bufferSize)
        {
            undoList.RemoveFirst();
        }
        redoList.Clear();
    }

    public void Undo()
    {
        //if (AdeOperationManager.Instance.HasOngoingOperation)
        //{
        //    AdeOperationManager.Instance.CancelOngoingOperation();
        //    return;
        //}
        if (preparing != null)
        {
            //AdeToast.Instance.Show("有正在进行的命令，暂时不能撤销");
            Debug.LogWarning("有正在进行的命令，暂时不能撤销");
            return;
        }
        if (undoList.Count == 0) return;
        ICommand cmd = undoList.Last.Value;
        undoList.RemoveLast();
        cmd.Undo();
        Debug.Log($"撤销指令: {cmd.Name}");
        redoList.AddLast(cmd);
    }

    public void Redo()
    {
        if (preparing != null)
        {
            //AdeToast.Instance.Show("有正在进行的命令，暂时不能重做");
            Debug.LogWarning("有正在进行的命令，暂时不能重做");
            return;
        }
        if (redoList.Count == 0) return;
        ICommand cmd = redoList.Last.Value;
        redoList.RemoveLast();
        cmd.Do();
        Debug.Log($"重做指令: {cmd.Name}");
        undoList.AddLast(cmd);
    }

    public void Cancel()
    {
        if (preparing != null)
        {
            preparing.Undo();
            preparing = null;
        }
    }

    public void Clear()
    {
        Cancel();
        undoList.Clear();
        redoList.Clear();
    }

    public void SetBufferSize(uint size)
    {
        bufferSize = size;
        while (undoList.Count + redoList.Count > bufferSize)
        {
            if (redoList.Count > 0)
            {
                redoList.RemoveFirst();
            }
            else
            {
                undoList.RemoveFirst();
            }
        }
    }

    public void Prepare(ICommand command)
    {
        if (preparing != null)
        {
            throw new Exception("有正在进行的命令，暂时不能准备新命令");
        }
        preparing = command;
        preparing.Do();
    }

    public void Commit()
    {
        if (preparing != null)
        {
            Debug.Log($"执行命令: {preparing.Name}");
            undoList.AddLast(preparing);
            if (undoList.Count > bufferSize)
            {
                undoList.RemoveFirst();
            }
            redoList.Clear();
            preparing = null;
        }
    }

    // System Functions
    void Awake()
    {
        _instance = this;
    }

    void Update()
    {
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.EditorBasic.Undo)) Undo();
        else if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.EditorBasic.Redo)) Redo();
        UndoClickable = undoList.Count != 0;
        RedoClickable = redoList.Count != 0;
    }
}
