using UnityEngine;
using System.Collections;
using BT;
using System;

public class DoRun : BTAction
{

	private string _destinationDataName;
	private int _destinationDataId;

	private Vector3 _destination;
	private float _tolerance = 0.25f;
	private float _speed;

	private Action<float> _DoAniMove;

	public DoRun(string destinationDataName, float speed, Action<float> DoAniMove)
	{
		_destinationDataName = destinationDataName;
		_speed = speed;
		_DoAniMove = DoAniMove;
	}

	public override void Activate(Database database)
	{
		base.Activate(database);

		_destinationDataId = database.GetDataId(_destinationDataName);
		_trans = database.GetData<Transform>("selfTransform");
		//Logging.HYLDDebug.LogError("trans" + _trans);
		//Logging.HYLDDebug.LogError("database"+database);
	}

	//	protected override void Enter () {
	//		database.GetComponent<Animator>().Play("Run");
	//	}

	protected override BTResult Execute()
	{
		//Logging.HYLDDebug.LogError($"execut开始{ this.GetType().ToString() } destinationDataName ={_destinationDataName}");
		UpdateDestination();
		return UpdateFaceDirection();
	}

	private void UpdateDestination()
	{
		_destination = database.GetData<Vector3>(_destinationDataId);
		//Logging.HYLDDebug.LogError($"UpdateDestination{ this.GetType().ToString() } checkTargert ={_destination}");
	}

	private BTResult UpdateFaceDirection()
	{
		/*
		float tempX = _destination.x - _trans.position.x;
		float tempZ = _destination.z - _trans.position.z;

		Vector3 offset = xAndY2UnitVector3(tempX,tempZ);
		*/


		if (CheckArrived()) return BTResult.Ended;
		MoveToDestination();

		return BTResult.Running;
	}

	private bool CheckArrived()
	{
		//Vector3 offset = _destination - _trans.position;

		return _destination.sqrMagnitude < _tolerance * _tolerance;
	}

	private void MoveToDestination()
	{
		//Logging.HYLDDebug.LogError(_destination + "!!!" + _trans.position);de

		//Vector3 start = _trans.position;
		//Vector3 end=start+_destination;

		//_trans.position = Vector3.Lerp(start, end,HYLDStaticValue.Players[database.GetData<int>("PlayerId")].hero.移动速度*Time.deltaTime);
		_trans.Translate((HYLDStaticValue.Players[database.GetData<int>("PlayerId")].移动速度 - 4) * Time.deltaTime * (_destination), Space.World);
		//Logging.HYLDDebug.LogError(Vector3.Distance(HYLDStaticValue.Players[database.GetData<int>("PlayerId")].playerPositon, _trans.position));
		_DoAniMove.Invoke(10*Vector3.Distance(HYLDStaticValue.Players[database.GetData<int>("PlayerId")].playerPositon, _trans.position));
		HYLDStaticValue.Players[database.GetData<int>("PlayerId")].playerPositon = _trans.position;
	}
}
