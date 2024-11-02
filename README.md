# UnityCustomJsonMgr
\n基于litjson实现的Json批量异步存储和读取管理类，关于litjson详情https://litjson.net/
\n使用了C#中反射、委托和异步功能，结合Unity中的协程实现，批量异步存储和读取可以直接使用Unity内部的协程协调器开启异步存取协程
\n环境为Unity2022.3.43f1c1
\n存储在PersistentDataPath文件夹中，用于打包后也可以实现读写
\n
\n可提供六种功能
\n1、同步单个Json存储(直接调用函数)
\n2、同步单个Json读取(直接调用函数)
\n3、异步单个Json存储(异步调用函数)
\n4、异步单个Json读取(异步调用函数)
\n5、异步批量Json存储(使用协程协调器开启协程)
\n6、异步批量Json读取(使用协程协调器开启协程)
