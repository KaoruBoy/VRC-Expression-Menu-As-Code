#if UNITY_EDITOR
using UnityEngine;
using AnimatorAsCode.V1.VRCDestructiveWorkflow;
using EMAC;
using param;

public class TogglesGenerator : ExpressionGenerator
{
    public override string SystemName => "Toggles";

    public GameObject Sword, Shirt, Socks, Hair;
    public Texture2D SwordIcon, ShirtIcon, SocksIcon, ColorIcon;

    public override void Generate()
    {
        string prefix = "Avatar/Toggles/";
        var param = new EPACBuilder(avatar.expressionParameters).WithPrefix(prefix);
        var menu = new EMACBuilder(avatar.expressionsMenu).WithPrefix(prefix);

        void ColorAdjust(SkinnedMeshRenderer mesh, string name)
        {
            var colorLayer = aac.CreateSupportingFxLayer(name + " Color");
            var colorParam = colorLayer.FloatParameter(prefix + param + "Color");
            param.Float(name + "Color", 0f, true, true);
            menu.RootMenu.Add(new EMACRadial(name + " Color", name));


            var state = colorLayer.NewState("Hue Adjust")
                .WithMotionTime(colorParam)
                .WithAnimation(aac.NewClip()
                    .Animating(c =>
                        {
                            // Toon Standard
                            c.Animates(mesh, "material._HueShift")
                                .WithFrameCountUnit(k => k
                                    .Linear(0, 0)
                                    .Linear(127, 2 * Mathf.PI));
                            // LilToon
                            c.Animates(mesh, "material._MainTexHSVG.x")
                                .WithFrameCountUnit(k => k
                                    .Linear(0, 0)
                                    .Linear(127, 1));
                            c.Animates(mesh, "material._MainTexHSVG.y").WithOneFrame(1);
                            c.Animates(mesh, "material._MainTexHSVG.z").WithOneFrame(1);
                            c.Animates(mesh, "material._MainTexHSVG.w").WithOneFrame(1);
                        }));

            colorLayer.WithDefaultState(state);
        }

        ColorAdjust(Shirt, "Shirt");
        ColorAdjust(Socks, "Socks");
        ColorAdjust(Hair, "Hair");

        void ObjectToggle(GameObject gameObject, string name, bool defaultState = true)
        {
            var layer = aac.CreateSupportingFxLayer(name);
            var boolParam = layer.BoolParameter(prefix + name);
            param.Bool(name, defaultState, true, true);
            menu.RootMenu.Add(new EMACToggle(name + " Toggle", name));

            var on = layer.NewState("On").WithAnimation(aac.NewClip().Toggling(gameObject, true));
            var off = layer.NewState("Off").WithAnimation(aac.NewClip().Toggling(gameObject, false));

            layer.WithDefaultState(defaultState ? on : off);
            on.TransitionsTo(off).When(boolParam.IsEqualTo(false));
            off.TransitionsTo(on).When(boolParam.IsEqualTo(true));
        }

        ObjectToggle(Sword, "Sword", false);
        ObjectToggle(Shirt, "Shirt");
        ObjectToggle(Socks, "Socks");

        param.Build();
        menu.Build();
    }
}
#endif