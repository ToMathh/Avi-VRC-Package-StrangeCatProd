
using UnityEngine;

// This is just to attach to the avatar, most of the code is donde in "CCPetsEditor"

namespace GabSith.AFKMuteBadge
{
    [ExecuteAlways]
    public class Installer : MonoBehaviour
    {
        //public float baseSize = 1f;
        [Range(0.1f, 4f)]
        public float size = 1f;
        [Range(0.1f, 4f)]
        public float position = 1f;
        public Transform badge, badgeST;
        public bool badgeSpawned = false, appliedSize, done = false, apply = false;
        public bool writeDefaultEnabled;

        public int platform;


        public void DestroyScriptInstance()
        {
            Installer inst = GetComponent<Installer>();
            DestroyImmediate(inst);
        }
    }
}




