using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BT;

// How to use:
// 1. Initiate values in the database for the children to use.
// 2. Initiate BT _root
// 3. Some actions & preconditions that will be used later
// 4. Add children nodes
// 5. Activate the _root, including the children nodes' initialization

public abstract class BTTree : MonoBehaviour {
	protected BTNode _root = null;

	[HideInInspector]
	public Database database;
	public Transform selfTransform;
	[HideInInspector]
	public bool isRunning = true;

	public const string RESET = "Rest";
	private static int _resetId;
	protected bool isRotate = false;
	protected List<GameObject> Obstacles = new List<GameObject>();
	private float timer = 0;
	void Start()
	{
		/*
		Init();
		_root.Activate(database);
		*/
	}
	protected bool CheckPosition(Transform selfTransform,Vector3 targetPos, float angle, float radius)

	{
		var cosAngle = Mathf.Cos(Mathf.Deg2Rad * angle * 0.5f); //以一位单位，取得Cos角度

		Vector3 circleCenter = selfTransform.position;

		Vector3 disV = targetPos - circleCenter;//从圆心到目标的方向

		float dis2 = disV.sqrMagnitude; // 得到 长度平方和

		if (dis2 < radius * radius) // 视距内 

		{

			disV.y = 0.0f;

			disV = disV.normalized; //向量除以它的长度   向量归一化   对向量  

			//开平方 得到向量长度    

			//归一化后，即是 单位向量了。

			//用当前物体 向前方向，和从圆心到目标的单位方向 做 点积；

			float cos = Vector3.Dot(selfTransform.forward, disV);//求点积

			//这样的结果就得到了cos角度*1

			if (cos - cosAngle >= 0)

			{

				return true; //在视野内

			}

		}

		return false;
	}
	private void FixedUpdate()
	{
		return;
		if (Toolbox.是否游戏结束) return;
		if (HYLDStaticValue.ModenName == "HYLDTryGame") return;
		/*
		if (timer>1f)
		{
			timer = 0;
			foreach (GameObject a in Obstacles)
			{
				if (CheckPosition(selfTransform, a.transform.position, 40, 20))
				{
					Logging.HYLDDebug.LogError(selfTransform);
					isRotate = true;
					selfTransform.Rotate(new Vector3(0, 10, 0));
					selfTransform.Translate(2 * Time.deltaTime * selfTransform.forward, Space.World);
					HYLDStaticValue.Players[database.GetData<int>("PlayerId")].playerPositon = selfTransform.position;
					isRotate = false;
					return;
				}
			}
		}
		*/
		//Logging.HYLDDebug.LogError(isRunning);
		//Logging.HYLDDebug.LogError(isRotate);
		//if (isRotate) return;
		if (_root == null) return;
		if (!isRunning) return;
		//Logging.HYLDDebug.LogError(database.GetData<bool>(RESET));
		if (database.GetData<bool>(RESET)) {
			Reset();	
			database.SetData<bool>(RESET, false);
		}
		//Logging.HYLDDebug.LogError(_root);
		//if(_root.Evaluate())
		//Logging.HYLDDebug.Log($"{_root}.Evaluate=---->( {_root.children[0]}    ,  {_root.children[1]}   )");
		// Iterate the BT tree now!
		if (_root.Evaluate()) {
			if (BTConfiguration.ENABLE_BTACTION_LOG)
			{   // For debug
				//Logging.HYLDDebug.Log($"{_root.children[0]}->{_root.children[0].children[0]}+{_root.children[0].children[1]}");
				//Logging.HYLDDebug.Log($"{_root.children[1]}->{_root.children[1].children[0]}+{_root.children[1].children[1]}");
				//Logging.HYLDDebug.Log(_root.children[2]);
				Logging.HYLDDebug.Log("Tick " + this.name + " [" + this.GetType().ToString() + "]");
			}
			
			_root.Tick();
		}
	}

	void OnDestroy () {
		if (_root != null) {
			_root.Clear();
		}
	}

	// Need to be called at the initialization code in the children.
	protected virtual void Init () {
		
		database = GetComponent<Database>();
		if (database == null) {
			database = gameObject.AddComponent<Database>();
		}

		_resetId = database.GetDataId(RESET);
		database.SetData<bool>(_resetId, false);
	}

	protected void Reset () {
		if (_root != null) {
			_root.Clear();	
		}
	}
}
