<#
.SYNOPSIS 
Defines NUnit test group.

.DESCRIPTION
Defines NUnit test group.
It allows to specify a GroupName, one or more TestAssembly paths and optionally a ReportName and NUnitVersion.

A defined tests could be later executed with Run-Tests function.

.EXAMPLE
PS> Define-NUnitTests -GroupName 'Unit tests' -TestAssembly "MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll"
#>
function Define-NUnitTests
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true, Position=0)]
        # Test group name. Used for display as well as naming reports. 
        [ValidateNotNullOrEmpty()]
        [Alias('Name','tgn')]
        [string]$GroupName,

        [Parameter(Mandatory=$true, Position=1)]
        # Test assembly path, where path supports * and ? wildcards.
        # It is possible to specify multiple paths.
        [ValidateNotNullOrEmpty()]
        [Alias('ta')]
        [string[]]$TestAssembly,

        [Parameter()]
        # Test report name. If not specified, a GroupName parameter would be used (spaces would be converted to underscores). 
        [AllowNull()]
        [string]$ReportName = $null,

        [Parameter()]
        # NUnit.Runners version. By default it is: 2.6.4
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[0-9]+(\.[0-9]+){0,3}$")]
        [Alias('RunnerVersion')]
        [string]$NUnitVersion = "2.6.4"
    )

    . $PSScriptRoot\internals.ps1
    if (($ReportName -eq $null) -or ($ReportName -eq '')) { $ReportName = $GroupName -replace ' ','_' }

    Create-Object @{
        Package='NUnit.Runners';
        PackageVersion=$NUnitVersion;
        GroupName=$GroupName;
        ReportName=$ReportName;
        Assemblies=[string[]](Resolve-TestAssemblies $TestAssembly);
        Runner='tools\nunit-console.exe';
        GetRunnerArgs={
            param([PSObject]$Definition, [string]$ReportDirectory)
            return $Definition.Assemblies + "/nologo", "/noshadow", "/domain:single", "/trace=Error", "/xml:$ReportDirectory\$($Definition.ReportName).xml"
        };}
}

<#
.SYNOPSIS 
Defines MbUnit test group.

.DESCRIPTION
Defines MbUnit test group.
It allows to specify a GroupName, one or more TestAssembly paths and optionally a ReportName and MbUnitVersion.

A defined tests could be later executed with Run-Tests function.

.EXAMPLE
PS> Define-MbUnitTests -GroupName 'Unit tests' -TestAssembly "MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll"
#>
function Define-MbUnitTests
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        # Test group name. Used for display as well as naming reports. 
        [ValidateNotNullOrEmpty()]
        [Alias('Name','tgn')]
        [string]$GroupName,

        [Parameter(Mandatory=$true, Position=1)]
        # Test assembly path, where path supports * and ? wildcards.
        # It is possible to specify multiple paths.
        [ValidateNotNullOrEmpty()]
        [Alias('ta')]
        [string[]]$TestAssembly,

        [Parameter()]
        # Test report name. If not specified, a GroupName parameter would be used (spaces would be converted to underscores). 
        [AllowNull()]
        [string]$ReportName = $null,
        
        [Parameter()]
        # GallioBundle version. By default it is: 3.4.14
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[0-9]+(\.[0-9]+){0,3}$")]
        [string]$MbUnitVersion = "3.4.14"
    )

    . $PSScriptRoot\internals.ps1
    if (($ReportName -eq $null) -or ($ReportName -eq '')) { $ReportName = $GroupName -replace ' ','_' }

    Create-Object @{
        Package='GallioBundle';
        PackageVersion=$MbUnitVersion;
        GroupName=$GroupName;
        ReportName=$ReportName;
        Assemblies=[string[]](Resolve-TestAssemblies $TestAssembly);
        Runner='bin\Gallio.Echo.exe';
        GetRunnerArgs={
            param([PSObject]$Definition, [string]$ReportDirectory) 
            return $Definition.Assemblies + "/no-logo", "/rt:Xml", "/rd:$ReportDirectory", "/rnf:$($Definition.ReportName)" 
        };}
}

<#
.SYNOPSIS 
Defines MsTest test group.

.DESCRIPTION
Defines MsTest test group.
It allows to specify a GroupName, one or more TestAssembly paths and optionally a ReportName and VisualStudioVersion.

A defined tests could be later executed with Run-Tests function.

