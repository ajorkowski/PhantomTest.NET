core:
	nuget push PhantomTest.NET\bin\Release\PhantomTest.NET.$(v).nupkg -Timeout 1000

nunit:
	nuget push PhantomTest.NET.NUnit\bin\Release\PhantomTest.NET.NUnit.$(v).nupkg

.PHONY: core nunit