#!/bin/sh -l

cd /app
dotnet ValidateWorkflow.dll \
    $GITHUB_REPOSITORY \
    $ACTION_TOKEN \
    $ACTION_WORKFLOW \
    $ACTION_BRANCH \
    $ACTION_STATUS
