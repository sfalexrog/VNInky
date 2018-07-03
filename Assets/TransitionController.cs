using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionController : MonoBehaviour
{

	public Image TransformingImage;
	public Image[] Anchors;

	public Text TransformingText;

	public AnimationCurve TransitionCurve;
	public float TransitionDuration;

	private int _currentAnchor;

	private class AnimGroup
	{
		public List<RectAnimation> RectAnims;
		public List<TextAnimation> TextAnims;

		public float TimeStart;
		public float TimeEnd;

		public AnimGroup()
		{
			RectAnims = new List<RectAnimation>();
			TextAnims = new List<TextAnimation>();

			TimeStart = TimeEnd = Time.time;
		}

		public AnimGroup AddAnimation(RectAnimation ra)
		{
			TimeStart = Mathf.Min(TimeStart, ra.TimeStart);
			TimeEnd = Mathf.Max(TimeEnd, ra.TimeEnd);
			RectAnims.Add(ra);
			return this;
		}

		public AnimGroup AddAnimation(TextAnimation ta)
		{
			TimeStart = Mathf.Min(TimeStart, ta.TimeStart);
			TimeEnd = Mathf.Max(TimeEnd, ta.TimeEnd);
			TextAnims.Add(ta);
			return this;
		}
	}

	private class TextAnimation
	{
		public float TimeStart;
		public float TimeEnd;

		public TextAnimation(float duration, float start, float initialOpactiy, float targetOpacity)
		{
			TimeStart = start;
			TimeEnd = TimeStart + duration;
			InitialOpacity = initialOpactiy;
			TargetOpacity = targetOpacity;
		}

		public float InitialOpacity;
		public float TargetOpacity;

	}

	private class RectAnimation
	{
		public float TimeStart;
		public float TimeEnd;

		public Vector2 InitialPosition;
		public Vector2 TargetPosition;

		public Vector2 InitialSize;
		public Vector2 TargetSize;

		public RectAnimation(float duration, float start, RectTransform initial, RectTransform target)
		{
			TimeStart = start;
			TimeEnd = start + duration;
			InitialPosition = initial.anchoredPosition;
			TargetPosition = target.anchoredPosition;

			InitialSize = new Vector2(initial.rect.width, initial.rect.height);
			TargetSize = new Vector2(target.rect.width, target.rect.height);
		}
	}

	private Queue<AnimGroup> _animQueue;

	public void OnRunAnimation()
	{
		var target = Anchors[_currentAnchor];
		_currentAnchor = (_currentAnchor + 1) % Anchors.Length;
		
		var animGroup = new AnimGroup();
		animGroup.AddAnimation(new TextAnimation(0.3f, Time.time, 1.0f, 0.0f))
			.AddAnimation(new RectAnimation(0.5f, Time.time + 0.3f, TransformingImage.rectTransform,
				target.rectTransform))
			.AddAnimation(new TextAnimation(0.3f, Time.time + 0.8f, 0.0f, 1.0f));
		
		
		_animQueue.Enqueue(animGroup);
	}
	
	// Use this for initialization
	void Start ()
	{
		_currentAnchor = 0;
		_animQueue = new Queue<AnimGroup>();
	}
	
	// Update is called once per frame
	void Update () {
		if (_animQueue.Count > 0)
		{
			var currentAnim = _animQueue.Peek();
			var t = Time.time;
			foreach (var textAnim in currentAnim.TextAnims)
			{
				if (textAnim.TimeStart > t) continue;
				if (textAnim.TimeEnd < t) continue;
				var dt = (t - textAnim.TimeStart) / (textAnim.TimeEnd - textAnim.TimeStart);
				var c = TransitionCurve.Evaluate(dt);
				var currentColor = TransformingText.color;
				currentColor.a = (1 - c) * textAnim.InitialOpacity + c * textAnim.TargetOpacity;
				TransformingText.color = currentColor;
			}

			foreach (var rectAnim in currentAnim.RectAnims)
			{
				if ((rectAnim.TimeStart > t) || (rectAnim.TimeEnd < t)) continue;
				var dt = (t - rectAnim.TimeStart) / (rectAnim.TimeEnd - rectAnim.TimeStart);
				var c = TransitionCurve.Evaluate(dt);
				var transitionPosition = c * rectAnim.TargetPosition + (1 - c) * rectAnim.InitialPosition;
				var transitionSize = c * rectAnim.TargetSize + (1 - c) * rectAnim.InitialSize;
				TransformingImage.rectTransform.anchoredPosition = transitionPosition;
				TransformingImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, transitionSize.x);
				TransformingImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, transitionSize.y);
				
			}

			if (currentAnim.TimeEnd < t)
			{
				_animQueue.Dequeue();
			}
		}
	}
}
