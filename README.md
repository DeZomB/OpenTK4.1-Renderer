# Welcome to our simple OpenTK Render

This small renderer is based on the openGL/openTK tutorials, plus individual refacoring. 
> **Note** The renderer uses OpenGL 4.1 - for MacOS compatibility. 

## Why

- We couldn't find a standalone renderer in c#.
- Unity and UE is more than we need for our application.

## What's not supported with OpenGL 4.1

Small overview over what's not supported by this render, because we want to go with MacOS compatibility:

| OpenGL Version |Features                          |
|----------------|-------------------------------|
|4.2|GLSL 4.20, Shaders with atomic counters, draw transform feedback instanced, shader packing, performance improvements         |
|4.3|GLSL 4.30, Compute shaders leveraging GPU parallelism, shader storage buffer objects, high-quality ETC2/EAC texture compression, increased memory security, a multi-application robustness extension, compatibility with OpenGL ES 3.0|
|4.4|GLSL 4.40, Buffer Placement Control, Efficient Asynchronous Queries, Shader Variable Layout, Efficient Multiple Object Binding, Streamlined Porting of Direct3D applications, **Bindless Texture Extension**, Sparse Texture Extension|
|4.5|GLSL 4.50, Direct State Access (DSA), Flush Control, Robustness, OpenGL ES 3.1 API and shader compatibility, DX11 emulation features|
|4.6|GLSL 4.60, More efficient geometry processing and shader execution, more information, no error context, polygon offset clamp, SPIR-V, anisotropic filtering|

See: [WikiPedia](https://en.wikipedia.org/wiki/OpenGL#Version_history) for more infos.

## Current state:
![Screenshot](screenshot.png)

- Runs on Apple M1
- Runs on Windows
- Runs on Linux / Steam Deck

## FYI

- The new Xbox Series X runs only with DX12 Renderer.
- 
# Goal:
- A 3D Rendercore with:
-   Deferred shading
-   Forward rendering
-   Shadow Mapping

After all features are implemented, we can move on to some additional stuff.
