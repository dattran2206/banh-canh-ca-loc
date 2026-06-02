---
description: Safe Dependency Installation
---

# /safe-install — Safe Dependency Installation

> **MANDATORY:** Use for running `npm install` gracefully.

---
Description: Safe Install Workflow for Malware Blocking Packages

---
**Purpose:** When a user requests a package installation or update, the Supervisor must force this workflow to scan for Supply Chain vulnerabilities before downloading.

//turbo-all

1. Scan for module vulnerabilities first (Security Audit Phase):

`npm audit`

2. If the above step does not report any serious errors/warnings, proceed with the safe installation to block hidden scripts (Installation Phase):

`npm ci --ignore-scripts`