using IdGen;
using IdGen.DependencyInjection;
using IdGenTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IdGenTests;

[TestClass]
public class DependencyInjectionTests
{
    [TestMethod]
    public void DependencyInjection_Resolves_IdGenerator()
    {
        var serviceProvider = new ServiceCollection()
            .AddIdGen(123)
            .BuildServiceProvider();

        var idgenerator = serviceProvider.GetRequiredService<IdGenerator>();
        Assert.IsNotNull(idgenerator);

        var id = idgenerator.CreateId();
        var target = idgenerator.FromId(id);

        Assert.AreEqual(123, target.GeneratorId);
    }

    [TestMethod]
    public void DependencyInjection_Resolves_IIdGenerator()
    {
        var serviceProvider = new ServiceCollection()
            .AddIdGen(456)
            .BuildServiceProvider();

        var idgenerator = serviceProvider.GetRequiredService<IIdGenerator<long>>();
        Assert.IsNotNull(idgenerator);
    }

    [TestMethod]
    public void DependencyInjection_Resolves_Singleton()
    {
        var serviceProvider = new ServiceCollection()
            .AddIdGen(789)
            .AddIdGen(654)  // This should be a no-op
            .BuildServiceProvider();

        var idgen1 = serviceProvider.GetRequiredService<IIdGenerator<long>>();
        var idgen2 = serviceProvider.GetRequiredService<IdGenerator>();

        Assert.ReferenceEquals(idgen1, idgen2);
        Assert.AreEqual(789, idgen2.FromId(idgen1.CreateId()).GeneratorId);
        Assert.AreEqual(789, idgen2.FromId(idgen2.CreateId()).GeneratorId);
    }

    [TestMethod]
    public void DependencyInjection_AppliesOptions()
    {
        var epoch = new DateTimeOffset(2022, 5, 18, 0, 0, 0, TimeSpan.Zero);
        var idstruct = new IdStructure(39, 11, 13);
        var ts = new MockTimeSource(69, TimeSpan.FromMinutes(1), epoch);

        var serviceProvider = new ServiceCollection()
            .AddIdGen(420, () => new IdGeneratorOptions(idstruct, ts))
            .BuildServiceProvider();

        var idgen = serviceProvider.GetRequiredService<IdGenerator>();
        var id = idgen.FromId(idgen.CreateId());

        Assert.AreEqual(0, id.SequenceNumber);
        Assert.AreEqual(420, id.GeneratorId);
        Assert.AreEqual(epoch.AddMinutes(69), id.DateTimeOffset);
    }
}
