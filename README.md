# Autonomous Vehicle Navigation with Reinforcement Learning 🚗🧠

[![License](https://img.shields.io/github/license/Gabrielebandino/AutonomousVehicles_RL)](LICENSE)
[![Contributors](https://img.shields.io/github/contributors/Gabrielebandino/AutonomousVehicles_RL)](https://github.com/Gabrielebandino/AutonomousVehicles_RL/graphs/contributors)
[![Issues](https://img.shields.io/github/issues/Gabrielebandino/AutonomousVehicles_RL)](https://github.com/Gabrielebandino/AutonomousVehicles_RL/issues)

A Unity + ML-Agents simulator that trains a self-driving agent with Reinforcement Learning (RL) to follow procedurally generated tracks, manage speed, and avoid leaving the road. The focus is on core driving skills (steering + speed control) in a controllable environment.

---

## 🎥 Demo

[![Watch the demo](https://img.youtube.com/vi/j_lbxTSV7i8/0.jpg)](https://youtu.be/j_lbxTSV7i8)


---

## 🚀 Features

- **RL-based Driving**: PPO-style training with configurable hyperparameters.
- **Dynamic Track Generation**: New road each episode via a simple procedural rule set.
- **Checkpoint System**: Guides navigation and enables progress tracking.
- **Reward Shaping**: Distance-to-goal, speed incentive, completion/penalty events.
- **Sensor Suite (RPS)**: Multiple Ray Perception Sensors for front/360° awareness.
- **Input Pruning**: Iterative observation reduction to speed up training.

---

## 🛠️ Technologies

- **Engine**: Unity
- **Scripting**: C#
- **RL Toolkit**: Unity ML-Agents (Torch backend)
- **Monitoring**: TensorBoard

---

## 🧩 How It Works (Quick Overview)

**Environment**
- Track is built from rectangular prefabs (width 10, depth 20) placed step-by-step.
- Each new segment is forward / diag-left / diag-right with simple “no immediate U-turn” rules.
- A checkpoint is spawned at the center of each segment; three upcoming checkpoints are tagged.

**Agent**
- A car prefab with wheel colliders and a custom controller (torque, brake torque, steer angle, top speed).
- The controller exposes `Move()` and `ManageSpeed()` to apply agent actions smoothly.

**Observations**
- Velocity (initially), multiple **RPS** (Ray Perception Sensor 3D) at different ranges/angles:
  - 360° sensor(s) near the car
  - Front sensors at medium/long ranges
  - Dedicated RPS to detect next 3 checkpoints
- Checkpoint angles (cosine via dot product) to the next 3 checkpoints.
- Later optimization removes velocity and some sensors; adds a small **stack** on 360° RPS to encode motion.

**Rewards (examples)**
- Progress toward goal (based on decreasing remaining distance along track axis).
- Speed shaping: positive near target speed, negative when too slow.
- +x on completion, −x if the car falls off the road.
- Time limit per episode to push faster completion.

---

## 📊 Results (Summary)

- With the full sensor suite + checkpoint angles, the agent **completes ~99%** of tracks.
- Removing key sensors drops performance (e.g., ~70% without velocity + near 360° sensor, ~50% without far front sensor).
- Small RPS stacking helps, but excessive stacking provides diminishing returns.

---

## 🧪 Getting Started

### 1) Prerequisites
- **Unity** (2021 LTS or newer recommended)
- **Python** 3.8–3.11
- **ML-Agents**:
  ```bash
  pip install mlagents mlagents-envs
