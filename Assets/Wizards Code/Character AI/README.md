# Character
Various scripts and utilities for controlling characters in Unity. The project is intended to be self-documenting. Drop into the [Scenes folder](https://github.com/TheWizardsCode/Character/tree/master/Assets/WizardsCode/Character/Scenes) and take a look at each of them.

This is open source, if it doesn't do what you need raise and issue to see if it's something we will add. It's much more likely to be added if you provide the code - but don't worry about simply making suggestions. Bug reports are especially valuable. Use the [issue tracker](https://github.com/TheWizardsCode/Character/issues) please.

# Documentation

We try to make the project self documenting. Load up any of the demo scenes and they should present a documentation file on run. You can find the source for these files in `Character/Documentation` along with some suplementary materials.

# Installation

You can either install as a package or from source. We recommend installing from source since this is open source code and we'd love you to contribute fixes, documentations, demo scenes and more. Below are instructions on how to install.

Note if you are on 2018 you will have to checkout the code into your project as we do not release unity packages. The below methods are best for 2019+

## Install Via Package Manager

This method is super easy and doesn't require Git. However, it will not autoupdate:

  1. `Window -> Package Manager`
  2. Click the '+" in the top left
  3. Select 'Add package from Git URL'
  4. Paste in `https://github.com/TheWizardsCode/Character-Unity-Package.git`
  
## Installation Of Development Code

We are a big fan of enabling our users to improve our code, so we would encourage you to use the source code. You can do this using out own [Character-Dev](https://github.com/TheWizardsCode/Character-Dev) project or you can do it within your own development project.

  1. Fork and clone the repo into your preferred location with `git clone [YOUR_FORK_URL]`
  2. Create a Unity project (or checkout [Character-Dev](https://github.com/TheWizardsCode/Character-Dev))
  3. Add this package to project `Window -> Package Manager`
  5. Click the '+" in the top left
  6. Select 'Add package from disk ...'
  7. Point to the `package.json` file in your clone of this repository

What happens here is exactly the same as the above version other than you have a local copy on disk which allows you to edit the code in place. You can then use Git to issue pull requests or update your version of this package (you can also stay up to date in the package manager of course).

Once you have tested the changes please issue a pull request against our repo so we can make the code better for everyone.

### INK support

Ink is an open source scripting language for writing interactive narrative, both for text-centric games as well as more graphical games that contain highly branching stories. It's designed to be easy to learn, but with powerful enough features to allow an advanced level of structuring. This package has code within it to allow your characters to be controlled from Ink scripts. By default, however, it will not be compiled as we don't want to assume everyone wants Ink. To enable it:

  1. Install the free [Ink package](https://bit.ly/InkNarrative) from the Unity Asset Store
  2. Add a script defines of `INK_PRESENT` to your project




