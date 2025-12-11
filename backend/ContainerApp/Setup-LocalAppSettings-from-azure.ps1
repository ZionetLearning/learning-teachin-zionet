<#
.SYNOPSIS
  Automatic local setup for developers.
  - STRICT matching: Only exact matches are filled.
  - SAFE filling: Only empty/null values are updated.
  - PRECISE: Uses a lookup index to prevent wrong secret injection.
#>

### --- CONFIGURATION ---
$TenantId     = "a814ee32-f813-4a36-9686-1b9268183e27"
$KeyVaultName = "local-teachin-kv"  # <--- ENTER YOUR VAULT NAME HERE

### ---------------------------------------------------------------
### 1. ENSURE AZURE LOGIN
### ---------------------------------------------------------------
Write-Host "Checking Azure login status..." -ForegroundColor Cyan

try {
    $currentTenant = az account show --query tenantId -o tsv 2>$null
} catch {
    $currentTenant = $null
}

if (-not $currentTenant -or $currentTenant -ne $TenantId) {
    Write-Host "Logging into Azure tenant $TenantId ..." -ForegroundColor Yellow
    az login --tenant $TenantId --allow-no-subscriptions | Out-Null
    Write-Host "Azure login complete." -ForegroundColor Green
} else {
    Write-Host "Already logged in to correct tenant." -ForegroundColor Green
}

### ---------------------------------------------------------------
### 2. VERIFY KEY VAULT ACCESS
### ---------------------------------------------------------------
Write-Host "`nVerifying access to Key Vault: [$KeyVaultName]..." -ForegroundColor Cyan

$vaultCheck = az keyvault show --name $KeyVaultName --query "name" -o tsv 2>$null

if (-not $vaultCheck) {
    Write-Error "Error: Could not access Key Vault '$KeyVaultName'."
    exit 1
}

Write-Host "Access confirmed." -ForegroundColor Green

### ---------------------------------------------------------------
### 3. LOAD SECRETS & BUILD LOOKUP INDEX
### ---------------------------------------------------------------
Write-Host "`nLoading secrets..." -ForegroundColor Cyan

$secretIds = az keyvault secret list --vault-name $KeyVaultName --query "[].id" -o tsv 

if (-not $secretIds) { Write-Warning "No secrets found in this vault."; exit 0 }

# We build a 'Lookup Table' so matching is exact and impossible to mess up
$SecretLookup = @{} 

$total = $secretIds.Count
$count = 0

foreach ($id in $secretIds) {
    $count++
    $pct = [math]::Round(($count / $total) * 100)
    $secretName = ($id -split "/")[-1]
    
    Write-Progress -Activity "Downloading Secrets" -Status "Fetching $secretName ($count / $total)" -PercentComplete $pct
    
    $s = az keyvault secret show --id $id 2>$null | ConvertFrom-Json
    
    # Create the Normalized Key for the Index
    # e.g., "Tavily--ApiKey" -> "tavilyapikey"
    $normKey = ($s.name -replace "[_\- ]","").ToLower()
    $SecretLookup[$normKey] = $s.value
}
Write-Progress -Activity "Downloading Secrets" -Completed
Write-Host "Loaded $($SecretLookup.Count) secrets into index." -ForegroundColor Green

### ---------------------------------------------------------------
### 4. KEY NORMALIZER HELPER
### ---------------------------------------------------------------
function Get-NormalizedKey ($key) {
    return ($key -replace "[_\- ]","").ToLower()
}

