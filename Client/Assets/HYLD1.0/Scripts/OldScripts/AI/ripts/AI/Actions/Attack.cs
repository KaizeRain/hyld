using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;
public class Attack : BTAction
{
		private string _destinationDataName;
		private int _destinationDataId;
		private Vector3 _destination;



		public Attack(string destinationDataName)
		{
			_destinationDataName = destinationDataName;

		}

		public override void Activate(Database database)
		{
			base.Activate(database);
			_destinationDataId = database.GetDataId(_destinationDataName);
			_trans = database.GetData<Transform>("selfTransform");
		}

		//	protected override void Enter () {
		//		database.GetComponent<Animator>().Play("Run");
		//	}

		protected override BTResult Execute()
		{
			//Logging.HYLDDebug.LogError($"Attack execut开始{ this.GetType().ToString() } destinationDataName ={_destinationDataName}");
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
			HYLDStaticValue.Players[database.GetData<int>("PlayerId")].fireState = FireState.PstolNormal;
			HYLDStaticValue.Players[database.GetData<int>("PlayerId")].fireTowards =_destination;
			return BTResult.Running;
		}
}
