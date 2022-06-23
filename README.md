# EiveoEngine

This repository will evolve into a game development framework.
Using the framework does not enforce any useage of other framework component.
Our principles are as simple as `take only what you want`.

**Why**
- We couldn't find a standalone renderer in c#.
- Unity and UE is more than we need for our application.

## Graphics

The renderer uses OpenGL 4.1 - for MacOS compatibility.
Due to that we had to work around some features:

- `Bindless Textures`
- `Shader Storage Buffer Objects`

See: [Wikipedia](https://en.wikipedia.org/wiki/OpenGL#Version_history) for more infos.

### Current state:
![Screenshot](screenshot.png)

- Runs on Windows, Apple M1, Linux / Steam Deck
- Deferred renderer
- Materials using Albedo, Normal, Specular, Emissive and Cube maps.
- Support for infinite Ambient, Directional, Point and Spot lights.

### Roadmap:
- Shadow mapping
- Composing the final render result
