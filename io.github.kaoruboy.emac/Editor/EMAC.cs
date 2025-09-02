#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;

namespace EMAC
{
    /// <summary>
    /// Expression Menu As Code Builder
    /// Class used to generate expression menus with code.
    /// </summary>
    public class EMACBuilder
    {
        public VRCExpressionsMenu TargetMenu;
        public EMACMenu RootMenu = new EMACMenu("");
        public string ParameterPrefix = "";

        public Texture2D DefaultFolderIcon = SampleIcons.ItemFolder;
        public Texture2D DefaultItemIcon = null;

        public Texture2D DefaultNextPageIcon = SampleIcons.ItemFolder;
        public string DefaultNextPageText = "Next Page";

        /// <summary>
        /// Create a new builder for expression menus.
        /// </summary>
        /// <param name="targetMenu">The root expression menu all items will be written to. Must be saved to a path. Subfolders will be added as a subresource</param>
        /// <exception cref="InvalidParameterException">Thrown if provided targetMenu is null</exception>
        public EMACBuilder(VRCExpressionsMenu targetMenu)
        {
            if (targetMenu == null)
                throw new InvalidParameterException("targetMenu may not be null");

            TargetMenu = targetMenu;
        }

        /// <summary>
        /// Set the prefix all the parameters will be prefixed with when building.
        /// </summary>
        public EMACBuilder WithPrefix(string prefix)
        {
            if (prefix == null) prefix = "";
            ParameterPrefix = prefix;
            return this;
        }

        /// <summary>
        /// Set the icon to be used by submenu items when no custom icon is defined.
        /// Set to null to revert to default
        /// </summary>
        public EMACBuilder WithDefaultFolderIcon(Texture2D icon)
        {
            DefaultFolderIcon = icon ?? SampleIcons.SymbolMagic;
            return this;
        }

        /// <summary>
        /// Set the icon to be used by menu items other than submenus when no custom icon is defined.
        /// Set to null to revert to default.
        /// </summary>
        public EMACBuilder WithDefaultItemIcon(Texture2D icon)
        {
            DefaultItemIcon = icon ?? SampleIcons.ItemFolder;
            return this;
        }

        /// <summary>
        /// Set the icon to be used by the "Next Page" menu created when more than 8 items are in a menu and no custom icon is defined.
        /// Set to null to revert to default.
        /// </summary>
        public EMACBuilder WithNextPageIcon(Texture2D icon)
        {
            DefaultNextPageIcon = icon ?? SampleIcons.ItemFolder;
            return this;
        }

        /// <summary>
        /// Set the name to be used by the "Next Page" menu created when more than 8 items are in a menu and no custom name is defined.
        /// Set to null to revert to default.
        /// </summary>
        public EMACBuilder WithNextPageText(string text)
        {
            DefaultNextPageText = text ?? "Next Page";
            return this;
        }

        void Clear()
        {
            var assetPath = AssetDatabase.GetAssetPath(TargetMenu);
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var obj in assets)
            {
                if (obj == TargetMenu) continue;
                AssetDatabase.RemoveObjectFromAsset(obj);
            }
            AssetDatabase.SaveAssets();
        }

        void PackAsset(UnityEngine.Object asset)
        {
            AssetDatabase.AddObjectToAsset(asset, TargetMenu);
        }

