using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using LitJson;

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Json数据存取器，支持单个Json数据存储和批量数据存储
/// 支持同步存储和异步存储
/// 批量异步存储可以
/// </summary>
public class CustomJsonMgr
{
    public class JsonSaveTask
    {
        public object data;
        public string filename;
        public JsonSaveTask(object data,string filename)
        {
            this.filename = filename;
            this.data = data;
        }
    }

    public class JsonLoadTask
    {
        public string filename;
        public Type dataType;
        public object data = null;
        public JsonLoadTask(string filename, Type dataType)
        {
            this.dataType = dataType;
            this.filename = filename;
        }
    }

    private static CustomJsonMgr instance = null;
    public static CustomJsonMgr Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CustomJsonMgr();
            }
            return instance;
        }
    }
    private CustomJsonMgr() {
        JsonSaveTasks = new Dictionary<string, JsonSaveTask>();
        JsonLoadTasks = new Dictionary<string, JsonLoadTask>();
    }

    /// <summary>
    /// 同步保存JSON数据方法
    /// </summary>
    /// <param name="data">数据对象</param>
    /// <param name="filename">存储路径(默认在persistentDataPath文件夹下)</param>
    public void SaveData(object data, string filename)
    {
        string path = Application.persistentDataPath + "/" + filename + ".json";
        string jsonStr = JsonMapper.ToJson(data);
        File.WriteAllText(path, jsonStr);
    }

    /// <summary>
    /// 反射同步读取Json数据方法
    /// </summary>
    /// <param name="filename">存储路径(默认在persistentDataPath文件夹下)</param>
    /// <param name="dataType">读取数据的类型</param>
    /// <returns>返回的数据对象</returns>
    public object LoadData(string filename, Type dataType)
    {
        string path = Application.persistentDataPath + "/" + filename + ".json";
        string jsonStr = File.ReadAllText(path);
        object data = JsonMapper.ToObject(jsonStr, dataType);
        return data;
    }

    /// <summary>
    /// 异步保存Json数据方法，请使用Task进行异常处理
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">数据对象</param>
    /// <param name="filename"></param>
    /// <returns>Task实例</returns>

    public async Task SaveDataAsync(object data, string filename)
    {
        string path = Application.persistentDataPath + "/" + filename + ".json";
        string jsonStr = JsonMapper.ToJson(data);
        await File.WriteAllTextAsync(path, jsonStr);
    }

    /// <summary>
    /// 反射异步读取Json数据方法，请使用Task进行异常处理
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="filename">存储路径</param>
    /// <returns>Task<数据对象></returns>
    public async Task<object> LoadDataAsync(string filename, Type dataType)
    {
        string path = Application.persistentDataPath + "/" + filename + ".json";
        string jsonStr = await File.ReadAllTextAsync(path);
        object data = JsonMapper.ToObject(jsonStr, dataType);
        return data;
    }

    //数据存储任务字典
    public Dictionary<string, JsonSaveTask> JsonSaveTasks;
    //数据读取任务字典
    public Dictionary<string, JsonLoadTask> JsonLoadTasks;
    //当前是否有进行中的任务，防止存储和读取同时进行
    //即使同时开启两个协程，协程运行也会有顺序关系
    private bool isRunning = false;

    /// <summary>
    /// 异步存储所有存储任务的协程
    /// </summary>
    /// <param name="afterAction">存储完成后的回调函数</param>
    /// <returns></returns>
    public IEnumerator SaveAllDataCoroutine(UnityAction afterAction = null)
    {
        if (isRunning)
        {
            Debug.LogError("当前有任务正在进行,无法再次开启任务");
            yield break;
        }
        isRunning = true;

        //储存所有异步任务
        List<Task> tasks = new List<Task>();

        //处理所有在队列中的任务
        foreach(var jsontask in JsonSaveTasks.Values)
        {
            tasks.Add(SaveDataAsync(jsontask.data, jsontask.filename));
        }

        //每帧检测所有任务是否完成，如果有任务没完成就继续等待
        bool result = false;
        while (!result)
        {
            yield return null; //至少一帧才能完成任务
            result = true;
            foreach (var task in tasks)
            {
                result = result && task.IsCompleted;
            }
        }

        //任务全部完成后，查看是否有内容存储失败，输出error
        foreach(var task in tasks)
        {
            if(task.IsFaulted)
                Debug.LogException(task.Exception);
        }

        afterAction?.Invoke();
        isRunning = false;
    }

    /// <summary>
    /// 异步读取所有数据的协程
    /// </summary>
    /// <param name="afterAction">回调函数</param>
    /// <returns></returns>
    public IEnumerator LoadAllDataCoroutine(UnityAction afterAction = null)
    {
        if (isRunning)
        {
            Debug.LogError("当前有任务正在进行,无法再次开启任务");
            yield break;
        }
        isRunning = true;

        //读取所有异步任务
        Dictionary<string,Task<object>> tasks = new Dictionary<string,Task<object>>();

        //处理所有在队列中的任务
        foreach (var jsontaskPair in JsonLoadTasks)
        {
            tasks.Add(jsontaskPair.Key
                ,LoadDataAsync(jsontaskPair.Value.filename, jsontaskPair.Value.dataType));
        }

        //每帧检测所有任务是否完成，如果有任务没完成就继续等待
        bool result = false;
        while (!result)
        {
            yield return null; //至少一帧才能完成任务
            result = true;
            foreach (var task in tasks)
            {
                result = result && task.Value.IsCompleted;
            }
        }

        //任务全部完成后，查看是否有内容读取失败，如果失败就打印信息
        foreach (var task in tasks)
        {
            if (task.Value.IsFaulted)
                Debug.LogException(task.Value.Exception);
            else
                JsonLoadTasks[task.Key].data = task.Value.Result;
        }
        afterAction?.Invoke();
        isRunning = false;
    }

    /// <summary>
    /// 清除所有队列中的任务
    /// </summary>
    public void ClearAllTask()
    {
        JsonLoadTasks.Clear();
        JsonSaveTasks.Clear();
    }
}
