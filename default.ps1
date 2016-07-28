$script:project_config = "Release"

properties {

  Framework '4.5.1'

  $project_name = "XlsToEf"

  if(-not $version)
  {
      $version = "0.0.0.1"
  }

  $date = Get-Date



  $ReleaseNumber =  $version

  Write-Host "**********************************************************************"
  Write-Host "Release Number: $ReleaseNumber"
  Write-Host "**********************************************************************"


  $base_dir = resolve-path .
  $build_dir = "$base_dir\build"
  $source_dir = "$base_dir\src"
  $app_dir = "$source_dir\$project_name"
  $app_test_dir = "$source_dir\$project_name.tests"
  $app_example_dir = "$source_dir\$project_name.example"
  $test_dir = "$build_dir\test"
  $result_dir = "$build_dir\results"
  $octopus_nuget_repo = "$build_dir\packages"

  $test_assembly_patterns_unit = @("*Tests.dll")

  $nuget_exe = "$base_dir\tools\nuget\nuget.exe"

  $roundhouse_dir = "$base_dir\tools\roundhouse"
  $roundhouse_output_dir = "$roundhouse_dir\output"
  $roundhouse_exe_path = "$roundhouse_dir\rh.exe"
  $roundhouse_local_backup_folder = "$base_dir\database_backups"

  $packageId = if ($env:package_id) { $env:package_id } else { "$project_name" }
  $db_server = if ($env:db_server) { $env:db_server } else { ".\SqlExpress" }
  $db_name = if ($env:db_name) { $env:db_name } else { "XlsToEf" }
  $example_db_name = if ($env:test_db_name) { $env:test_db_name } else { "$db_name.Example" }
  $test_db_name = if ($env:test_db_name) { $env:test_db_name } else { "$db_name.Tests" }

  $example_connection_string_name = "$project_name.Example.ConnectionString"
  $test_connection_string_name = "$project_name.Tests.ConnectionString"

  $exampleConnectionString = if(test-path env:$example_connection_string_name) { (get-item env:$example_connection_string_name).Value } else { "Server=$db_server;Database=$example_db_name;Trusted_Connection=True;MultipleActiveResultSets=true" }
  $testConnectionString = if(test-path env:$test_connection_string_name) { (get-item env:$test_connection_string_name).Value } else { "Server=$db_server;Database=$test_db_name;Trusted_Connection=True;MultipleActiveResultSets=true" }
  $db_scripts_dir = "$source_dir\DatabaseMigration"

  $auth_mode = if ($env:auth_mode) { $env:auth_mode } else { "None" }
}

#These are aliases for other build tasks. They typically are named after the camelcase letters (rd = Rebuild Databases)
#aliases should be all lowercase, conventionally
#please list all aliases in the help task
task default -depends InitialPrivateBuild
task dev -depends DeveloperBuild
task ci -depends CIBuild
task udb -depends UpdateExampleDatabase
task utdb -depends UpdateTestDatabase
task rdb -depends RebuildAllDatabase
task rdbni -depends RebuildAllDatabaseNoIndexes
task rat -depends RunAllTests
task ri -depends RunImport
task ? -depends help

task help {
   Write-Help-Header
   Write-Help-Section-Header "Comprehensive Building"
   Write-Help-For-Alias "(default)" "Intended for first build or when you want a fresh, clean local copy"
   Write-Help-For-Alias "dev" "Optimized for local dev; Most noteably UPDATES databases instead of REBUILDING"
   Write-Help-For-Alias "ci" "Continuous Integration build (long and thorough) with packaging"
   Write-Help-Section-Header "Database Maintence"
   Write-Help-For-Alias "udb" "Update the Database to the latest version (leave db up to date with migration scripts)"
   Write-Help-For-Alias "rdb" "Rebuild all Databases (dev and test) to the latest version from scratch (useful while working on the schema)"
   Write-Help-For-Alias "rdbni" "Rebuild all Databases w/o Indexes (dev and test) to the latest version from scratch"
   Write-Help-Section-Header "Running Tests"
   Write-Help-For-Alias "rat" "Run all tests"
   Write-Help-Section-Header "Development"
   Write-Help-Footer
   exit 0
}

#These are the actual build tasks. They should be Pascal case by convention

task InitialPrivateBuild -depends Clean, Compile, UpdateExampleDatabase, UpdateTestDatabase, RunAllTests

task CIBuild -depends InstallAceDriver, SetBuildDb, Clean, Compile, UpdateTestDatabase, RunAllTests

task DeveloperBuild -depends Clean, SetDebugBuild, Compile, UpdateExampleDatabase, UpdateTestDatabase, RunAllTests

task CompileOnly -depends Clean, SetDebugBuild, Compile

task ReleaseBuild -depends SetReleaseBuild, CommonAssemblyInfo, ApplicationConfiguration, Clean, Compile