.EXAMPLE
PS> Define-MsTests -GroupName 'Unit tests' -TestAssembly "MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll"
#>
function Define-MsTests
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        # Test group name. Used for display as well as naming reports. 
        [ValidateNotNullOrEmpty()]
        [Alias('Name','tgn')]
        [string]$GroupName,

        [Parameter(Mandatory=$true, Position=1)]
        # Test assembly path, where path supports * and ? wildcards.
        # It is possible to specify multiple paths.
        [ValidateNotNullOrEmpty()]
        [Alias('ta')]
        [string[]]$TestAssembly,

        [Parameter()]
        # Test report name. If not specified, a GroupName parameter would be used (spaces would be converted to underscores). 
        [AllowNull()]
        [string]$ReportName = $null,
        
        [Parameter()]
        # Visual Studio version used to find mstest.exe. The default is: 12.0
        [ValidateNotNullOrEmpty()]
        [string]$VisualStudioVersion = "12.0"
    )

    . $PSScriptRoot\internals.ps1
    if (($ReportName -eq $null) -or ($ReportName -eq '')) { $ReportName = $GroupName -replace ' ','_' }

    Create-Object @{
        Package=$null;
        PackageVersion=$null;
        GroupName=$GroupName;
        ReportName=$ReportName;
        Assemblies=[string[]](Resolve-TestAssemblies $TestAssembly);
        Runner="${env:ProgramFiles(x86)}\Microsoft Visual Studio $VisualStudioVersion\Common7\IDE\mstest.exe";
        GetRunnerArgs={
            param([PSObject]$Definition, [string]$ReportDirectory) 
            [string[]] $asms = $Definition.Assemblies | %{ "/testcontainer:$_"}
            return ($asms + "/nologo", "/resultsfile:$ReportDirectory\$($Definition.ReportName).trx") 
        };}
}

<#
.SYNOPSIS 
Defines XUnit test group.

.DESCRIPTION
Defines XUnit test group.
It allows to specify a GroupName, one or more TestAssembly paths and optionally a ReportName and XUnitVersion.

A defined tests could be later executed with Run-Tests function.

.EXAMPLE
PS> Define-XUnitTests -GroupName 'Unit tests' -TestAssembly "MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll"
#>
function Define-XUnitTests
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true, Position=0)]
        # Test group name. Used for display as well as naming reports. 
        [ValidateNotNullOrEmpty()]
        [Alias('Name','tgn')]
        [string]$GroupName,

        [Parameter(Mandatory=$true, Position=1)]
        # Test assembly path, where path supports * and ? wildcards.
        # It is possible to specify multiple paths.
        [ValidateNotNullOrEmpty()]
        [Alias('ta')]
        [string[]]$TestAssembly,

        [Parameter()]
        # Test report name. If not specified, a GroupName parameter would be used (spaces would be converted to underscores). 
        [AllowNull()]
        [string]$ReportName = $null,

        [Parameter()]
        # XUnit.Runner.Console version. By default it is: 2.0.0
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[0-9]+(\.[0-9]+){0,3}$")]
        [Alias('RunnerVersion')]
        [string]$XUnitVersion = "2.0.0"
    )

    . $PSScriptRoot\internals.ps1
    if (($ReportName -eq $null) -or ($ReportName -eq '')) { $ReportName = $GroupName -replace ' ','_' }

    Create-Object @{
        Package='xunit.runner.console';
        PackageVersion=$XUnitVersion;
        GroupName=$GroupName;
        ReportName=$ReportName;
        Assemblies=[string[]](Resolve-TestAssemblies $TestAssembly);
        Runner='tools\xunit.console.x86.exe';
        GetRunnerArgs={
            param([PSObject]$Definition, [string]$ReportDirectory)
            return $Definition.Assemblies + "-nologo", "-noshadow", "-quiet", "-nunit", "$ReportDirectory\$($Definition.ReportName).xml"
        };}
}

<#
.SYNOPSIS 
Executes tests from one or more specified test definitions.

.DESCRIPTION
This function allows to execute tests from one or more specified test definitions.
If multiple test definitions are provided, they are executed sequentially.
If given test group fail, others are not executed.

If -Cover switch is specified, a test coverage would be calculated for each group.
The test execution reports and coverage reports would be located in directory specified by -ReportDirectory.
The convention for coverage report is [ReportName]_coverage.xml, where ReportName is specified in tests defintion.

.EXAMPLE
PS> Define-NUnitTests -GroupName 'Unit tests' -TestAssembly "MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll" | Run-Tests 

Executes NUnit tests from MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll assembly.

.EXAMPLE
PS> Define-NUnitTests -GroupName 'Unit tests' -TestAssembly "MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll" | Run-Tests -Cover -CodeFilter '+[MyProject*]* -[*Tests*]*' -TestFilter '*Tests.dll'

Executes NUnit tests from MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll assembly and calculates test coverage for them.

.EXAMPLE

