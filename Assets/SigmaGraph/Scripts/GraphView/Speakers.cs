using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Speakers", menuName = "Scriptable Objects/Speakers")]
public class Speakers : ScriptableObject
{
    public List<SpeakerInfo> speakers;
}

[System.Serializable]
public enum Espeaker
{
    None = 0,
    Bourdon = 3,
    Gendarme = 4,
    Papillon = 5,
    Cloporte = 6,
    Mantis = 7
}
