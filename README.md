# Azure Devops Monitor

## Overview 
.Net Core Service that creates and responds to Webhook notifications from Azure DevOps

This is an ASP.NET Core Web project configured to run as either a Windows or Systemd service.

## Prerequisites

An Azure DevOps subscription and appropriate permissons to create subscriptions.  If you wish the service to interact with Azure DevOps when a notification is received, permissions will also be required for those actions.

The following configuration values are required for operation and need to be generated and / or obtained:
- A PAT token with permission to create subscriptions
- A strong password used to encrypt a Json Web Token

These are accessed via configuration but should not be checked into source control.  Use appropriate configuration sources for each of your environments.





