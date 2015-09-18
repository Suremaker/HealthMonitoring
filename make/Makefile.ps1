Define-Step -Name 'Building' -Target 'build' -Body {
	call "$($Context.NuGetExe)" restore HealthMonitoring.sln
	call "msbuild.exe" HealthMonitoring.sln /t:"Clean,Build" /p:Configuration=Release /m /verbosity:m /nologo /p:TreatWarningsAsErrors=true
}

Define-Step -Name 'Testing' -Target 'build,deploy' -Body {
	. (require 'psmake.mod.testing')
	
	$tests = @()
	$tests += Define-XUnitTests -GroupName 'Unit tests' -TestAssembly "*\bin\Release\*.UnitTests.dll"
	$tests += Define-XUnitTests -GroupName 'Acceptance tests' -TestAssembly "*\bin\Release\*.AcceptanceTests.dll"
	
	$tests | Run-Tests -EraseReportDirectory -Cover -CodeFilter '+[HealthMonitoring*]* -[*Tests*]*' -TestFilter '*Tests.dll' | Generate-CoverageSummary | Check-AcceptableCoverage -AcceptableCoverage 90
}
