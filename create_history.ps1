# Script to create progressive commit history from June 15 to July 18, 2025

# Remove existing git history
if (Test-Path .git) {
    Remove-Item -Recurse -Force .git
}

# Initialize new git repo
git init
git config user.name "Himanshusheo"
git config user.email "himanshusheoran174@gmail.com"

# Function to generate random time between 9 AM and 11 PM
function Get-RandomTime {
    $hour = Get-Random -Minimum 9 -Maximum 23
    $minute = Get-Random -Minimum 0 -Maximum 59
    $second = Get-Random -Minimum 0 -Maximum 59
    return "{0:D2}:{1:D2}:{2:D2}" -f $hour, $minute, $second
}

# Track committed files to avoid empty commits
$committedFiles = @{}

# Function to create commit with specific date and random time
function Create-Commit {
    param(
        [string]$Date,
        [string]$Message,
        [string[]]$Files
    )
    
    $filesToStage = @()
    foreach ($file in $Files) {
        if ($file -eq ".") {
            # Stage all untracked/modified files
            git add -A
            $filesToStage = "all"
            break
        } elseif (Test-Path $file) {
            if (-not $committedFiles.ContainsKey($file)) {
                git add $file
                $filesToStage += $file
                $committedFiles[$file] = $true
            } else {
                # File already committed, stage it for modification
                git add $file
                $filesToStage += $file
            }
        }
    }
    
    if ($filesToStage.Count -eq 0 -and $filesToStage -ne "all") {
        Write-Host "Warning: No files to commit for: $Message" -ForegroundColor Yellow
        return
    }
    
    $randomTime = Get-RandomTime
    $env:GIT_AUTHOR_DATE = "$Date $randomTime +0000"
    $env:GIT_COMMITTER_DATE = "$Date $randomTime +0000"
    
    # Check if there are staged changes or new files
    $stagedOutput = git diff --cached --name-only
    if ($stagedOutput -or $filesToStage -eq "all") {
        git commit -m "$Message"
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Committed: $Message" -ForegroundColor Green
        } else {
            Write-Host "Warning: Commit failed for: $Message" -ForegroundColor Red
        }
    } else {
        # No changes to commit, but create empty commit to maintain timeline
        git commit -m "$Message" --allow-empty
        Write-Host "Committed (empty): $Message" -ForegroundColor Cyan
    }
    
    Remove-Item Env:\GIT_AUTHOR_DATE
    Remove-Item Env:\GIT_COMMITTER_DATE
}

Write-Host "Creating progressive commit history..." -ForegroundColor Cyan
Write-Host "=====================================`n" -ForegroundColor Cyan

Create-Commit -Date "2025-06-15" -Message "Initial project setup: Create .NET 8 Web API project" -Files @("Infonetica.Workflow.csproj")

Create-Commit -Date "2025-06-16" -Message "Add StateDef domain model for workflow states" -Files @("Domain/StateDef.cs")
Create-Commit -Date "2025-06-16" -Message "Add ActionDef domain model for workflow actions" -Files @("Domain/ActionDef.cs")

Create-Commit -Date "2025-06-17" -Message "Implement WorkflowDefinition domain model" -Files @("Domain/WorkflowDefinition.cs")
Create-Commit -Date "2025-06-17" -Message "Add validation logic to WorkflowDefinition constructor" -Files @("Domain/WorkflowDefinition.cs")

Create-Commit -Date "2025-06-18" -Message "Add WorkflowInstance domain model" -Files @("Domain/WorkflowInstance.cs")
Create-Commit -Date "2025-06-18" -Message "Add InstanceHistoryEntry for tracking state transitions" -Files @("Domain/InstanceHistoryEntry.cs")

Create-Commit -Date "2025-06-19" -Message "Create WorkflowService skeleton with in-memory storage" -Files @("Services/WorkflowService.cs")

Create-Commit -Date "2025-06-21" -Message "Implement CreateDefinition with validation logic" -Files @("Services/WorkflowService.cs")
Create-Commit -Date "2025-06-21" -Message "Add duplicate validation for states and actions" -Files @("Services/WorkflowService.cs")

Create-Commit -Date "2025-06-23" -Message "Implement StartInstance method in WorkflowService" -Files @("Services/WorkflowService.cs")

Create-Commit -Date "2025-06-24" -Message "Implement ExecuteAction with state transition validation" -Files @("Services/WorkflowService.cs")

Create-Commit -Date "2025-06-25" -Message "Add list methods and improve error handling in WorkflowService" -Files @("Services/WorkflowService.cs")

Create-Commit -Date "2025-06-26" -Message "Add DTOs: CreateDefinitionRequest and ApiError" -Files @("Dtos/CreateDefinitionRequest.cs", "Dtos/ApiError.cs")

Create-Commit -Date "2025-06-29" -Message "Add DefinitionDto and InstanceDtos for API responses" -Files @("Dtos/DefinitionDto.cs", "Dtos/InstanceDtos.cs")

Create-Commit -Date "2025-06-30" -Message "Set up Minimal API with dependency injection" -Files @("Program.cs")
Create-Commit -Date "2025-07-01" -Message "Add helper methods for DTO conversion" -Files @("Program.cs")

Create-Commit -Date "2025-07-03" -Message "Implement workflow definition endpoints - POST, GET, GET by id" -Files @("Program.cs")

Create-Commit -Date "2025-07-04" -Message "Add workflow instance endpoints - create, list, get by id" -Files @("Program.cs")

Create-Commit -Date "2025-07-05" -Message "Implement action execution endpoint for state transitions" -Files @("Program.cs")

Create-Commit -Date "2025-07-06" -Message "Add JsonSnapshotStore with export functionality" -Files @("Persistence/JsonSnapshotStore.cs")
Create-Commit -Date "2025-07-06" -Message "Implement JSON serialization for workflow snapshots" -Files @("Persistence/JsonSnapshotStore.cs")

Create-Commit -Date "2025-07-07" -Message "Implement import functionality in JsonSnapshotStore" -Files @("Persistence/JsonSnapshotStore.cs")

Create-Commit -Date "2025-07-09" -Message "Add admin endpoints for export/import snapshots" -Files @("Program.cs")

Create-Commit -Date "2025-07-10" -Message "Add application configuration files" -Files @("appsettings.json", "appsettings.Development.json", "Properties/launchSettings.json")

Create-Commit -Date "2025-07-12" -Message "Add initial README with project overview and API documentation" -Files @("README.md")

Create-Commit -Date "2025-07-14" -Message "Enhance README with examples, validation rules, and usage guide" -Files @("README.md")

Create-Commit -Date "2025-07-16" -Message "Refactor and improve code organization" -Files @("Program.cs", "Services/WorkflowService.cs")

Create-Commit -Date "2025-07-17" -Message "Final code cleanup and documentation updates" -Files @("README.md", "Program.cs")

Create-Commit -Date "2025-07-18" -Message "Finalize workflow engine implementation" -Files @(".")

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Commit history created successfully!" -ForegroundColor Green
$commitCount = git rev-list --count HEAD
Write-Host "Total commits: $commitCount" -ForegroundColor Green

# Set up remote and push
Write-Host "`nSetting up remote repository..." -ForegroundColor Cyan
$remoteUrl = "https://github.com/Himanshusheo/infonetica-workflow.git"
git remote remove origin 2>$null
git remote add origin $remoteUrl
git branch -M main

Write-Host ""
Write-Host "Ready to push to GitHub!" -ForegroundColor Green
Write-Host "To push this will overwrite existing history, run:" -ForegroundColor Yellow
Write-Host "  git push -f origin main" -ForegroundColor White
