using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System;
using System.Reflection;


public class DBManager : MonoBehaviour {

	#region 单例
	static DBManager instance;
	public static DBManager Instance(){
		return instance;
	}
	void Awake(){
		instance = this;
	}
	#endregion


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//打开数据库链接
	DbAccess Connect(){
		string dbPath = "data source="+Application.streamingAssetsPath+"/sqliteDB.sqlite";
		DbAccess db = new DbAccess(dbPath);
		return db;
	}
	//关闭数据库链接
	void Close(DbAccess db){
		db.CloseSqlConnection();
	}

	/// <summary>
	/// Demos the test.
	/// </summary>
	public List<MapItem> DemoTest(){
		//用来存储实例化的类
		List<MapItem> itemList = new List<MapItem>();
		//链接数据库,并生成测试表
		DbAccess db = Connect ();
		db.CreateTable("MapItemTemp",new string[]{"id","name","parentId"}, new string[]{"int","text","int"});
		db.InsertInto("MapItemTemp", new string[]{ "1","'城镇'","0"});
		db.InsertInto("MapItemTemp", new string[]{ "2","'村庄'","1"});


		string sql = "select * from MapItemTemp ";
		SqliteDataReader sqReader = db.ExecuteQuery (sql);
		while (sqReader.Read ()) {
			MapItem newItme = CreateObject<MapItem> (sqReader);//将sqReader的数据实例化类

			Debug.Log(newItme.name);
			itemList.Add (newItme);
		}

		//数据清理,并关闭链接
		sql = "drop table MapItemTemp ";
		db.ExecuteQuery (sql);
		Close (db);

		return itemList;
	}

	/// <summary>
	/// Creates the object.
	/// 读取reader中数据,用数据实例化一个类
	/// </summary>
	/// <returns>The object.</returns>
	/// <param name="sqReader">Sq reader.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public T CreateObject<T>(SqliteDataReader sqReader){
		//实例化一个空类
		T newObject = System.Activator.CreateInstance<T>();
		//获取类型
		Type type = newObject.GetType ();
		//获取类型中所有public属性
		FieldInfo[] fields =  type.GetFields ();//BindingFlags.Public);
		//遍历所有属性
		foreach (FieldInfo field in fields) {
			string attrName = field.Name;//获取属性名
			int colIndex = sqReader.GetOrdinal (attrName);//获取这个属性在reader中的位置
			//从reader中取值
			object newValue = new object();
			if(field.FieldType == typeof(int)){
				newValue = sqReader.GetInt32 (colIndex);
			}else if(field.FieldType == typeof(string)){
				newValue = sqReader.GetString (colIndex);
			}
			//赋值给属性
			type.GetField (attrName).SetValue (newObject,newValue);
		}
		return newObject;
	}


}


public class MapItem{
	public int id;
	public string name;
	public int parentId;
}



