# Simple-ECS

## Screenshots

https://github.com/user-attachments/assets/02bcae1d-40d3-4ed2-ad44-43b7bad11c7d

## Table of Contents
1. [Description](#description)
2. [Installation](#installation)
3. [Run](#run)
4. [Credits](#credits)
5. [Contributing](#contributing)
6. [License](#license)

## Description

This prototype is an initial exploration into ECS (Entity Component System) and DOTS (Data-Oriented Technology Stack) programming in Unity.

### Purpose

The goal of this project is to allow users to design a circuit and assign it to a car spawner. Spawned cars will then follow the defined circuit until they reach the end.

I chose to implement this project using Unity’s latest recommendations, as some older methods are deprecated or scheduled for deprecation. One of the key limitations of traditional Unity OOP is its reliance on a single-threaded architecture. To overcome this, I leveraged Unity's ECS (Entity Component System) and ScheduleParallel, which allows tasks to run across multiple threads for improving performance and scalability.

## Controls

Left click to place a circuit on editor.

### Technologies used

- **Unity** – Version 6000.2.2f1
- **C#** – Used for gameplay scripting
  
### Challenges and Future Features

Before starting this project, I had heard about ECS and DOTS but had never actually programmed with them. The biggest challenge was shifting from traditional OOP (Object-Oriented Programming) to the ECS paradigm. Concepts like baking, subscene authoring, and systems were completely new to me, and it was difficult at first to visualize how to build a functioning system using this approach.

Much of my previous Unity experience didn’t translate directly, as ECS programming steers you away from many of the familiar UnityEngine functions and patterns. This made the learning curve quite steep.

## Installation

Clone the repository:
`
git clone https://github.com/Vpekdas/Simple-ECS/
`

Then open the `Sample Scene` in Unity.

## Run

To create and run a circuit in the scene, follow these steps:

### Elements
1. Ground
   - Located in the `Assets/Prefabs/` folder.
   - Drag it into the scene. This enables raycasting, which is required for placing circuit points.
2. LevelEditor
   - Also found in the `Assets/Prefabs/` folder.
   - Place it in the scene. It allows you to create a circuit using left-click.
   - Once you're done placing the circuit, you can either remove the LevelEditor or simply disable it in the editor.
3. BakeScene (Subscene)
   - Located found in the `Assets/Scenes/` folder.
   - This subscene is crucial, it tells Unity to convert GameObjects into Entities.
   - Be sure to check the small checkbox on the right of the BakeScene in the Hierarchy to enable editing.
3. CarSpawner
   - Located found in the `Assets/Prefabs/` folder.
   - Place it in the subscene. You can customize the color, speed, and spawn interval of the cars.
   - Don’t forget to assign the circuit (created in the next step) to the spawner.

When you have placed all of those, you need to check the little fillbox at right of BakeScene to allow editing.

### Creating a Circuit
1. After placing the circuit points in the scene using the LevelEditor, group them all under a single parent GameObject.
2. Drag this parent object into the subscene to convert it into an entity.
3. Place a `CarSpawner` in the subscene and configure its parameters:
   - `CarPrefab`
   - `CircuitParent` : Give the parent you created previously.
   - `MoveSpeed`
   - `ColorPicker`
   - `SpawnInterval`

> [!Warning]
> All these elements (Ground, Circuit, CarSpawner) must be inside the subscene to be processed correctly by Unity’s ECS/DOTS pipeline.

Repeat the process for each circuit you want to create.

## Credits

Thanks to the interviewer that sent me this test ! It was very cool to program and learn.

## Contributing

To report issues, please create an issue here:  [issue tracker](https://github.com/Vpekdas/Simple-ECS/issues).

If you'd like to contribute, please follow the steps outlined in [CONTRIBUTING.md](CONTRIBUTING.md).

## License

This project is licensed under the [MIT License](LICENSE).
