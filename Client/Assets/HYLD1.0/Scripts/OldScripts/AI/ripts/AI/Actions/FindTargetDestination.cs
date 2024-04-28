using UnityEngine;
using System.Collections;
using BT;



public abstract class FindTargetDestination : BTAction {
	protected Vector3 _target;
	protected PlayerInformation[] _enemys;
	protected string _destinationDataName;
	protected int _destinationDataId;

	protected bool 可以追 = false;
	protected Transform _trans;

	public FindTargetDestination(PlayerInformation[] enemys, string destinationDataName, BTPrecondition precondition = null) : base(precondition) {
		_enemys = enemys;
		_destinationDataName = destinationDataName;
	}

	public override void Activate(Database database) {
		base.Activate(database);

		_trans = database.GetData<Transform>("selfTransform");
		_destinationDataId = database.GetDataId(_destinationDataName);
	}
	//zhidaozuijindemubiao
	protected int changeTarget()
	{
		float min = 9999999;
		int ans = -1;
		for (int i = 0; i < _enemys.Length; i++)
		{
			if (_enemys[i].isNotDie)
			{
				float temp = (_enemys[i].playerPositon - _trans.position).magnitude;
				if (min > temp)
				{
					min = temp;
					ans = i;
				}
			}
		

		}
		可以追 = false;
		if (ans == -1) ans = 0;

		if (HYLDStaticValue.Players[ans].playerBloodValue < 1200)
			可以追 = true;
		

		return ans;
	}
	protected Vector3 GetToTargetOffset() {
		_target = _enemys[changeTarget()].playerPositon;
		
		if (!checkTarget()) {
			return Vector3.zero;
		}

		return _target - _trans.position;
	}

	protected bool checkTarget() {
		return _target != null;
	}

	protected Vector3 getDestination(Vector3 _destination){
		float tempX = _destination.x - _trans.position.x;
		float tempZ = _destination.z - _trans.position.z;
		 Vector3 offset=xAndY2UnitVector3(tempX, tempZ);
		return offset;
	}
}


public class FindEscapeDestination : FindTargetDestination {
	private float _safeDistance;
	
	public FindEscapeDestination (PlayerInformation[] enemys, string destinationDataName, float safeDistance, BTPrecondition precondition = null) : base (enemys, destinationDataName, precondition) {
		_safeDistance = safeDistance;
	}
	
	protected override BTResult Execute () {
		Logging.HYLDDebug.LogError(HYLDStaticValue.Players[database.GetData<int>("PlayerId")].isNotDie);
		if (!HYLDStaticValue.Players[database.GetData<int>("PlayerId")].isNotDie)
			return BTResult.Ended;
		if (!checkTarget()) {
			return BTResult.Ended;
		}
		
		Vector3 offset = GetToTargetOffset();
		Vector3 destination = getDestination(_target);
		_trans.LookAt(_trans.position + destination);
		if (offset.sqrMagnitude <= _safeDistance * _safeDistance) {
			database.SetData<Vector3>(_destinationDataId, destination);
			return BTResult.Running;
		}

		return BTResult.Ended;
	}
}


public class FindToTargetDestination : FindTargetDestination {
	private float _minDistance;

	public FindToTargetDestination(PlayerInformation[] enemys, string destinationDataName, float minDistance, BTPrecondition precondition = null) : base(enemys, destinationDataName, precondition)
	{
		_minDistance = minDistance;
	}

	protected override BTResult Execute() {
		//Logging.HYLDDebug.LogError($"execut开始{ this.GetType().ToString() } checkTargert ={ checkTarget()}");
		if (!HYLDStaticValue.Players[database.GetData<int>("PlayerId")].isNotDie)
			return BTResult.Ended;
		if (!checkTarget()) {
			return BTResult.Ended;
		}

		Vector3 offset = GetToTargetOffset();
		//Logging.HYLDDebug.LogError($"execut过程{ this.GetType().ToString()}   offset={offset.sqrMagnitude}  >?? minDistance={_minDistance*_minDistance}");

		Vector3 destination = getDestination(_target);

		if (MyRandom.MyRandomRange(0, 100) > 90)
			destination += new Vector3(MyRandom.MyRandomRange(-1f, 1f), 0, MyRandom.MyRandomRange(-1f, 1f));
		destination *= MyRandom.MyRandomRange(-1, 200) > 0 ? MyRandom.MyRandomRange(1f, 1.5f) : MyRandom.MyRandomRange(-1.5f, -1f);


		//Logging.HYLDDebug.LogError(_trans);
		_trans.LookAt(_trans.position + destination);

		if (可以追 || offset.sqrMagnitude >= _minDistance * _minDistance)
		{
			database.SetData<Vector3>(_destinationDataId, destination);
			return BTResult.Running;
		}
		else if (offset.sqrMagnitude < _minDistance * _minDistance - 0.5f)
		{
			database.SetData<Vector3>(_destinationDataId, -destination);
			return BTResult.Running;
		}
		else return BTResult.Ended;
	}
}