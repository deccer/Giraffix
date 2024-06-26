using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Giraffix.Native.Ktx;

public static partial class Ktx
{
    private static nint _nativeLibraryHandle = nint.Zero;

    private const string SrLibKtxNotInitialized =
        "Ktx not initialized. Call Ktx.Init somewhere in your application startup first";

    public static bool Init()
    {
        var libraryName = GetLibraryName();
        if (NativeLibrary.TryLoad(libraryName, out _nativeLibraryHandle))
        {
            return true;
        }
        
        Debug.WriteLine("Ktx: Unable to load native library");
        return false;
    }

    public static void Terminate()
    {
        if (_nativeLibraryHandle != nint.Zero)
        {
            NativeLibrary.Free(_nativeLibraryHandle);
        }
    }

    public static unsafe KtxTexture* LoadFromMemory(ReadOnlyMemory<byte> data)
    {
        if (_nativeLibraryHandle == nint.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        KtxTexture* ktxTexture = null;
        var createFlagBits = KtxTextureCreateFlagBits.LoadImageDataBit;
        var result = _ktxTexture2CreateFromMemoryDelegate(data.Pin().Pointer, data.Length, createFlagBits, &ktxTexture);
        return result != KtxErrorCode.KtxSuccess ? null : ktxTexture;
    }

    public static unsafe KtxTexture* LoadFromFile(string fileName)
    {
        if (_nativeLibraryHandle == nint.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        var fileNamePtr = Marshal.StringToHGlobalAnsi(fileName);
        KtxTexture* ktxTexture = null;
        var createFlagBits = KtxTextureCreateFlagBits.LoadImageDataBit;
        var result = _ktxTexture2CreateFromNamedFileDelegate(fileNamePtr, createFlagBits, &ktxTexture);
        Marshal.FreeHGlobal(fileNamePtr);        
        return result != KtxErrorCode.KtxSuccess ? null : ktxTexture;
    }

    public static unsafe void Destroy(KtxTexture* texture)
    {
        if (_nativeLibraryHandle == nint.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        _ktxTexture2DestroyDelegate(texture);
    }

    public static unsafe bool NeedsTranscoding(KtxTexture* texture)
    {
        if (_nativeLibraryHandle == nint.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        return _ktxTexture2NeedsTranscodingDelegate(texture) == 1;
    }

    public static unsafe KtxErrorCode Transcode(KtxTexture* texture, TranscodeFormat transcodeFormat, TranscodeFlagBits transcodeFlagBits)
    {
        if (_nativeLibraryHandle == nint.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        return _ktxTexture2TranscodeBasisDelegate(texture, transcodeFormat, transcodeFlagBits);
    }

    public static unsafe uint GetNumComponents(KtxTexture* texture)
    {
        if (_nativeLibraryHandle == nint.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        return _ktxTexture2GetNumComponentsDelegate(texture);
    }

    public static unsafe uint GetImageOffset(KtxTexture* texture, uint mipLevel, uint layer, uint faceIndex)
    {
        if (_nativeLibraryHandle == nint.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        uint imageOffset = 0;
        var result = _ktxTexture2GetImageOffsetDelegate(texture, mipLevel, layer, faceIndex, &imageOffset);
        if (result == KtxErrorCode.KtxSuccess)
        {
            return imageOffset;
        }

        throw new InvalidOperationException("Handle this properly");
    }

    public static unsafe uint GetImageSize(KtxTexture* texture, uint mipLevel)
    {
        if (_nativeLibraryHandle == nint.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        return _ktxTexture2GetImageSizeDelegate(texture, mipLevel);
    }

    private static string GetLibraryName()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? File.Exists("runtimes/win-x64/native/ktx.dll")
                ? "runtimes/win-x64/native/ktx.dll"
                : "ktx.dll"
            : File.Exists("runtimes/linux-x64/native/libktx.so.4.2.1")
                ? "runtimes/linux-x64/native/libktx.so.4.2.1"
                : "libktx.so";
    }
}