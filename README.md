# CLAY-studio-Hanoi
Hanoi tower project made in Unity

# Quick notes:

- The project is located in the [Hanoi Unity Project](Hanoi%20Unity%20Project) directory. 
- The project is designed to run on a 16:9 screen
- There is a windows build in the [Hanoi Unity Project / Build](Hanoi%20Unity%20Project/Build) directory
- There is a gameplay video capture in the [Video Capture](Video%20Capture) directory

# Approach

My general approach was to work with 3 different scripts:

- DiscBehaviour.cs is added to each disc, it controls the mouse clicks, the color changes and the movement animation 
- RodBehaviour.cs is added to each rod, it controls the mouse clicks plus color fades
- GameManager.cs is the main script. It uses a state machine to control the flow of the game.  

![](/Readme-images/hanoiflow.png)

