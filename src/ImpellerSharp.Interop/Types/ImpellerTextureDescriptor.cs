using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct ImpellerTextureDescriptor
{
    public ImpellerPixelFormat PixelFormat;
    public ImpellerISize Size;
    public uint MipCount;

    public ImpellerTextureDescriptor(ImpellerPixelFormat pixelFormat, ImpellerISize size, uint mipCount = 1)
    {
        PixelFormat = pixelFormat;
        Size = size;
        MipCount = mipCount;
    }
}
