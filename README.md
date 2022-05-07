# Unity Character-Dev
This is developer project to accompany the [Character Unity Package](https://github.com/TheWizardsCode/Character-Unity-Package). 
If you want to use the character package in your own project you do not need this project, however, this is useful for developing
the character package itself. It also contains some configurations of popular character packs and animation packs. 

# Dev Setup

  * Checkout this project using `git clone https://github.com/TheWizardsCode/Character-Dev.git`
  * Checkout [Character Unity Package](https://github.com/TheWizardsCode/Character-Unity-Package) using `git clone https://github.com/TheWizardsCode/Character-Unity-Package.git`
  * Open the Character Dev project in Unity.
  * In Package Manager add the Character Unity Package path to the disk ![image](https://user-images.githubusercontent.com/250240/159505725-4bc5311b-c8a5-4128-9d64-58e6b6259abd.png)

# Getting Started

This section will walk you through getting started with the Character Dev project.

## Testing Character Unity Package

The Character Unity Package is where all the action happens. This project is a placeholder for development work. So lets start off by taking a look at the Character Unity Package project first.
Since it is improted as a package it's files are in the `Packages/Wizards Code - Character` folder. Let's start by opening `Packages/Wizards Code - Character/Scenes/Village Life`. 
This scene has some reasonably complex AI characters that eat, sleep work and play. Go ahead and hit the play button in Unity, the characters will start to live their lives. 
Note the animations and models used in these demo scenes are minimalistic. We are trying to ensure this package can be used without being required to purchase additional assets.

There are many test scenes in the Character Unity Package project.

## Creating a new Scene

In this section we will create a new scene from scratch, although the final output of this scene is available in the project at `Packages/Wizards Code - Character/Scenes/Croweds/201 Pedestrians`, you can follow along or simply use that scene as a reference.

  * Create a new scene
  * Add a plane called `Ground`
    * Reset position
    * Size = 10, 1, 10
    * Material = grass
  * Add a small city, easiest way is to drag in `Packages/Wizards Code - Character/Prefabs/Prototyping/City Blocks`
    * NOTE: if you use this prefab it is already setup with some required NavMesh settings, these will have to be setup manually if you create your city in a different way. More on this later.
  * Add an empty game object called `Crowd Spawner` roughly at the center of one block
    * Add a `Spawner` component
      * Spawn Definition = `City Pedestrians in Three Colors`
      * Number of Spawns = 100
      * Radius = 100
      * On Nav Mesh = true
  * Duplicate the `Crowd Spawner` and move it to roughly the center of the other block
  * Open the Navigation Window
    * If using the City Block example prefab then NavMesh areas will be setup already, otherwise these will need to be setup
    * Bake the NavMesh
    * Inspect the navmesh to ensure there are joints between the various navmesh areas. If you have gaps descreas the voxel size - for the demo prefabv 0.06 works will
  * Position the camera so you can see the city
  * Hit Play
  
## Create a City with CiDy 2

  * Create a new scene
  * Add a terrain
  * `Window - CiDy -> CiDy Editor`
  * Click `Find or Create Scene Graph`
  * Create the blocks
    * `Shift + Right Click` to add nodes
    * `Shift + Left Click` to select a node
  * Set the block type
    * `Shift + Left Click` on the cell for the block
    * Set `Cell District Theme` in the inspector
    * Click `Generate`
  * Create some curvey roads
    * `Shift click` the road
    * Move the handles
    * Click `Generate`
  
## Add Traffic

  * A city with traffic - https://www.youtube.com/watch?v=qJ1pscUC3p0&t=184s

## Add Pedestrians

  * https://www.youtube.com/watch?v=qJ1pscUC3p0&t=197s

## Using your own models for the Pedestrians

## Using your own models for the city

## Building into the terrain

  





