var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");

Task("Clean")
    .Does(() =>
{
    // Clean directories.
    CleanDirectories("./src/**/bin/" + configuration);
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    MSBuild("./src/DataContextScope.sln", s => 
        s.SetConfiguration(configuration));
});

RunTarget(target);