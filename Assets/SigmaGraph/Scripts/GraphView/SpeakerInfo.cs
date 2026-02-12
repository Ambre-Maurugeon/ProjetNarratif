using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SpeakerInfo
{
    public const string AssetExtension = "simpleg";
    
    public string Name;
    public string DisplayName;
    public Espeaker speakEnum;
    public List<SpriteHumeur> SpritesHumeur;
    
    public Sprite GetSpriteForHumeur(HumeurSpeaker humeur)
    {
        foreach (var spriteHumeur in SpritesHumeur)
        {
            if (spriteHumeur.humeur == humeur)
            {
                return spriteHumeur.sprite;
            }
        }
        return null;
    }
}

[System.Serializable]
public class SpriteHumeur
{
    public Sprite sprite;
    public HumeurSpeaker humeur;
}

public enum HumeurSpeaker
{
    Colere,
    Shy,
    Neutre
}
