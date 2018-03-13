using System;
using System.Collections.Generic;
using UnityEngine;

public class AtlasHelper : SimleManagerTemplate<AtlasHelper>
{
    private Dictionary<string, Dictionary<string, Sprite>> m_sprites = new Dictionary<string, Dictionary<string, Sprite>>();
    private Dictionary<string, Dictionary<string, GameObject>> m_tk2ds = new Dictionary<string, Dictionary<string, GameObject>>();

    public void GetSpriteAsync(string assetBundleName, string spriteName, Action<Sprite> callback)
    {
        Dictionary<string, Sprite> sprites;
        if (!m_sprites.TryGetValue(assetBundleName, out sprites))
        {
            sprites = new Dictionary<string, Sprite>();
            m_sprites.Add(assetBundleName, sprites);
        }

        Sprite sprite;
        if (!sprites.TryGetValue(spriteName, out sprite))
        {
            AssetBundleManager.instance.GetResourceAsync<Sprite>(assetBundleName, spriteName, (asset, ret) =>
            {
                if (!sprites.TryGetValue(spriteName, out sprite))
                {
                    sprites.Add(spriteName, asset);
                }

                if (callback == null || callback.Target == null || "null".Equals(callback.Target.ToString()))
                    return;

                callback(asset);
            });
        }
        else
        {
            if (callback == null || callback.Target == null || "null".Equals(callback.Target.ToString()))
                return;

            callback(sprite);
        }
    }

    public void GetTK2dAnimationAsync(string assetBundleName, string resName, Action<GameObject> callback)
    {
        Dictionary<string, GameObject> sprites;
        if (!m_tk2ds.TryGetValue(assetBundleName, out sprites))
        {
            sprites = new Dictionary<string, GameObject>();
            m_tk2ds.Add(assetBundleName, sprites);
        }

        GameObject sprite;
        if (!sprites.TryGetValue(resName, out sprite))
        {
            AssetBundleManager.instance.GetResourceAsync<GameObject>(assetBundleName, resName, (asset, ret) =>
            {
                if (!sprites.TryGetValue(resName, out sprite))
                {
                    sprites.Add(resName, asset);
                }

                if (callback == null || callback.Target == null || "null".Equals(callback.Target.ToString()))
                    return;

                callback(asset);
            });
        }
        else
        {
            if (callback == null || callback.Target == null || "null".Equals(callback.Target.ToString()))
                return;

            callback(sprite);
        }
    }
}
