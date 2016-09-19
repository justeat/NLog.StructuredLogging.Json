param(
  [Parameter(Mandatory=$true, HelpMessage="The version number to publish, eg 1.2.3. Set this in CI first.")]
  [string] $version,
  [Parameter(Mandatory=$false, HelpMessage="CI project owner")]
  [string] $owner = "justeattech"
)

if (($version -eq $null) -or ($version -eq '')) {
  throw "Must supply version number in semver format eg 1.2.3"
}
$ci_name = "nlog-structuredlogging-json"
$ci_uri = "https://ci.appveyor.com/project/$owner/$ci_name"
$tag = "v$version"

write-host "You should be on master now." -foregroundcolor green
write-host "Your current status" -foregroundcolor green
& git status

read-host "hit enter when you're ready"
write-host "Tagging & branching. tag: $tag / branch: $release" -foregroundcolor green
& git pull upstream master --tags
& git tag -a $tag -m "Release $tag"
& git checkout $tag
write-host "Pushing" -foregroundcolor green
& git push --tags upstream
write-host "Done."
write-host "Check $ci_uri"
& git checkout master
write-host "Putting you back on master branch" -foregroundcolor green
exit 0