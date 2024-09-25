import os
import glob
import argparse
import pandas as pd
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import numpy as np

def find_latest_telemetry_file(directory='./Logs/'):
    list_of_files = glob.glob(os.path.join(directory, 'sim_telemetry_*.csv'))
    if not list_of_files:
        print(f"No telemetry files found in {directory}")
        return None
    latest_file = max(list_of_files, key=os.path.getctime)
    print(f"Using latest telemetry file: {latest_file}")
    return latest_file

def plot_telemetry(file_path):
    # Read the telemetry CSV file
    df = pd.read_csv(file_path)


    # Create a 3D plot
    fig = plt.figure(figsize=(12, 8))
    ax = fig.add_subplot(111, projection='3d')

    # Define colors for different agent types
    colors = {'T': 'red', 'M': 'blue'}

    # Group data by AgentID
    for agent_id, agent_data in df.groupby('AgentID'):
        agent_type = agent_data['AgentType'].iloc[0]
        color = colors.get(agent_type, 'black')
        downsampled = agent_data.iloc[::10]

        ax.plot(
            downsampled['AgentX'],
            downsampled['AgentZ'],
            downsampled['AgentY'],
            color=color,
            alpha=0.5,
            linewidth=0.5,
            label=f"{agent_type}"
        )


    ax.set_xlabel('X (m)')
    ax.set_ylabel('Z (m)')
    ax.set_zlabel('Y (m)')
    

    ax.view_init(elev=20, azim=45)

    # Add a ground plane
    x_min, x_max = ax.get_xlim()
    z_min, z_max = ax.get_ylim()
    xx, zz = np.meshgrid(np.linspace(x_min, x_max, 2), np.linspace(z_min, z_max, 2))
    yy = np.zeros_like(xx)
    ax.plot_surface(xx, zz, yy, alpha=0.2, color='green')

    plt.title('Agents Trajectories (X: Right, Z: Forward, Y: Up)')
    legend = [
        plt.Line2D([0], [0], color='red', lw=2, label='Threat'),
        plt.Line2D([0], [0], color='blue', lw=2, label='Interceptor')
    ]
    plt.legend(handles=legend)
    plt.tight_layout()
    plt.show()
    
if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Visualize telemetry data.')
    parser.add_argument('file', nargs='?', default=None, help='Path to telemetry CSV file.')
    args = parser.parse_args()

    if args.file:
        file_path = args.file
    else:
        file_path = find_latest_telemetry_file()
        if file_path is None:
            exit(1)

    plot_telemetry(file_path)