task SetDebugBuild {
    $script:project_config = "Debug"
}

task SetReleaseBuild {
    $script:project_config = "Release"
}

task RebuildAllDatabase -depends RebuildExampleDatabase, RebuildTestDatabase

task RebuildAllDatabaseNoIndexes {
  deploy-database "Rebuild" $exampleConnectionString $db_scripts_dir "EXAMPLE" "none"
  deploy-database "Rebuild" $testConnectionString $db_scripts_dir "TEST" "none"
}

task RebuildExampleDatabase{
  deploy-database "Rebuild" $exampleConnectionString $db_scripts_dir "EXAMPLE"
}

task RebuildTestDatabase {
      deploy-database "Rebuild" $testConnectionString $db_scripts_dir "TEST"
}

task UpdateExampleDatabase {
    deploy-database "Update" $exampleConnectionString $db_scripts_dir "EXAMPLE"
}

task UpdateTestDatabase {
    deploy-database "Update" $testConnectionString $db_scripts_dir "TEST"
}

task CommonAssemblyInfo {
    create-commonAssemblyInfo "$ReleaseNumber" $project_name "$source_dir\SharedAssemblyInfo.cs"
}

task SetBuildDb {
  poke-xml "$app_test_dir\app.config" "configuration/connectionStrings/add[@name='XlsToEfTestDatabase']/@connectionString" $testConnectionString
}

task ApplicationConfiguration {
  poke-xml "$app_dir\web.config" "configuration/system.web/authentication/@mode" $auth_mode
  poke-xml "$app_dir\web.config" "configuration/appSettings/add[@key='ApiUrl']/@value" $api_url
  poke-xml "$app_dir\web.config" "configuration/system.webServer/staticContent/clientCache/@cacheControlMode" "UseMaxAge"
}

task CopyAssembliesForTest -Depends Compile {
    copy_all_assemblies_for_test $test_dir
}

task RunAllTests -Depends CopyAssembliesForTest {
    $test_assembly_patterns_unit | %{ run_fixie_tests $_ }
}

task Compile -depends Clean, CommonAssemblyInfo {
    exec { & $nuget_exe restore $source_dir\$project_name.sln }
    exec { msbuild.exe /t:build /v:q /p:Configuration=$project_config /p:Platform="Any CPU" /nologo $source_dir\$project_name.sln }
}

task InstallAceDriver {
    exec { choco install msaccess2010-redist-x64 }

}


task Clean {
    delete_file $package_file
    delete_directory $build_dir
    create_directory $test_dir
    create_directory $result_dir

    exec { msbuild /t:clean /v:q /p:Configuration=$project_config /p:Platform="Any CPU" $source_dir\$project_name.sln }
}

task Package -depends SetReleaseBuild {
    delete_directory $package_dir
    zip_directory $package_dir $package_file
}

task RunImport -depends CompileOnly {
    $destination = "$build_dir\console\$console_app_name"
    copy_all_assemblies_for_console_app $destination
    exec { &"$destination\$console_app_name.exe" "$active_import_files" }
}

# -------------------------------------------------------------------------------------------------------------
# generalized functions added by Headspring for Help Section
# --------------------------------------------------------------------------------------------------------------

function Write-Help-Header($description) {
   Write-Host ""
   Write-Host "********************************" -foregroundcolor DarkGreen -nonewline;
   Write-Host " HELP " -foregroundcolor Green  -nonewline;
   Write-Host "********************************"  -foregroundcolor DarkGreen
   Write-Host ""
   Write-Host "This build script has the following common build " -nonewline;
   Write-Host "task " -foregroundcolor Green -nonewline;
   Write-Host "aliases set up:"
}

function Write-Help-Footer($description) {
   Write-Host ""
   Write-Host " For a complete list of build tasks, view default.ps1."
   Write-Host ""
   Write-Host "**********************************************************************" -foregroundcolor DarkGreen
}

function Write-Help-Section-Header($description) {
   Write-Host ""
   Write-Host " $description" -foregroundcolor DarkGreen
}

function Write-Help-For-Alias($alias,$description) {
   Write-Host "  > " -nonewline;
   Write-Host "$alias" -foregroundcolor Green -nonewline;
   Write-Host " = " -nonewline;
   Write-Host "$description"
}

