Define-Step -Name 'Update version info' -Target 'build' -Body {
	. (require 'psmake.mod.update-version-info')
	Update-VersionInAssemblyInfo $VERSION
}

Define-Step -Name 'Building' -Target 'build' -Body {
	call "$($Context.NuGetExe)" restore HealthMonitoring.sln -ConfigFile "$($Context.NuGetConfig)"
	call "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" HealthMonitoring.sln /t:"Clean,Build" /p:Configuration=Release /m /verbosity:m /nologo /p:TreatWarningsAsErrors=true /tv:14.0
}

Define-Step -Name 'Testing' -Target 'build' -Body {
	. (require 'psmake.mod.testing')
	
	$tests = @()
	$tests += Define-XUnitTests -GroupName 'Management Core Unit tests' -XUnitVersion '2.1.0' -TestAssembly "*\bin\Release\*.Management.Core.UnitTests.dll"
	$tests += Define-XUnitTests -GroupName 'Api Unit tests' -XUnitVersion '2.1.0' -TestAssembly "*\bin\Release\*.Api.UnitTests.dll"
	$tests += Define-XUnitTests -GroupName 'Monitors Core Unit tests' -XUnitVersion '2.1.0' -TestAssembly "*\bin\Release\*.Monitors.Core.UnitTests.dll"
	$tests += Define-XUnitTests -GroupName 'Acceptance tests' -XUnitVersion '2.1.0' -TestAssembly "*\bin\Release\*.AcceptanceTests.dll"

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
	. (require 'psmake.mod.packaging')

	Find-VSProjectsForPackaging | Package-VSProject
	
	Find-NuSpecFiles -filter "*-deploy.nuspec" | Package-DeployableNuSpec -Version $VERSION
}
