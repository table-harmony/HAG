# Harmony Image Converter

A comprehensive image conversion solution with support for multiple formats and a custom compression format (HAG).

## Projects

- **ImageProcessing**: Core library for image conversion and HAG format implementation
- **ConsoleApp**: Command-line interface for image conversion
- **CoreApp**: Web-based interface for image conversion

## Features

- Convert between multiple image formats
- Custom HAG format based on QOI algorithm
- Both CLI and web interfaces
- Efficient memory usage with stream processing
- Extensible architecture for new formats

## HAG Format

HAG (Harmonic Advanced Graphics) is inspired by the [QOI (Quite OK Image Format)](https://qoiformat.org/), implementing similar compression techniques with some modifications:

### Specification

1. Header (8 bytes):

   - Width (4 bytes, big-endian)
   - Height (4 bytes, big-endian)

2. Pixel Encoding Commands:

   - `0xC0`: CLE (Copy-Length Encoding)
   - `0x40`: Small Delta
   - `0x80`: Big Delta
   - `0x00`: Lookup Table Reference
   - `0xFE`: RGB Pixel
   - `0xFF`: RGBA Pixel

3. Compression Features:
   - Run-length encoding for repeated pixels
   - Delta encoding for similar pixels (2-bit and 6-bit variants)
   - 64-pixel lookup table for frequent colors

## Architecture

The solution uses a layered architecture:

1. Core Layer (ImageProcessing):

   - Format-agnostic image representation (SIF)
   - Format-specific serializers
   - Compression codecs

2. Interface Layer:
   - Console interface for scripting/automation
   - Web interface for user interaction

## Contributing

1. Fork the repository
2. Create a feature branch
3. Submit a pull request

## License

MIT License - See LICENSE file for details
