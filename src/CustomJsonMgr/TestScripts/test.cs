using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public class TestClass
    {
        public string name;
        public int age;
        public Dictionary<string, string> data = new Dictionary<string, string>();
    }
    void Start()
    {
        TestClass testClass1 = new TestClass();
        testClass1.name = "小阙";
        testClass1.age = 1;
        testClass1.data["傻逼"] = "是";

        TestClass testClass2 = new TestClass();
        testClass2.name = "小贾";
        testClass2.age = 2;
        testClass2.data["傻逼"] = "否";

        CustomJsonMgr.Instance.JsonSaveTasks["小阙"] =
            new CustomJsonMgr.JsonSaveTask(testClass1, "xiaoque");
        CustomJsonMgr.Instance.JsonSaveTasks["小贾"] =
            new CustomJsonMgr.JsonSaveTask(testClass2, "xiaojia");

        StartCoroutine(CustomJsonMgr.Instance.SaveAllDataCoroutine(() => { print("存储完成"); }));

        CustomJsonMgr.Instance.JsonLoadTasks["小阙"] =
            new CustomJsonMgr.JsonLoadTask("xiaoque", typeof(TestClass));
        CustomJsonMgr.Instance.JsonLoadTasks["小贾"] =
            new CustomJsonMgr.JsonLoadTask("xiaojia", typeof(TestClass));

        StartCoroutine(CustomJsonMgr.Instance.LoadAllDataCoroutine(
            () =>
            {
                Debug.Log("异步批量读取完成");
                PrintTest(CustomJsonMgr.Instance.JsonLoadTasks["小阙"].data as TestClass);
                PrintTest(CustomJsonMgr.Instance.JsonLoadTasks["小贾"].data as TestClass);
            }));
    }
    void PrintTest(TestClass testClass)
    {
        Debug.Log(testClass.name);
        Debug.Log(testClass.age);
        foreach(var i in testClass.data)
        {
            Debug.Log($"{i.Key}:{i.Value}");
        }
    }
}
