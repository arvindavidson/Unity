using UnityEngine;

/// <summary>
/// Placed on a dead enemy's body. Other enemies can detect this and investigate.
/// </summary>
public class EnemyCorpse : MonoBehaviour
{
    public bool discovered = false;
    public float discoveryRadius = 8f; // How close an enemy needs to be to notice the body
}