PS> $tests = @()
PS>    $tests += Define-NUnitTests -GroupName 'NUnit tests' -TestAssembly "MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll"
PS> $tests += Define-MbUnitTests -GroupName 'MbUnit tests' -TestAssembly "MyProject.MbUnit.UnitTests\bin\Release\MyProject.MbUnit.UnitTests.dll"    
PS> $tests += Define-MsTests -GroupName 'MsTest tests' -TestAssembly "MyProject.MsTest.UnitTests\bin\Release\MyProject.MsTest.UnitTests.dll"
PS> $tests | Run-Tests -ReportDirectory 'test_reports' -EraseReportDirectory -Cover -CodeFilter '+[MyProject*]* -[*Tests*]*' -TestFilter '*Tests.dll'

Executes 3 test groups of NUnit, MbUnit and MsTest types, calculates test coverage for all of them and puts all execution result files to test_reports directory.
The test_reports directory is deleted before test execution.
#>
function Run-Tests
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true,ValueFromPipeline=$true,ParameterSetName="coverage")]
        [Parameter(Mandatory=$true,ValueFromPipeline=$true,ParameterSetName="test")]
        # An array of test definitions.
        [ValidateNotNullOrEmpty()]
        [PSObject[]]$TestDefinition,
        
        [Parameter(Mandatory=$true,ParameterSetName="coverage")]
        # Run tests with OpenCover to determine coverage level
        [switch]$Cover,
        
        [Parameter(Mandatory=$true,ParameterSetName="coverage")]
        # OpenCover code filter (used for -filter param), like: +[Company.Project.*]* -[*Tests*]*
        [ValidateNotNullOrEmpty()]
        [string]$CodeFilter,
                
        [Parameter(ParameterSetName="coverage")]
        # OpenCover test filter (used for -coverbytest param), like: *.Tests.Unit.dll
        [ValidateNotNullOrEmpty()]
        [string]$TestFilter="*Tests.dll",
        
        [Parameter()]
        # Reports directory. By default it is 'reports'
        [ValidateNotNullOrEmpty()]
        [string]$ReportDirectory = "reports",

        [Parameter()]
        # Delete reports directory before execution. By default it is: $false
        [switch]$EraseReportDirectory = $false,
        
        [Parameter(ParameterSetName="coverage")]
        # OpenCover version. By default it is: 4.5.2506
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[0-9]+(\.[0-9]+){0,3}$")]
        [string]$OpenCoverVersion="4.5.2506"
    )
    begin
    {
        . $PSScriptRoot\internals.ps1
        Prepare-ReportDirectory $ReportDirectory $EraseReportDirectory
        
        $coverageReports = @()
    }

    process
    {
        Write-Status "Executing tests: $($_.GroupName)"
        $runnerArgs = & $_.GetRunnerArgs $_ $ReportDirectory
        if($_.Package -ne $null)
        { 
            $runnerPath = Fetch-Package $_.Package $_.PackageVersion
            $runner = "$runnerPath\$($_.Runner)"
        }
        else { $runner = $_.Runner }

        if (! $Cover)
        { 
            Write-ShortStatus "Running tests"
            call $runner -args $runnerArgs
        }
        else
        {    
            $CoverageReport = "$ReportDirectory\$($_.ReportName)_coverage.xml"
            Run-OpenCover -OpenCoverVersion $OpenCoverVersion -Runner $runner -RunnerArgs $runnerArgs -CodeFilter $CodeFilter -TestFilter $TestFilter -Output $CoverageReport

            $coverageReports += $CoverageReport
        }
    
    }

    end
    {
        if($coverageReports.Length -gt 0) { Create-Object @{ ReportDirectory=$ReportDirectory; CoverageReports=$coverageReports; } }
    }
}

<#
.SYNOPSIS 
Generates Coverage Summary report for specified coverage report files, generates by Run-Tests command.

.DESCRIPTION
Generates Coverage Summary report for specified coverage report files, generates by Run-Tests command.
It is possible to call this function with explicit values, or pipe it with Run-Tests command.

The summary report is generated in two forms: HTML and XML.
HTML report path is [ReportDirectory]\summary\index.htm
XML report path is [ReportDirectory]\summary\summary.xml

If piped with Run-Tests, the ReportDirectory would be passed from Run-Tests command.

The XML report could be used later by Check-AcceptableCoverage function.

If multiple coverage report files are specified, a summary report would include all of them.

.EXAMPLE
PS> Define-NUnitTests -GroupName 'Unit tests' -TestAssembly "MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll" | Run-Tests -Cover -CodeFilter '+[MyProject*]* -[*Tests*]*' -TestFilter '*Tests.dll' | Generate-CoverageSummary

Executes NUnit tests from MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll assembly, calculates test coverage for them and generates coverage summary report.

.EXAMPLE

