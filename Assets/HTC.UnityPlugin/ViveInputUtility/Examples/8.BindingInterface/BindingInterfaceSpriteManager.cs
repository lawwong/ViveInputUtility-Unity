using System.Collections.Generic;
using UnityEngine;

public class BindingInterfaceSpriteManager : MonoBehaviour
{
    private static Dictionary<string, Sprite> s_spriteTable;

    [SerializeField]
    private string[] texturePath;

    private void Awake()
    {
        if (s_spriteTable != null) { return; }

        s_spriteTable = new Dictionary<string, Sprite>();

        foreach (var texture in texturePath)
        {
            if (string.IsNullOrEmpty(texture)) { continue; }

            var atlas = Resources.LoadAll<Sprite>(texture);
            foreach (var sprite in atlas)
            {
                s_spriteTable.Add(sprite.name, sprite);
            }
        }
    }

    public static Sprite GetSprite(string spriteName)
    {
        Sprite sprite;
        if (s_spriteTable == null || !s_spriteTable.TryGetValue(spriteName, out sprite)) { return null; }
        return sprite;
    }
}
