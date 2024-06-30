using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlitchedCatStudios.Wardrobe
{
    public class GcsWardrobeInfo : MonoBehaviour
    {
        public GcsWardrobeCosmeticType type;
        [Space(15)]
        public GcsHeadCosmetic headCosmetic;
        [Space]
        public GcsFaceCosmetic faceCosmetic;
        [Space]
        public GcsBodyCosmetic bodyCosmetic;
        [Space]
        public GcsHoldableCosmetic holdableCosmetic;
    }

    public enum GcsWardrobeCosmeticType
    {
        Head,
        Face,
        Body,
        Holdable
    }
}