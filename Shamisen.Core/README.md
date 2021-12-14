# Shamisen.Core - Core components of [Shamisen](https://github.com/MineCake147E/Shamisen)

This library provides core components of [Shamisen](https://github.com/MineCake147E/Shamisen), excluding IOs, Codecs, and Pipelines.

## Currently implemented features in Shamisen.Core

### Digital Signal Processing

+ Attenuation
+ Catmull-Rom Spline Upsampling
+ BiQuad Filters
+ Simple Mixing
+ Multiplying two audio sources
+ Conversion between some PCM formats and IEEE754 Binary32
  + LinearPCM(Signed 16bit, Signed 24bit, Signed 32bit, Signed 8bit Excess-128)
  + G.711(8bit μ-Law, 8bit A-Law)
+ Waveform Synthesis
  + Sinusoidal Wave
  + Square Wave
  + Triangular Wave
  + Sawtooth Wave
  + Silence and DC Offset

### WAVE File Manipulation

+ WAVE File Parser(Decoder)
  + RF64 and BWF support
+ WAVE File Composer(Encoder)
  + RF64 and BWF support

## License

### Shamisen.Core itself

[Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0)
