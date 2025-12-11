<#
.SYNOPSIS
  Automatic local setup for developers.
  TARGETS A SPECIFIC KEY VAULT ONLY.
  - Logs into Azure
  - Verifies access to the specific Key Vault
  - Loads all secrets (With Progress Bar)
  - Generates appsettings.Local.json with strict 2-space formatting
#>

### --- CONFIGURATION ---
$TenantId     = "a814ee32-f813-4a36-9686-1b9268183e27"
$KeyVaultName = "teachin-local-dev"  # <--- ENTER YOUR VAULT NAME HERE

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

# Check if the specific vault exists and is accessible
$vaultCheck = az keyvault show --name $KeyVaultName --query "name" -o tsv 2>$null

if (-not $vaultCheck) {
    Write-Error "Error: Could not access Key Vault '$KeyVaultName'."
    Write-Error "Possible causes:"
    Write-Error "1. The name is incorrect."
    Write-Error "2. You do not have 'Key Vault Secrets User' permissions."
    Write-Error "3. You are logged into the wrong subscription."
    exit 1
}

Write-Host "Access confirmed." -ForegroundColor Green

### ---------------------------------------------------------------
### 3. LOAD ALL SECRETS (WITH PROGRESS BAR)
### ---------------------------------------------------------------
Write-Host "`nLoading secrets..." -ForegroundColor Cyan

$secretIds = az keyvault secret list --vault-name $KeyVaultName --query "[].id" -o tsv 

if (-not $secretIds) { Write-Warning "No secrets found in this vault."; exit 0 }

$AzureSecrets = @{}
$total = $secretIds.Count
$count = 0

foreach ($id in $secretIds) {
    $count++
    $pct = [math]::Round(($count / $total) * 100)
    $secretName = ($id -split "/")[-1]
    
    Write-Progress -Activity "Downloading Secrets" -Status "Fetching $secretName ($count / $total)" -PercentComplete $pct
    
    # Fetch secret value safely
    $s = az keyvault secret show --id $id 2>$null | ConvertFrom-Json
    $AzureSecrets[$s.name] = $s.value
}
Write-Progress -Activity "Downloading Secrets" -Completed
Write-Host "Loaded $($AzureSecrets.Count) secrets." -ForegroundColor Green

### ---------------------------------------------------------------
### 4. KEY NORMALIZER
### ---------------------------------------------------------------
function Get-NormalizedKey ($key) {
    return ($key -replace "[_\- ]","").ToLower()
}

### ---------------------------------------------------------------
### 5. UPDATE JSON OBJECT (Recursive & Array Aware)
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
                # Leaf check
                $normalizedJsonKey = Get-NormalizedKey $currentPath
                $match = $AzureSecrets.Keys | Where-Object { Get-NormalizedKey $_ -eq $normalizedJsonKey } | Select-Object -First 1

                if ($match) {
                    $p.Value = $AzureSecrets[$match]
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