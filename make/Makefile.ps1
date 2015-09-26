Define-Step -Name 'Update version info' -Target 'build' -Body {
	. (require 'psmake.mod.update-version-info')
	Update-VersionInAssemblyInfo $($Context.Version) 
}

Define-Step -Name 'Building' -Target 'build' -Body {
	call "$($Context.NuGetExe)" restore HealthMonitoring.sln
	call "msbuild.exe" HealthMonitoring.sln /t:"Clean,Build" /p:Configuration=Release /m /verbosity:m /nologo /p:TreatWarningsAsErrors=true
}

Define-Step -Name 'Testing' -Target 'build' -Body {
	. (require 'psmake.mod.testing')
	
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

	Get-ChildItem . -filter "*-deploy.nuspec" -recurse | Foreach {
		Write-Host "Packing $($_.fullname)"
		call "$($Context.NuGetExe)" pack $($_.fullname) -NoPackageAnalysis -version $($Context.Version)
	}
	
	Get-ChildItem . -filter "*.nuspec" -recurse -exclude "*-deploy.nuspec" | Foreach {
		$csprj = $_.fullname -replace '.nuspec','.csproj'
		Write-Host "Packing $csprj"
		call "$($Context.NuGetExe)" pack $csprj -Prop Configuration=Release

	}
}
