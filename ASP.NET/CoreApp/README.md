# Image Converter Web Application

A web-based interface for converting images between different formats using the Harmony image processing library.

## Features

- User-friendly web interface
- Drag-and-drop file upload
- Multiple output format support
- Instant file download after conversion
- Error handling and validation

## Development Setup

1. Prerequisites

   - .NET 8.0 SDK
   - Visual Studio 2022 or VS Code

2. Build and Run ```bash

   # Restore dependencies

   dotnet restore

   # Run the application

   dotnet run ```

3. Access the application at `http://localhost:7070`

## Usage

1. Open the web interface
2. Upload an image using the file picker or drag-and-drop
3. Select the desired output format
4. Click "Convert" to process the image
5. The converted file will automatically download

## Supported Formats

- PNG (.png)
- JPEG (.jpg, .jpeg)
- HAG (.hag) - Harmony Advanced Graphics
- BMP (.bmp)
- WebP (.webp)
- QOI (.qoi)
