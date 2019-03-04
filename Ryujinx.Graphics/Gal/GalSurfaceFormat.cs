﻿namespace Ryujinx.Graphics.Gal
{
    public enum GalSurfaceFormat
    {
        Bitmap               = 0x1c,
        Unknown1D            = 0x1d,
        Rgba32Float          = 0xc0,
        Rgba32Sint           = 0xc1,
        Rgba32Uint           = 0xc2,
        Rgbx32Float          = 0xc3,
        Rgbx32Sint           = 0xc4,
        Rgbx32Uint           = 0xc5,
        Rgba16Unorm          = 0xc6,
        Rgba16Snorm          = 0xc7,
        Rgba16Sint           = 0xc8,
        Rgba16Uint           = 0xc9,
        Rgba16Float          = 0xca,
        Rg32Float            = 0xcb,
        Rg32Sint             = 0xcc,
        Rg32Uint             = 0xcd,
        Rgbx16Float          = 0xce,
        Bgra8Unorm           = 0xcf,
        Bgra8Srgb            = 0xd0,
        Rgb10A2Unorm         = 0xd1,
        Rgb10A2Uint          = 0xd2,
        Rgba8Unorm           = 0xd5,
        Rgba8Srgb            = 0xd6,
        Rgba8Snorm           = 0xd7,
        Rgba8Sint            = 0xd8,
        Rgba8Uint            = 0xd9,
        Rg16Unorm            = 0xda,
        Rg16Snorm            = 0xdb,
        Rg16Sint             = 0xdc,
        Rg16Uint             = 0xdd,
        Rg16Float            = 0xde,
        Bgr10A2Unorm         = 0xdf,
        R11G11B10Float       = 0xe0,
        R32Sint              = 0xe3,
        R32Uint              = 0xe4,
        R32Float             = 0xe5,
        Bgrx8Unorm           = 0xe6,
        Bgrx8Srgb            = 0xe7,
        B5G6R5Unorm          = 0xe8,
        Bgr5A1Unorm          = 0xe9,
        Rg8Unorm             = 0xea,
        Rg8Snorm             = 0xeb,
        Rg8Sint              = 0xec,
        Rg8Uint              = 0xed,
        R16Unorm             = 0xee,
        R16Snorm             = 0xef,
        R16Sint              = 0xf0,
        R16Uint              = 0xf1,
        R16Float             = 0xf2,
        R8Unorm              = 0xf3,
        R8Snorm              = 0xf4,
        R8Sint               = 0xf5,
        R8Uint               = 0xf6,
        A8Unorm              = 0xf7,
        Bgr5x1Unorm          = 0xf8,
        Rgbx8Unorm           = 0xf9,
        Rgbx8Srgb            = 0xfa,
        Bgr5x1UnormUnknownFB = 0xfb,
        Bgr5x1UnormUnknownFC = 0xfc,
        Bgrx8UnormUnknownFD  = 0xfd,
        Bgrx8UnormUnknownFE  = 0xfe,
        Y32UintUnknownFF     = 0xff
    }
}