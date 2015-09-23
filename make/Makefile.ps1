Define-Step -Name 'Building' -Target 'build' -Body {
	call "$($Context.NuGetExe)" restore HealthMonitoring.sln
	call "msbuild.exe" HealthMonitoring.sln /t:"Clean,Build" /p:Configuration=Release /m /verbosity:m /nologo /p:TreatWarningsAsErrors=true
}

Define-Step -Name 'Testing' -Target 'build' -Body {
	. (require 'psmake.mod.testing')
	
	$ErrorActionPreference = "SilentlyContinue"
	$tests = @()
	$tests += Define-XUnitTests -GroupName 'Unit tests' -TestAssembly "*\bin\Release\*.UnitTests.dll"
	$tests += Define-XUnitTests -GroupName 'Acceptance tests' -TestAssembly "*\bin\Release\*.AcceptanceTests.dll"

	try {
		$tests | Run-Tests -EraseReportDirectory -Cover -CodeFilter '+[HealthMonitoring*]* -[*Tests*]*' -TestFilter '*Tests.dll' | Generate-CoverageSummary | Check-AcceptableCoverage -AcceptableCoverage 90
	}
	finally{
		if(Test-Path HealthMonitoring.AcceptanceTests\bin\Release\Reports)
		{
			cp HealthMonitoring.AcceptanceTests\bin\Release\Reports\FeaturesSummary.* reports 
		}
	}
}

Define-Step -Name 'Packaging' -Target 'build' -Body {

	Get-ChildItem . -filter "*.nuspec" -recurse -exclude "*.PublicMessages.nuspec","Wonga.Application.Client*nuspec" | Foreach {
		Write-Host "Packing $($_.fullname)"
		call "$($Context.NuGetExe)" pack $($_.fullname) -NoPackageAnalysis -version $($Context.Version)
	}

}
