using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;

namespace SharpWoW.Models.WMO
{

    public class WMOMaterialConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (typeof(WMOMaterial) == destinationType)
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is WMOMaterial)
            {
                WMOMaterial mat = value as WMOMaterial;
                return "Material - Texture id: " + mat.Material.ofsTexture1;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    [TypeConverter(typeof(WMOMaterialConverter))]
    public class WMOMaterial
    {
        public WMOMaterial(MOMT mat)
        {
            mMaterial = mat;
        }

        //[Editor(typeof(DynamicGridComboBox), typeof(UITypeEditor))]
        public string[] Textures
        {
            get { return new string[] { }; }
        }

        public MOMT Material { get { return mMaterial; } }

        private MOMT mMaterial;
        private WMOFile mFile;
    }

    public class WMOEditDescriptor
    {
        private WMOFile mFile;
        private UI.Dialogs.WMOEditor mEditor;

        public WMOEditDescriptor(WMOFile file, UI.Dialogs.WMOEditor editor)
        {
            mEditor = editor;
            mFile = file;
        }

        [Category("Visual Attributes")]
        [Description("List of all available textures in the WMO")]
        public string[] Textures
        {
            get
            {
                return mFile.TextureNames;
            }

            set
            {
                foreach (var str in value)
                    if (!Stormlib.MPQFile.Exists(str))
                        return;

                mEditor.Invoke(new Action(() =>
                    {
                        mFile.SetTextures(value);
                    }));
            }
        }

        [Category("Visual Attributes")]
        [Description("List of all materials in the WMO")]
        public WMOMaterial[] Materials
        {
            get
            {
                var mats = mFile.Materials;
                WMOMaterial[] ret = new WMOMaterial[mats.Length];
                for (int i = 0; i < mats.Length; ++i)
                    ret[i] = new WMOMaterial(mats[i]);

                return ret;
            }
            set { }
        }
    }
}
