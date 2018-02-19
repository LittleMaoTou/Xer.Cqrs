#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&prerelease

Environment.SetVariableNames();

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./Src",
                            title: "Xer.Cqrs",
                            solutionFilePath: "./Xer.Cqrs.sln",
                            repositoryOwner: "XerProjects",
                            repositoryName: "Xer.Cqrs",
                            appVeyorAccountName: "mvput",
                            testFilePattern: "/**/*Tests.csproj",
                            testDirectoryPath: "./Tests",
                            shouldRunDupFinder: false,
                            shouldRunDotNetCorePack: true);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context,
                            testCoverageFilter: "+[*]* -[xunit.*]* -[Cake.Core]* -[Cake.Testing]* -[*.Tests]* ",
                            testCoverageExcludeByAttribute: "*.ExcludeFromCodeCoverage*",
                            testCoverageExcludeByFile: "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs");
Build.RunDotNetCore();