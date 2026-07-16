# Unity RPG Data Tools

## Overview

Unity RPG Data Tools is a collection of custom Unity Editor tools designed to simplify the creation and management of RPG data using ScriptableObjects.

The project focuses on improving content creation workflows by providing searchable asset browsers, dedicated editing windows, and a flexible data-driven architecture. Rather than relying solely on Unity's default Inspector, these tools allow designers and developers to quickly create, locate, edit, and organize game data from a centralized interface.

This project was built both as a productivity tool for future game development and as a demonstration of Unity Editor scripting, C#, object-oriented design, and scalable data architecture.

This tool was extracted from an active Unity project, so some modules may reference project-specific data types. The intended structure is modular: use the Core folder plus whichever module folders are relevant, then remove or replace project-specific references as needed.

---

## Features

* Custom Unity Editor windows for managing ScriptableObjects
* Searchable and sortable asset browser
* Dedicated editors for creating and modifying assets
* Dynamic tag system for flexible organization
* Centralized workflow for managing game data
* Inheritance-based data architecture for reusable systems
* Blank samples for user-driven module creation and customization

---

## Current Asset Types

### EntityData

Base class shared by all supported assets.

Common fields include:

* ID
* Label
* Icon
* Tag List

### ItemData

Base item class extended by:

* WeaponData
* ArmorData
* ConsumableData

### ActorData

Stores actor-specific information.

### AbilityData

Stores ability-specific information.

### CustomTag

Provides a flexible tagging system used for organization and future gameplay systems.

---

## Editor Workflow

The primary editor window provides a searchable list of available assets.

From this window you can:

* Search assets
* Sort assets
* Select an existing asset for editing
* Create new assets
* Open dedicated editing windows
* Save changes directly from the custom editor

This workflow is intended to reduce time spent navigating folders and repeatedly opening individual ScriptableObjects.

---

## Design Goals

This project emphasizes a data-driven approach.

Instead of hardcoding game content, gameplay data is stored in ScriptableObjects that can be created and modified entirely through custom editor tools.

The long-term goal is to support systems such as:

* Procedural item generation
* Dynamic loot tables
* Tag-driven gameplay systems
* Flexible RPG content pipelines
* AI-assisted content generation

---

## Technologies

* Unity
* C#
* ScriptableObjects
* Unity Editor Scripting
* Custom Editor Windows

---

## Repository Status

This repository is under active development.

The current version demonstrates the editor framework and supporting data architecture. Additional gameplay systems and procedural generation features are planned as separate expansions built on top of this foundation.

---

## Future Plans

Planned improvements include:

* Expanded actor hierarchy
* Enhanced loot generation
* Additional editor tools
* Procedural content generation
* Integration with future RPG projects
* AI-assisted content creation workflows

---

## Purpose

This project serves as both a production tool for future game development and a portfolio project demonstrating Unity Editor tooling, reusable architecture, and scalable game data management.

## Update Log
### 7-8-2026
* Refactored the Core Module to use a CoreDataRegistry asset for configuration instead of hard-coded ScriptableObject types.
* Added support for dynamically generating tabs and discovering assets based on the modules registered in the CoreDataRegistry.
* Introduced configurable module metadata, including:
  * Tab Name
  * Display Name
  * ScriptableObject Type
  * Asset Search Path
  * Optional Custom View
  * UseCustomView setting
* Implemented reflection-based support for launching custom editor windows without introducing direct dependencies between the Core Module and individual editor modules.
* Generic ScriptableObject types can now be displayed and browsed through the Core window, while modules with custom views retain their existing create and edit workflows.

### 7-12-2026
* Refactored the editor framework to support a modular data architecture.
* Moved the tag system into a dedicated `TagModule`, removing tag storage from the core `EntityData` implementation.
* Added support for optional `EntityModule`s through a dynamic module system using `SerializeReference`.
* Created a `DisplayerCore` architecture, allowing each module to provide its own custom editor UI.
* Implemented automatic discovery of available modules for editor windows.
* Added an **Add Module** workflow, allowing optional modules to be attached directly from custom editor windows.
* Added a **Remove Module** workflow with confirmation prompts to safely detach optional modules.
* Updated all existing editor windows to use the new module system.
* Converted the existing Ability functionality into an `AbilityModule` as the first optional module implementation.
* Verified module persistence across save/load cycles for both existing and newly created assets.

### 7-15-2026
* Restructured Core folder to be more organized
* Added an Example folder for empty data sets for user customization
