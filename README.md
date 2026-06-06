# AquaFocus - 3D Serious Game for ADHD Cognitive Training

AquaFocus is a Unity-based 3D Serious Game developed as a Bachelor's Thesis project. It is specifically designed for cognitive training and attention enhancement in children diagnosed with ADHD (Attention Deficit Hyperactivity Disorder), combining evidence-based cognitive tasks with engaging gameplay mechanics.

<p align="center">
  <img src="https://github.com/user-attachments/assets/a1c07da6-6ddd-4de3-8477-bda0e93d1d79" width="48%" alt="AquaFocus Gameplay" />
  <img src="https://github.com/user-attachments/assets/aa857642-a873-4a6c-b28f-b89d9a05b87d" width="48%" alt="AquaFocus Target Inhibition" />
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
  <img src="https://github.com/user-attachments/assets/c8d61ae7-3593-4804-9f93-c744bc79d093" width="75%" alt="Parent Analytics Dashboard" />
</p>

---

## 🎨 3D Asset Pipeline & Level Design
To ensure an optimal and controlled environment for cognitive tasks, the 3D assets and track boundaries were custom-sculpted in Blender before being imported and processed by Unity's procedural logic.

<p align="center">
  <img src="https://github.com/user-attachments/assets/31e025fc-3ef1-4f81-9dab-3a161b843aa1" width="48%" alt="Blender Track Layout" />
  <img src="https://github.com/user-attachments/assets/4d8a4e25-4577-4956-ac6a-a9428bbf8c75" width="48%" alt="Blender Wall Sculpting" />
</p>

---

## 📂 Repository Structure

Since this repository serves as a code portfolio, heavy graphics, textures, and 3D environment assets have been excluded via `.gitignore` to keep the codebase clean and lightweight. 

The core C# scripts inside `Assets/Scripts/` are structured as follows:

* **`BootLoader/` & `Managers/`** — Game initialization, state management, and core gameplay loop orchestration.
* **`SupaBaseDB/` & `Telemetry/`** — Asynchronous backend communication pipeline, database models, and cloud telemetry logging.
* **`Submarine_Scripts/` & `JellyFish/`** — Player controller mechanics, physics-based movement, and dynamic/distractor AI agent behaviors.
* **`SessionScripts/`, `Map2/` & `Map3/`** — Runtime cognitive tasks execution, level logic, and procedural layout boundary tracking.
* **`ProfileSystem/` & `SaveSystem/`** — Multi-user profile management, local serialization state, and clinical data structures.
* **`Menus/`, `UI Buttons/`, `LevelSelect/`, `MapSelection/` & `Options/`** — Modular UI/UX system, quality settings adaptation, and screen flow controllers.
* **`DailyMissions/` & `Store/`** — Meta-game progression systems and reward mechanics used to maintain engagement (Gamification).
* **`Camera/`, `Sounds/`, `FPS/` & `CoralsGlow/`** — Environment behavior scripts, audio handling, execution debugging tools, and visual effects feedback.

---

## 📱 User Interface (UI/UX) & Gamification

The user interface was built to be intuitive and engaging for children, featuring dynamic profile customization and streamlined navigation.

<p align="center">
  <img src="https://github.com/user-attachments/assets/426fe541-2f0e-4a10-b837-c68c18c912d6" width="45%" alt="Profile Selection" />
</p>
