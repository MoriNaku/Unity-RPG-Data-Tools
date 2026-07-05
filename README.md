# Unity RPG Data Tools

## Overview

Unity RPG Data Tools is a collection of custom Unity Editor tools designed to simplify the creation and management of RPG data using ScriptableObjects.

The project focuses on improving content creation workflows by providing searchable asset browsers, dedicated editing windows, and a flexible data-driven architecture. Rather than relying solely on Unity's default Inspector, these tools allow designers and developers to quickly create, locate, edit, and organize game data from a centralized interface.

This project was built both as a productivity tool for future game development and as a demonstration of Unity Editor scripting, C#, object-oriented design, and scalable data architecture.

---

## Features

* Custom Unity Editor windows for managing ScriptableObjects
* Searchable and sortable asset browser
* Dedicated editors for creating and modifying assets
* Dynamic tag system for flexible organization
* Centralized workflow for managing game data
* Inheritance-based data architecture for reusable systems

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

### MonsterData

Stores monster-specific information, including abilities and loot.

*(Planned to evolve into a more general ActorData base class for players, NPCs, monsters, and other actors.)*

### SkillData

Stores skill definitions used by actors.

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
