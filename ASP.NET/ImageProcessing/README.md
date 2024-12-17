# Image Processing Library

Core library for image format conversion and the HAG (Harmony Advanced Graphics) format implementation.

## Features

- Convert between multiple image formats (PNG, JPEG, BMP, WebP, QOI, HAG)
- HAG format implementation with advanced compression techniques
- Standardized intermediate format (SIF) for consistent conversion

## Architecture

- **Core/** - Base classes and interfaces for image representation
- **Serializers/** - Format-specific image serializers
- **Services/** - Core conversion and codec logic

## HAG Format Specification

The HAG format uses several compression techniques:

1. Copy-Length Encoding (CLE)

   - Compresses repeated pixels
   - Command byte: 0xC0 | count

2. Delta Encoding

   - Small deltas (2 bits per channel)
   - Large deltas (6 bits red, 5 bits green/blue)

3. Lookup Table
   - Stores up to 63 unique pixels
   - References pixels using 6-bit indices
