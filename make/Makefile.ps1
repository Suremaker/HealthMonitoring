Define-Step -Name 'Step one' -Target 'build' -Body {
	call "msbuild.exe" HealthMonitoring.sln /t:"Clean,Build" /p:Configuration=Release /m /verbosity:m /nologo /p:TreatWarningsAsErrors=true
}

Define-Step -Name 'Step two' -Target 'build,deploy' -Body {
	$tests = @()
	$tests += Define-XUnitTests -GroupName 'Unit tests' -TestAssembly "*\bin\Release\*.UnitTests.dll"
	$tests += Define-XUnitTests -GroupName 'Acceptance tests' -TestAssembly "*\bin\Release\*.AcceptanceTests.dll"
	
	$tests | Run-Tests -EraseReportDirectory -Cover -CodeFilter '+[HealthMonitoring*]* -[*Tests*]*' -TestFilter '*Tests.dll' | Generate-CoverageSummary | Check-AcceptableCoverage -AcceptableCoverage 95
}
