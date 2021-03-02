# Validate Workflow Status (Github Action)

![CI](https://github.com/xembly/validate-workflow-status/workflows/ci/badge.svg)
![CD](https://github.com/xembly/validate-workflow-status/workflows/cd/badge.svg)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=xembly_validate-workflow-status&metric=coverage)](https://sonarcloud.io/dashboard?id=xembly_validate-workflow-status)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=xembly_validate-workflow-status&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=xembly_validate-workflow-status)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=xembly_validate-workflow-status&metric=security_rating)](https://sonarcloud.io/dashboard?id=xembly_validate-workflow-status)


A Github Action that validates another workflow's status. For instance, validating build/test workflow before a workflow to create a release. See [change log](https://github.com/xembly/validate-workflow-status/blob/master/CHANGELOG.md) for all the release notes.

## Usage

### Prerequisites
- Create a workflow `.yml` file in your repositories `.github/workflows` directory. An [example workflow](#example-workflow---upload-a-release-asset) is available below. For more information, reference the GitHub Help Documentation for [Creating a workflow file](https://help.github.com/en/articles/configuring-a-workflow#creating-a-workflow-file).
- Another workflow already in use to reference for the workflow containing the Validate Workflow Status step.

### Inputs
- `token`: Your Github API token. Recommended to use `${{ secrets.GITHUB_TOKEN }}`
- `workflow`: The name of the workflow to validate against. **Must be in the same project**.
- `branch`: The branch name to check and validate the status against. Defaults to `main`.
- `status`: The string value of the workflow status to match against. Defaults to `success`.

### Outputs
- *none*

### Example workflow - create a release
On every `push` to a tag matching the pattern `v*`, [create a release](https://developer.github.com/v3/repos/releases/#create-a-release) by validating another build is successful. This Workflow example assumes you'll use the [`@actions/create-release`](https://www.github.com/actions/create-release) action to create the release step:

```yaml
on:
  push:
    # Sequence of patterns matched against refs/tags
    tags:
    - 'v*' # Push events to matching v*, i.e. v1.0, v20.15.10

name: Create Release

jobs:
  build:
    name: Create Release
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Validate Release
        uses: xembly/validate-workflow-status@v1
        with:
          token: ${{ secrets.GITHUB_TOKEN }}  # This token is provided by Github Actions, you do not need to create your own token
          workflow: build
          branch: main     #optional
          status: success  # optional
      # @actions/create-release step...
```

## License
Licensed under the terms of the [BSD-3-Clause License](https://opensource.org/licenses/BSD-3-Clause)
