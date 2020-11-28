# AntLife

A 3D implementation of Langton's Ant and Conway's Game of Life in Unity and C#

Langton's Ant is a 2-Dimensional cellular automata taking place on an orthogonal (no-diagonals) square tiling. It is often used as an example of complex behaviour emerging from simple rules. It also demonstrates attractors can be hidden behind complex behaviour.

Conway's Game of Life is also a 2-Dimensional cellular automata on a square grid although in this case diagonal cells are connected. While Langton's Ant acts at a specific point in the space (which can be thought of as analogous to a read/write head on magnetic tape); Game of Life updates all of the cells of the grid at once.

AntLife implements a generalised form of Langton's Ant and runs it on a board which can be managed by Conway's Game of Life. This generalised form of Langton's Ant can implement more tile states than just on/off and the ant can react to those tiles by turning in any direction. The board can have tiles of various shapes (Triangles, Squares and Hexagons) and various connection graphs (Square tiles can be both orthogonally and diagonally connected). The Game of Life can have custom Overcrowding, Starvation and Reproduction rules. The number of ant steps per board step and the initial configuration of the board and number and positions of the ants on it can also be customised.

AntLife answers a question for which I have wondered the answer to for a long time: "What happens when you run Langton's Ant on Conway's Game of Life?" It also allows free exploration of the generalised variants of one of both of these cellular automata. Have fun exploring the possibilities. What will you find?

[Download the latest release here.](https://github.com/WillJBrown/AntLife/releases "Releases")
