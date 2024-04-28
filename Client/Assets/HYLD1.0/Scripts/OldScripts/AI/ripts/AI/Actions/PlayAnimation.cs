using UnityEngine;
using System.Collections;
using BT;

public class PlayAnimation : BTAction {
	private string _animationName;


	public PlayAnimation(string animationName, BTPrecondition precondition = null) : base(precondition)
	{
		_animationName = animationName;
	}

	protected override void Enter()
	{
		//Animator animator = database.GetComponent<Animator>();
		if (_animationName == "Run")
		{
			Logging.HYLDDebug.LogError("Run");
		}
		else if (_animationName == "Fight")
		{
			Logging.HYLDDebug.LogError("Fight");
			HYLDStaticValue.Players[database.GetData<int>("PlayerId")].fireState = FireState.PstolNormal;
		}
		else
		{

			Logging.HYLDDebug.LogError("Idle");
		}
		//animator.Play(_animationName);
	}
}
