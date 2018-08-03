# Introduction

There is an extension [.NET Core Test Explorer for Visual Studio Code](https://github.com/formulahendry/vscode-dotnet-test-explorer) which currently makes it possible to discover and execute test cases for **xunit**, **nunit** and **mstest**.

The tests are discovered using command [`dotnet vstest`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-vstest) and run via [`dotnet test`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test).

As stated in https://github.com/Microsoft/vstest/issues/637#issuecomment-291762855 both these commands are essentially wrappers on `vstest.console.dll`.

So the idea would be to write a test adapter for **nspec** which integrates with [**VSTest**](https://github.com/Microsoft/vstest) and targets .NET Core / .NET Standard 1.6.

**nspec** tests should then be both discoverable and runnable out of the box in the Test Explorer extension.


# Writing a new test adapter

VSTest allows for adapter extensibility to support 3rd party test frameworks:
https://github.com/Microsoft/vstest-docs/blob/master/RFCs/0004-Adapter-Extensibility.md

# Logic to discover and execute tests

The logic to discover test examples, along with the logic to run selected test examples can be found in [**NSpec.VsAdapter**](https://github.com/nspec/NSpec.VsAdapter).

# Existing Test Adapter

There is already a test adapter at https://github.com/nspec/DotNetTestNSpec, which leverages the dotnet test command line interface. However it currenly supports only old porjects based on .NET Core Tools Preview 2, the ones with project.json and .xproj files.

# Using vstest.console.exe

![TestAdapterCli](/pictures/testadaptercli.png)

# Microsoft.Net.Test.Sdk

Microsoft.Net.Test.Sdk injects OutputType
https://andrewlock.net/fixing-the-error-program-has-more-than-one-entry-point-defined-for-console-apps-containing-xunit-tests/