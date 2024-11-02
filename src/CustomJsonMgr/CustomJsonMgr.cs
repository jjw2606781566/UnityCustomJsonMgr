using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using LitJson;

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Json���ݴ�ȡ����֧�ֵ���Json���ݴ洢���������ݴ洢
/// ֧��ͬ���洢���첽�洢
/// �����첽�洢����
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
    /// ͬ������JSON���ݷ���
    /// </summary>
    /// <param name="data">���ݶ���</param>
    /// <param name="filename">�洢·��(Ĭ����persistentDataPath�ļ�����)</param>
    public void SaveData(object data, string filename)
    {
        string path = Application.persistentDataPath + "/" + filename + ".json";
        string jsonStr = JsonMapper.ToJson(data);
        File.WriteAllText(path, jsonStr);
    }

    /// <summary>
    /// ����ͬ����ȡJson���ݷ���
    /// </summary>
    /// <param name="filename">�洢·��(Ĭ����persistentDataPath�ļ�����)</param>
    /// <param name="dataType">��ȡ���ݵ�����</param>
    /// <returns>���ص����ݶ���</returns>
    public object LoadData(string filename, Type dataType)
    {
        string path = Application.persistentDataPath + "/" + filename + ".json";
        string jsonStr = File.ReadAllText(path);
        object data = JsonMapper.ToObject(jsonStr, dataType);
        return data;
    }

    /// <summary>
    /// �첽����Json���ݷ�������ʹ��Task�����쳣����
    /// </summary>
    /// <typeparam name="T">��������</typeparam>
    /// <param name="data">���ݶ���</param>
    /// <param name="filename"></param>
    /// <returns>Taskʵ��</returns>

    public async Task SaveDataAsync(object data, string filename)
    {
        string path = Application.persistentDataPath + "/" + filename + ".json";
        string jsonStr = JsonMapper.ToJson(data);
        await File.WriteAllTextAsync(path, jsonStr);
    }

    /// <summary>
    /// �����첽��ȡJson���ݷ�������ʹ��Task�����쳣����
    /// </summary>
    /// <typeparam name="T">��������</typeparam>
    /// <param name="filename">�洢·��</param>
    /// <returns>Task<���ݶ���></returns>
    public async Task<object> LoadDataAsync(string filename, Type dataType)
    {
        string path = Application.persistentDataPath + "/" + filename + ".json";
        string jsonStr = await File.ReadAllTextAsync(path);
        object data = JsonMapper.ToObject(jsonStr, dataType);
        return data;
    }

    //���ݴ洢�����ֵ�
    public Dictionary<string, JsonSaveTask> JsonSaveTasks;
    //���ݶ�ȡ�����ֵ�
    public Dictionary<string, JsonLoadTask> JsonLoadTasks;
    //��ǰ�Ƿ��н����е����񣬷�ֹ�洢�Ͷ�ȡͬʱ����
    //��ʹͬʱ��������Э�̣�Э������Ҳ����˳���ϵ
    private bool isRunning = false;

    /// <summary>
    /// �첽�洢���д洢�����Э��
    /// </summary>
    /// <param name="afterAction">�洢��ɺ�Ļص�����</param>
    /// <returns></returns>
    public IEnumerator SaveAllDataCoroutine(UnityAction afterAction = null)
    {
        if (isRunning)
        {
            Debug.LogError("��ǰ���������ڽ���,�޷��ٴο�������");
            yield break;
        }
        isRunning = true;

        //���������첽����
        List<Task> tasks = new List<Task>();

        //���������ڶ����е�����
        foreach(var jsontask in JsonSaveTasks.Values)
        {
            tasks.Add(SaveDataAsync(jsontask.data, jsontask.filename));
        }

        //ÿ֡������������Ƿ���ɣ����������û��ɾͼ����ȴ�
        bool result = false;
        while (!result)
        {
            yield return null; //����һ֡�����������
            result = true;
            foreach (var task in tasks)
            {
                result = result && task.IsCompleted;
            }
        }

        //����ȫ����ɺ󣬲鿴�Ƿ������ݴ洢ʧ�ܣ����error
        foreach(var task in tasks)
        {
            if(task.IsFaulted)
                Debug.LogException(task.Exception);
        }

        afterAction?.Invoke();
        isRunning = false;
    }

    /// <summary>
    /// �첽��ȡ�������ݵ�Э��
    /// </summary>
    /// <param name="afterAction">�ص�����</param>
    /// <returns></returns>
    public IEnumerator LoadAllDataCoroutine(UnityAction afterAction = null)
    {
        if (isRunning)
        {
            Debug.LogError("��ǰ���������ڽ���,�޷��ٴο�������");
            yield break;
        }
        isRunning = true;

        //��ȡ�����첽����
        Dictionary<string,Task<object>> tasks = new Dictionary<string,Task<object>>();

        //���������ڶ����е�����
        foreach (var jsontaskPair in JsonLoadTasks)
        {
            tasks.Add(jsontaskPair.Key
                ,LoadDataAsync(jsontaskPair.Value.filename, jsontaskPair.Value.dataType));
        }

        //ÿ֡������������Ƿ���ɣ����������û��ɾͼ����ȴ�
        bool result = false;
        while (!result)
        {
            yield return null; //����һ֡�����������
            result = true;
            foreach (var task in tasks)
            {
                result = result && task.Value.IsCompleted;
            }
        }

        //����ȫ����ɺ󣬲鿴�Ƿ������ݶ�ȡʧ�ܣ����ʧ�ܾʹ�ӡ��Ϣ
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
    /// ������ж����е�����
    /// </summary>
    public void ClearAllTask()
    {
        JsonLoadTasks.Clear();
        JsonSaveTasks.Clear();
    }
}
