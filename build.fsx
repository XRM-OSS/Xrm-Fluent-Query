// include Fake libs
#I @"packages\FAKE\tools\"
#r @"packages\FAKE\tools\FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open System.IO
open Fake.Paket
open Fake.OpenCoverHelper
open Fake.ReportGeneratorHelper
open Fake.FileHelper
open Fake.Testing.NUnit3

//Project config
let projectName = "Xrm.Oss.FluentQuery"
let projectDescription = "A Dynamics CRM / Dynamics365 library for fluent queries"
let authors = ["Florian Kroenert"]

// Directories
let buildDir  = @".\build\"
let libbuildDir = buildDir + @"lib\"

let testDir   = @".\test\"

let nUnitPath = "packages" @@ "nunit.consolerunner" @@ "tools" @@ "nunit3-console.exe"
let deployDir = @".\Publish\"
let libdeployDir = deployDir + @"lib\"
let nugetDir = @".\nuget\"
let packagesDir = @".\packages\"

// version info
let mutable majorversion    = "2"
let mutable minorversion    = "0"
let mutable build           = "0"
let mutable nugetVersion    = ""
let mutable asmVersion      = ""
let mutable asmInfoVersion  = ""

let sha                     = "" //Git.Information.getCurrentHash() 

// Targets
Target "Clean" (fun _ ->

    CleanDirs [buildDir; testDir; deployDir; nugetDir]
)

Target "BuildVersions" (fun _ ->
    asmVersion      <- majorversion + "." + minorversion + "." + build
    asmInfoVersion  <- asmVersion + " - " + sha

    let nugetBuildNumber = if not isLocalBuild then build else "0"
    
    nugetVersion    <- majorversion + "." + minorversion + "." + nugetBuildNumber

    SetBuildNumber nugetVersion   // Publish version to TeamCity
)

Target "AssemblyInfo" (fun _ ->
    BulkReplaceAssemblyInfoVersions "src" (fun f -> 
                                              {f with
                                                  AssemblyVersion = asmVersion
                                                  AssemblyInformationalVersion = asmInfoVersion
                                                  AssemblyFileVersion = asmVersion})
)

Target "BuildLib" (fun _ ->
    !! @"src\lib\**\*.csproj"
        |> MSBuildRelease libbuildDir "Build"
        |> Log "Build-Output: "
)

Target "BuildTest" (fun _ ->
    !! @"src\test\**\*.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "Build Log: "
)

Target "NUnit" (fun _ ->
    let testFiles = !!(testDir @@ @"\**\*.Tests.dll")
    
    if testFiles.Includes.Length <> 0 then
      testFiles
        |> NUnit3 (fun test ->
             {test with
                   ShadowCopy = false;
                   ToolPath = nUnitPath;})
)

Target "CodeCoverage" (fun _ ->
    OpenCover (fun p -> { p with 
                                TestRunnerExePath = nUnitPath
                                ExePath ="packages" @@ "OpenCover" @@ "tools" @@ "OpenCover.Console.exe"
                                Register = RegisterType.RegisterUser
                                WorkingDir = (testDir)
                                Filter = "+[Xrm.Oss*]* -[*.Tests*]*"
                                Output = "../coverage.xml"
                        }) "Xrm.Oss.FluentQuery.Tests.dll"
)

Target "ReportCodeCoverage" (fun _ ->
    ReportGenerator (fun p -> { p with 
                                    ExePath = "packages" @@ "ReportGenerator" @@ "tools" @@ "ReportGenerator.exe"
                                    WorkingDir = (testDir)
                                    TargetDir = "../reports"
                                    ReportTypes = [ReportGeneratorReportType.Html; ReportGeneratorReportType.Badges ]
                               }) [ "..\coverage.xml" ]
    
)

Target "Publish" (fun _ ->
    CreateDir libdeployDir

    !! (libbuildDir @@ @"*.*")
            |> CopyTo libdeployDir
)

Target "CreateNuget" (fun _ ->
    Pack (fun p ->
            {p with
                Version = nugetVersion
            })
)

// Dependencies
"Clean"
  ==> "BuildVersions"
  =?> ("AssemblyInfo", not isLocalBuild )
  ==> "BuildLib"
  ==> "BuildTest"
  ==> "NUnit"
  ==> "CodeCoverage"
  ==> "ReportCodeCoverage"
  ==> "Publish"
  ==> "CreateNuget"

// start build
RunTargetOrDefault "Publish"
