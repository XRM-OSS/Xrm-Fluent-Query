# XRM Fluent Query [![Build status](https://ci.appveyor.com/api/projects/status/x0o7dqnhnwi2i8bk?svg=true)](https://ci.appveyor.com/project/DigitalFlow/xrm-fluent-query)

|Line Coverage|Branch Coverage|
|-----|-----------------|
|[![Line coverage](https://cdn.rawgit.com/digitalflow/xrm-fluent-query/master/reports/badge_linecoverage.svg)](https://cdn.rawgit.com/digitalflow/xrm-fluent-query/master/reports/index.htm)|[![Branch coverage](https://cdn.rawgit.com/digitalflow/xrm-fluent-query/master/reports/badge_branchcoverage.svg)](https://cdn.rawgit.com/digitalflow/xrm-fluent-query/master/reports/index.htm)|

This is a library for fluent query operations in Dynamics CRM / Dynamics365.

# Requirements
This library can be used in CRM Plugins / Workflow Activities and in code of external applications. It is distributed as source file, so you don't need to merge DLLs.
It does not include references / dependencies to any CRM SDK, so you can just install it and choose the CRM SDK that you need yourself.
All CRM versions from 2011 to 365 are supported, just include the one you need to your project.

# Purpose
QueryExpressions add nice IntelliSense, however they tend to be quite verbose, which leads to poor readability.
This fluent interface aims to make queries as short and readable as possible while preserving IntelliSense.

This could look something like this:
```C#
var records = service.Query("account")
                .IncludeColumns("name", "address1_line1")
		.With.UniqueRecords()
                .RetrieveAll();
```

# How to build it
If you want to build this library yourself, just call 

```PowerShell
.\build.cmd
```
