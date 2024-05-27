# ![Logo](https://raw.githubusercontent.com/RobThree/IdGen/master/logo.png) IdGen

![Build Status](https://img.shields.io/github/actions/workflow/status/RobThree/IdGen/test.yml?branch=master&style=flat-square) [![Nuget version](https://img.shields.io/nuget/v/IdGen.svg?style=flat-square)](https://www.nuget.org/packages/IdGen/)

Twitter Snowflake-alike ID generator for .Net. Available as [Nuget package](https://www.nuget.org/packages/IdGen)

## Why

In certain situations you need a low-latency, distributed, uncoordinated, (roughly) time ordered, compact and highly available Id generation system. This project was inspired by [Twitter's Snowflake](https://github.com/twitter/snowflake) project which has been retired. Note that this project was inspired by Snowflake but is not an *exact* implementation. This library provides a basis for Id generation; it does **not** provide a service for handing out these Id's nor does it provide generator-id ('worker-id') coordination.

## How it works

IdGen generates, like Snowflake, 64 bit Id's. The [Sign Bit](https://en.wikipedia.org/wiki/Sign_bit) is unused since this can cause incorrect ordering on some systems that cannot use unsigned types and/or make it hard to get correct ordering. So, in effect, IdGen generates 63 bit Id's. An Id consists of 3 parts:

* Timestamp
* Generator-id
* Sequence 

An Id generated with a **Default** `IdStructure` is structured as follows: 

![Id structure](https://raw.githubusercontent.com/RobThree/IdGen/master/structure.png)

However, using the `IdStructure` class you can tune the structure of the created Id's to your own needs; you can use 45 bits for the timestamp, 2 bits for the generator-id and 16 bits for the sequence if you prefer. As long as all 3 parts (timestamp, generator and sequence) add up to 63 bits you're good to go!

The **timestamp**-part of the Id should speak for itself; by default this is incremented every millisecond and represents the number of milliseconds since a certain epoch. However, IdGen relies on an [`ITimeSource`](IdGen/ITimeSource.cs) which uses a 'tick' that can be defined to be anything; be it a millisecond (default), a second or even a day or nanosecond (hardware support etc. permitting). By default IdGen uses 2015-01-01 0:00:00Z as epoch, but you can specify a custom epoch too. 

The **generator-id**-part of the Id is the part that you 'configure'; it could correspond to a host, thread, datacenter or continent: it's up to you. However, the generator-id should be unique in the system: if you have several hosts or threads generating Id's, each host or thread should have it's own generator-id. This could be based on the hostname, a config-file value or even be retrieved from an coordinating service. Remember: a generator-id should be unique within the entire system to avoid collisions!

The **sequence**-part is simply a value that is incremented each time a new Id is generated within the same tick (again, by default, a millisecond but can be anything); it is reset every time the tick changes.

## System Clock Dependency

We recommend you use NTP to keep your system clock accurate. IdGen protects from non-monotonic clocks, i.e. clocks that run backwards. The [`DefaultTimeSource`](IdGen/DefaultTimeSource.cs) relies on a 64bit monotonic, increasing only, system counter. However, we still recommend you use NTP to keep your system clock accurate; this will prevent duplicate Id's between system restarts for example.

The [`DefaultTimeSource`](IdGen/DefaultTimeSource.cs) relies on a [`Stopwatch`](https://msdn.microsoft.com/en-us/library/system.diagnostics.stopwatch.aspx) for calculating the 'ticks' but you can implement your own time source by simply implementing the [`ITimeSource`](IdGen/ITimeSource.cs) interface.


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
        // Example id: 862817670527975424
    }
}
```

Voila. You have created your first Id! Want to create 100 Id's? Instead of:

`var id = generator.CreateId();`

write:

`var id = generator.Take(100);`

This is because the `IdGenerator()` implements `IEnumerable` providing you with a never-ending stream of Id's (so you might want to be careful doing a `.Select(...)` or `Count()` on it!).

The above example creates a default `IdGenerator` with the GeneratorId (or: 'Worker Id') set to 0 and using a [`DefaultTimeSource`](IdGen/DefaultTimeSource.cs). If you're using multiple generators (across machines or in separate threads or...) you'll want to make sure each generator is assigned it's own unique Id. One way of doing this is by simply storing a value in your configuration file for example, another way may involve a service handing out GeneratorId's to machines/threads. IdGen **does not** provide a solution for this since each project or setup may have different requirements or infrastructure to provide these generator-id's.

The below sample is a bit more complicated; we set a custom epoch, define our own id-structure for generated Id's and then display some information about the setup:

```c#
using IdGen;
using System;

class Program
{
    static void Main(string[] args)
    {
        // Let's say we take april 1st 2020 as our epoch
        var epoch = new DateTime(2020, 4, 1, 0, 0, 0, DateTimeKind.Utc);
            
        // Create an ID with 45 bits for timestamp, 2 for generator-id 
        // and 16 for sequence
        var structure = new IdStructure(45, 2, 16);
            
        // Prepare options
        var options = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch));
            
        // Create an IdGenerator with it's generator-id set to 0, our custom epoch 
        // and id-structure
        var generator = new IdGenerator(0, options);

        // Let's ask the id-structure how many generators we could instantiate 
        // in this setup (2 bits)
        Console.WriteLine("Max. generators       : {0}", structure.MaxGenerators);

        // Let's ask the id-structure how many sequential Id's we could generate 
        // in a single ms in this setup (16 bits)
        Console.WriteLine("Id's/ms per generator : {0}", structure.MaxSequenceIds);

        // Let's calculate the number of Id's we could generate, per ms, should we use
        // the maximum number of generators
        Console.WriteLine("Id's/ms total         : {0}", structure.MaxGenerators * structure.MaxSequenceIds);


        // Let's ask the id-structure configuration for how long we could generate Id's before
        // we experience a 'wraparound' of the timestamp
        Console.WriteLine("Wraparound interval   : {0}", structure.WraparoundInterval(generator.Options.TimeSource));

        // And finally: let's ask the id-structure when this wraparound will happen
        // (we'll have to tell it the generator's epoch)
        Console.WriteLine("Wraparound date       : {0}", structure.WraparoundDate(generator.Options.TimeSource.Epoch, generator.Options.TimeSource).ToString("O"));
    }
}
```

Output:
```
Max. generators       : 4
Id's/ms per generator : 65536
Id's/ms total         : 262144
Wraparound interval   : 407226.12:41:28.8320000 (about 1114 years)
Wraparound date       : 3135-03-14T12:41:28.8320000+00:00
```

IdGen also provides an `ITimeSouce` interface; this can be handy for [unittesting](IdGenTests/IdGeneratorTests.cs) purposes or if you want to provide a time-source for the timestamp part of your Id's that is not based on the system time. For unittesting we use our own [`MockTimeSource`](IdGenTests/Mocks/MockTimeSource.cs).

```xml
<configuration>
  <configSections>
    <section name="idGenSection" type="IdGen.Configuration.IdGeneratorsSection, IdGen.Configuration" />
  </configSections>

  <idGenSection>
    <idGenerators>
      <idGenerator name="foo" id="123"  epoch="2016-01-02T12:34:56" timestampBits="39" generatorIdBits="11" sequenceBits="13" tickDuration="0:00:00.001" />
      <idGenerator name="bar" id="987"  epoch="2016-02-01 01:23:45" timestampBits="20" generatorIdBits="21" sequenceBits="22" />
      <idGenerator name="baz" id="2047" epoch="2016-02-29"          timestampBits="21" generatorIdBits="21" sequenceBits="21" sequenceOverflowStrategy="SpinWait" />
    </idGenerators>
  </idGenSection>

</configuration>
```

The attributes (`name`, `id`, `epoch`, `timestampBits`, `generatorIdBits` and `sequenceBits`) are required. The `tickDuration` is optional and defaults to the default tickduration from a `DefaultTimeSource`. The `sequenceOverflowStrategy` is optional too and defaults to `Throw`. Valid DateTime notations for the epoch are:

* `yyyy-MM-ddTHH:mm:ss`
* `yyyy-MM-dd HH:mm:ss`
* `yyyy-MM-dd`

You can get the IdGenerator from the config using the following code:

`var generator = AppConfigFactory.GetFromConfig("foo");`

## Dependency Injection

There is an [IdGen.DependencyInjection NuGet package](https://www.nuget.org/packages/IdGen.DependencyInjection) available that allows for easy integration with the commonly used [Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection).

Usage is straightforward:

```c#
services.AddIdGen(123); // Where 123 is the generator-id
```

Or, when you want to use non-default options:

```c#
services.AddIdGen(123, () => new IdGeneratorOptions(...));  // Where 123 is the generator-id
```

This registers both an `IdGenerator` as well as an `IIdGenerator<long>`, both pointing to the same singleton generator.

## Upgrading from 2.x to 3.x

Upgrading from 2.x to 3.x should be pretty straightforward. The following things have changed:

* Most of the constructor overloads for the `IdGenerator` have been replaced with a single constructor which accepts `IdGeneratorOptions` that contains the `ITimeSource`, `IdStructure` and `SequenceOverflowStrategy`
* The `MaskConfig` class is now more appropriately named `IdStructure` since it describes the structure of the generated ID's.
* The `UseSpinWait` property has moved to the `IdGeneratorOptions` and is now an enum of type `SequenceOverflowStrategy` instead of a boolean value. Note that this property has also been renamed in the config file (from `useSpinWait` to `sequenceOverflowStrategy`) and is no longer a boolean but requires one of the values from `SequenceOverflowStrategy`.
* `ID` is now `Id` (only used as return value by the `FromId()` method)

The generated 2.x ID's are still compatible with 3.x ID's. This release is mostly better and more consistent naming of objects.

# FAQ

**Q**: Help, I'm getting duplicate ID's or collisions?

**A**: Then you're probably not using IdGen as intended: It should be a singleton (per thread/process/host/...), and if you insist on having multiple instances around they should all have their own unique GeneratorId.

**A**: Also: Don't change the structure; once you've picked an `IdStructure` and go into production commit to it, stick with it. This means that careful planning is needed to ensure enough ID's can be generated by enough generators for long enough. Although changing the structure at a later stage isn't impossible, careful consideration is needed to ensure no collisions will occur.

**Q**: I'm experiencing weird results when these ID's are used in Javascript?

**A**: Remember that generated ID's are 64 (actually 63) bits wide. Javascript uses floats to store all numbers and the [maximum integer value you can safely store](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/MAX_SAFE_INTEGER) is 53 bits. If you need to handle these ID's in Javascript, treat them as `strings`.

<hr>

Icon made by [Freepik](http://www.flaticon.com/authors/freepik) from [www.flaticon.com](http://www.flaticon.com) is licensed by [CC 3.0](http://creativecommons.org/licenses/by/3.0/).
