<#
.SYNOPSIS
    Generates appsettings.Local.json files with STRICT 2-SPACE FORMATTING.
    Updates:
    - Now supports recursing into Arrays/Lists.
    - Saves files in UTF-8 to support special characters.
#>

$EnvFilePath = "$PSScriptRoot\.env"
$RootDir = $PSScriptRoot

# --- 1. Load .env file ---
Write-Host "Reading secrets from $EnvFilePath..." -ForegroundColor Cyan
if (-not (Test-Path $EnvFilePath)) { Write-Error "No .env file found!"; exit 1 }

$EnvVars = @{}
Get-Content $EnvFilePath | ForEach-Object {
    $line = $_.Trim()
    if ($line -notmatch "^#" -and $line -match "=") {
        $parts = $line -split "=", 2
        $EnvVars[$parts[0].Trim()] = $parts[1].Trim()
    }
}
Write-Host "Loaded $($EnvVars.Count) secrets." -ForegroundColor Green

# --- 2. Key Normalizer ---
function Get-NormalizedKey ($key) { return $key.Replace("_", "").ToLower() }

# --- 3. Recursive Updater (Fixed for Arrays) ---
function Update-JsonObject {
    param (
        [Object]$JsonObject, 
        [string]$ParentPath
    )
    
    # CASE A: Handle Arrays (Lists)
    if ($JsonObject -is [System.Collections.IList]) {
        for ($i = 0; $i -lt $JsonObject.Count; $i++) {
            # Standard .NET Config pattern for arrays is Parent__Index
            $currentPath = if ([string]::IsNullOrEmpty($ParentPath)) { "$i" } else { "${ParentPath}__$i" }
            
            # Recurse into the array item
            if ($JsonObject[$i] -is [System.Management.Automation.PSCustomObject] -or $JsonObject[$i] -is [System.Collections.IList]) {
                Update-JsonObject -JsonObject $JsonObject[$i] -ParentPath $currentPath
            }
        }
    }
    # CASE B: Handle Objects (Dictionaries)
    elseif ($JsonObject -is [System.Management.Automation.PSCustomObject]) {
        $properties = $JsonObject.PSObject.Properties | Where-Object { $_.MemberType -eq "NoteProperty" }
        foreach ($prop in $properties) {
            $currentKey = $prop.Name
            $currentPath = if ([string]::IsNullOrEmpty($ParentPath)) { $currentKey } else { "${ParentPath}__$currentKey" }

            if ($prop.Value -is [System.Management.Automation.PSCustomObject] -or $prop.Value -is [System.Collections.IList]) {
                Update-JsonObject -JsonObject $prop.Value -ParentPath $currentPath
            } else {
                # Leaf Node: Check for match
                $jsonNormalized = Get-NormalizedKey $currentPath
                $match = $EnvVars.Keys | Where-Object { (Get-NormalizedKey $_) -eq $jsonNormalized } | Select-Object -First 1
                
                if ($match) { 
                    $prop.Value = $EnvVars[$match]
                    Write-Host "    Patching: $currentPath" -ForegroundColor DarkGray
                }
            }
        }
    }
}

# --- 4. The Strict Formatter (Rebuilds JSON from scratch) ---
function Format-JsonStrict {
    param ([string]$JsonString)
    
    $indentStep = "  " # <--- 2-space indent
    $indentLevel = 0
    $result = [System.Text.StringBuilder]::new()
    $inQuote = $false
    $isEscaped = $false
    
    # Compress input first
    $compressed = $JsonString -replace "\r\n", "" -replace "\n", "" -replace "\t", ""
    
    $chars = $compressed.ToCharArray()
    for ($i = 0; $i -lt $chars.Length; $i++) {
        $c = $chars[$i]
        
        # Handle Quotes and Escaping
        if ($c -eq '"' -and -not $isEscaped) { $inQuote = -not $inQuote }
        if ($c -eq '\' -and -not $isEscaped) { $isEscaped = $true } else { $isEscaped = $false }
        
        if (-not $inQuote) {
            if ($c -eq '{' -or $c -eq '[') {
                [void]$result.Append($c)
                [void]$result.AppendLine()
                $indentLevel++
                [void]$result.Append(($indentStep * $indentLevel))
            }
            elseif ($c -eq '}' -or $c -eq ']') {
                [void]$result.AppendLine()
                $indentLevel--
                [void]$result.Append(($indentStep * $indentLevel))
                [void]$result.Append($c)
            }
            elseif ($c -eq ',') {
                [void]$result.Append($c)
                [void]$result.AppendLine()
                [void]$result.Append(($indentStep * $indentLevel))
            }
            elseif ($c -eq ':') {
                [void]$result.Append(": ")
            }
            elseif ($c -eq ' ' -or $c -eq "`t") {
                # Skip whitespace outside quotes
            }
            else {
                [void]$result.Append($c)
            }
        } else {
            [void]$result.Append($c)
        }
    }
    return $result.ToString()
}

# --- 5. Main Loop ---
$templateFiles = Get-ChildItem -Path $RootDir -Recurse -Filter "appsettings.json" | 
                  Where-Object { $_.FullName -notmatch "[\\/]bin[\\/]" -and $_.FullName -notmatch "[\\/]obj[\\/]" }

foreach ($file in $templateFiles) {
    Write-Host "`nProcessing: $($file.Directory.Name)" -ForegroundColor Yellow
    try {
        $jsonObj = Get-Content $file.FullName -Raw | ConvertFrom-Json
        
        # Run recursive update
        Update-JsonObject -JsonObject $jsonObj -ParentPath ""
        
        # Convert to raw string first, then format strictly
        $rawJson = $jsonObj | ConvertTo-Json -Depth 100 -Compress
        $finalJson = Format-JsonStrict -JsonString $rawJson
        
        $outputPath = Join-Path $file.DirectoryName "appsettings.Local.json"
        
        # FIX: Force UTF8 encoding to handle special characters
        $finalJson | Set-Content $outputPath -Encoding UTF8
        
        Write-Host "  Success! Created Clean File" -ForegroundColor Green
    } catch {
        Write-Error "  Error: $_"
    }
}
Write-Host "`nDone." -ForegroundColor Cyan