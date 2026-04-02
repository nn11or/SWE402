# ProductShowcase - Neon Cyberpunk Museum

Lab 7: Product Showcase Scene (Shaders & Materials + Lighting)

## Theme
**Neon Cyberpunk Museum** - A dark, moody museum with neon accents, metallic surfaces, and glowing displays.

## Quick Setup

### Step 1: Create URP Project
1. Open **Unity Hub**
2. Click **New Project** > Select **3D (URP)** template
3. Name it `ProductShowcase`
4. Create the project

### Step 2: Copy Files
Copy the `Assets/` folder contents from this repo into your Unity project's `Assets/` folder:
- `Assets/Editor/SceneSetupEditor.cs`
- `Assets/Editor/ShaderGraphHelper.cs`

### Step 3: Build the Scene
1. In Unity, go to **Tools > Build Product Showcase Scene**
2. This automatically creates:
   - Ground plane + back wall
   - 5 plinths/platforms
   - 6 product objects with distinct materials
   - 7 materials (matte, shiny, metal, transparent, emissive, textured, gold)
   - Directional moonlight + point light + spot light
   - Three-point lighting on center capsule
   - Area light (baked)
   - Light Probe Group
   - Reflection Probe
   - Organized hierarchy (Environment, Plinths, Products, Lights, Probes)
   - Procedural textures (checker base map + normal map)

### Step 4: Create Shader Graph
1. Go to **Tools > Shader Graph Instructions** for step-by-step guide
2. OR use **Tools > Create Shimmer Shader (Code Fallback)** for a code-based shader
3. Assign the material to `Product_ShaderGraphDiamond`

### Step 5: Bake Lighting
1. Save the scene to `Assets/Scenes/ProductShowcase.unity`
2. Go to **Window > Rendering > Lighting**
3. Click **Generate Lighting**
4. Wait for bake to complete

## Checklist Coverage

| Section | Items | Covered |
|---------|-------|---------|
| Setup | 3 | All automated |
| A. Materials & Surfaces | 6 | All automated |
| B. Textures & Detail | 3 | All automated |
| C. Shader Graph | 2 | Manual step (see Step 4) |
| D. Light Sources | 4 | All automated |
| E. Shadows & Baking | 4 | Bake is manual (Step 5) |
| F. Probes & Polish | 3 | All automated |
| G. Theme & Cohesion | 2 | Theme: Neon Cyberpunk Museum |

## Materials Created
1. **M_DarkStone_Matte** - Metallic=0, Smoothness=0.1 (plinths)
2. **M_CyanCeramic_Shiny** - Metallic=0, Smoothness=0.85 (shiny non-metal)
3. **M_Chrome_Metal** - Metallic=1, Smoothness=0.92 (chrome)
4. **M_NeonGlass_Transparent** - Transparent, alpha=0.45 (glass)
5. **M_NeonGlow_Emissive** - Emission enabled, HDR pink glow
6. **M_CyberpunkFloor_Textured** - Checker texture + normal map
7. **M_Gold_Metal** - Metallic=1, Smoothness=0.85 (gold)
