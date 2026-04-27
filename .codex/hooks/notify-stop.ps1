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

    $statePath = Join-Path $PSScriptRoot "notify-stop-state.json"
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

function Is-InputNeeded {
    param(
        [string]$Message
    )

    if ([string]::IsNullOrWhiteSpace($Message)) {
        return $false
    }

    if ($Message -match '\?$') {
        return $true
    }

    if ($Message -match 'confirm|input|proceed|select|approve') {
        return $true
    }

    return $false
}

try {
    $inputJson = [Console]::In.ReadToEnd()
    if ([string]::IsNullOrWhiteSpace($inputJson)) {
        Write-Output '{"continue": true}'
        exit 0
    }

    $data = $inputJson | ConvertFrom-Json
    $message = [string]$data.last_assistant_message

    if ((Is-InputNeeded -Message $message)) {
        $title = New-Text -CodePoints @(67,111,100,101,120,32,51077,47141,32,54596,50836)
        $body = if ([string]::IsNullOrWhiteSpace($message)) {
            New-Text -CodePoints @(67,111,100,101,120,44032,32,49324,50857,51088,32,51025,45813,51012,32,44592,47532,47549,45768,45796,46)
        } else {
            $message
        }
    } else {
        $title = New-Text -CodePoints @(67,111,100,101,120,32,51089,50629,32,50756,47308)
        $body = New-Text -CodePoints @(54788,51116,32,53580,51060,32,51333,47308,46104,50632,49845,45768,45796,46)
    }

    if (-not (Should-SuppressToast -Title $title -Body $body)) {
        Show-Notification -Title $title -Body $body
    }

    Write-Output '{"continue": true}'
    exit 0
} catch {
    Write-Output '{"continue": true}'
    exit 0
}
