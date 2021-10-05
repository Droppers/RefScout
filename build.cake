#addin Cake.FileHelpers&version=4.0.1
#tool nuget:?package=NuGet.CommandLine&version=5.9.1
#tool nuget:?package=xunit.runner.console&version=2.4.1

var target = Argument<string>("target", "Pack");
var configuration = Argument<string>("configuration", "Release");
var targetFramework = Argument<string>("targetFrameworkNetCore", "net6.0");


var _baseName = "RefScout";
var _publishName = "refscout";
var _solution = "./RefScout.sln";
var _appVersion = "0.1.0";
var _outputDir = Directory($"_output");
var _testOutputDir = Directory($"_testOutput");

Task("Clean")
    .Description("Cleans all directories that are used during the build process.")
    .Does(() =>
{
	CleanDirectory(_outputDir);
	CleanDirectories("./**/bin/" + configuration);
	CleanDirectories("./**/obj/");
});

Task("Restore")
    .Description("Restores all the NuGet packages that are used by the specified solution.")
    .Does(() =>
{
	Information("Restoring {0}...", _solution);
	NuGetRestore(_solution);
});


Task("Build")
    .Description("Builds all the different parts of the project.")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
	Information("Building {0} for tests", _solution);

	MSBuild($"./src/{_baseName}.IPC.FrameworkRuntime/{_baseName}.IPC.FrameworkRuntime.csproj", settings =>
		settings.SetPlatformTarget(PlatformTarget.MSIL)
			.SetMSBuildPlatform(MSBuildPlatform.x64)
			.UseToolVersion(MSBuildToolVersion.VS2019)
			.WithTarget("Build")
			.SetConfiguration(configuration));

	// MSBuild(_solution, settings =>
		// settings.SetPlatformTarget(PlatformTarget.MSIL)
			// .SetMSBuildPlatform(MSBuildPlatform.x64)
			// .UseToolVersion(MSBuildToolVersion.VS2019) // 2022 when cake updated, 2019 does not support 6.0
			// .WithTarget("Build")
			// .SetConfiguration(configuration));
});

Task("Test")
	.Does(() => 
{
     var settings = new DotNetCoreTestSettings
     {
         Configuration = "Release"
     };

     var projectFiles = GetFiles("./tests/**/*.csproj");
     foreach(var file in projectFiles)
     {
         DotNetCoreTest(file.FullPath, settings);
     }
});

// Publishes CLI variant for Linux (x64)
Task("Publish-Linux")
	.Does(() => 
{
	var settings = new DotNetCorePublishSettings
	{
		Framework = targetFramework,
		Configuration = "Release",
		Runtime = "linux-x64",
		SelfContained = false,
		PublishSingleFile = true,
		PublishReadyToRun = false,
	};
	settings.OutputDirectory = $"{_outputDir.Path}/{settings.Runtime}/";
	DotNetCorePublish($"./src/{_baseName}.Cli/{_baseName}.Cli.csproj", settings);
	MoveFile($"{settings.OutputDirectory}/{_baseName}.Cli", $"{settings.OutputDirectory}/{_publishName}");
});

// Publishes Linux-contained CLI variant for Windows (x64) 
Task("Publish-Linux-SelfContained")
	.Does(() => 
{
	var settings = new DotNetCorePublishSettings
	{
		Framework = targetFramework,
		Configuration = "Release",
		Runtime = "linux-x64",
		PublishSingleFile = true,
		PublishReadyToRun = false,
		SelfContained = true,
		PublishTrimmed = true
	};
	settings.OutputDirectory = $"{_outputDir.Path}/{settings.Runtime}-contained/";
	DotNetCorePublish($"./src/{_baseName}.Cli/{_baseName}.Cli.csproj", settings);	
	MoveFile($"{settings.OutputDirectory}/{_baseName}.Cli", $"{settings.OutputDirectory}/{_publishName}");
});

