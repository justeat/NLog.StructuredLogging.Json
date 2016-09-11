# Contributing

The easier your PRs are to review and merge, the more likely your contribution will be accepted. :-)

## Develop
* Work in a feature branch in a fork, PR to our master
* One logical change per PR, please - do refactorings in separate PRs, ahead of your feature change(s)
* Have [editorconfig plugin](http://editorconfig.org) for your editor(s) installed so that your file touches are consistent with ours and the diff is reduced.
* Test coverage should not go down
* Flag breaking changes in your PR description
* Add a comment linking to passing tests in CI, proof in Kibana dashboards ("share temporary"), etc
* Link to any specifications / JIRAs that you're working against if applicable
* CI should be green!

## Releases
* CI should be green on master
* Bump the version number in CI - follow [SemVer rules](http://semver.org)
* Bump the version in appveyor.yml to match
* Update the CHANGELOG.md
* Run `release.ps1`
```shell
./release.ps1 -version 1.2.3
```
* CI should
  * build the tag
  * push nuget packages to nuget.org
