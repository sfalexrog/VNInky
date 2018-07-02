using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionController : MonoBehaviour
{

	public Image TransformingImage;
	public Image[] Anchors;

	public AnimationCurve TransitionCurve;
	public float TransitionDuration;

	private int _currentAnchor;

	private class PendingAnimation
	{
		public float TimeStart;
		public float TimeEnd;

		public Vector2 InitialPosition;
		public Vector2 TargetPosition;

		public Vector2 InitialSize;
		public Vector2 TargetSize;


	}

	private PendingAnimation _animation;

	public void OnRunAnimation()
	{
		var target = Anchors[_currentAnchor];
		_currentAnchor = (_currentAnchor + 1) % Anchors.Length;
		
		_animation = new PendingAnimation();
		_animation.TimeStart = Time.time;
		_animation.TimeEnd = _animation.TimeStart + TransitionDuration;
		_animation.InitialPosition = TransformingImage.rectTransform.anchoredPosition;
		_animation.TargetPosition = target.rectTransform.anchoredPosition;

		var initialRect = TransformingImage.rectTransform.rect;
		_animation.InitialSize = new Vector2(initialRect.width, initialRect.height);
		var targetRect = target.rectTransform.rect;
		_animation.TargetSize = new Vector2(targetRect.width, targetRect.height);
	}
	
	// Use this for initialization
	void Start ()
	{
		_currentAnchor = 0;
		_animation = null;
	}
	
	// Update is called once per frame
	void Update () {
		if (_animation != null)
		{
			var dt = (Time.time - _animation.TimeStart) / (_animation.TimeEnd - _animation.TimeStart);
			var c = TransitionCurve.Evaluate(dt);
			var transitionPosition = c * _animation.TargetPosition + (1 - c) * _animation.InitialPosition;
			var transitionSize = c * _animation.TargetSize + (1 - c) * _animation.InitialSize;
			TransformingImage.rectTransform.anchoredPosition = transitionPosition;
			TransformingImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, transitionSize.x);
			TransformingImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, transitionSize.y);
			


			if (Time.time > _animation.TimeEnd) _animation = null;
		}
	}
}
