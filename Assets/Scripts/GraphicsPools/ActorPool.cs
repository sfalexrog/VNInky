using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace GraphicsPools
{
    // Resource pool for actors
    public class ActorPool
    {
        
        private static string _actorManifestResource = "Actors/manifest";

        private static string _tag = "[ActorPool]";

        // Actor descriptor from the manifest
        private class ActorDescriptor
        {
            // Actor name
            public string name;
            // Key-value pairs of emotions and related resources
            public Dictionary<string, string> emotions;
        }
        
        // Actor descriptors, filled in by name
        private Dictionary<string, ActorDescriptor> _actors;

        // Actor sprites, filled in by thier resource name
        private Dictionary<string, Sprite> _actorSpritePool;
        
        public ActorPool(bool preloadActors = false)
        {
            var actorManifestJson = Resources.Load<TextAsset>(_actorManifestResource);
            var actorList = JsonConvert.DeserializeObject<List<ActorDescriptor>>(actorManifestJson.text);
            
            _actors = new Dictionary<string, ActorDescriptor>();
            foreach (var actor in actorList)
            {
                _actors[actor.name] = actor;
            }
            _actorSpritePool = new Dictionary<string, Sprite>();

            if (preloadActors)
            {
                Debug.Log($"{_tag} Preloading all actors; this may improve performance, but will use more memory");
                foreach (var actor in _actors.Values)
                {
                    foreach (var emotion in actor.emotions)
                    {
                        GetActorSprite(actor.name, emotion.Key);
                    }
                }
            }
        }

        private Sprite LoadResource(string resource)
        {
            Sprite actorSprite = null;
            if (!_actorSpritePool.TryGetValue(resource, out actorSprite))
            {
                Debug.Log($"{_tag} Actor resource {resource} not loaded yet, caching");
                actorSprite = _actorSpritePool[resource] = Resources.Load<Sprite>(resource);
            }

            return actorSprite;
        }

        public Sprite GetActorSprite(string actorName, string actorEmotion = "default")
        {
            ActorDescriptor actor = null;
            if (!_actors.TryGetValue(actorName, out actor))
            {
                Debug.LogError($"{_tag} Actor name {actorName} is not in the manifest");
                return null;
            }

            string actorResource = null;
            if (!actor.emotions.TryGetValue(actorEmotion, out actorResource))
            {
                Debug.LogWarning($"{_tag} Emotion {actorEmotion} is not in descriptor for {actorName}, substituting default emotion instead");
                if (!actor.emotions.TryGetValue("default", out actorResource))
                {
                    Debug.LogError($"{_tag} Default emotion is not in descriptor for {actorName}, bailing out");
                    return null;
                }
            }

            var actorSprite = LoadResource(actorResource);
            if (actorSprite == null)
            {
                Debug.LogError($"{_tag} Resource {actorResource} for {actorName} (emotion {actorEmotion}) is missing!");
            }

            return actorSprite;
        }
        
        public HashSet<string> GetActorNames()
        {
            return new HashSet<string>(_actors.Keys.ToList());

        }
    }
}