### ---------------------------------------------------------------
### 5. UPDATE JSON OBJECT (Precise Lookup)
### ---------------------------------------------------------------
function Update-JsonObject {
    param (
        [Object]$JsonObject,
        [string]$ParentPath
    )

    # A. Handle Arrays
    if ($JsonObject -is [System.Collections.IList]) {
        for ($i = 0; $i -lt $JsonObject.Count; $i++) {
            $currentPath = if ($ParentPath) { "${ParentPath}__$i" } else { "$i" }
            if ($JsonObject[$i] -is [System.Management.Automation.PSCustomObject] -or $JsonObject[$i] -is [System.Collections.IList]) {
                Update-JsonObject -JsonObject $JsonObject[$i] -ParentPath $currentPath
            }
        }
    }
    # B. Handle Objects
    elseif ($JsonObject -is [System.Management.Automation.PSCustomObject]) {
        $props = $JsonObject.PSObject.Properties | Where-Object { $_.MemberType -eq "NoteProperty" }
        foreach ($p in $props) {
            $currentKey = $p.Name
            $currentPath = if ($ParentPath) { "$ParentPath`__$currentKey" } else { $currentKey }

            if ($p.Value -is [System.Management.Automation.PSCustomObject] -or $p.Value -is [System.Collections.IList]) {
                Update-JsonObject -JsonObject $p.Value -ParentPath $currentPath
            }
            else {
                # 1. SKIP if value is NOT empty (Safety Check)
                if (-not [string]::IsNullOrWhiteSpace($p.Value)) {
                    continue
                }

                # 2. Normalize the current JSON path
                $normalizedJsonKey = Get-NormalizedKey $currentPath
                
                # 3. EXACT LOOKUP in our Index
                if ($SecretLookup.ContainsKey($normalizedJsonKey)) {
                    $p.Value = $SecretLookup[$normalizedJsonKey]
                    Write-Host "   - Filled: $currentPath" -ForegroundColor DarkGray
                }
            }
        }
    }
}

### ---------------------------------------------------------------
### 6. STRICT JSON FORMATTER
### ---------------------------------------------------------------
function Format-JsonStrict {
    param ([string]$JsonString)

    $indentStep = "  "
    $indentLevel = 0
    $result = [System.Text.StringBuilder]::new()
    $inQuote = $false
    $isEscaped = $false

    $compressed = $JsonString -replace "\r\n", "" -replace "\n", "" -replace "\t", ""
    $chars = $compressed.ToCharArray()

    for ($i = 0; $i -lt $chars.Length; $i++) {
        $c = $chars[$i]

        if ($c -eq '"' -and -not $isEscaped) { $inQuote = -not $inQuote }
        if ($c -eq '\' -and -not $isEscaped) { $isEscaped = $true } else { $isEscaped = $false }

        if (-not $inQuote) {
            switch ($c) {
                '{' { $result.Append($c) | Out-Null; $result.AppendLine() | Out-Null; $indentLevel++; $result.Append(($indentStep * $indentLevel)) | Out-Null }
                '}' { $result.AppendLine() | Out-Null; $indentLevel--; $result.Append(($indentStep * $indentLevel)) | Out-Null; $result.Append($c) | Out-Null }
                '[' { $result.Append($c) | Out-Null; $result.AppendLine() | Out-Null; $indentLevel++; $result.Append(($indentStep * $indentLevel)) | Out-Null }
                ']' { $result.AppendLine() | Out-Null; $indentLevel--; $result.Append(($indentStep * $indentLevel)) | Out-Null; $result.Append($c) | Out-Null }
                ',' { $result.Append($c) | Out-Null; $result.AppendLine() | Out-Null; $result.Append(($indentStep * $indentLevel)) | Out-Null }
                ':' { $result.Append(": ") | Out-Null }
                ' ' {}
                "`t" {}
                default { $result.Append($c) | Out-Null }
            }
        } else {
            $result.Append($c) | Out-Null
        }
    }
    return $result.ToString()
}

### ---------------------------------------------------------------
### 7. PROCESS FILES
### ---------------------------------------------------------------
Write-Host "`nGenerating appsettings.Local.json..." -ForegroundColor Cyan

$root = $PSScriptRoot
$templates = Get-ChildItem -Path $root -Recurse -Filter "appsettings.json" |
             Where-Object { $_.FullName -notmatch "([\\/](bin|obj)[\\/])" }

if (-not $templates) { Write-Host "No appsettings.json found." -ForegroundColor Yellow; exit 0 }

foreach ($file in $templates) {
    Write-Host "`nService: $($file.Directory.Name)" -ForegroundColor Yellow
    try {
        $jsonObj = Get-Content $file.FullName -Raw | ConvertFrom-Json
        Update-JsonObject -JsonObject $jsonObj -ParentPath ""

        $raw = $jsonObj | ConvertTo-Json -Depth 100 -Compress
        $strict = Format-JsonStrict -JsonString $raw

        $outPath = Join-Path $file.DirectoryName "appsettings.Local.json"
        $strict | Out-File -FilePath $outPath -Encoding utf8

        Write-Host "Created: $outPath" -ForegroundColor Green
    }
    catch {
        Write-Error "Error: $_"
    }
}

Write-Host "`nDone! Local environment configured." -ForegroundColor Cyan