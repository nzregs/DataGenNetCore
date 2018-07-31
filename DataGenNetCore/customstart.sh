#!/bin/bash
set -e

echo "{\"EventHubConnectionString\":\"$EVENTHUBCONNECTIONSTRING\",\"EventHubPath\":\"$EVENTHUB\"}" > appsettings.json

if [[ -z "$VERBOSE" ]]; then
    dotnet DataGenNetCore.dll sendmessages --message-type telco $VELOCITY
else
    dotnet DataGenNetCore.dll sendmessages --message-type telco --verbose $VELOCITY 
fi