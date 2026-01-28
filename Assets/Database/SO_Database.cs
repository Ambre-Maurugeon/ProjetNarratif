using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Database", menuName = "Scriptable Objects/SO_Database")]
public class SO_Database : ScriptableObject
{
    public List<CharacterEntry> entries = new List<CharacterEntry>();

}