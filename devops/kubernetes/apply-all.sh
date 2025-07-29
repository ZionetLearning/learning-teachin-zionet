#!/bin/bash

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Change to the Kubernetes directory (where this script is located)
cd "$SCRIPT_DIR" || exit 1

# Ordered list of directories to apply
folders=("namespaces" "config" "deployments" "services")

for folder in "${folders[@]}"; do
  echo "Applying YAMLs in: $folder"
  
  # Check if folder exists before proceeding
  if [ ! -d "$folder" ]; then
    echo "Warning: Directory '$folder' does not exist, skipping..."
    continue
  fi
  
  find "$folder" -type f \( -name "*.yaml" -o -name "*.yml" \) | while read -r file; do
    echo "Applying: $file"
    if ! kubectl apply -f "$file"; then
      echo "Error: Failed to apply $file" >&2
    fi
  done
done

echo "Kubernetes deployment completed!"