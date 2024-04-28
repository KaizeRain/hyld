using UnityEngine;
using System.Collections;
using BT;

public class CheckInSight : BTPrecondition {
	private float _sightLength;
	private Vector3 _target;
	private PlayerInformation[] _enemys; 
	private Transform _trans;

	protected string 攻击目标;
	protected int 攻击目标id;
	public CheckInSight (float sightLength, PlayerInformation[] enemy,string _攻击目标) {
		_sightLength = sightLength;
		//Logging.HYLDDebug.LogError("gouzao");
		//Logging.HYLDDebug.LogError(enemy.Length);
		_enemys = enemy;
		攻击目标 = _攻击目标;
	}

	public override void Activate (Database database) {
		base.Activate (database);

		_trans = database.GetData<Transform>("selfTransform");
		攻击目标id = database.GetDataId(攻击目标);
	}

	public override bool Check () {
		float min = 9999999;
		//Logging.HYLDDebug.LogError($"enemyName={_enemys.Length}");
		for (int i = 0; i < _enemys.Length; i++)
		{
			if(_enemys[i].isNotDie)
			{
				float temp = (_enemys[i].playerPositon - _trans.position).magnitude;
				if (min > temp)
				{
					min = temp;
					_target = _enemys[i].playerPositon;
				}
			}
			//Logging.HYLDDebug.LogError($"targetGo={_enemys[i]}");
			
		}

		//Logging.HYLDDebug.LogError(_target);
		if (_target == null) return false;

		Vector3 offset = _target - _trans.position;
		database.SetData<Vector3>(攻击目标id, offset.normalized);
		return offset.sqrMagnitude <= _sightLength * _sightLength;
	}
}
