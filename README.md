# Description
The game is Pong: a two player game in which you control paddles (moving them up and down) to prevent a ball from reaching
the right or left side of the screen. One player controls the left paddle with W/S (for moving up and down respectively),
and the second player controls the right paddle with the Up and Down arrow keys.

## Differences between FancyPong and normal Pong
- This game is prettier (it even has particles)
- The paddles are arched, so you can control which way the ball faces when it bounces off of your paddle
- Power Boxes spawn randomly which have different effects (right now they're all speed boosts though)

# TODO List
- DONE: when the ball hits something, there should be concentric circles which spawn from the collision point
- DONE: the game should scale well with resolution changes
- DONE: particles should shoot out from behind the ball as it moves around
- DONE: there should be a dashed line going down the middle of the screen
- DONE: there should be a system for spawning boxes randomly that the ball can hit and "trigger"
- DONE: a "speed box" should make the ball speed up for a certain time then slow down
- DONE: the ball should glow while the speed effect is active
- the ball should somehow show how long is left for the speed effect
- an "ice box" should freeze both paddles for a certain amount of time
- there should be some sort of snowflake in the background or something to show that the ice box is active
- the indicator should also show how long is left for the ice effect
- hitting a "speed box" while the speed effect is already active should  reset the timer for the speed box
- hitting an "ice box" while the ice effect is already active should reset the timer for the ice box
