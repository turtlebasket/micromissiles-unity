# micromissiles-unity

## Build ${{ github.ref }}

## Instructions

Download and extract the archive corresponding to your platform (Windows or Mac) from the binaries listed below.

## Windows

1. Download the zip file for Windows: `micromissiles-${{ github.ref }}-windows_x86_64.zip`.
2. Unzip the zip file. The zip file should contain a single directory called `micromissiles-${{ github.ref }}-windows_x86_64`.
3. Run `micromissiles-<version>-StandaloneWindows64.exe`.

## Mac

1. Download the zip file for Darwin: `micromissiles-${{ github.ref }}-darwin.zip`.
2. Unzip the zip file. The zip file should contain a single app file.
3. Change the permission of the app file recursively by running:
   ```bash
   chmod -R +x micromissiles-<version>-StandaloneOSX.app
   ```
4. Open the app file.
5. If you get a warning that Apple cannot check the application for malicious software:
     * Open `System Preferences`.
     * Navigate to `Privacy & Security`.
     * Click on `Open Anyway` to bypass Apple's developer check.
