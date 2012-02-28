using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using SlimDX.Direct3D9;

namespace SharpWoW.Video
{
    public enum TextureFilterMode
    {
        Bilinear,
        Trilinear,
        Anisotropic
    }

    public class VideoConfig
    {
        public static VideoConfig Load(bool useRegistry, bool useDialog)
        {
            if (useRegistry == false && useDialog == false)
                return LoadDefault();

            if (useDialog == false)
                return TryLoadRegistry();

            if (useRegistry != false)
            {
                try
                {
                    VideoConfig cfg = TryLoadRegistry();
                    if (cfg != null)
                        return cfg;
                }
                catch (Exception)
                {
                }
            }

            UI.Dialogs.DeviceDialog dvd = new UI.Dialogs.DeviceDialog();
            dvd.ShowDialog();
            var ret = new VideoConfig()
            {
                MultisampleQuality = (uint)dvd.MultisampleQuality,
                Multisampling = dvd.Multisampling,
                Filtering = dvd.FilterMode,
                Anisotropy = dvd.Anisotropy,
                Adapter = dvd.SelectedAdapter.Details,
                DepthStencilFormat = dvd.DepthStencil
            };

            return ret;
        }

        private static VideoConfig LoadDefault()
        {
            VideoConfig ret = new VideoConfig()
            {
                Multisampling = MultisampleType.None,
                MultisampleQuality = 0,
                Filtering = TextureFilterMode.Bilinear,
                Anisotropy = 0,
                Adapter = Game.GameManager.GraphicsThread.GraphicsManager.Direct3D.GetAdapterIdentifier(0),
                DepthStencilFormat = Format.D16
            };

            return ret;
        }

        private static VideoConfig TryLoadRegistry()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Yias\\SharpWoW\\Video");
            if (key == null)
                return null;

            VideoConfig ret = new VideoConfig();
            ret.loadFromRegistry(key);
            return ret;
        }

        private void loadFromRegistry(RegistryKey key)
        {
            var strMs = (string)key.GetValue("Multisampling");
            MultisampleType ms;
            if (Enum.TryParse<MultisampleType>(strMs, out ms) == false)
                throw new InvalidOperationException("Registry has stored an invalid Multisampling!");

            Multisampling = ms;
            uint msq = 0;

            if (uint.TryParse((string)key.GetValue("MultisampleQuality"), out msq) == false)
                throw new InvalidOperationException("Registry has stored an invalid multisample quality!");

            MultisampleQuality = msq;

            TextureFilterMode fm;
            if (Enum.TryParse<TextureFilterMode>((string)key.GetValue("TextureFilter"), out fm) == false)
                throw new InvalidOperationException("Registry has stored an invalid filter mode!");

            Filtering = fm;

            if (uint.TryParse((string)key.GetValue("Anisotropy"), out msq) == false)
                throw new InvalidOperationException("Registry has stored an invalid anisotropy value!");

            Anisotropy = msq;

            if (uint.TryParse((string)key.GetValue("AdapterOrdinal"), out msq) == false)
                throw new InvalidOperationException("Registry has stored an invalid adapter ordinal!");

            Adapter = Game.GameManager.GraphicsThread.GraphicsManager.Direct3D.GetAdapterIdentifier((int)msq);

            Format fmt;
            if (Enum.TryParse<Format>((string)key.GetValue("DepthStencil"), out fmt) == false)
                throw new InvalidOperationException("Registry has stored an invalid depth stencil format!");

            DepthStencilFormat = fmt;
        }

        public MultisampleType Multisampling { get; set; }
        public uint MultisampleQuality { get; set; }
        public TextureFilterMode Filtering { get; set; }
        public uint Anisotropy { get; set; }
        public Format DepthStencilFormat { get; set; }
        public AdapterDetails Adapter { get; set; }
    }
}
