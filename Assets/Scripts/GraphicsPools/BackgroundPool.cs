using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace GraphicsPools
{
    public class BackgroundPool
    {
        private static string _backgroundManifestResource = "Backgrounds/manifest";

        private static string _tag = "[BackgroundPool]";
        
        private class BackgroundResource
        {
            public string name;
            public string image;
        }

        private Dictionary<string, string> _backgroundResources;

        private Dictionary<string, Sprite> _backgroundPool;

        public BackgroundPool(bool preload = false)
        {
            var backgroundManifestJson = Resources.Load<TextAsset>(_backgroundManifestResource);
            var backgroundList = JsonConvert.DeserializeObject<List<BackgroundResource>>(backgroundManifestJson.text);
            
            _backgroundResources = new Dictionary<string, string>();
            foreach (var bg in backgroundList)
            {
                _backgroundResources[bg.name.ToLower()] = bg.image;
            }

            _backgroundPool = new Dictionary<string, Sprite>();

            if (preload)
            {
                foreach (var bg in _backgroundResources.Keys)
                {
                    GetBackgroundSprite(bg);
                }
            }
        }


        private Sprite LoadResource(string resource)
        {
            Sprite bgSprite = null;
            if (!_backgroundPool.TryGetValue(resource, out bgSprite))
            {
                Debug.Log($"{_tag} Resource {resource} not loaded yet, caching");
                bgSprite = _backgroundPool[resource] = Resources.Load<Sprite>(resource);
            }

            return bgSprite;
        }

        public Sprite GetBackgroundSprite(string backgroundName)
        {
            string backgroundResource = null;
            if (!_backgroundResources.TryGetValue(backgroundName, out backgroundResource))
            {
                Debug.LogError($"{_tag} Background {backgroundName} not in manifest!");
                return null;
            }

            var backgroundSprite = LoadResource(backgroundResource);
            if (backgroundSprite == null)
            {
                Debug.LogError($"{_tag} Resource {backgroundResource} for background {backgroundName} is missing!");
            }

            return backgroundSprite;
        }
    }
}