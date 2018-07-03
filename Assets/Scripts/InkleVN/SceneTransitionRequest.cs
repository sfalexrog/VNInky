using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace InkleVN
{
    public class SceneTransitionRequest
    {
        public string TransitionBackground { get; private set; }

        public string TransitionSpeaker { get; private set; }

        public string TransitionSpeakerEmotion { get; private set; }

        public string TransitionPhrase { get; private set; }
        
        public string[] TransitionChoices { get; private set; }

        private SceneTransitionRequest()
        {
            
        }

        public class Builder
        {
            private SceneTransitionRequest _str;

            private List<string> _choices;

            public Builder()
            {
                _str = new SceneTransitionRequest();
            }

            public Builder SetBackground(string backgroundName)
            {
                _str.TransitionBackground = backgroundName;
                return this;
            }

            public Builder SetSpeaker(string speakerName, string speakerEmotion = "default")
            {
                _str.TransitionSpeaker = speakerName;
                _str.TransitionSpeakerEmotion = speakerEmotion;
                return this;
            }

            public Builder SetPhrase(string phrase)
            {
                _str.TransitionPhrase = phrase;
                return this;
            }

            public Builder AddChoice(string choice)
            {
                if (_choices == null) _choices = new List<string>();
                _choices.Add(choice);
                return this;
            }

            public SceneTransitionRequest Build()
            {
                if (_choices != null)
                {
                    _str.TransitionChoices = _choices.ToArray();
                }
                return _str;
            }
        }
    }
}