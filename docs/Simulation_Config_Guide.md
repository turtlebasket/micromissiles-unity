# Simulation Configuration Guide

This guide provides instructions on how to configure the simulation by editing the configuration files. You can customize missile and target behaviors, simulation parameters, and more to suit your research needs.

## Configuration Files

The main configuration files you will work with are:

- **Simulation Configurations**:
  - JSON File: [`1_salvo_1_hydra_7_drones.json`](Assets/StreamingAssets/Configs/1_salvo_1_hydra_7_drones.json)
  - C# Script: [`SimulationConfig.cs`](Assets/Scripts/Config/SimulationConfig.cs)

- **Model Configurations**:
  - JSON File: [`micromissile.json`](Assets/StreamingAssets/Configs/Models/micromissile.json)
  - C# Script: [`StaticConfig.cs`](Assets/Scripts/Config/StaticConfig.cs)

### File Locations

- **Simulation Configurations** are found in `Assets/StreamingAssets/Configs/`.
- **Model Configurations** are located in `Assets/StreamingAssets/Configs/Models/`.

## Simulation Configurations

### Editing Simulation Configurations

#### `1_salvo_1_hydra_7_drones.json`

This JSON file outlines the initial setup for missiles and targets.

```json:Assets/StreamingAssets/Configs/1_salvo_1_hydra_7_drones.json
{
  "timeScale": 1,
  "missile_swarm_configs": [
    {
      "num_agents": 1,
      "agent_config": {
        "missile_type": "HYDRA_70",
        // Additional configurations...
      }
    }
  ],
  "target_swarm_configs": [
    {
      "num_agents": 7,
      "agent_config": {
        "target_type": "DRONE",
        // Additional configurations...
      }
    }
  ]
}
```

- **`timeScale`**: Adjusts the speed of the simulation.
- **`missile_swarm_configs`**: Contains settings for missile swarms.
- **`target_swarm_configs`**: Contains settings for target swarms.

#### Key Configuration Parameters

- **`num_agents`**: Specifies how many agents (missiles or targets) are involved.
- **`agent_config`**: Contains settings for each agent, including:
  - **`missile_type`** / **`target_type`**: Defines the type of missile or target.
  - **`initial_state`**: Sets the starting position, rotation, and velocity.
  - **`standard_deviation`**: Adds random noise to initial states.
  - **`dynamic_config`**: Includes time-dependent settings like launch times and sensor configurations.
  - **`submunitions_config`**: Details for any submunitions used.

### Adding or Modifying Agents

1. **Add a New Swarm Configuration**:

   To introduce a new missile or target swarm, create a new entry in `missile_swarm_configs` or `target_swarm_configs`.

   ```json
   {
     "num_agents": 5,
     "agent_config": {
       "missile_type": "MICROMISSILE",
       // Additional configurations...
     }
   }
   ```

2. **Modify Existing Configurations**:

   Change parameters like `num_agents`, `initial_state`, or `dynamic_config` to adjust the behavior of existing agents.

### `SimulationConfig.cs`

This script defines the data structures used to interpret the JSON configuration files.

```csharp:Assets/Scripts/Config/SimulationConfig.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[Serializable]
public class SimulationConfig {
    [Header("Simulation Settings")]
    public float timeScale = 0.05f;

    [Header("Missile Swarm Configurations")]
    public List<SwarmConfig> missile_swarm_configs = new List<SwarmConfig>();

    [Header("Target Swarm Configurations")]
    public List<SwarmConfig> target_swarm_configs = new List<SwarmConfig>();
}

// Additional classes and enums...
```

- **Classes**:
  - `SimulationConfig`: Contains all simulation settings.
  - `SwarmConfig`: Represents a group of agents.
  - `AgentConfig`: Configuration for individual agents.

- **Enums**:
  - `MissileType`, `TargetType`, and `SensorType` define available types.

#### Editing Enums

To add new missile or target types, update the enums accordingly.

```csharp:Assets/Scripts/Config/SimulationConfig.cs
[JsonConverter(typeof(StringEnumConverter))]
public enum MissileType { HYDRA_70, MICROMISSILE, NEW_MISSILE_TYPE }
```

## Model Configurations

### Editing Model Configurations

#### `micromissile.json`

This file defines the physical and performance characteristics of the missile models.

```json:Assets/StreamingAssets/Configs/Models/micromissile.json
{
  "accelerationConfig": {
    "maxReferenceAcceleration": 300,
    "referenceSpeed": 1000
  },
  "boostConfig": {
    "boostTime": 0.3,
    "boostAcceleration": 350
  },
  // Additional configurations...
}
```

- **`accelerationConfig`**: Parameters for acceleration.
- **`boostConfig`**: Settings for the boost phase.
- **`liftDragConfig`**: Aerodynamic properties.
- **`bodyConfig`**: Physical properties like mass and area.
- **`hitConfig`**: Collision and hit detection properties.

### Modifying Parameters

Adjust values to change the missile's behavior, such as increasing acceleration or changing mass.

### Adding New Models

To define a new missile or target model:

1. **Create a New JSON File** in `Assets/StreamingAssets/Configs/Models/`.
2. **Define Model Parameters** similar to `micromissile.json`.
3. **Update the Code** to recognize and load the new model.

### `StaticConfig.cs`

This file defines classes corresponding to the model configuration JSON structure.

```csharp:Assets/Scripts/Config/StaticConfig.cs
using System;

[Serializable]
public class StaticConfig {
  // Nested classes for configuration parameters...
}
```

#### Updating Classes

If you add new parameters to the JSON model files, ensure the corresponding classes in `StaticConfig.cs` are updated.

## Using the Deployment Build

When using the deployment build:

- Ensure all required configuration files are included.
- Place simulation configuration files in the appropriate directory (`StreamingAssets/Configs/`).
- Modify the JSON files to adjust the simulation without needing to rebuild the application.

---

**Note**: Always back up configuration files before making significant changes. Incorrect configurations can lead to simulation errors.

For further assistance, refer to the comments and documentation within the code files:

- [`SimManager.cs`](Assets/Scripts/SimManager.cs): Manages simulation state and agent creation.
- [`InputManager.cs`](Assets/Scripts/Managers/InputManager.cs): Handles user input and interactions.

---

*This guide aims to help you set up and customize the simulation project effectively. If you encounter any issues or have questions, please reach out to the project maintainers.*