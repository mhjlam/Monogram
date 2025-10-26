# Monogram

Real-Time 3D Model and Scene Renderer

<p align="center">
   <img src="media/monogram.gif" alt="Demo Animation" width="720"/>
</p>

## Overview

Monogram is a real-time 3D rendering demo built with MonoGame, showcasing advanced rendering techniques, lighting models, and post-processing effects. The project uses the MonoGame framework, leveraging .NET 8 and C# 12. It features modular scene management, a flexible camera system, and shader-based rendering.

## Features

- **Shaders**: Lambertian, Blinn-Phong, Cook-Torrance.
- **Post-Processing**: Monochrome and Gaussian blur filters.
- **Dynamic Lighting**: Multiple light sources, spotlight effects, and projected texture lighting.
- **Camera System**: Orbit, zoom, and reset camera with smooth mouse controls.
- **User Interface**: Scene selection dropdown box, FPS counter and per-scene overlay text.

## Scenes

| Scene                         | Description                                           |
|-------------------------------|-------------------------------------------------------|
| Terrain                       | Heightmap-based terrain with animated scanline effect |
| Lambertian                    | Lambertian diffuse shading                            |
| Blinn-Phong                   | Visually approximate Blinn-Phong BRDF                 |
| Cook-Torrance                 | Physically-based Cook-Torrance BRDF                   |
| Spotlight                     | Single spotlight illumination                         |
| MultiLight                    | Multiple dynamic light sources                        |
| Texture Projection            | Projected texture mapping                             |
| Post-Processor: Monochrome    | Monochrome post-processing effect                     |
| Post-Processor: Gaussian Blur | Gaussian blur post-processing                         |
| Culling                       | Animated row of models with frustum culling           |

## Controls

| Key Combination   | Action                            |
|-------------------|-----------------------------------|
| **Tab**           | Next scene                        |
| **Shift+Tab**     | Previous scene                    |
| **Mouse**         | Orbit camera (hold right button)  |
| **Scroll Wheel**  | Rotate model                      |
| **A and D**       | Rotate model left and right       |
| **Backspace**     | Reset camera and model rotation   |
| **Escape**        | Exit application                  |

## Implementation

- **Renderer**: The core of the application. It manages all scenes, handles switching between them, and coordinates rendering and camera updates.
- **Scene**: Represents each visual demo, such as different shading models or effects. Supports custom overlays and post-processing.
- **Model**: Represents 3D objects in each scene, supporting transformations like scaling, rotation, and translation. Models can be loaded from files or generated procedurally (e.g., terrain).
- **Shaders**: Provide the visual effects and lighting models, including Lambertian, Phong, Cook-Torrance, and special effects like spotlight and texture projection.
- **Filter**: Adds post-processing effects such as monochrome and Gaussian blur to enhance the rendered image.
- **Input**: Handles all keyboard and mouse input, including camera movement, model rotation, and scene selection. Integrates with the dropdown and overlay for a seamless user experience.
- **Camera**: Provides smooth orbit, zoom, and reset controls for viewing scenes from any angle. Essential for exploring 3D content.
- **Overlay**: Displays the FPS counter, scene selection dropdown, and any scene-specific overlays (like culling stats), making it easy to monitor and interact with the demo.

## License

This software is licensed under the [CC BY-NC-SA](https://creativecommons.org/licenses/by-nc-sa/4.0/) license.
