![Shamisen Logo](Shamisen-Logo.svg)

# Shamisen - .NET Audio Library

[![CodeFactor](https://www.codefactor.io/repository/github/minecake147e/shamisen/badge)](https://www.codefactor.io/repository/github/minecake147e/shamisen)
[![.NET](https://github.com/MineCake147E/Shamisen/actions/workflows/dotnet.yml/badge.svg)](https://github.com/MineCake147E/Shamisen/actions/workflows/dotnet.yml)  
A Cross-Platform Audio Library for .NET 8.

## Usage of Shamisen

- Fast Digital Signal Processing
- Abstraction Layer for Audio I/O

## Currently implemented features

### Audio I/O and bindings

#### Managed backends using existing library

| Name (Backend)                                                                                       | Author (Backend)                          | License (Binding)                                                                      | Windows10 Desktop | Windows10 UWP | Android | Linux | iOS | Mac OSX |
| ---------------------------------------------------------------------------------------------------- | ----------------------------------------- | -------------------------------------------------------------------------------------- | :-------------: | :-----------: | :-----: | :---: | :-: | :-----: |
| [OpenTK (OpenAL)](https://github.com/opentk/opentk)                                                           | [OpenTK](https://github.com/opentk)       | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) |       ✅        |      ❓       |   ❓    |  ❓   | ❓  |   ❓    |
| [AudioGraph on<br/>.NET 5 or later](https://docs.microsoft.com/en-us/windows/apps/desktop/modernize/desktop-to-uwp-enhance) | [Microsoft](https://github.com/microsoft) | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) |       ✅        |      ❎       |   ❎    |  ❎   | ❎  |   ❎    |
| [NAudio (WaveOut, ASIO, DirectSound, WASAPI)](https://github.com/naudio/NAudio)                                                           | [NAudio](https://github.com/naudio)       | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) |       ✅        |      ❓       |   ❎    |  ❎   | ❎  |   ❎    |
| [Xamarin.Android (AudioTrack)](https://github.com/xamarin/xamarin-android)                                        | [Xamarin](https://github.com/xamarin)     | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) |       ❎        |      ❎       |   ✅    |  ❎   | ❎  |   ❎    |

❓: Not Tested or needs more information  
✅: Tested  
❎: Impossible

### Digital Signal Processing (Cross-Platform)

#### Fast and smooth Up-sampling using Catmull-Rom Spline

- Utilizes `System.Numerics.Vectors` and `System.Runtime.Intrinsics` for resampling calculation.
- Uses Direct/Wrapped caching for Catmull-Rom spline coefficients.

[Benchmarks on .Net 5, Intel Core i7 4790](Shamisen.Benchmarks.ResamplerBenchmarks-report-github.md)

#### Highly Optimized Cooley–Tukey FFT algorithm for single-precision 1D complex data.

- Utilizes `System.Runtime.Intrinsics.X86.Avx` and `System.Runtime.Intrinsics.X86.Avx2` for FFT calculation.
- Forward transformation rescales result by 1/N.
- The memory consumption is O(N) even though the FFT is done in-place.
- Does not need initialization for certain fixed size by default.
  - Fixed-size variant is also implemented and is slightly faster.
- It requires that the size of the data be a power of two. Otherwise, the data will be implicitly resized to the size of the largest power of two less than the original size.

#### Fast conversion between PCM sample formats

| To\From | IEEE 754<br>Binary32(float) | 32bit Linear<br>PCM(Q0.31) | 24bit Linear<br>PCM(Q0.23) | 16bit Linear<br>PCM(Q0.15) | 8bit LPCM<br>(Excess-128) | G.711<br>μ−Law | G.711<br>A-Law |
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| IEEE 754<br>Binary32(float) | ✖️ | ✅* | ✅ | ✅ | ✅ | ✅ | ✅ |
| 32bit Linear<br>PCM(Q0.31) | ✅* | ✖️ | ☑️ | ☑️ | ☑️ | ☑️ | ☑️ |
| 24bit Linear<br>PCM(Q0.23) | ✅* | ☑️* | ✖️ | ☑️ | ☑️ | ☑️ | ☑️ |
| 16bit Linear<br>PCM(Q0.15) | ✅* | ☑️* | ☑️* | ✖️ | ☑️ | ☑️ | ☑️ |
| 8bit LPCM<br>(Excess-128) | ✅* | ☑️* | ☑️* | ☑️* | ✖️ | ☑️* | ☑️* |
| G.711 μ−Law | ✅* | ☑️* | ☑️* | ☑️* | ☑️* | ✖️ | ☑️* |
| G.711 A-Law | ✅* | ☑️* | ☑️* | ☑️* | ☑️* | ☑️* | ✖️ |

Legends:  
✅: Shamisen has optimized implementation of direct conversion.  
☑️: Shamisen can handle conversion by 2 or more converter. Can be partially optimized. Depending on the combination, noise due to quantization error may occur.  
✔: Shamisen has simple implementation of direct conversion.  
⭕: Shamisen can handle conversion by 2 or more converter. Both converter is implemented in simple way. Depending on the combination, noise due to quantization error may occur.  
❎: Shamisen has no support for conversion.  
✖️: No conversion needed(same format).  
\* : Such conversion can cause noise due to quantization errors.  

#### Optimized BiQuad Filters that supports some filtering

- Uses `Vector2` and `Vector3` for filter calculations in each channels.
- Unrolls channel loop for Monaural and <5ch filter calculation.
- For some special cases, it utilizes SSEx.x and AVX(2) intrinsics for the calculation.

#### Other Features

- `FastFill` for some types that fills quickly using `Vector<T>`.

### File Formats and Codecs

#### Cross-Platform

| Container Name    | Typical File Extensions | Implemented Codec                              | Library contains Decoder/Encoder | License                                                                                | Decoding |  Encoding   |
| ----------------- | ----------------------- | ---------------------------------------------- | -------------------------------- | -------------------------------------------------------------------------------------- | :------: | :---------: |
| Waveform<br/>RF64 | `.wav`                  | Linear PCM, IEEE 754 Floating-Point PCM, A-Law, μ-law | Shamisen                         | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) |    ✅    | ✅<br/>(RF64 as default) |
| FLAC              | `.flac`                 | FLAC                                           | Shamisen.Codecs.Flac | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) |    ✅    | ❎(Planned) |

Legends:  
✅: Supported by Shamisen itself  
✔: Supported by another library and its wrapper for Shamisen  
❎: Not supported by Shamisen without any custom integration

#### Platform-Dependent

- Any formats supported by platform-dependent binding libraries

## Dependencies and system requirements

- Currently, **_Unity IS NOT SUPPORTED AT ALL!_**
- Requires [DivideSharp](https://github.com/MineCake147E/DivideSharp) for frequently appearing divide-by-number-of-channels operations.
- The most processing in this library fully depends on SINGLE core.
  - Because `Span<T>` does not support multi-thread processing at all.

## Features planned or under development

### Audio I/O and bindings

#### Native backends

✅: Possible  
❓: Needs more information  
❎: Impossible

| Name of Backend                        | Author of Backend                   | License (binding)                                                                      | Target Platform | Status                |
| -------------------------------------- | ----------------------------------- | -------------------------------------------------------------------------------------- | --------------- | --------------------- |
| [Oboe](https://github.com/google/oboe) | [Google](https://github.com/google) | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | Android >10     | Planned |

#### Managed backends

✅: Possible  
❓: Needs more information  
❎: Impossible

| Name of Backend | Author of Backend | License (binding)                                                                      | Target Platforms | Status  |
| --------------- | ----------------- | -------------------------------------------------------------------------------------- | ---------------- | ------- |
| [Silk.NET](https://github.com/dotnet/Silk.NET) OpenAL | [.NET Foundation Silk.NET Team](https://github.com/dotnet) | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | Cross Platform(OpenAL) | Gathering Information |
| [Silk.NET](https://github.com/dotnet/Silk.NET) XAudio | [.NET Foundation Silk.NET Team](https://github.com/dotnet) | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | Windows-Like(XAudio) | Planned |
| Xamarin.iOS     | Microsoft         | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | iOS              | Planned |

### File Formats and Codecs

#### Cross-Platform

✅: Shamisen will have Managed Implementation of decoder/encoder itself  
⭕: Shamisen will have Managed Wrapper for another library  
❎: Not included in plan currently

| Container Name | Typical File Extensions | Target Codec | Planned Library containing Decoder/Encoder          | Planned Library License                                                                | Decoding | Encoding | Status              |
| -------------- | ----------------------- | ------------ | --------------------------------------------------- | -------------------------------------------------------------------------------------- | :------: | :------: | ------------------- |
| FLAC           | `.flac`                 | FLAC         | Shamisen.Codecs.Flac                                       | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) |    ✅    |    ✅    | Implemented Decoder |
| Opus           | `.opus`                 | Opus         | Shamisen.Codecs.Opus | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) |    ✅    |    ✅    | Planned             |
| Ogg            | `.ogg`                  | Vorbis       | Shamisen.Codecs.Ogg                                 | [MIT License](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) |    ⭕    |    ⭕    | Planned             |
