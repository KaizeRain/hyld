using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BT {

	/// <summary>
	/// BTAction is the base class for behavior node.
	/// 
	/// It cannot add / remove child.
	/// 
	/// Override the following to build a behavior (all are optional):
	/// - Enter
	/// - Execute
	/// - Exit
	/// - Clear
	/// </summary>
	public class BTAction : BTNode {
		private BTActionStatus _status = BTActionStatus.Ready;
		public Transform _trans;
		public BTAction (BTPrecondition precondition = null) : base (precondition) {}


		protected virtual void Enter () {
			if (BTConfiguration.ENABLE_BTACTION_LOG) {	// For debug
				Logging.HYLDDebug.Log("Enter " + this.name + " [" + this.GetType().ToString() + "]");
			}
		}

		protected virtual void Exit () {
			if (BTConfiguration.ENABLE_BTACTION_LOG) {	// For debug
				Logging.HYLDDebug.Log("Exit " + this.name + " [" + this.GetType().ToString() + "]");
			}
		}

		protected virtual BTResult Execute () {
			if (BTConfiguration.ENABLE_BTACTION_LOG)
			{   // For debug
				Logging.HYLDDebug.Log("Execute " + this.name + " [" + this.GetType().ToString() + "]");
			}
			return BTResult.Running;
		}
		
		public override void Clear () {
			if (_status != BTActionStatus.Ready) {	// not cleared yet
				Exit();
				_status = BTActionStatus.Ready;
			}
		}
		
		public override BTResult Tick () {
			if (BTConfiguration.ENABLE_BTACTION_LOG)
			{   // For debug
				Logging.HYLDDebug.Log("Tick " + this.name + " [" + this.GetType().ToString() + "]"+"当前的status="+_status);
			}
			BTResult result = BTResult.Ended;
			if (_status == BTActionStatus.Ready) {
				Enter();
				_status = BTActionStatus.Running;
			}
			if (_status == BTActionStatus.Running) {		// not using else so that the status changes reflect instantly
				result = Execute();
				//Logging.HYLDDebug.LogError($"Tick过程{this.GetType().ToString() } result={result} _status={_status}");
				if (result != BTResult.Running) {
					Exit();
					_status = BTActionStatus.Ready;
				}
			}
			//Logging.HYLDDebug.LogError($"Tick完成{this.GetType().ToString() } result={result} _status={_status}");
			return result;
		}

		public override void AddChild (BTNode aNode) {
			Logging.HYLDDebug.LogError("BTAction: Cannot add a node into BTAction.");
		}

		public override void RemoveChild (BTNode aNode) {
			Logging.HYLDDebug.LogError("BTAction: Cannot remove a node into BTAction.");
		}

		protected Vector3 xAndY2UnitVector3(float x, float z)
		{
			//Logging.HYLDDebug.LogError(x + "?:AD" + z);
			float temp = Mathf.Sqrt(x * x + z * z);
			if (0 == temp) temp = 0.001f;
			float sin1 = x / (float.Parse(temp.ToString()));
			float cos1 = (float.Parse(Mathf.Sqrt(1 - sin1 * sin1).ToString()));
			
			//Logging.HYLDDebug.LogError(cos1);
			return new Vector3(sin1, 0, z > 0 ? cos1 : -cos1);
		}
		private enum BTActionStatus {
			Ready = 1,
			Running = 2,
		}
	}
}