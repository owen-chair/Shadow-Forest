# Shadow Forest: A Multiplayer Stealth Game for Unity / VRChat

Shadow Forest is a multiplayer stealth game designed for VRChat, where players navigate a procedurally generated forest, evade skeletons, and reach the temple to join or kill the furry temple guardian.

### 🌟 Key Features
- **Randomly Generated Levels**: 80x80 grid with a randomly generated path through a dense forest. 80x80 is maximum due to Udon performance. 
- **Optimized for Performance**: Highly tuned for Quest1 and UdonSharp transpiler output. Tested at 72fps on Quest1,2,3 and PC. Typical tri count is ~50k, max ~100k (per eye)

---
### 🔧 How it works
- [LevelGenerator.cs](https://github.com/owen-chair/Shadow-Forest/blob/master/Assets/Scenes/VRCDefaultWorldScene_UdonProgramSources/LevelGenerator.cs) uses the tile objects below to create a level from a grid
- Levels are generated with a custom algorithm to generate a twisty path with loops
- [EnemyAI_Animated.cs](https://github.com/owen-chair/Shadow-Forest/blob/master/Assets/Scenes/VRCDefaultWorldScene_UdonProgramSources/EnemyAI_Animated.cs) paths are around loops, found using "Find Islands" algorithms 
- <img src="https://github.com/user-attachments/assets/b3473cc1-4909-4ee8-940b-1914d8973472" width="300" />
- The levels look something like this
- <img src="https://github.com/user-attachments/assets/25cc2da8-5289-45fb-a066-792698560392" width="300" />
- And then players are spawned inside
- <image src="https://github.com/user-attachments/assets/fd844f58-1dc5-4723-b233-fd0d8be09516" width="300" />



### 🎥 Videos
<a href="https://www.youtube.com/watch?v=I6rl0ijOzUQ">
  <img src="https://img.youtube.com/vi/I6rl0ijOzUQ/0.jpg" width="200"/>
</a>
<a href="https://www.youtube.com/watch?v=sZexLC5x-WE">
  <img src="https://img.youtube.com/vi/sZexLC5x-WE/0.jpg" width="200"/>
</a>

### 🔧 How to modify
#### Modify the Unity Project
1. Clone this repository.  
2. Import it with **VRChat Creator Companion** using Unity 2022.3.22f1.  
3. Open the project and drag the VRCDefaultWorldScene into the hierachy and replace the default

#### Modify the Code 
1. Edit the `.cs` files located in [this directory](https://github.com/owen-chair/Shadow-Forest/tree/master/Assets/Scenes/VRCDefaultWorldScene_UdonProgramSources).  
2. Save your changes and recompile via the UdonSharp menu in Unity  

---

### 🛠 Tools Used  
 
- ![Unity](https://img.shields.io/badge/Unity-000000?style=flat&logo=unity&logoColor=white) [Unity](https://unity.com/)  
- <img src="https://images.squarespace-cdn.com/content/v1/5f0770791aaf57311515b23d/64ea7bc8-02c5-4c1e-97cf-5c3aa79300f6/VRC_Logo.png?format=1500w" alt="VRChat Creator Companion" width="50"> [VRChat Creator Companion](https://vrchat.com/home/download)
- <img src="https://www.blender.org/wp-content/uploads/2020/07/blender_logo-1280x391.png" alt="Blender Logo" width="100"> [Blender](https://www.blender.org/)
- ![3D Coat Textura](https://img.shields.io/badge/3D_Coat_Textura-00BFB3?style=flat) [3D Coat Textura](https://3dcoat.com/)
- <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/Adobe_Fireworks_CS6_Icon.png/240px-Adobe_Fireworks_CS6_Icon.png" alt="FWlogo" width="25"> [Adobe Fireworks CS5](https://adobe.com/)
- <img src="https://www.audacityteam.org/_astro/Audacity_Logo.FMlith9s.svg" alt="Audacity Logo" width="25"> [Audacity](https://www.audacityteam.org/)

