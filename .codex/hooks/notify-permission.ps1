$ErrorActionPreference = "Stop"
[Console]::InputEncoding = [System.Text.Encoding]::UTF8

function New-Text {
    param(
        [Parameter(Mandatory = $true)][int[]]$CodePoints
    )

    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

function Show-Notification {
    param(
        [Parameter(Mandatory = $true)][string]$Title,
        [Parameter(Mandatory = $true)][string]$Body
    )

    Add-Type -AssemblyName PresentationFramework | Out-Null
    [System.Windows.MessageBox]::Show($Body, $Title) | Out-Null
}

function Should-SuppressToast {
    param(
        [Parameter(Mandatory = $true)][string]$Title,
        [Parameter(Mandatory = $true)][string]$Body
    )

    $statePath = Join-Path $PSScriptRoot "notify-permission-state.json"
    $now = Get-Date
    $fingerprint = "$Title`n$Body"

    if (Test-Path $statePath) {
        try {
            $state = Get-Content $statePath -Raw | ConvertFrom-Json
            $lastFingerprint = [string]$state.fingerprint
            $lastSentAt = [datetime]$state.sentAt

            if ($lastFingerprint -eq $fingerprint -and (($now - $lastSentAt).TotalSeconds -lt 10)) {
                return $true
            }
        } catch {
        }
    }

    $newState = @{
        fingerprint = $fingerprint
        sentAt = $now.ToString("o")
    } | ConvertTo-Json -Compress

    Set-Content -LiteralPath $statePath -Value $newState -Encoding UTF8
    return $false
}

try {
    $inputJson = [Console]::In.ReadToEnd()
    if ([string]::IsNullOrWhiteSpace($inputJson)) {
        exit 0
    }

    $data = $inputJson | ConvertFrom-Json
    $toolName = [string]$data.tool_name
    $description = [string]$data.tool_input.description

    if ([string]::IsNullOrWhiteSpace($description)) {
        $description = New-Text -CodePoints @(67,111,100,101,120,44032,32,52628,44032,32,49849,51064,32,50836,52397,51012,51012,45796,46)
    }

    if (-not [string]::IsNullOrWhiteSpace($toolName)) {
        $toolPrefix = New-Text -CodePoints @(46020,44396,58,32)
        $description = "$toolPrefix$toolName`n$description"
    }

    $title = New-Text -CodePoints @(67,111,100,101,120,32,49849,51064,32,54596,50836)

    if (-not (Should-SuppressToast -Title $title -Body $description)) {
        Show-Notification -Title $title -Body $description
    }

    exit 0
} catch {
    exit 0
}
