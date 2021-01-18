# Character
Various scripts and utilities for controlling characters in Unity.

# Installation

You can either install as a package or from source. We recommend installing from source since this is open source code and we'd love you to contribute fixes, documentations, demo scenes and more. Below are instructions on how to install.

## Install Via Package Manager

This method is super easy and doesn't require Git. However, it will not autoupdate:

  1. `Window -> Package Manager`
  2. Click the '+" in the top left
  3. Select 'Add package from Git URL'
  4. Paste in `https://github.com/TheWizardsCode/Character.git#release/stable`
  
## Installation Of Development Code

We are a big fan of enabling our users to improve Dev Logger, so we would encourage you to use the source code, it's not much harder than using the package manager method and has the advantage of auto updating your projects when you make local modifications or do `git pull`:

  1. Fork and clone the repo and submodules into your preferred location with `git clone --recurse-submodules [YOUR_FORK_URL]`
  2. In the project view select `Assets/DevTest PackageManifestConfig`
  3. In the inspector click `Export Package Source`, this will export the package to a folder next to your checkout director called "DevLogger-Package"
  4. To use this package in your development environments go to `Window -> Package Manager`
  5. Click the '+" in the top left
  6. Select 'Add package from disk ...'
  7. Point to the `package.json` file in the `DevLogger-Package` directory 
  
If you find a bug or want to make an improvement do it inside the DevLogger project in Unity. To make it available to your work projects repeat step 2 and 3 above. This will re-publish your package locally and will be automatically picked up when you next give your development environment focus. 

Once you have tested the changes please issue a pull request against our repo so we can make the code better for everyone.

# Release Process

We use [PackageTools](https://github.com/3dtbd/unity-package-tools) to create our releases. To build a release:

  0. Alongside your working repository checkout the `release/stable` branch of this repo
  1. Update (at least) the version number in the `PackageManifestConfig` in the root of the `Assets` folder
  2. Click `Generate VersionConstants.cs` in the inspector
  3. Commit the new constants file to Git
  4. Click `Export Package Source`
  5. Commit and push the changes in your release project to GitHub

