#!/usr/bin/env pwsh
# Dev helper (issue #21): sign the test account in via Supabase's password grant
# and print a magic-link-style URL for the local preview. Reuses the app's
# existing URL-fragment session pickup — there is no auth bypass in the app.
#
# Requires a git-ignored dev-credentials.local.json (copy dev-credentials.example.json).
# Uses only the publishable key already shipped in the app — no service_role,
# no secrets in this script or the repo. See docs/testing-authenticated-pages.md.
[CmdletBinding()]
param(
    [string]$CredsPath,
    [string]$BaseUrl = "http://localhost:5080"
)

$ErrorActionPreference = 'Stop'

# Resolve the credentials file. Canonical location is OUT of the repo (so it's
# shared across sessions on this machine and can never be committed). Order:
#   1. -CredsPath / $env:DWTDG_DEV_CREDENTIALS  (explicit override)
#   2. ~/.dwtdg/dev-credentials.json            (canonical, out of repo)
#   3. <repo>/dev-credentials.local.json        (git-ignored fallback)
if (-not $CredsPath) {
    $candidates = @(
        $env:DWTDG_DEV_CREDENTIALS,
        (Join-Path $HOME '.dwtdg/dev-credentials.json'),
        (Join-Path $PSScriptRoot '../dev-credentials.local.json')
    ) | Where-Object { $_ }
    $CredsPath = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}
if (-not $CredsPath -or -not (Test-Path $CredsPath)) {
    throw "No dev credentials found. Create ~/.dwtdg/dev-credentials.json from " +
          "dev-credentials.example.json (see docs/testing-authenticated-pages.md), " +
          "or set `$env:DWTDG_DEV_CREDENTIALS to its path."
}
$creds = Get-Content $CredsPath -Raw | ConvertFrom-Json
if ([string]::IsNullOrWhiteSpace($creds.email) -or $creds.email -like 'REPLACE_*') {
    throw "Fill in 'email' and 'password' in $CredsPath first."
}

# Pull the (public) URL + publishable key straight from the app config so they
# live in exactly one place.
$configPath = "$PSScriptRoot/../src/DoWellToDoGood/Services/SupabaseConfig.cs"
$config = Get-Content $configPath -Raw
$supabaseUrl = [regex]::Match($config, 'Url\s*=\s*"([^"]+)"').Groups[1].Value
$apiKey = [regex]::Match($config, 'PublishableKey\s*=\s*"([^"]+)"').Groups[1].Value

$body = @{ email = $creds.email; password = $creds.password } | ConvertTo-Json
try {
    $resp = Invoke-RestMethod -Method Post `
        -Uri "$supabaseUrl/auth/v1/token?grant_type=password" `
        -Headers @{ apikey = $apiKey; 'Content-Type' = 'application/json' } `
        -Body $body
}
catch {
    $msg = $_.ErrorDetails.Message
    throw "Sign-in failed. Supabase said: $msg`n" +
          "Check the email/password, that the user is confirmed, and that the " +
          "Email provider (password) is enabled in Supabase Auth settings."
}

$fragment = "access_token=$($resp.access_token)" +
            "&refresh_token=$($resp.refresh_token)" +
            "&expires_in=$($resp.expires_in)&token_type=bearer&type=magiclink"
$loginUrl = "$BaseUrl/#$fragment"

Write-Host "Signed in as $($creds.email)." -ForegroundColor Green
Write-Host ""
Write-Host "Open this URL in the preview to establish the session:"
Write-Host $loginUrl
Write-Host ""
Write-Host "Then unlock the vault with your 'vaultPassphrase'." -ForegroundColor DarkGray
