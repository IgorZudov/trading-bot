#tool "nuget:?package=NUnit.ConsoleRunner"
#addin Cake.Ftp

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration","Release");

//////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////

var SOLUTION_FILE = "./CryptoTrader.sln";
var BUILD_DIR = "./build";
var PUBLISH_DIR = "./publish";
var ARTIFACTS_DIR ="./artifacts";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean").Does(() => 
{
    CleanDirectory(BUILD_DIR);
    CleanDirectory(PUBLISH_DIR);
    CleanDirectory(ARTIFACTS_DIR);
    CleanDirectories("./src/**/bin");
    CleanDirectories("./src/**/obj");
    CleanDirectories("./test/**/bin");
    CleanDirectories("./test/**/obj");
});

Task("Restore-NuGet-Packages").Does(() => 
{
    DotNetCoreRestore();
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    var path = GetProjectPath("CryptoTrader.Launcher");
    DotNetCoreBuild(path,new DotNetCoreBuildSettings(){
        Configuration = configuration
    });
});

//////////////////////////////////////////////////////////////////////
// TARGET TASKS
//////////////////////////////////////////////////////////////////////

Task("Tests").IsDependentOn("Build").Does(() => 
{
    NUnit3("./test/**/bin/Debug/*.Tests.dll", new NUnit3Settings {
       NoResults = true
    });
});

Task("Publish").IsDependentOn("Tests").Does(() => 
{
    var settings = new DotNetCorePublishSettings(){
        Configuration = "Release",
        Runtime = "win10-x64",
        OutputDirectory = PUBLISH_DIR,
    };

    var path = GetProjectPath("CryptoTrader.Launcher");
    DotNetCorePublish(path,settings);

    path = GetProjectPath("CryptoTrader.HypeAnalyzer");
    DotNetCorePublish(path,settings);
    
    Zip("./publish", $"{ARTIFACTS_DIR}/publish.zip");
    var fileToUpload = File($"{ARTIFACTS_DIR}/publish.zip");
    var ftpSettings = new FtpSettings() {Username = "Administrator", Password = "*"};
    FtpUploadFile(new Uri($"ftp://*/{Guid.NewGuid()}.build.zip"), fileToUpload, ftpSettings);
});


//////////////////////////////////////////////////////////////////////
// HELPER METHODS - BUILD
//////////////////////////////////////////////////////////////////////

string GetProjectPath(string projectName)
{
    return $"./src/{projectName}/{projectName}.csproj";
}

void BuildProject(string projectName, string configuration)
{
    var path = GetProjectPath(projectName);
    var buildSettings =new MSBuildSettings { 
        Verbosity = Verbosity.Minimal, 
        Configuration = configuration, 
        PlatformTarget = PlatformTarget.MSIL 
    };
    MSBuild(path, buildSettings); 
    var bin_dir = $"./src/{projectName}/bin/{configuration}/";
    CopyDirectory(bin_dir,BUILD_DIR);
}

void BuildSolution(string configuration)
{
    var buildSettings =new MSBuildSettings { 
        Verbosity = Verbosity.Minimal, 
        Configuration = configuration, 
        PlatformTarget = PlatformTarget.MSIL 
    };
    MSBuild(SOLUTION_FILE, buildSettings); 
}
//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
Task("Default").IsDependentOn("Publish");
RunTarget(target);
