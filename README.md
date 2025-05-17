# Monogram: Real-Time 3D Rendering Demo

## Overview

Monogram is a real-time 3D rendering demo built with MonoGame, showcasing advanced rendering techniques, lighting models, and post-processing effects. The project uses the MonoGame framework, leveraging .NET 8 and C# 12. It features modular scene management, a flexible camera system, and shader-based rendering.

## Features

- **Shaders**: Lambertian, Blinn-Phong, Cook-Torrance.
- **Post-Processing**: Monochrome and Gaussian Blur filters.
- **Dynamic Lighting**: Supports multiple light sources, spotlight effects, and textured lighting projection.
- **Terrain Rendering**: Generate terrain from heightmaps with computed normals.
- **Camera System**: Orbit around scenes with the mouse and rotate models.

## Scenes

| Scene                         | Description                         |
|-------------------------------|-------------------------------------|
| Terrain                       | Heightmap-based terrain rendering   |
| Lambertian                    | Lambertian diffuse shading          |
| Blinn-Phong                   | Blinn-Phong specular highlights     |
| Cook-Torrance                 | Physically-based Cook-Torrance BRDF |
| Spotlight                     | Single spotlight illumination       |
| MultiLight                    | Multiple dynamic light sources      |
| Texture Projection            | Projected texture mapping           |
| Post-Processor: Monochrome    | Monochrome post-processing effect   |
| Post-Processor: Gaussian Blur | Gaussian blur post-processing       |

And more.

## Controls

| Key Combination   | Action                            |
|-------------------|-----------------------------------|
| **Space**         | Next scene                        |
| **Shift+Space**   | Previous scene                    |
| **Mouse**         | Orbit camera (hold right button)  |
| **Scroll Wheel**  | Rotate model left and right       |
| **A and D**       | Rotate model left and right       |
| **R**             | Reset camera and model rotation   |
| **Escape**        | Exit application                  |

## Implementation

- **Renderer**: Manages scene creation, switching, and rendering.
- **Input**: Centralizes keyboard and mouse input for camera and scene control.
- **Camera**: Allows look-at rotation, orbit, and zoom functionality.
- **Scene**: A scene demonstrates a specific rendering technique. It is defined by a shader, one or several models, and optional post-processor.
- **Shaders**: Includes Lambertian, Phong, Cook-Torrance, and custom shaders for effects like spotlight and texture projection.
- **Filter**: Adds effects like monochrome and Gaussian blur as post-process rendering stages.
