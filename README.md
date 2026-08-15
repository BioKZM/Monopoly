[ [Türkçe](README_TR.md) ] | [ EN ]

<table>
  <tr>
    <td width="50%" align="center">
      <img src="docs/screenshots/Lobby.png" alt="Lobby Screen"/><br/>
      <b>Lobby</b>
    </td>
    <td width="50%" align="center">
      <img src="docs/screenshots/dice roll.png" alt="In Game Movement"/><br/>
      <b>Board & Movement</b>
    </td>
  </tr>
  <tr>
    <td width="50%" align="center">
      <img src="docs/screenshots/tile with one building.png" alt="Purchase Screen"/><br/>
      <b>Owned Tile</b>
    </td>
    <td width="50%" align="center">
      <img src="docs/screenshots/tiles.png" alt="Property Cards"/><br/>
      <b>Tile Details on Right Panel (Interactable)</b>
    </td>
  </tr>
</table>


# 🎲 Monopoly Multiplayer (Unity & Mirror Networking)

![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black?style=for-the-badge&logo=unity)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Mirror](https://img.shields.io/badge/Networking-Mirror-blue?style=for-the-badge)
![Architecture](https://img.shields.io/badge/Architecture-Authoritative%20Server-red?style=for-the-badge)

A multiplayer board game project recreating classic Monopoly game mechanics using **Unity Engine** and **Mirror Networking**, built on an **Authoritative Server** architecture.

---

## 📌 Key Features

* **Authoritative Server Architecture:** To prevent cheating and state desynchronization, all core game logic—including dice rolls, property purchases, and turn order—is strictly validated and executed on the server.
* **Networked Turn & Timer System:** A dynamic 30-second turn timer for players. Includes server-forced automated dice rolls and turn-passing when a player times out or stays idle.
* **Dynamic Property & Economy System:** Real-time player balance tracking, property purchasing, rent deduction, and bankruptcy (`Bankrupt`) state management.
* **Race Condition & State Guarding:** Mitigated double-turn skipping and memory leak exceptions (`NullReferenceException`) caused by network latency during state transitions.

---

## 🛠️ Technical Architecture & Tech Stack

* **Game Engine:** Unity
* **Language:** C#
* **Networking Framework:** Mirror Networking
* **Network Topology:** Host-Client (Listen Server)

### Component Architecture

| Component | Responsibility |
| :--- | :--- |
| **`GameManager`** | Controls the main state machine of the game. Handles turn order, turn timers, pause states, and victory conditions. |
| **`PlayerScript`** | The player data model attached to `NetworkIdentity`. Synchronizes balances, owned properties, and turn state locks across the network. |
| **`TurnManager`** | Handles turn lock mechanisms and synchronizes turn indicators across all connected clients. |

---

## 🚀 Key Technical Challenges & Solutions

Core networking challenges encountered and resolved during development:

### 1. Double Turn Skip in the Update Loop
* **Problem:** When the turn timer expired, `Update()` loops running across frames executed faster than incoming network packets, causing turns to skip twice in rapid succession.
* **Solution:** Applied state locking instantly upon timer expiration and channeled execution through a Single Source of Truth server method (`ServerExecutePass`), completely eliminating the race condition.

### 2. Graceful Shutdown on Disconnection (Null Guarding)
* **Problem:** When the host stopped or clients disconnected, frame-rate dependent `Update()` calls executed while network objects were being destroyed, throwing `NullReferenceException` errors.
* **Solution:** Implemented defensive code patterns (null checks) across state validations to ensure clean teardowns and maintain a zero-error console on disconnect.

---

## 🎮 Installation & Running the Project

To test and build the project locally:

1. Clone the repository:
   ```bash
   git clone https://github.com/BioKZM/Monopoly.git
   ```
2. Open the project via Unity Hub (ensure you checkout the multiplayer branch).

3. Open the Main Scene located at Assets/Monopoly/Scenes/.

4. To test multiplayer locally, use the ParrelSync package or create a secondary build via File > Build and Run.
