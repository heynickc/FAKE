﻿[<AutoOpen>]
/// Contains a task to run the msbuild runner of [Sonar Qube analyzer](http://sonarqube.org).
module Fake.SonarQubeHelper

/// The supported commands of Sonar Qube. It is called with Begin before compilation, and End after compilation.
type SonarQubeCall = Begin | End

/// Parameter type to configure the sonar qube runner.
type SonarQubeParams =
    { /// FileName of the sonar qube runner exe. 
      ToolsPath : string
      /// Key to identify the sonar qube project
      Key : string
      /// Name of the project
      Name : string
      /// Version number of the project
      Version : string
    }

/// SonarQube default parameters - tries to locate MSBuild.SonarQube.exe in any subfolder.
let SonarQubeDefaults = 
    { ToolsPath = findToolInSubPath "MSBuild.SonarQube.Runner.exe" (currentDirectory @@ "tools" @@ "SonarQube")
      Key = null
      Name = null
      Version = "1.0" }

/// Execute the external msbuild runner of Sonar Qube. Parameters are fiven to the command line tool as required.
let SonarQubeCall (call: SonarQubeCall) (parameters : SonarQubeParams) =
  let sonarPath = parameters.ToolsPath 
  let args = match call with
    | Begin -> "begin /k:\"" + parameters.Key + "\" /n:\"" + parameters.Name + "\" /v:\"" + parameters.Version + "\""
    | End -> "end"
  let result =
    ExecProcess (fun info ->
      info.FileName <- sonarPath
      info.Arguments <- args) System.TimeSpan.MaxValue
  if result <> 0 then failwithf "Error during sonar qube call " (call.ToString())

/// This task to can be used to run [Sonar Qube](http://conarqube.org/) on a project.
/// ## Parameters
///
///  - `call` - Begin or End, to start analysis or finish it
///  - `setParams` - Function used to overwrite the SonarQube default parameters.
///
/// ## Sample

///   SonarQube Begin (fun p ->
///    {p with
///      Key = "MyProject"
///      Name = "MainTool"
///      Version = "1.0 })
///
let SonarQube (call: SonarQubeCall) setParams = 
    traceStartTask "SonarQube" (call.ToString())
    let parameters = setParams SonarQubeDefaults
    SonarQubeCall call parameters
    traceEndTask "SonarQube" (call.ToString())
