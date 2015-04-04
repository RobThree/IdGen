# ![Logo](https://raw.githubusercontent.com/RobThree/IdGen/master/IdGenDocumentation/icons/Help.png) IdGen
Twitter Snowflake-alike ID generator for .Net. Available as [Nuget package](https://www.nuget.org/packages/IdGen)

## How it works

`// TODO...`

An Id generated with a **Default** `MaskConfig` is structured as follows: 

![Id structure](https://raw.githubusercontent.com/RobThree/IdGen/master/IdGenDocumentation/Media/structure.png)

However, using the `MaskConfig` class you can tune the structure of the created Id's to your own needs; you can use 45 bits for the timestamp, 2 bits for the generator-id and 16 bits for the sequence to allow, for example, generating 65536 id's per millisecond distributed over 4 hosts/threads (e.g. generators) giving you a total of 262144 id's per millisecond. As long as all 3 parts (timestamp, generator and sequence) add up to 63 bits you're good to go!

## Getting started

Install the [Nuget package](https://www.nuget.org/packages/IdGen) and write the following code:

```c#
using IdGen;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        var generator = new IdGenerator(0);
        var id = generator.CreateId();
    }
}
```

Voila. You have created your first Id! Want to create 100 Id's? Instead of:

`var id = generator.CreateId();`

write:

`var id = generator.Take(100);`

This is because the `IdGenerator()` implements `IEnumerable` providing you with a never-ending stream of Id's (so you might want to be careful doing a `.Select(...)` on it!).

The above example creates a default `IdGenerator` with the GeneratorId (or: 'Worker Id') set to 0. If you're using multiple generators (across machines or in separate threads or...) you'll want to make sure each generator is assigned it's own unique Id. One way of doing this is by simply storing a value in your configuration file for example, another way may involve a service handing out GeneratorId's to machines/threads. IdGen doesn't provide a solution for this since each project or setup may have different requirements or infrastructure to provide these Id's.

`// TODO: more complicated example(s) demonstrating other setups and/or usage of parts of the library`

<hr>

[![Build status](https://ci.appveyor.com/api/projects/status/24wqqq91u0arkf5t)](https://ci.appveyor.com/project/RobIII/idgen) <a href="https://www.nuget.org/packages/IdGen/"><img src="http://img.shields.io/nuget/v/IdGen.svg?style=flat-square" alt="NuGet version" height="18"></a> <a href="https://www.nuget.org/packages/IdGen/"><img src="http://img.shields.io/nuget/dt/IdGen.svg?style=flat-square" alt="NuGet downloads" height="18"></a>

Icon made by [Freepik](http://www.flaticon.com/authors/freepik) from [www.flaticon.com](http://www.flaticon.com) is licensed by [CC 3.0](http://creativecommons.org/licenses/by/3.0/).
