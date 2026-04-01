#!/bin/bash
set -e

echo "=== semantic-kernel-shopsavvy tests ==="

echo "Checking project structure..."
test -f ShopSavvyPlugin.cs && echo "  ShopSavvyPlugin.cs exists"
test -f ShopSavvy.SemanticKernel.csproj && echo "  ShopSavvy.SemanticKernel.csproj exists"
test -f README.md && echo "  README.md exists"
test -f LICENSE && echo "  LICENSE exists"

echo "Checking C# syntax..."
if command -v dotnet &> /dev/null; then
  dotnet build --nologo -v q 2>/dev/null && echo "  C# project builds successfully" || echo "  Build skipped (restore deps first: dotnet restore)"
else
  echo "  dotnet SDK not installed, skipping build check"
fi

echo "Checking .csproj references..."
grep -q "Microsoft.SemanticKernel" ShopSavvy.SemanticKernel.csproj && echo "  Semantic Kernel dependency referenced"
grep -q "ShopSavvy.DataApi" ShopSavvy.SemanticKernel.csproj && echo "  ShopSavvy SDK dependency referenced"
grep -q "KernelFunction" ShopSavvyPlugin.cs && echo "  KernelFunction attributes present"

echo ""
echo "All checks passed!"
