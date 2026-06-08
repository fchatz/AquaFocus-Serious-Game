# AquaFocus - 3D Serious Game for ADHD Cognitive Training

AquaFocus is a Unity-based 3D Serious Game developed as a Bachelor's Thesis project. It is specifically designed for cognitive training and attention enhancement in children diagnosed with ADHD (Attention Deficit Hyperactivity Disorder), combining evidence-based cognitive tasks with engaging gameplay mechanics.

<p align="center">
  <img src="https://github.com/user-attachments/assets/c3304055-71b9-44d4-a040-0a211241fbd6" width="48%" alt="AquaFocus Gameplay" />
  <img src="https://github.com/user-attachments/assets/d254e11f-ad60-4058-8b97-dfd5a6099f11" width="48%" alt="AquaFocus Target Inhibition" />
</p>

## 🚀 Core Features & Architecture

* **Dynamic Difficulty Adaptation (DDA):** Implements a real-time sliding performance window (`PerformanceWindow`) that dynamically evaluates player metrics to adjust game difficulty. This maintains the user's cognitive Flow State without historical bias or artificial difficulty spikes.
* **Cognitive Inhibition & Focal Transition:** Integrates active target-distractor mechanics (utilizing stimuli like Red vs. Blue targets) forcing the player into strategic cognitive inhibition and rapid focus shifting.
* **Procedural Road & Boundary Tracking:** Features a raycast-based navigation boundary mapping system for dynamic spatial tracking and real-time environment alignment.
* **Cloud Telemetry Integration:** Incorporates an asynchronous analytics pipeline that directly logs key performance indicators (KPIs) and gameplay metrics to a Supabase database backend for session review and evaluation.

## 🛠️ Technical Stack & Analytics

* **Game Engine:** Unity 2022.3 LTS (C# / .NET Standard 2.1)
* **3D Modeling & Environment:** Blender (Environment design & 3D sculpting)
* **Backend & Database:** Supabase (REST API Integration for secure asynchronous data streaming)

### 📊 Real-time Data Logging & Parent Dashboard
The game features a secure parent/clinician dashboard that fetches and displays synchronized telemetry data directly from the cloud backend, tracking cognitive performance over time:

<p align="center">
  <img src="https://github.com/user-attachments/assets/6f478ee9-6ad4-4591-991d-71410a97e12c" width="75%" alt="Parent Analytics Dashboard" />
</p>

---

## 🎨 3D Asset Pipeline & Level Design
To ensure an optimal and controlled environment for cognitive tasks, the 3D assets and track boundaries were custom-sculpted in Blender before being imported and processed by Unity's procedural logic.

<p align="center">
  <img src="https://github.com/user-attachments/assets/2eb33f81-25a4-4b67-8a45-eb8d97b8c64a" width="48%" alt="Blender Track Layout" />
  <img src="https://github.com/user-attachments/assets/ee86e40a-e433-4cb8-a457-e6a2546e389a" width="48%" alt="Blender Wall Sculpting" />
</p>

---

## 📂 Repository Structure

Since this repository serves as a code portfolio, heavy graphics, textures, and 3D environment assets have been excluded via `.gitignore` to keep the codebase clean and lightweight. 

The core C# scripts inside `Assets/Scripts/` are structured as follows:

* **BootLoader/ & Managers/** — Game initialization, state management, and core gameplay loop orchestration.
* **SupaBaseDB/ & Telemetry/** — Asynchronous backend communication pipeline, database models, and cloud telemetry logging.
* **Submarine_Scripts/ & JellyFish/** — Player controller mechanics, physics-based movement, and dynamic/distractor AI agent behaviors.
* **SessionScripts/, Map2/ & Map3/** — Runtime cognitive tasks execution, level logic, and procedural layout boundary tracking.
* **ProfileSystem/ & SaveSystem/** — Multi-user profile management, local serialization state, and user performance data models.
* **Menus/, UI Buttons/, LevelSelect/, MapSelection/ & Options/** — Modular UI/UX system, quality settings adaptation, and screen flow controllers.
* **DailyMissions/ & Store/** — Meta-game progression systems and reward mechanics used to maintain engagement (Gamification).
* **Camera/, Sounds/, FPS/ & CoralsGlow/** — Environment behavior scripts, audio handling, execution debugging tools, and visual effects feedback.

---

## 📱 User Interface (UI/UX) & Gamification

The user interface was built to be intuitive and engaging for children, featuring dynamic profile customization and streamlined navigation.

<p align="center">
  <img src="https://github.com/user-attachments/assets/fb18b702-1ee6-4d0b-bbe4-81598bbbc75f" width="75%" alt="Profile Selection" />
</p>
