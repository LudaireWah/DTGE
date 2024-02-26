# Setting up your development environment
This page describes how to set up your development environment so you can get started making changes to the DTGE Engine!

## Prerequisites
Before getting started you'll need to install the following:
- The Godot game engine. You can download this from [Godot's website](https://godotengine.org/download). DTGE is currently using Godot version [4.2.1](https://godotengine.org/download/archive/4.2.1-stable).
- Visual Studio 2022. You can find this on Visual Studio's [download page](https://visualstudio.microsoft.com/downloads/).
  > **_NOTE:_**  Godot supports editors other than Visual Studio, which you are free to use. However, all DTGE documentation will have instructions for Visual Studio.

## First time steps
You'll need to do these steps the first time you clone the DTGE repository, but shouldn't need to in the future.

### 1) Clone your repository
You can clone the latest version of our repository with the command `git clone git@github.com:LudaireWah/DTGE.git`. Replace the URL with your individual fork if needed.

### 2) Setup your scene directory
In order to test the engine you'll likely want to have some kind of scene for the game to load. We've included some sample scenes in the `<Repo root>/Sample Scenes` directory. Create a new directory at `<Repo root>/DTGEScenes` and copy the scenes from our samples, or any of your choice into it.

### 3) Setup your launch settings
- The `Properties/launchSettings.json` defines different profiles for launching your project in Visual Studio. In order to launch DTGE from VS you'll want to set it up. 
- The easiest way to get started is:
  - Copy the included sample file at `Properties/launchSettings-Sample.json` and create a new copy at `Properties/launchSettings.json`.
  - Then update the "executablePath" property to point to the location Godot is install on your system.
- Currently the sample includes two profiles, one to launch the engine and another to launch the game directly.

## Try it!
If everything has been setup correctly you should be able to import the project into Godot to modify/run, or open `DTGE.sln` in Visual Studio and build/debug the engine!