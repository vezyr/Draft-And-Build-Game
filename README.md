**Disclaimer:** This project was created as a prototype for a programming portfolio only.
It was never ment to be a complete, production ready game. 

# Game Design Document: Settlement Draft (Prototype)

**Genre:** Minimalist City Builder / Spatial Puzzle
**Platform:** PC (Desktop/Web)
**Controls:** Mouse and Keyboard (with othter controllers as options in the future)
**Project Goal:** Demo for a programming portfolio (max 1-2 weeks "after howurs" work).

---

https://github.com/user-attachments/assets/6bca1dd4-84c5-41d8-be02-6759b2aabfbf

---

## 1. Core Gameplay Loop

The player expands a small settlement through a "drafting" mechanic. The game heavily relies on spatial relationships and optimizing layout on a constrained grid.

*   **The Grid:** Default a 6x6 grid (36 tiles). Starts empty.
*   **End Condition:** The game lasts exactly **36 turns** (when the grid is completely filled).
*   **Objective:** Maximize the final Score by the end of the game.
*   **Turn Breakdown:**
    1. The system draws 3 random building "cards" from the available pool (card deck - they can be defined, with total random option in the future).
    2. The player drafts (selects) exactly 1 card; the other 2 stays on hand.
    3. The player places the drafted building on an empty, available (which means - it met building's condition) tile.
    4. The score for this placement is calculated based on orthogonal neighbors (Up, Down, Left, Right) and added to the global pool.

---

## 2. Building Types & Adjacency Rules

Buildings only interact orthogonally (no diagonals).

| Building Type | Base Pts | Adjacency Rules (Modifiers)                                                                                                     | Logical Challenge / Constraint                                                                |
|:--------------|:---------|:--------------------------------------------------------------------------------------------------------------------------------|:----------------------------------------------------------------------------------------------|
| **Town Hall** | 0        | No innate rules.                                                                                                                | Auto-generated at the start. Serves as an anchor for other buildings. (**Currentl not used)** |
| **House**     | +1       | **+2** for each adjacent Well or Market.<br>**-3** for each adjacent Mine.                                                      | A "filler" building, highly dependent on support structures.                                  |
| **Well**      | 0        | **+2** for each adjacent House and Farm.<br>**-5** for each adjacent Mine (water pollution).                                    | No base points. Forces the creation of residential/farming clusters.                          |
| **Farm**      | +2       | **+1** for each other adjacent Farm.<br>**+3** if adjacent to the Town Hall.                                                    | Rewards creating large, contiguous agricultural zones.                                        |
| **Market**    | 0        | **+4** for each adjacent House.<br>**Rule:** Cannot be placed adjacent to another Market.                                       | Requires placement validation (tile locking) before building.                                 |
| **Mine**      | +8       | **-3** for each adjacent House.<br>-5 for each adjacent Well.<br>**Rule:** Can *only* be placed on the outer edges of the grid. | Constraint based on global grid coordinates (e.g., X=0 or X=5).                               |

### Key Mechanic: Directional On-Placement Scoring (Asymmetry)

The game utilizes an asymmetric scoring system where points are calculated **only at the exact moment a building is placed**.
* **No Retroactive Scoring:** When a new building is placed, the game checks its rules and applies points based on its neighbors. Existing neighbors already on the board **are not** recalculated.
* **Game Design Purpose:** The asymmetry of the rules forces players to plan their placement order. (e.g., Placing 4 Houses around an empty tile and then dropping a Market in the center).

---

## 3. Visuals & UI (Minimalist Approach)

*   **3D Graphics:** Simple 3D Objects available in the Unity Editor, with different colors.
*   **User Interface:** A minimalistic design with default elements from UI Toolkit (like Labels and default Buttons).
