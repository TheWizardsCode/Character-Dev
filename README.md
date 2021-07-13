# Character
Various scripts and utilities for controlling characters in Unity. The project is intended to be self-documenting. Drop into the [Scenes folder](https://github.com/TheWizardsCode/Character/tree/master/Assets/WizardsCode/Character/Scenes) and take a look at each of them.

This is open source, if it doesn't do what you need raise and issue to see if it's something we will add. It's much more likely to be added if you provide the code - but don't worry about simply making suggestions. Bug reports are especially valuable. Use the [issue tracker](https://github.com/TheWizardsCode/Character/issues) please.

# Installation

You can either install as a package or from source. We recommend installing from source since this is open source code and we'd love you to contribute fixes, documentations, demo scenes and more. Below are instructions on how to install.

Note if you are on 2018 you will have to checkout the code into your project as we do not release unity packages. The below methods are best for 2019+

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
  3. In the inspector click `Export Package Source`, this will export the package to a folder next to your checkout director called "Character-Release"
  4. To use this package in your development environments go to `Window -> Package Manager`
  5. Click the '+" in the top left
  6. Select 'Add package from disk ...'
  7. Point to the `package.json` file in the `Character-Release` directory 
  
If you find a bug or want to make an improvement do it inside the Character project in Unity. To make it available to your work projects repeat step 2 and 3 above. This will re-publish your package locally and will be automatically picked up when you next give your development environment focus. 

Once you have tested the changes please issue a pull request against our repo so we can make the code better for everyone.

### INK support

Ink is an open source scripting language for writing interactive narrative, both for text-centric games as well as more graphical games that contain highly branching stories. It's designed to be easy to learn, but with powerful enough features to allow an advanced level of structuring. This package has code within it to allow your characters to be controlled from Ink scripts. By default, however, it will not be compiled as we don't want to assume everyone wants Ink. To enable it:

  1. Install the free [Ink package](https://bit.ly/InkNarrative) from the Unity Asset Store
  2. Add a script defines of `INK_PREASENT` to your project

# Release Process

We use [PackageTools](https://github.com/jeffcampbellmakesgames/unity-package-tools) to create our releases. To build a release:

  0. Alongside your working repository checkout the `release/stable` branch of this repo into a directory called `Character-Release` using `git clone --single-branch --branch release/stable git@github.com:TheWizardsCode/Character.git Character-Release`
  1. Update the version number in the `Release PackageManifestConfig` to match that in `DevTest PackageManifestConfig` (both are in the root of the `Assets` folder)
  2. Increase the version number in the `DevTest PackageManifestConfig` to represent the next release number (not this release)
  3. Click `Generate VersionConstants.cs` in the inspector from the release package manifest
  4. Commit the new constants file to Git
  5. Click `Export Package Source` in the inspector for the `Release Candidate PackageManifestConfig`
  6. Commit and push the changes in `DevLogger-Release` to GitHub [But SEE BELOW]
  7. Click `Generate VersionConstants.cs` in the inspector from the DevTest package manifest

NOTE there is currently a [bug](https://github.com/jeffcampbellmakesgames/unity-package-tools/issues/11) in the package manager tool that prevents the above from working, at least on my machine. You can work around the bug with the following steps:

1. Delete the existing package directory
2. Export the package source as described above
3. cd into the package directory
4. `git init`
5. `git remote add origin git@github.com:TheWizardsCode/Character.git`
6. `git fetch`
7. `git add .`
8. `git commit -m "Release v0.2.5`
9. `git branch -m master release/stable`
10. `git push -f -u origin release/stable`


