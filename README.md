# Unity VDF Save Editor

This repository contains a lightweight Unity Editor window for inspecting and editing VDF
save files. It also supports a ScriptableObject asset to document what each VDF parameter
means, so designers can understand save data at a glance.

## How to Use

1. Copy the `Assets` folder into your Unity project.
2. Open the editor from **Tools → VDF Save Editor**.
3. Browse to a `.vdf` save file and click **Reload**.
4. (Optional) Create or assign a **VDF Field Descriptions** asset to show tooltips that
   explain what each parameter means.
5. Edit values and click **Save** to write the updated VDF back to disk.

## Field Descriptions

Create a `VdfFieldDescriptions` asset to provide documentation for specific VDF paths. Each
path uses dot notation like `player.stats.health` and matches the nested key structure in
the save file.
