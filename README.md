# Christmas Lights Controller

## Overview

This project is a custom-built Christmas light show controller that synchronizes lights with music. I started this project because I wanted to build my own system rather than buy a pre-made controller.

The system uses a Unity interface to create light patterns, an Arduino to control the light outputs, and a Raspberry Pi to coordinate the light patterns with music playback.

Since I self-taught myself to code, I didn't about version control till after the project was complete so everything here was uploaded after completion. :)

## How It Works

The system has three main components:

- Unity: Provides an interface for working with light timing and patterns.
- Raspberry Pi: Runs a Python program that plays music and sends the corresponding light patterns over a serial connection.
- Arduino: Receives the patterns and controls 16 light strands using shift registers and digital output signals.

The Raspberry Pi sends a 16-character string representing the state of each light strand. The Arduino reads each pattern and updates the outputs accordingly.

## Hardware and Software

### Hardware

- Arduino Uno microcontroller
- Raspberry Pi
- 74HC595 shift registers
- 2, 8-channel relay boards
- Custom-built outlet box
- Christmas light strands

### Software

- C++
- Python
- Unity
- Serial communication
- VLC media player library for Python

## Repository Structure

- `Assets/` — Unity project assets and scripts
- `Packages/` — Unity package configuration
- `ProjectSettings/` — Unity project settings
- `Arduino/` — Arduino controller code
- `Raspberry Pi/` — Python music and lighting controller

## Goals

My goal was to understand and build the system myself, from software and timing logic to the electronics that control the lights, while keeping the project affordable.

## Engineering Challenges

One of the main challenges was working within the limitations of the microcontroller while coordinating the lights with music playback. The project required connecting the software that creates the patterns with the hardware that physically switches the lights.

I developed the system incrementally, testing the control hardware and software and adjusting the design to work within the available resources.
