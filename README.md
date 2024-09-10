# CLAY-studio-Hanoi
Hanoi tower prototype made in Unity

![](/Readme-images/screenshot.jpg)

# Quick notes:

- The project is located in the [Hanoi Unity Project](Hanoi%20Unity%20Project) directory. 
- The project is designed to run on a 16:9 screen
- There is a windows build in the [Hanoi Unity Project / Build](Hanoi%20Unity%20Project/Build) directory
- There is a gameplay video capture in the [Video capture](Video%20capture) directory
- The project has been uploaded without the *Library* folder to keep the repository lightweight

# Approach

My general approach was to work with only one scene, as the complexity of the prototype didn't warrant a menu or any other additional scenes. All 3D objects are basic 3D primitives. I wrote 3 different scripts:

- [*DiscBehaviour.cs*](Hanoi%20Unity%20Project/Assets/Scripts/DiscBehaviour.cs) is added to each disc, it controls the mouse clicks, the color changes and the movement animation 
- [*RodBehaviour.cs*](Hanoi%20Unity%20Project/Assets/Scripts/RodBehaviour.cs) is added to each rod, it controls the mouse clicks plus color fades
- [*GameManager.cs*](Hanoi%20Unity%20Project/Assets/Scripts/GameManager.cs)is the main script. It uses a state machine to control the flow of the game. This includes determining when a disk or rod may be clicked, evaluating the validity of each move the player attempts, executing each move and evaluating whether the game has been won.  



![](/Readme-images/hanoiflow.png)
State chart for Game Manager

### Additional features

- I added a reset button that reloads the scene. This is the only element not controlled by the game manager script. It only reloads the scene if a move has been successfully made. 
- I added a counter that displays the number of moves done. Only valid moves are counted (attempting to move a disc to its current rod does not add a move to the counter)
- The rods have a color fade effect when the mouse hovers over them. This is controlled by the rod behaviour script. 
- The discs have a color highlight effect when selected. This is controlled by the disc behaviour script. 
- The discs are automatically colored based on their size. There is a "top material" for the smallest disc and a "Bottom material" for the largest. The others are interpolated automatically.
- I added simple sound effects to the different actions. I think they really improve the overall experience and add fluidity to the gameplay. 
- I added some simple animations to the disc movements. The movement is interpolated with some easing, and a full rotation on the X axis is also performed. The duration of the movement can be controlled from the game manager.
- There are some simple instructions shown at the beginning of the scene. They disappear when the player makes the first move.

### A note on the animations:

I'm not very convinced by them. They seem a little crude and they make the gameplay a little slower and more cumbersome. For this reason, the duration of the animation can simply be set to 0, which will skip the interpolation (also, the "swoosh" sound effect won't be played).

### Challenges and future improvements

- Some challenges arose as I added more features. For example, I initially decided against creating an AudioManager class because I thought it would add too much complexity. As I added more sounds, I realized that a separate Audio Manager would have made things more neat and better organized. The same happened as I added the color fades and changes for the discs and rods. Perhaps a MaterialManager class would have been a better way to handle this.

- It was also a challenge get the UI elements to scale properly. The unity UI system is something that I need to gain more experience with. 

- I believe the animations are quite crude at this stage. Since the discs interpolate along a straight path, there is clipping with other objects. 

![](/Readme-images/animation-01.png)
Current animation trajectory
![](/Readme-images/animation-02.png)
![](/Readme-images/animation-03.png)

- I would be curious to see if a movement path like these two examples would be better. I imagine the movement would be a lot nicer, but it would also make the gameplay slower and more tedious. 

- I believe the code could be optimized more (for example, I'm not sure if using GameObject.getComponent() so much is performant or if there are better approaches). I also the code more elaborate at points in an effort to improve legibility, which means some of the code could be more concise. I would love to learn more best practices for Unity and C#, in order to write better code. 

- I also believe that a clear next step would be to add the ability to change the number of discs in order to increase or decrease the game difficulty. It would also be nice to add some auto-solving animations that show different solving algorithms. 

- Currently the sounds are of mixed origins. I would create custom sound clips and make them all normalized.

- Lastly, I would make 3D meshes for the objects. Currently the discs are simply short cylinders, when they could be in fact discs with a hole in the middle. I would also work more on materials and textures, as well as lighting. A custom cursor for the mouse would be nice as well. 








