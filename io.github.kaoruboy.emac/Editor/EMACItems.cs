#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace EMAC
{
    /// <summary>
    /// Base class for all expression menu items
    /// </summary>
    public abstract class EMACItem
    {
        public EMACItem Parent;

        public Texture2D Icon;
        public string Name;

        public string ActionParameter;
        public float ActionValue;

        /// <summary>
        /// Absolute path of the item within the menu
        /// </summary>
        public string Path
        {
            get
            {
                string path = Name;
                var p = Parent;
                while (p != null)
                {
                    path = p.Name + "/" + path;
                    p = p.Parent;
                }
                return path;
            }
        }

        /// <summary>
        /// Set the parameter to be set when the action for an item is preformed.
        /// The activation differs on different menu item types.
        /// It's recommended to call this method as last in the chain as it returns the base EMACItem.
        /// </summary>
        /// <param name="param">Name of the parameter to set the value of</param>
        /// <param name="value">Value to set the parameter to</param>
        public EMACItem WithActionParameter(string param, int value)
        {
            return WithActionParameter(param, value);
        }

        /// <summary>
        /// Set the parameter to be set when the action for an item is preformed.
        /// The activation differs on different menu item types.
        /// It's recommended to call this method as last in the chain as it returns the base EMACItem.
        /// </summary>
        /// <param name="param">Name of the parameter to set the value of</param>
        /// <param name="value">Value to set the parameter to</param>
        public EMACItem WithActionParameter(string param, bool value)
        {
            return WithActionParameter(param, value ? 1f : 0f);
        }

        /// <summary>
        /// Set the parameter to be set when the action for an item is preformed.
        /// The activation differs on different menu item types.
        /// It's recommended to call this method as last in the chain as it returns the base EMACItem.
        /// </summary>
        /// <param name="param">Name of the parameter to set the value of</param>
        /// <param name="value">Value to set the parameter to</param>
        public EMACItem WithActionParameter(string param, float value)
        {
            ActionParameter = param;
            ActionValue = value;
            return this;
        }
    }

    /// <summary>
    /// Menu item, which can contain multiple other items.
    /// Entering the menu will set the action parameter.
    /// </summary>
    public class EMACMenu : EMACItem
    {
        public Texture2D NextPageIcon;
        public string NextPageText;

        public List<EMACItem> Items = new List<EMACItem>();

        /// <summary>
        /// Create a new menu with a parameter to be set when entered.
        /// Use .Add() to add items to the menu
        /// </summary>
        /// <param name="name">Name of the menu</param>
        /// <param name="parameter">Name of the parameter to be set when entered</param>
        /// <param name="value">Value to set the parameter to</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultFolderIcon is used</param>
        public EMACMenu(string name, string parameter, float value, Texture2D icon = null)
        {
            Name = name;
            ActionParameter = parameter;
            ActionValue = value;
            Icon = icon;
        }

        /// <summary>
        /// Create a new menu.
        /// Use .Add() to add items to the menu
        /// </summary>
        /// <param name="name">Name of the menu</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultFolderIcon is used</param>
        public EMACMenu(string name, Texture2D icon = null) : this(name, null, 0f, icon) { }
        /// <summary>
        /// Create a new menu with a parameter to be set when entered.
        /// Use .Add() to add items to the menu
        /// </summary>
        /// <param name="name">Name of the menu</param>
        /// <param name="parameter">Name of the parameter to be set when entered</param>
        /// <param name="value">Value to set the parameter to</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultFolderIcon is used</param>
        public EMACMenu(string name, string parameter, Texture2D icon = null) : this(name, parameter, 1f, icon) { }
        /// <summary>
        /// Create a new menu with a parameter to be set when entered.
        /// Use .Add() to add items to the menu
        /// </summary>
        /// <param name="name">Name of the menu</param>
        /// <param name="parameter">Name of the parameter to be set when entered</param>
        /// <param name="value">Value to set the parameter to</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultFolderIcon is used</param>
        public EMACMenu(string name, string parameter, bool value, Texture2D icon = null) : this(name, parameter, value ? 1f : 0f, icon) { }
        /// <summary>
        /// Create a new menu with a parameter to be set when entered.
        /// Use .Add() to add items to the menu
        /// </summary>
        /// <param name="name">Name of the menu</param>
        /// <param name="parameter">Name of the parameter to be set when entered</param>
        /// <param name="value">Value to set the parameter to</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultFolderIcon is used</param>
        public EMACMenu(string name, string parameter, int value, Texture2D icon = null) : this(name, parameter, (float)value, icon) { }

        /// <summary>
        /// Override the icon to be used by the "Next Page" submenu created when this menu contains more than 8 items.
        /// When null, EMACBuilder.DefaultNextPageIcon will be used
        /// </summary>
        public EMACMenu WithNextPageIcon(Texture2D icon)
        {
            NextPageIcon = icon;
            return this;
        }

        /// <summary>
        /// Override the text to be used by the "Next Page" submenu created when this menu contains more than 8 items.
        /// When null, EMACBuilder.DefaultNextPageText will be used
        /// </summary>
        public EMACMenu WithNextPageText(string text)
        {
            NextPageText = text;
            return this;
        }
        
        /// <summary>
        /// Add a new item to the menu.
        /// If an item with the same name already exists, it will be removed.
        /// </summary>
        /// <param name="item">The item to add</param>
        public EMACMenu Add(EMACItem item)
        {
            Items.RemoveAll(i => i.Name == item.Name);
            Items.Add(item);
            item.Parent = this;
            return this;
        }

        /// <summary>
        /// Merge the items from another menu into this one.
        /// </summary>
        /// <param name="otherMenu">The menu from which items will be copied from</param>
        /// <param name="replace">If true, exising items with the same name will be removed</param>
        public EMACMenu Merge(EMACMenu otherMenu, bool replace = false)
        {
            var otherItems = otherMenu.Items;

            if (replace)
                Items = Items
                    .Where(i => !otherItems
                        .Any(o => o.Name == i.Name))
                    .Concat(otherItems)
                    .ToList();
            else
                Items = Items
                    .Concat(otherItems
                        .Where(oi => !Items.Any(i => i.Name == oi.Name)))
                    .ToList();

            foreach (var i in Items)
                i.Parent = this;

            return this;
        }
    }

    /// <summary>
    /// Menu item, which sets the parameter to the given value when toggled
    /// </summary>
    public class EMACToggle : EMACItem
    {
        /// <summary>
        /// Create a new toggle item.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="parameter">Name of the parameter to be set when toggled</param>
        /// <param name="value">Value to set the parameter to</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultItemIcon is used</param>
        public EMACToggle(string name, string parameter, float value, Texture2D icon = null)
        {
            Name = name;
            ActionParameter = parameter;
            ActionValue = value;
            Icon = icon;
        }

        /// <summary>
        /// Create a new toggle item.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="parameter">Name of the parameter to be set when toggled</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultItemIcon is used</param>
        public EMACToggle(string name, string parameter, Texture2D icon = null) : this(name, parameter, 1f, icon) { }
        /// <summary>
        /// Create a new toggle item.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="parameter">Name of the parameter to be set when toggled</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultItemIcon is used</param>
        public EMACToggle(string name, string parameter, bool value, Texture2D icon = null) : this(name, parameter, value ? 1f : 0f, icon) { }
        /// <summary>
        /// Create a new toggle item.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="parameter">Name of the parameter to be set when toggled</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultItemIcon is used</param>
        public EMACToggle(string name, string parameter, int value, Texture2D icon = null) : this(name, parameter, (float)value, icon) { }
    }
    
    /// <summary>
    /// Menu item, which sets the parameter to the given value when touched
    /// </summary>
    public class EMACButton : EMACItem
    {
        /// <summary>
        /// Create a new button item.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="parameter">Name of the parameter to be set when touched</param>
        /// <param name="value">Value to set the parameter to</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultItemIcon is used</param>
        public EMACButton(string name, string parameter, float value, Texture2D icon = null)
        {
            Name = name;
            ActionParameter = parameter;
            ActionValue = value;
            Icon = icon;
        }

        /// <summary>
        /// Create a new button item.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="parameter">Name of the parameter to be set when touched</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultItemIcon is used</param>
        public EMACButton(string name, string parameter, Texture2D icon = null) : this(name, parameter, 1f, icon) { }
        /// <summary>
        /// Create a new button item.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="parameter">Name of the parameter to be set when touched</param>
        /// <param name="value">Value to set the parameter to</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultItemIcon is used</param>
        public EMACButton(string name, string parameter, bool value, Texture2D icon = null) : this(name, parameter, value ? 1f : 0f, icon) { }
        /// <summary>
        /// Create a new button item.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="parameter">Name of the parameter to be set when touched</param>
        /// <param name="value">Value to set the parameter to</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultItemIcon is used</param>
        public EMACButton(string name, string parameter, int value, Texture2D icon = null) : this(name, parameter, (float)value, icon) { }
    }
    
    /// <summary>
    /// Menu item, which sets the given float parameter to the clockwise rotation of this item.
    /// 0° rotation will set the parameter to 0.0, 180° rotation will set it to 1.0
    /// When opened, action parameter is set to the given value. Use .WithActionParameter() at the end of your chain to set it.
    /// </summary>
    public class EMACRadial : EMACItem
    {
        public string RotationParameter;

        /// <summary>
        /// Create a new radial item with a parameter to be set when rotated.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="parameter">Name of the parameter to be set when rotated</param>
        /// <param name="value">Value to set the parameter to</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultItemIcon is used</param>
        public EMACRadial(string name, string parameter, Texture2D icon = null)
        {
            Name = name;
            RotationParameter = parameter;
            Icon = icon;
        }
    }

    /// <summary>
    /// Base class for 2 and 4 axis puppets.
    /// </summary>
    public abstract class EMACPuppet : EMACItem
    {
        public string UpText, DownText, LeftText, RightText;
        public Texture2D UpIcon, DownIcon, LeftIcon, RightIcon;

        public EMACPuppet WithDirectionalIcons(Texture2D up, Texture2D right, Texture2D down, Texture2D left)
        {
            UpIcon = up;
            RightIcon = right;
            DownIcon = down;
            LeftIcon = left;

            return this;
        }

        public EMACPuppet WithDirectionalLabels(string up, string right, string down, string left)
        {
            UpText = up;
            RightText = right;
            DownText = down;
            LeftText = left;

            return this;
        }
    }

    /// <summary>
    /// Menu item, which sets the given float parameters to the vertical and horizontal position of the joystick.
    /// Moving the joystick to the top sets the vertical value to 1.0.
    /// Moving the joystick to the bottom sets the vertical value to -1.0.
    /// Moving the joystick to the left sets the horizontal value to -1.0.
    /// Moving the joystick to the right sets the horizontal value to 1.0.
    /// The puppet may be given icons and labels using .WithDirectionalIcons() and .WithDirectionalLabels().
    /// When opened, action parameter is set to the given value. Use .WithActionParameter() at the end of your chain to set it.
    /// </summary>
    public class EMACTwoAxisPuppet : EMACPuppet
    {
        public string VerticalParameter, HorizontalParameter;

        /// <summary>
        /// Create a new 2 axis puppet item with a parameter to be set when the joystick is moved.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="vParameter">Name of the parameter to be set when the joystick is moved vertically</param>
        /// <param name="hParameter">Name of the parameter to be set when the joystick is moved horizontally</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultItemIcon is used</param> 
        public EMACTwoAxisPuppet(string name, string vParameter, string hParameter, Texture2D icon = null)
        {
            Name = name;
            Icon = icon;
            VerticalParameter = vParameter;
            HorizontalParameter = hParameter;
        }
    }

    /// <summary>
    /// Menu item, which sets the given float parameters to the direction the joystick is moved to.
    /// When a joystick is moved, the value of the parameter for the given direction is set to the distance the joystick is from the center between 0.0 and 1.0.
    /// Opposing directions are set to 0.0.
    /// When opened, action parameter is set to the given value. Use .WithActionParameter() at the end of your chain to set it.
    /// </summary>
    public class EMACFourAxisPuppet : EMACPuppet
    {
        public string UpParameter, DownParameter, LeftParameter, RightParameter;

        /// <summary>
        /// Create a new 2 axis puppet item with a parameter to be set when the joystick is moved.
        /// </summary>
        /// <param name="name">Name of the item</param>
        /// <param name="upParameter">Name of the parameter to be set when the joystick is moved up</param>
        /// <param name="rightParameter">Name of the parameter to be set when the joystick is moved right</param>
        /// <param name="downParameter">Name of the parameter to be set when the joystick is moved down</param>
        /// <param name="leftParameter">Name of the parameter to be set when the joystick is moved left</param>
        /// <param name="icon">Optional icon. When null, EMACBuilder.DefaultItemIcon is used</param> 
        public EMACFourAxisPuppet(string name, string upParameter, string rightParameter, string downParameter, string leftParameter, Texture2D icon = null)
        {
            Name = name;
            Icon = icon;
            UpParameter = upParameter;
            RightParameter = rightParameter;
            DownParameter = downParameter;
            LeftParameter = leftParameter;
        }
    }
}
#endif