# -------------------------------------------------------------------------------------------------------------
# generalized functions
# --------------------------------------------------------------------------------------------------------------
function deploy-database($action, $connectionString, $scripts_dir, $env, $indexes) {

    write-host "action: $action"
    write-host "connectionString: $connectionString"
    write-host "scripts_dir: $scripts_dir"
    write-host "env: $env"

    if (!$env) {
        $env = "LOCAL"
        Write-Host "RoundhousE environment variable is not specified... defaulting to 'LOCAL'"
    } else {
        Write-Host "Executing RoundhousE for environment:" $env
    }

    # Run roundhouse commands on $scripts_dir
    if ($action -eq "Update"){
       exec { &$roundhouse_exe_path -cs "$connectionString" --commandtimeout=300 -f $scripts_dir --env $env --silent -o $roundhouse_output_dir --transaction --amg afterMigration }
    }
    if ($action -eq "Rebuild"){
      $indexesFolder = if ($indexes -ne $null) { $indexes } else { "indexes" }
       exec { &$roundhouse_exe_path -cs "$connectionString" --commandtimeout=300 --env $env --silent -drop -o $roundhouse_output_dir }
       exec { &$roundhouse_exe_path -cs "$connectionString" --commandtimeout=300 -f $scripts_dir -env $env --silent --simple -o $roundhouse_output_dir --transaction --amg afterMigration --indexes $indexesFolder }
    }
}

function run_fixie_tests([string]$pattern) {
    $items = Get-ChildItem -Path $test_dir $pattern
    $items | %{ run_fixie $_.Name }
}

function global:zip_directory($directory,$file) {
    write-host "Zipping folder: " $test_assembly
    delete_file $file
    cd $directory
    & "$base_dir\tools\7zip\7za.exe" a -mx=9 -r $file
    cd $base_dir
}

function global:delete_file($file) {
    if($file) { remove-item $file -force -ErrorAction SilentlyContinue | out-null }
}

function global:delete_directory($directory_name) {
  rd $directory_name -recurse -force  -ErrorAction SilentlyContinue | out-null
}

function global:create_directory($directory_name) {
  mkdir $directory_name  -ErrorAction SilentlyContinue  | out-null
}

function global:run_fixie ($test_assembly) {
   $assembly_to_test = $test_dir + "\" + $test_assembly
   $results_output = $result_dir + "\" + $test_assembly + ".xml"
    write-host "Running Fixie Tests in: $test_assembly"
    exec { & tools\fixie\Fixie.Console.exe $assembly_to_test }
}

function global:Copy_and_flatten ($source,$include,$dest) {
   Get-ChildItem $source -include $include -r | cp -dest $dest
}

function global:copy_all_assemblies_for_console_app($destination) {
    $bin_dir_match_pattern = "$source_dir\**\bin\$project_config"
    create_directory $destination
    Copy_and_flatten $bin_dir_match_pattern @("*.exe","*.dll","*.config","*.pdb") $destination
}

function global:copy_all_assemblies_for_test($destination){
   $bin_dir_match_pattern = "$source_dir\**\bin\$project_config"
   create_directory $destination
   Copy_and_flatten $bin_dir_match_pattern @("*.exe","*.dll","*.config","*.pdb","*.sql","*.xlsx","*.csv") $destination
}

function global:copy_website_files($source,$destination){
    $exclude = @('*.user','*.dtd','*.tt','*.cs','*.csproj')
    copy_files $source $destination $exclude
   delete_directory "$destination\obj"
}

function global:copy_files($source,$destination,$exclude=@()){
    create_directory $destination
    Get-ChildItem $source -Recurse -Exclude $exclude | Copy-Item -Destination {Join-Path $destination $_.FullName.Substring($source.length)}
}

function global:create-commonAssemblyInfo($version,$applicationName,$filename) {
$date = Get-Date
$currentYear = $date.Year
"using System.Reflection;
using System.Runtime.CompilerServices;

// Version information for an assembly consists of the following four values:
//
//      Year                    (Expressed as YYYY)
//      Major Release           (i.e. New Project / Namespace added to Solution or New File / Class added to Project)
//      Minor Release           (i.e. Fixes or Feature changes)
//      Build Date & Revsion    (Expressed as MMDD)
//
[assembly: AssemblyCompany("""")]
[assembly: AssemblyCopyright("""")]
[assembly: AssemblyProduct("""")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]
[assembly: AssemblyVersion(""$version"")]
[assembly: AssemblyFileVersion(""$version"")]" | out-file $filename -encoding "utf8"
}

function script:poke-xml($filePath, $xpath, $value, $namespaces = @{}) {
    [xml] $fileXml = Get-Content $filePath

    if($namespaces -ne $null -and $namespaces.Count -gt 0) {
        $ns = New-Object Xml.XmlNamespaceManager $fileXml.NameTable
        $namespaces.GetEnumerator() | %{ $ns.AddNamespace($_.Key,$_.Value) }
        $node = $fileXml.SelectSingleNode($xpath,$ns)
    } else {
        $node = $fileXml.SelectSingleNode($xpath)
    }

    Assert ($node -ne $null) "could not find node @ $xpath"

    if($node.NodeType -eq "Element") {
        $node.InnerText = $value
    } else {
        $node.Value = $value
    }

    $fileXml.Save($filePath)
}