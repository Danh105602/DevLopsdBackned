param(
    [Parameter(Mandatory = $true)]
    [string]$DeviceCode,

    [string]$BaseUrl = "http://localhost:5269",

    [ValidateRange(1, 3600)]
    [int]$IntervalSeconds = 60,

    [switch]$Once
)

$apiKey = $env:SENSOR_DEVICE_KEY
if ([string]::IsNullOrWhiteSpace($apiKey)) {
    throw "Set SENSOR_DEVICE_KEY to the device key shown when the device was provisioned."
}

$endpoint = "$($BaseUrl.TrimEnd('/'))/api/iot/readings"
while ($true) {
    $payload = @{
        deviceCode = $DeviceCode
        messageId = [guid]::NewGuid().ToString("N")
        measuredAtUtc = [DateTime]::UtcNow.ToString("o")
        temperatureC = [Math]::Round((Get-Random -Minimum 180 -Maximum 360) / 10.0, 1)
        airHumidityPercent = [Math]::Round((Get-Random -Minimum 350 -Maximum 851) / 10.0, 1)
        soilMoisturePercent = [Math]::Round((Get-Random -Minimum 200 -Maximum 701) / 10.0, 1)
    } | ConvertTo-Json -Compress

    try {
        $result = Invoke-RestMethod -Method Post -Uri $endpoint `
            -Headers @{ "X-Device-Key" = $apiKey } `
            -ContentType "application/json" -Body $payload
        "Accepted at $($result.receivedAtUtc); duplicate=$($result.duplicate)"
    }
    catch {
        Write-Error "Sensor request failed: $($_.Exception.Message)"
    }

    if ($Once) { break }
    Start-Sleep -Seconds $IntervalSeconds
}
