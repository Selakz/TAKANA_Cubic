using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ExceptionHandler : MonoBehaviour
{
    // Public
    public static ExceptionHandler Instance => _instance;

    public bool IsQuitWhenException { get; set; }

    public List<Application.LogCallback> LogCallbacks { get; private set; } = new();

    // Private
    private string logPath;
    //private string BugExePath;

    // Static
    private static ExceptionHandler _instance;

    // System Functions
    void Awake()
    {
        _instance = this;

        logPath = Path.Combine(Application.streamingAssetsPath, "Logs");
        //BugExePath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")) + "\\Bug.exe";

        Application.logMessageReceived += Handler;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= Handler;
    }

    void Handler(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            string logFilePath = Path.Combine(logPath, $"{DateTime.Now:yyyy-MM-dd} - Exceptions.log");
            // 打印日志
            Directory.CreateDirectory(logPath);
            File.AppendAllLines(logFilePath, new[] {
                $"[time]: {DateTime.Now}",
                $"[type]: {type}",
                $"[exception message]: {logString}",
                $"[stack trace]: {stackTrace}",
            });

            // 启动bug反馈程序
            //if (File.Exists(BugExePath))
            //{
            //    ProcessStartInfo pros = new ProcessStartInfo();
            //    pros.FileName = BugExePath;
            //    pros.Arguments = "\"" + logPath + "\"";
            //    Process pro = new Process();
            //    pro.StartInfo = pros;
            //    pro.Start();
            //}

            // 调用自定义异常内容
            foreach (var callback in LogCallbacks)
            {
                callback.Invoke(logString, stackTrace, type);
            }

            //退出程序，bug反馈程序重启主程序
            if (IsQuitWhenException)
            {
                Application.Quit();
            }
        }
    }
}
