# UnityCustomJsonMgr
基于litjson实现的Json批量异步存储和读取管理类，关于litjson详情https://litjson.net/
使用了C#中反射、委托和异步功能，结合Unity中的协程实现，批量异步存储和读取可以直接使用Unity内部的协程协调器开启异步存取协程
环境为Unity2022.3.43f1c1
默认存储在PersistentDataPath文件夹中，用于打包后也可以实现读写

可提供六种功能
1、同步单个Json存储(直接调用函数)
2、同步单个Json读取(直接调用函数)
3、异步单个Json存储(异步调用函数)
4、异步单个Json读取(异步调用函数)
5、异步批量Json存储(使用协程协调器开启协程)
6、异步批量Json读取(使用协程协调器开启协程)
