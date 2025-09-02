
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace EMAC
{
    /// <summary>
    /// Helper class containing all the sample icons included with the VRChat SDK.
    /// </summary>
    public static class SampleIcons
    {
        static Texture2D GetSampleIcon(string name)
            => AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Expressions Menu/Icons/" + name + ".png");

        public static Texture2D FaceAngry => GetSampleIcon("face_angry");
        public static Texture2D FaceGasp => GetSampleIcon("face_gasp");
        public static Texture2D FaceHappy => GetSampleIcon("face_happy");
        public static Texture2D FaceMeh => GetSampleIcon("face_meh");
        public static Texture2D FaceSmile => GetSampleIcon("face_smile");
        public static Texture2D HandNormal => GetSampleIcon("hand_normal");
        public static Texture2D HandRock => GetSampleIcon("hand_rock");
        public static Texture2D HandWaving => GetSampleIcon("hand_waving");
        public static Texture2D ItemFlashlight => GetSampleIcon("item_flashlight");
        public static Texture2D ItemFolder => GetSampleIcon("item_folder");
        public static Texture2D ItemLight => GetSampleIcon("item_light");
        public static Texture2D ItemPistol => GetSampleIcon("item_pistol");
        public static Texture2D ItemSword => GetSampleIcon("item_sword");
        public static Texture2D ItemWand => GetSampleIcon("item_wand");
        public static Texture2D PersonDance => GetSampleIcon("person_dance");
        public static Texture2D PersonRunning => GetSampleIcon("person_running");
        public static Texture2D PersonWave => GetSampleIcon("person_wave");
        public static Texture2D SymbolColors => GetSampleIcon("symbol_colors");
        public static Texture2D SymbolHeart => GetSampleIcon("symbol_heart");
        public static Texture2D SymbolMagic => GetSampleIcon("symbol_magic");
        public static Texture2D SymbolMusic => GetSampleIcon("symbol_music");
        public static Texture2D SymbolPaw => GetSampleIcon("symbol_paw");
    }
}
#endif