PS> $tests = @()
PS>    $tests += Define-NUnitTests -GroupName 'NUnit tests' -TestAssembly "MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll"
PS> $tests += Define-MbUnitTests -GroupName 'MbUnit tests' -TestAssembly "MyProject.MbUnit.UnitTests\bin\Release\MyProject.MbUnit.UnitTests.dll"    
PS> $tests += Define-MsTests -GroupName 'MsTest tests' -TestAssembly "MyProject.MsTest.UnitTests\bin\Release\MyProject.MsTest.UnitTests.dll"
PS> $tests | Run-Tests -Cover -CodeFilter '+[MyProject*]* -[*Tests*]*' -TestFilter '*Tests.dll' | Generate-CoverageSummary

Executes 3 test groups of NUnit, MbUnit and MsTest types, calculates test coverage for all of them and generates coverage summary report containing merged information about all test groups.
#>
function Generate-CoverageSummary
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true,ParameterSetName="standard")]
        # Path to coverage report(s).
        [ValidateNotNullOrEmpty()]
        [string[]]$CoverageReport,

        [Parameter(ParameterSetName="standard")]
        # Report directory. Default: reports
        [ValidateNotNullOrEmpty()]
        [string]$ReportDirectory = 'reports',

        [Parameter(Mandatory=$true,ValueFromPipeline=$true,ParameterSetName="reportInput")]
        # Run-Tests result.
        [ValidateNotNull()]
        [PSObject]$TestResult,

        [Parameter()]
        # ReportGenerator version. By default it is: 1.9.1.0
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[0-9]+(\.[0-9]+){0,3}$")]
        [string]$ReportGeneratorVersion="1.9.1.0"
    )

    Write-ShortStatus "Preparing ReportGenerator"
    $reportGenPath = Fetch-Package "ReportGenerator" $ReportGeneratorVersion
    $ReportGeneratorPath="$reportGenPath\ReportGenerator.exe"
    
    if ($TestResult -ne $null)
    {
        $CoverageReport = $TestResult.CoverageReports
        $ReportDirectory = $TestResult.ReportDirectory
    }

    Write-ShortStatus "Generating coverage reports"
    $reports = $CoverageReport -join ';'
    call "$ReportGeneratorPath" "-reporttypes:html,xmlsummary" "-verbosity:error" "-reports:$reports" "-targetdir:$ReportDirectory\summary"
    Write-Output "$ReportDirectory\summary\Summary.xml"
}

<#
.SYNOPSIS 
Checks that acceptable coverage is over specified limit.

.DESCRIPTION
Checks that coverage level is over specified limit.
It requires a coverage summary XML report, generated by Generate-CoverageSummary function.
If coverage level is below specified limit, and exception is thrown.

.EXAMPLE
PS> Define-NUnitTests -GroupName 'Unit tests' -TestAssembly "MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll" | Run-Tests -Cover -CodeFilter '+[MyProject*]* -[*Tests*]*' -TestFilter '*Tests.dll' | Generate-CoverageSummary | Check-AcceptableCoverage -AcceptableCoverage 95

Executes NUnit tests from MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll assembly, calculates test coverage for them, generates coverage summary report and ensures that coverage level is at least 95%.

.EXAMPLE

PS> $tests = @()
PS>    $tests += Define-NUnitTests -GroupName 'NUnit tests' -TestAssembly "MyProject.UnitTests\bin\Release\MyProject.UnitTests.dll"
PS> $tests += Define-MbUnitTests -GroupName 'MbUnit tests' -TestAssembly "MyProject.MbUnit.UnitTests\bin\Release\MyProject.MbUnit.UnitTests.dll"    
PS> $tests += Define-MsTests -GroupName 'MsTest tests' -TestAssembly "MyProject.MsTest.UnitTests\bin\Release\MyProject.MsTest.UnitTests.dll"
PS> $tests | Run-Tests -Cover -CodeFilter '+[MyProject*]* -[*Tests*]*' -TestFilter '*Tests.dll' | Generate-CoverageSummary | Check-AcceptableCoverage -AcceptableCoverage 95

Executes 3 test groups of NUnit, MbUnit and MsTest types, calculates test coverage for all of them, generates coverage summary report containing merged information about all test groups and ensures that coverage level is at least 95%.
#>
function Check-AcceptableCoverage
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true,ValueFromPipeline=$true)]
        # Path to coverage summary report (generated by Generate-CoverageSummary function).
        [ValidateNotNullOrEmpty()]
        [string]$SummaryReport,

        [Parameter(Mandatory=$true)]
        # Minimal acceptable coverage
        [ValidateRange(0,100)] 
        [int]$AcceptableCoverage
    )
    Write-ShortStatus "Validating code coverage being at least $AcceptableCoverage%"

    [xml]$coverage = Get-Content $SummaryReport
    $actualCoverage = [double]($coverage.CoverageReport.Summary.Coverage -replace '%','')
    Write-Host "Coverage is $actualCoverage%"
    if($actualCoverage -lt $AcceptableCoverage) {
        throw "Coverage $($actualCoverage)% is below threshold $($AcceptableCoverage)%"
    }
}