// Publishes Desktop, CLI variants for Windows (x64) 
Task("Publish-Windows")
	.Does(() => 
{
	var settings = new DotNetCorePublishSettings
	{
		Framework = targetFramework,
		Configuration = "Release",
		Runtime = "win-x64",
		SelfContained = false,
		PublishSingleFile = true,
		PublishReadyToRun = false, // TODO: investigate why this does not work
	};
	settings.OutputDirectory = $"{_outputDir.Path}/{settings.Runtime}/";
	DotNetCorePublish($"./src/{_baseName}.Cli/{_baseName}.Cli.csproj", settings);	
	MoveFile($"{settings.OutputDirectory}/{_baseName}.Cli.exe", $"{settings.OutputDirectory}/{_publishName}.exe");
	
	settings.Framework = $"{targetFramework}-windows";
	DotNetCorePublish($"./src/{_baseName}.Wpf/{_baseName}.Wpf.csproj", settings);
	MoveFile($"{settings.OutputDirectory}/{_baseName}.Wpf.exe", $"{settings.OutputDirectory}/{_publishName}-desktop.exe");
});

// Publishes self-contained CLI and Desktop variant for Windows (x64) 
Task("Publish-Windows-SelfContained")
	.Does(() => 
{
	var settings = new DotNetCorePublishSettings
	{
		Framework = targetFramework,
		Configuration = "Release",
		Runtime = "win-x64",
		IncludeNativeLibrariesForSelfExtract = true,
		PublishSingleFile = true,
		PublishReadyToRun = false,
		SelfContained = true,
		PublishTrimmed = true
	};
	settings.OutputDirectory = $"{_outputDir.Path}/{settings.Runtime}-contained/";
	DotNetCorePublish($"./src/{_baseName}.CLI/{_baseName}.CLI.csproj", settings);	
	MoveFile($"{settings.OutputDirectory}/{_baseName}.CLI.exe", $"{settings.OutputDirectory}/{_publishName}.exe");

    settings.PublishTrimmed = false;	
	settings.Framework = $"{targetFramework}-windows";
	settings.ArgumentCustomization = args => args.Append("-p:EnableCompressionInSingleFile=True");
	DotNetCorePublish($"./src/{_baseName}.Wpf/{_baseName}.Wpf.csproj", settings);	
	MoveFile($"{settings.OutputDirectory}/{_baseName}.Wpf.exe", $"{settings.OutputDirectory}/{_publishName}-desktop.exe");
});

Task("Publish")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Publish-Linux")
	.IsDependentOn("Publish-Linux-SelfContained")
	.IsDependentOn("Publish-Windows")
	.IsDependentOn("Publish-Windows-SelfContained")
	.Does(() => 
{
	foreach (var extension in new string[]{"pdb", "config", "xml"})
		DeleteFiles(_outputDir.Path + "/**/*." + extension);	
});

Task("Pack")
	.IsDependentOn("Publish")
	.Does(() => 
{
	var appFiles = GetFiles($"{_outputDir}/linux-x64/**/*");
	Zip(_outputDir.Path, _outputDir.Path + $"/linux-{_publishName}-{_appVersion}.zip", appFiles);
	
	appFiles = GetFiles($"{_outputDir}/win-x64/**/*");
	Zip(_outputDir.Path, _outputDir.Path + $"/win-{_publishName}-{_appVersion}.zip", appFiles);
	
	appFiles = GetFiles($"{_outputDir}/linux-x64-contained/{_publishName}");
	Zip(_outputDir.Path, _outputDir.Path + $"/linux-contained-{_publishName}-{_appVersion}.zip", appFiles);
	
	appFiles = GetFiles($"{_outputDir}/win-x64-contained/{_publishName}.exe");
	Zip(_outputDir.Path, _outputDir.Path + $"/win-contained-{_publishName}-{_appVersion}.zip", appFiles);
	
	appFiles = GetFiles($"{_outputDir}/win-x64-contained/{_publishName}-desktop.exe");
	Zip(_outputDir.Path, _outputDir.Path + $"/win-contained-desktop-{_publishName}-{_appVersion}.zip", appFiles);
});

RunTarget(target);