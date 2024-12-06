# Unity Character-Dev

This package contains code for creating AI controlled characters in Unity. The AI system is based on a Utility AI model.

Remember this is an open source project in a state of ongoing development. Sometimes it goes for long periods with no attention
other times it is under very active development. When under development it will usually be a specific set of features that are
being worked on. We try to not break things, but the reality is that we do sometimes make mistakes.

Let us know if you find bugs via the issue tracker, ideally give us a patch to fix it too.

# User Setup

Install, via Package Manager, from Git:
 * https://github.com/TheWizardsCode/Character-Dev.git?path=/Assets/Wizards%20Code/Character%20AI

# Dev Setup

  * Fork this repo and checkout your fork
  * Open the Character Dev project in Unity
  * If you want to use the INK integration install the Ink package and add `INK_RESENT` to the script defines.

# Getting Started

This section will walk you through getting started with the Character Dev project.

## Documentation

The project is intended to be self documenting through demo scenes, so get started in the `Assets/Wizards Code/Character AI/Scenes/` folder. Within there you will find a set of numbered subfolders, each covering a different feature set. Inside each subfolder are some numbered scenes. The intention is that you will work through the scenes in order as each one introduces a new concept. 

When you load a scene it will first show you a brief description of the scene. The goal is for this description to give you enough information to dig into the scene and objects within to understand what it happening. Therefore, if you don't know your way around Unity yet you will be pretty lost. But if you are comfortable in unity you should be able to find your way around. If you want to view the text of these scenes without loading the scenes up you can find it in `Assets/Wizards Code/Character AI/Character/Documentation`.

Note this is an ever evolving project and you are working with development code. There is no guarantee that things will work as expected. The code is not necessarily well structured either, we do try to refactor as we go, but we've not yet released an alpha stage, let alone a beta or production stage. This code is for the brave. 

We welcome your questions on [Discord](http://bit.ly/WizardsCodeDiscord) - better yet we welcome your contributions to make the code and documentation more valuable.

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

  





