- [Overview](#overview)
- [Current Maintainers](#current-maintainers)
- [Maintainer Responsibilities](#maintainer-responsibilities)
  - [Uphold Code of Conduct](#uphold-code-of-conduct)
  - [Prioritize Security](#prioritize-security)
  - [Review Pull Requests](#review-pull-requests)
  - [Triage Open Issues](#triage-open-issues)
  - [Backports](#backports)
  - [Be Responsive](#be-responsive)
  - [Maintain Overall Health of the Repo](#maintain-overall-health-of-the-repo)
  - [Manage Roadmap](#manage-roadmap)
  - [Add Continuous Integration Checks](#add-continuous-integration-checks)
  - [Use Semver](#use-semver)
  - [Release Frequently](#release-frequently)
  - [Promote Other Maintainers](#promote-other-maintainers)
  - [Describe the Repo](#describe-the-repo)
- [Becoming a Maintainer](#becoming-a-maintainer)
  - [Nomination](#nomination)
  - [Interest](#interest)
  - [Addition](#addition)
- [Removing a Maintainer](#removing-a-maintainer)
  - [Moving On](#moving-on)
  - [Inactivity](#inactivity)
  - [Negative Impact on the Project](#negative-impact-on-the-project)
## Overview

This document explains who the maintainers are (see below), what they do in this repo, and how they should be doing it. If you're interested in contributing, see [CONTRIBUTING](CONTRIBUTING.md).

## Current Maintainers

| Maintainer               | GitHub ID                                                          | Affiliation |
| ------------------------ | ------------------------------------------------------------------ | ----------- |
| Anirudha Jadhav          | [anirudha](https://github.com/anirudha)                            |   Amazon    |
| Daniel Doubrovkine       | [dblock](https://github.com/dblock)                                |   Amazon    |
| Joshua Li                | [joshuali925](https://github.com/joshuali925)                      |   Amazon    |
| Thomas Farr              | [Xtansia](https://github.com/Xtansia)                              |   Amazon    |
| Vacha Shah               | [VachaShah](https://github.com/VachaShah)                          |   Amazon    |
| Max Ksyunz               | [MaxKsyunz](https://github.com/MaxKsyunz)                          |  Bit Quill  |
| Yury Fridlyand           | [Yury-Fridlyand](https://github.com/Yury-Fridlyand)                |  Bit Quill  |

## Maintainer Responsibilities

Maintainers are active and visible members of the community, and have [maintain-level permissions on a repository](https://docs.github.com/en/organizations/managing-access-to-your-organizations-repositories/repository-permission-levels-for-an-organization). Use those privileges to serve the community and evolve code as follows.

### Uphold Code of Conduct

Model the behavior set forward by the [Code of Conduct](CODE_OF_CONDUCT.md) and raise any violations to other maintainers and admins.

### Prioritize Security

Security is your number one priority. Maintainer's Github keys must be password protected securely and any reported security vulnerabilities are addressed before features or bugs.

Note that this repository is monitored and supported 24/7 by Amazon Security, see [Reporting a Vulnerability](SECURITY.md) for details.

### Review Pull Requests

Review pull requests regularly, comment, suggest, reject, merge and close. Accept only high quality pull-requests. Provide code reviews and guidance on incoming pull requests. Don't let PRs be stale and do your best to be helpful to contributors.

### Triage Open Issues

Manage labels, review issues regularly, and triage by labelling them. 

All repositories in this organization have a standard set of labels, including `bug`, `documentation`, `duplicate`, `enhancement`, `good first issue`, `help wanted`, `blocker`, `invalid`, `question`, `wontfix`, and `untriaged`, along with release labels, such as `v1.0.0`, `v1.1.0`, `v2.0.0`, `patch`, and `backport`.

Use labels to target an issue or a PR for a given release, add `help wanted` to good issues for new community members, and `blocker` for issues that scare you or need immediate attention. Request for more information from a submitter if an issue is not clear. Create new labels as needed by the project.

### Backports

The Github workflow in [backport.yml](.github/workflows/backport.yml) creates backport PRs automatically when the original PR with an appropriate label `backport <backport-branch-name>` is merged to main. To backport a PR to `1.x`, add a label `backport 1.x` to the PR, once this PR is merged to main, the workflow will create a backport PR to the `1.x` branch.

### Be Responsive

Respond to enhancement requests, and forum posts. Allocate time to reviewing and commenting on issues and conversations as they come in. 

### Maintain Overall Health of the Repo

Keep the `main` branch at production quality at all times. Backport features as needed. Cut release branches and tags to enable future patches. 

### Manage Roadmap

Ensure the repo highlights features that should be elevated to the project roadmap. Be clear about the feature’s status, priority, target version, and whether or not it should be elevated to the roadmap. Any feature that you want highlighted on the OpenSearch Roadmap should be tagged with "roadmap". The OpenSearch [project-meta maintainers](https://github.com/opensearch-project/project-meta/blob/main/MAINTAINERS.md) will highlight features tagged "roadmap" on the project wide [OpenSearch Roadmap](https://github.com/orgs/opensearch-project/projects/1).

### Add Continuous Integration Checks

Add integration checks that validate pull requests and pushes to ease the burden on Pull Request reviewers.

### Use Semver

Use and enforce [semantic versioning](https://semver.org/) and do not let breaking changes be made outside of major releases.

### Release Frequently

Make frequent project releases to the community.

### Promote Other Maintainers

Assist, add, and remove [MAINTAINERS](MAINTAINERS.md). Exercise good judgement, and propose high quality contributors to become co-maintainers. See [Becoming a Maintainer](#becoming-a-maintainer) for more information.

### Describe the Repo

Make sure the repo has a well-written, accurate, and complete description. See [opensearch-project/.github#38](https://github.com/opensearch-project/.github/issues/38) for some helpful tips to describe your repo.

## Becoming a Maintainer

You can become a maintainer by actively [contributing](CONTRIBUTING.md) to any project, and being nominated by an existing maintainer.

### Nomination

Any current maintainer starts a private e-mail thread (until we have a better mechanism, e-mail addresses can usually be found via MAINTAINERS.md + DCO) with all other maintainers on that repository to discuss nomination using the template below. In order to be approved, at least three positive (+1) maintainer votes are necessary, and no vetoes (-1). In rare cases when there are fewer than three maintainers, the positive (+1) votes from all maintainers are required. Any disagreements can be escalated to the repo admin.

The nomination should clearly identify the person with their real name and a link to their GitHub profile, and the rationale for the nomination, with concrete example contributions.

### Interest

Upon receiving at least three positive (+1) maintainer votes, and no vetoes (-1), from existing maintainers after a one week period, the nominating maintainer asks the nominee whether they might be interested in becoming a maintainer on the repository via private e-mail message.

> This is great work! Based on your valuable contribution and ongoing engagement with the project, the current maintainers invite you to become a co-maintainer for this project. Please respond and let us know if you accept the invitation to become maintainer.

Individuals accept the nomination by replying, or commenting, for example _"Thank you! I would love to."_

### Addition

Upon receiving three positive (+1) maintainer votes, and no vetoes (-1), from other maintainers, and after having privately confirmed interest with the nominee, the maintainer opens a pull request adding the proposed co-maintainer to MAINTAINERS.md. The pull request is approved and merged.

> _Content from the above nomination._
> 
> The maintainers have voted and agreed to this nomination.

The repo admin adjusts the new maintainer’s permissions accordingly, and merges the pull request.

## Removing a Maintainer

Removing a maintainer is a disruptive action that the community of maintainers should not undertake lightly. There are several reasons a maintainer will be removed from the project, such as violating the [Code of Conduct](CODE_OF_CONDUCT.md), or taking other actions that negatively impact the project.

### Moving On

There are plenty of reasons that might cause someone to want to take a step back or even a hiatus from a project. Existing maintainers can choose to leave the project at any time, with or without reason, by making a pull request to move themselves to the "Emeritus" section of MAINTAINERS.md, and asking an admin to remove their permissions.

### Inactivity

Maintainer status never expires. If a maintainer becomes inactive for a time (usually several months), the repo admin may revoke maintainer level access to the repository for security reasons. A maintainer can reach out to the repo admin to get their permissions reinstated.

If the repo is left without any maintainers, either by maintainer inactivity or moving on, the repo is considered unmaintained. The repo admin will seek out new maintainers and note the maintenance status in the repo README file.

### Negative Impact on the Project

Actions that negatively impact the project will be handled by the admins, in coordination with other maintainers, in balance with the urgency of the issue. Examples would be [Code of Conduct](CODE_OF_CONDUCT.md) violations, deliberate harmful or malicious actions, and security risks.