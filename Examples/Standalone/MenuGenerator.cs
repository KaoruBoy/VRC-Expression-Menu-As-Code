#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using VRC.SDK3.Avatars.ScriptableObjects;
using EMAC;
using EPAC;

public class MenuGenerator : MonoBehaviour
{
    public VRCExpressionsMenu vrcmenu;
    public VRCExpressionParameters vrcparameters;
    public Texture2D FolderIcon, ItemIcon, ClothesIcon, HueIcon;

    public void Generate()
    {
        var prefix = "MyAvatar/Toggles/";
        var menu = new EMACBuilder(vrcmenu)
            .WithPrefix(prefix)
            .WithDefaultItemIcon(ItemIcon)
            .WithDefaultFolderIcon(FolderIcon);
        var parameters = new EPACBuilder(vrcparameters).WithPrefix(prefix);
    
        parameters
            .Bool("Shirt", true, true)
            .Float("ShirtColor", 0.0f, true)
            .Bool("Socks", true, true)
            .Float("SocksColor", 0.0f, true)
            .Bool("Sword", false, true)
            .Float("HairColor", 0.0f, true);

        menu.RootMenu
            .Add(new EMACToggle("Shirt Toggle", "Shirt"), ClothesIcon)
            .Add(new EMACRadial("Shirt Color", "ShirtColor", HueIcon))
            .Add(new EMACToggle("Socks Toggle", "Socks"), ClothesIcon)
            .Add(new EMACRadial("Socks Color", "SocksColor", HueIcon))
            .Add(new EMACToggle("Sword Toggle", "Sword", SampleIcons.ItemSword))
            .Add(new EMACRadial("Hair Color", "HairColor", HueIcon));

        menu.Build();
        parameters.Build();
    }
}

[CustomEditor(typeof(MenuGenerator))]
public class CsxWatcherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
            ((MenuGenerator)target).Generate();
    }
}

#endif