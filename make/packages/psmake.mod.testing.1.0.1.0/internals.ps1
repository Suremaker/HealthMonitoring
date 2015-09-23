function Prepare-ReportDirectory($ReportDirectory, $erase)
{
	if ($EraseReportDirectory) 
	{
		Write-ShortStatus "Cleaning $ReportDirectory..."; 
		Remove-Item $ReportDirectory -Force -Recurse -ErrorAction SilentlyContinue
	}
	mkdir $ReportDirectory -ErrorAction SilentlyContinue | Out-Null
}

function Run-OpenCover($OpenCoverVersion, $Runner, $RunnerArgs, $CodeFilter, $TestFilter, $Output)
{
	Write-ShortStatus "Preparing OpenCover"
	$openCoverPath = Fetch-Package "OpenCover" $OpenCoverVersion
	$OpenCoverConsole="$openCoverPath\OpenCover.Console.exe"

	Write-ShortStatus "Running tests with OpenCover"
	call "$OpenCoverConsole" "-log:Error" "-showunvisited" "-register:user" "-target:$Runner"  "-filter:$CodeFilter" "-output:$Output" "-returntargetcode" "-coverbytest:$TestFilter" "-targetargs:$RunnerArgs"
}

function Resolve-TestAssemblies
{
    [CmdletBinding()]
	param (
		[Parameter(Mandatory=$true)]
		# An array of tests assembly paths.
        # If given path does not contain wildcard characters, it would be returned immediately.
        # If given path contains wildcard characters (i.e.: *?), the $SolutionDirectory and it's subdirectories would be scanned and all assemblies matching to this patch would be returned.
        # Please note that paths with wildcards and referring to directories outside $SolutionDirectory will not return any assemblies. In such case, please use paths without wildcards.
		[ValidateNotNullOrEmpty()]
		[string[]]$TestAssemblies,

        [Parameter()]
        # Solution base directory. By default it is '.'
        [ValidateNotNullOrEmpty()]
        [string]$SolutionDirectory = '.'
    )

	$results = @()
    $includes = @()
    $results += $TestAssemblies | Where-Object {$_.IndexOfAny('*?'.ToCharArray()) -lt 0}
    $includes += $TestAssemblies | Where-Object {$_.IndexOfAny('*?'.ToCharArray()) -ge 0} | %{ '^'+ ($_ -replace '\\','\\' -replace '\.','\.' -replace '\?','.' -replace '\*','.*') +'$' }

    if($includes.Length -gt 0) 
    { 
        Write-ShortStatus "Scanning for test assemblies in $SolutionDirectory and subdirectories"
        $pattern = $includes -join '|'
        $results += Get-ChildItem $SolutionDirectory -Recurse | Where-Object { $_.FullName -match $pattern } | %{$_.FullName}        
        $results | Write-Host
    }

    return $results
}