# ![Logo](https://raw.githubusercontent.com/RobThree/IdGen/master/IdGenDocumentation/icons/Help.png) IdGen
Twitter Snowflake-alike ID generator for .Net. Available as [Nuget package](https://www.nuget.org/packages/IdGen)

## Why

In certain situations you need a low-latency uncoordinated, (roughly) time ordered, compact and highly available Id generation system. This project was inspired by [Twitter's Snowflake](https://github.com/twitter/snowflake) project which has been retired. Note that this project was inspired by Snowflake but is not an *exact* implementation. This library provides a basis for Id generation; it does **not** provide a service for handing out these Id's nor does it provide generator-id ('worker-id') coordination.

## How it works

IdGen generates, like Snowflake, 64 bit Id's. The [Sign Bit](https://en.wikipedia.org/wiki/Sign_bit) is unused since this can cause incorrect ordering on some systems that cannot use unsigned types and/or make it hard to get correct ordering. So, in effect, IdGen generates 63 bit Id's. An Id consists of 3 parts:

* Timestamp
* Generator-id
* Sequence 

An Id generated with a **Default** `MaskConfig` is structured as follows: 

![Id structure](https://raw.githubusercontent.com/RobThree/IdGen/master/IdGenDocumentation/Media/structure.png)

However, using the `MaskConfig` class you can tune the structure of the created Id's to your own needs; you can use 45 bits for the timestamp (≈1114 years), 2 bits for the generator-id and 16 bits for the sequence to allow, for example, generating 65536 id's per millisecond per generator distributed over 4 hosts/threads giving you a total of 262144 id's per millisecond. As long as all 3 parts (timestamp, generator and sequence) add up to 63 bits you're good to go!

The **timestamp**-part of the Id should speak for itself; this is incremented every millisecond and represents the number of milliseconds since a certain epoch. By default IdGen uses 2015-01-01 0:00:00Z as epoch, but you can specify a custom epoch.

The **generator-id**-part of the Id is the part that you 'configure'; it could correspond to a host, thread, datacenter or continent: it's up to you. However, the generator-id should be unique in the system: if you have several hosts generating Id's, each host should have it's own generator-id. This could be based on the hostname, a config-file value or even be retrieved from an coordinating service. Remember: a generator-id should be unique within the entire system to avoid collisions!

The **sequence**-part is simply a value that is incremented each time a new Id is generated within the same millisecond timespan; it is reset every time the timestamp changes. Speaking of this:

## System Clock Dependency

IdGen protects from non-monotonic clocks, i.e. clocks that run backwards. The [DefaultTimeSource](https://github.com/RobThree/IdGen/blob/master/IdGen/DefaultTimeSource.cs) relies on a [64bit system counter](https://msdn.microsoft.com/en-us/library/windows/desktop/ms644904.aspx) that is only incremented<sup>[1](#note1)</sup>. However, we still recommend you use NTP to keep your system clock accurate; this will prevent duplicate Id's between system restarts.

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

The above example creates a default `IdGenerator` with the GeneratorId (or: 'Worker Id') set to 0. If you're using multiple generators (across machines or in separate threads or...) you'll want to make sure each generator is assigned it's own unique Id. One way of doing this is by simply storing a value in your configuration file for example, another way may involve a service handing out GeneratorId's to machines/threads. IdGen **does not** provide a solution for this since each project or setup may have different requirements or infrastructure to provide these generator-id's.

The below sample is a bit more complicated; we set a custom epoch, define our own (bit)mask configuration for generated Id's and then display some information about the setup:

```c#
using IdGen;
using System;

class Program
{
    static void Main(string[] args)
    {
        // Let's say we take april 1st 2015 as our epoch
        var epoch = new DateTime(2015, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        // Create a mask configuration of 45 bits for timestamp, 2 for generator-id 
        // and 16 for sequence
        var mc = new MaskConfig(45, 2, 16);
        // Create an IdGenerator with it's generator-id set to 0, our custom epoch 
        // and mask configuration
        var generator = new IdGenerator(0, epoch, mc);

        // Let's ask the mask configuration how many generators we could instantiate 
        // in this setup (2 bits)
        Console.WriteLine("Max. generators       : {0}", mc.MaxGenerators);

        // Let's ask the mask configuration how many sequential Id's we could generate 
        // in a single ms in this setup (16 bits)
        Console.WriteLine("Id's/ms per generator : {0}", mc.MaxSequenceIds);

        // Let's calculate the number of Id's we could generate, per ms, should we use
        // the maximum number of generators
        Console.WriteLine("Id's/ms total         : {0}", mc.MaxGenerators * mc.MaxSequenceIds);

        // Let's ask the mask configuration for how long we could generate Id's before
        // we experience a 'wraparound' of the timestamp
        Console.WriteLine("Wraparound interval   : {0}", mc.WraparoundInterval());

        // And finally: let's ask the mask configuration when this wraparound will happen
        // (we'll have to tell it the generator's epoch)
        Console.WriteLine("Wraparound date       : {0}", mc.WraparoundDate(generator.Epoch).ToString("O"));
    }
}
```

Output:
```
Max. generators       : 4
Id's/ms per generator : 65536
Id's/ms total         : 262144
Wraparound interval   : 407226.12:41:28.8320000 (about 1114 years)
Wraparound date       : 3130-03-13T12:41:28.8320000Z
```

IdGen also provides an `ITimeSouce` interface; this can be handy for [unittesting](IdGenTests/IdGenTests.cs) purposes or if you want to provide a time-source for the timestamp part of your Id's that is not based on the system time. By default the IdGenerator uses the `DefaultTimeSource` which, internally, uses [QueryPerformanceCounter](https://msdn.microsoft.com/en-us/library/windows/desktop/ms644904.aspx). For unittesting we use our own [MockTimeSource](IdGenTests/MockTimeSource.cs).

The following constructor overloads are available:

```c#
IdGenerator(int generatorId)
IdGenerator(int generatorId, DateTime epoch)
IdGenerator(int generatorId, DateTime epoch, MaskConfig maskConfig)
IdGenerator(int generatorId, DateTime epoch, MaskConfig maskConfig, ITimeSource timeSource)
```

All properties are read-only to prevent changes once an `IdGenerator` has been instantiated.

The `IdGenerator` class provides three 'factory methods' to quickly create a machine-specific (based on the hostname), thread-specific or based on `app.config` (or `web.config`) `IdGenerator`:

`var generator = IdGenerator.CreateMachineSpecificGenerator();`

or:

`var generator = IdGenerator.CreateThreadSpecificGenerator();`

These methods (and their overloads that allow you to specify the epoch, `MaskConfig` and `TimeSource`) create an `IdGenerator` based on hostname or (managed) thread-id. However, it is recommended you explicitly set / configure a generator-id since these two methods could cause 'collisions' when machinenames' hashes result in the same id's or when thread-id's collide with thread-id's on other machines.

Then we have `GetFromConfig("name")`; it differs in a subtle way (besides using values from the configuration file) with the previous two methods: `CreateMachineSpecificGenerator` and `CreateThreadSpecificGenerator` both create a new instance every call whereas `GetFromConfig(string)` returns the same instance with the same name every time. You can add the following to your configuration:

```xml
<configuration>
  <configSections>
    <section name="idGenSection" type="IdGen.Configuration.IdGeneratorsSection, IdGen" />
  </configSections>

  <idGenSection>
    <idGenerators>
      <idGenerator name="foo" id="123"  epoch="2016-01-02T12:34:56" timestampBits="39" generatorIdBits="11" sequenceBits="13" />
      <idGenerator name="bar" id="987"  epoch="2016-02-01 01:23:45" timestampBits="20" generatorIdBits="21" sequenceBits="22" />
      <idGenerator name="baz" id="2047" epoch="2016-02-29"          timestampBits="21" generatorIdBits="21" sequenceBits="21" />
    </idGenerators>
  </idGenSection>
  
</configuration>
```

All attributes (`name`, `id`, `epoch`, `timestampBits`, `generatorIdBits` and `sequenceBits`) are required. Valid DateTime notations for the epoch are:

* `yyyy-MM-ddTHH:mm:ss`
* `yyyy-MM-dd HH:mm:ss`
* `yyyy-MM-dd`

You can get the IdGenerator from the config using the following code:

`var generator = IdGenerator.GetFromConfig("foo");`

<hr>

[![Build status](https://ci.appveyor.com/api/projects/status/24wqqq91u0arkf5t)](https://ci.appveyor.com/project/RobIII/idgen) <a href="https://www.nuget.org/packages/IdGen/"><img src="http://img.shields.io/nuget/v/IdGen.svg?style=flat-square" alt="NuGet version" height="18"></a> <a href="https://www.nuget.org/packages/IdGen/"><img src="http://img.shields.io/nuget/dt/IdGen.svg?style=flat-square" alt="NuGet downloads" height="18"></a>

Icon made by [Freepik](http://www.flaticon.com/authors/freepik) from [www.flaticon.com](http://www.flaticon.com) is licensed by [CC 3.0](http://creativecommons.org/licenses/by/3.0/).

<sup><a name="note1">1</a></sup> It is possible for this counter to ['leap' sometimes](https://support.microsoft.com/en-us/kb/274323/en-gb); however this shouldn't be a problem for generating Id's.
