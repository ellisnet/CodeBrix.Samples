// RegisterServicesTests.cs - the one line the application calls at startup: AddReading() must
// hand back all three services, as singletons, and must be safe to call twice.

using System;
using System.Linq;
using InannaRosette.Reading.Services;
using Microsoft.Extensions.DependencyInjection;
using SilverAssertions;
using Xunit;

namespace InannaRosette.Reading.Tests;

public class RegisterServicesTests
{
    [Fact]
    public void AddReading_registers_the_interpreter_the_serializer_and_the_report_builder()
    {
        //Arrange
        var services = new ServiceCollection();

        //Act
        services.AddReading();
        using var provider = services.BuildServiceProvider();

        //Assert
        provider.GetService<IReadingInterpreter>().Should().NotBeNull();
        provider.GetService<IReadingSerializer>().Should().NotBeNull();
        provider.GetService<IPdfReportBuilder>().Should().NotBeNull();
    }

    [Fact]
    public void AddReading_registers_the_concrete_types_the_library_ships()
    {
        //Arrange
        var services = new ServiceCollection();

        //Act
        services.AddReading();
        using var provider = services.BuildServiceProvider();

        //Assert
        provider.GetService<IReadingInterpreter>().Should().BeOfType<ReadingInterpreter>();
        provider.GetService<IReadingSerializer>().Should().BeOfType<ReadingSerializer>();
        provider.GetService<IPdfReportBuilder>().Should().BeOfType<PdfReportBuilder>();
    }

    [Fact]
    public void AddReading_registers_the_interpreter_as_a_singleton()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddReading();
        using var provider = services.BuildServiceProvider();

        //Act
        var first = provider.GetService<IReadingInterpreter>();
        var second = provider.GetService<IReadingInterpreter>();

        //Assert
        second.Should().BeSameAs(first);
    }

    [Fact]
    public void AddReading_registers_the_serializer_as_a_singleton()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddReading();
        using var provider = services.BuildServiceProvider();

        //Act
        var first = provider.GetService<IReadingSerializer>();
        var second = provider.GetService<IReadingSerializer>();

        //Assert
        second.Should().BeSameAs(first);
    }

    [Fact]
    public void AddReading_registers_the_report_builder_as_a_singleton()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddReading();
        using var provider = services.BuildServiceProvider();

        //Act
        var first = provider.GetService<IPdfReportBuilder>();
        var second = provider.GetService<IPdfReportBuilder>();

        //Assert
        second.Should().BeSameAs(first);
    }

    [Fact]
    public void AddReading_declares_all_three_registrations_as_singleton_lifetimes()
    {
        //Arrange
        var services = new ServiceCollection();

        //Act
        services.AddReading();

        //Assert
        services.Should().AllSatisfy(d => d.Lifetime.Should().Be(ServiceLifetime.Singleton));
    }

    [Fact]
    public void AddReading_adds_exactly_three_registrations()
    {
        //Arrange
        var services = new ServiceCollection();

        //Act
        services.AddReading();

        //Assert
        services.Should().HaveCount(3);
    }

    [Fact]
    public void calling_AddReading_twice_does_not_duplicate_the_registrations()
    {
        //Arrange
        var services = new ServiceCollection();

        //Act
        services.AddReading();
        services.AddReading();

        //Assert
        services.Should().HaveCount(3);
        services.Count(d => d.ServiceType == typeof(IReadingInterpreter)).Should().Be(1);
        services.Count(d => d.ServiceType == typeof(IReadingSerializer)).Should().Be(1);
        services.Count(d => d.ServiceType == typeof(IPdfReportBuilder)).Should().Be(1);
    }

    [Fact]
    public void calling_AddReading_twice_still_resolves_one_instance_of_each()
    {
        //Arrange
        var services = new ServiceCollection();

        //Act
        services.AddReading();
        services.AddReading();
        using var provider = services.BuildServiceProvider();

        //Assert
        provider.GetServices<IReadingInterpreter>().Should().HaveCount(1);
        provider.GetServices<IReadingSerializer>().Should().HaveCount(1);
        provider.GetServices<IPdfReportBuilder>().Should().HaveCount(1);
    }

    [Fact]
    public void AddReading_leaves_a_registration_the_application_made_itself()
    {
        //Arrange
        var services = new ServiceCollection();
        var ownInterpreter = new ReadingInterpreter();
        services.AddSingleton<IReadingInterpreter>(ownInterpreter);

        //Act
        services.AddReading();
        using var provider = services.BuildServiceProvider();

        //Assert
        provider.GetService<IReadingInterpreter>().Should().BeSameAs(ownInterpreter);
    }

    [Fact]
    public void AddReading_returns_the_collection_so_the_call_can_be_chained()
    {
        //Arrange
        var services = new ServiceCollection();

        //Act
        var returned = services.AddReading();

        //Assert
        returned.Should().BeSameAs(services);
    }

    [Fact]
    public void AddReading_rejects_a_missing_service_collection()
    {
        //Arrange
        IServiceCollection services = null;

        //Act
        Action act = () => services.AddReading();

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void the_resolved_services_actually_work_end_to_end()
    {
        //Arrange - exactly the way the view model uses them
        var services = new ServiceCollection();
        services.AddReading();
        using var provider = services.BuildServiceProvider();

        //Act
        var interpretation = provider.GetService<IReadingInterpreter>().Interpret(TestData.FullReading());
        var json = provider.GetService<IReadingSerializer>().ToJson(interpretation.Reading);
        var pdf = provider.GetService<IPdfReportBuilder>().Build(interpretation);

        //Assert
        interpretation.Positions.Should().HaveCount(9);
        json.Should().Contain("\"placements\"");
        pdf.Length.Should().BeGreaterThan(20_000);
    }
}