        /// <summary>
        /// Build the expression menu.
        /// All subresources of target menu will be removed.
        /// Submenus will be packed as a subresource of target menu.
        /// A parameter list asset will be generated as a subresource to prevent warnings in the inspector.
        /// </summary>
        /// <exception cref="InvalidParameterException"></exception>
        /// <exception cref="Exception"></exception>
        public void Build()
        {
            Clear();

            var menuPath = AssetDatabase.GetAssetPath(TargetMenu);
            var assets = AssetDatabase.LoadAllAssetsAtPath(menuPath);
            foreach (var obj in assets)
            {
                if (obj == TargetMenu) continue;
                AssetDatabase.RemoveObjectFromAsset(obj);
            }

            List<VRCExpressionParameters.Parameter> paramsList = new();
            VRCExpressionParameters param = new();
            param.name = "z Generated Parameter List - DO NOT USE!!!";

            VRCExpressionsMenu BuildMenu(EMACMenu menu, VRCExpressionsMenu target = null)
            {
                VRCExpressionsMenu.Control.Parameter Param(string param)
                {
                    var name = (param == null) ? null : (ParameterPrefix + param);
                    if (!paramsList.Any(p => p.name == name))
                        paramsList.Add(new VRCExpressionParameters.Parameter()
                        {
                            name = name,
                            valueType = VRCExpressionParameters.ValueType.Float,
                            saved = false,
                            networkSynced = false,
                            defaultValue = 0.0f
                        });
                    return new VRCExpressionsMenu.Control.Parameter() { name = name };
                }

                var mainMenu = target ?? new VRCExpressionsMenu();
                mainMenu.name = menu.Path;
                mainMenu.Parameters = param;

                var controls = new List<VRCExpressionsMenu.Control>();
                foreach (var item in menu.Items)
                {
                    if (item.Parent != menu)
                        throw new InvalidParameterException("Parent of item does not match! Did you add an item twice or forgot to use EMACMenu.Add()?");

                    var control = new VRCExpressionsMenu.Control()
                    {
                        name = item.Name,
                        icon = item.Icon ?? menu.ResolvedItemIcon ?? DefaultItemIcon,
                        parameter = Param(item.ActionParameter),
                        value = item.ActionValue
                    };

                    if (item is EMACMenu im)
                    {
                        var sub = BuildMenu(im);
                        PackAsset(sub);
                        control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                        control.subMenu = sub;
                        control.icon = item.Icon ?? menu.ResolvedFolderIcon ?? DefaultFolderIcon;
                    }
                    else if (item is EMACToggle t)
                        control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
                    else if (item is EMACButton b)
                        control.type = VRCExpressionsMenu.Control.ControlType.Button;
                    else if (item is EMACRadial rad)
                    {
                        control.type = VRCExpressionsMenu.Control.ControlType.RadialPuppet;
                        control.subParameters = new[] { Param(rad.RotationParameter) };
                    }
                    else if (item is EMACPuppet pup)
                    {
                        VRCExpressionsMenu.Control.Parameter[] subs = null;

                        if (pup is EMACTwoAxisPuppet tap)
                        {
                            control.type = VRCExpressionsMenu.Control.ControlType.TwoAxisPuppet;
                            subs = new[] { Param(tap.HorizontalParameter), Param(tap.VerticalParameter) };
                        }
                        else if (pup is EMACFourAxisPuppet fap)
                        {
                            control.type = VRCExpressionsMenu.Control.ControlType.FourAxisPuppet;
                            subs = new[] { Param(fap.UpParameter), Param(fap.RightParameter), Param(fap.DownParameter), Param(fap.LeftParameter) };
                        }

                        control.subParameters = subs;
                        control.labels = new[] {
                            new VRCExpressionsMenu.Control.Label(){ name = pup.UpText, icon = pup.UpIcon},
                            new VRCExpressionsMenu.Control.Label(){ name = pup.RightText, icon = pup.RightIcon},
                            new VRCExpressionsMenu.Control.Label(){ name = pup.DownText, icon = pup.DownIcon},
                            new VRCExpressionsMenu.Control.Label(){ name = pup.LeftText, icon = pup.LeftIcon},
                        };
                    }

                    controls.Add(control);
                }

                if (controls.Count > 8)
                {
                    var remainingItems = controls.Skip(7).ToList();
                    controls = controls.Take(7).ToList();

                    VRCExpressionsMenu CreatePagedMenu(List<VRCExpressionsMenu.Control> items)
                    {
                        var pageMenu = new VRCExpressionsMenu();
                        pageMenu.name = menu.ResolvedNextPageText ?? DefaultNextPageText;
                        pageMenu.Parameters = param;

                        var pageControls = items.Take(7).ToList();
                        var nextItems = items.Skip(7).ToList();

                        if (nextItems.Count > 1)
                        {
                            var nextPageControl = new VRCExpressionsMenu.Control()
                            {
                                name = menu.ResolvedNextPageText ?? DefaultNextPageText,
                                icon = menu.ResolvedNextPageIcon ?? DefaultNextPageIcon ,
                                type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                                subMenu = CreatePagedMenu(nextItems)
                            };
                            pageControls.Add(nextPageControl);
                        }
                        else if (nextItems.Count == 1)
                        {
                            pageControls.Add(nextItems[0]);
                        }

                        pageMenu.controls = pageControls;
                        PackAsset(pageMenu);
                        return pageMenu;
                    }

                    if (remainingItems.Any())
                    {
                        var nextPageMenu = new VRCExpressionsMenu.Control()
                        {
                            name = menu.NextPageText ?? DefaultNextPageText,
                            icon = menu.NextPageIcon ?? DefaultNextPageIcon,
                            type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                            subMenu = CreatePagedMenu(remainingItems)
                        };
                        controls.Add(nextPageMenu);
                    }
                }

                mainMenu.controls = controls;
                return mainMenu;
            }

            BuildMenu(RootMenu, TargetMenu);

            param.parameters = paramsList.ToArray();
            PackAsset(param);

            TargetMenu.name = System.IO.Path.GetFileNameWithoutExtension(menuPath);
            EditorUtility.SetDirty(TargetMenu);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif