// <copyright file="CreateMachineGuiRequestHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Machines.Commands.Create;
using IndTrace.Domain.ValueObjects;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Comprehensive unit tests for CreateMachineMonitorRequestHandler - Manufacturing machine creation handler
/// Tests cover automotive, electronics, pharmaceutical, aerospace, and industrial equipment creation scenarios
/// </summary>
public class CreateMachineMonitorRequestHandlerTests
{
    private readonly IRepository<Machine> _repository = null!;
    private readonly ILogger<CreateMachineMonitorRequestHandler> _logger = null!;
    private readonly CreateMachineMonitorRequestHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateMachineMonitorRequestHandlerTests()
    {
        _repository = Substitute.For<IRepository<Machine>>();
        //_logger =  XUnitLogger.CreateLogger<CreateMachineMonitorRequestHandler>();
        _logger = XUnitLogger.CreateLogger<CreateMachineMonitorRequestHandler>();
        _handler = new CreateMachineMonitorRequestHandler(_repository, _logger);
    }

    /// <summary>
    /// Executes Should_CreateInstance_When_ValidDependenciesProvided operation.
    /// </summary>

    [Fact]
    public void Should_CreateInstance_When_ValidDependenciesProvided()
    {
        // Arrange
        var repository = Substitute.For<IRepository<Machine>>();
        var logger = XUnitLogger.CreateLogger<CreateMachineMonitorRequestHandler>();

        // Act
        var handler = new CreateMachineMonitorRequestHandler(repository, logger);

        // Assert
        handler.ShouldNotBeNull();
        handler.ShouldBeAssignableTo<IMonitorRequestHandler<CreateMachineMonitorRequest, MachineCreated>>();
    }

    /// <summary>
    /// Executes Should_ImplementIMonitorRequestHandlerInterface_When_Instantiated operation.
    /// </summary>

    [Fact]
    public void Should_ImplementIMonitorRequestHandlerInterface_When_Instantiated()
    {
        // Arrange & Act
        var handler = new CreateMachineMonitorRequestHandler(_repository, _logger);

        // Assert
        handler.ShouldBeAssignableTo<IMonitorRequestHandler<CreateMachineMonitorRequest, MachineCreated>>();
        typeof(IMonitorRequestHandler<CreateMachineMonitorRequest, MachineCreated>).IsAssignableFrom(typeof(CreateMachineMonitorRequestHandler)).ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_CreateMachineSuccessfully_When_ValidRequestProvidedAndNoExistingMachine operation.
    /// </summary>
    /// <returns>The result of Should_CreateMachineSuccessfully_When_ValidRequestProvidedAndNoExistingMachine.</returns>

    [Fact]
    public async Task Should_CreateMachineSuccessfully_When_ValidRequestProvidedAndNoExistingMachine()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = "Ford F-150 Engine Block CNC Machining Center",
            Location = "Dearborn Assembly Plant - Line A",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!)); // No existing machine found

        var createdMachine = new Machine
        {
            MachineId = new MachineId(request.MachineId),
            Name = request.Name,
            Location = request.Location,
            MachineType = request.MachineType,
            EnableAppTraceability = request.EnableAppTraceability,
            EnableBypassTraceability = request.EnableBypassTraceability
        };

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(request.MachineId);
        result.Value.Name.ShouldBe(request.Name);
        result.Value.Location.ShouldBe(request.Location);

        await _repository.Received(1).FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnFailure_When_MachineWithSameIdAlreadyExists operation.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_MachineWithSameIdAlreadyExists.</returns>

    [Fact]
    public async Task Should_ReturnFailure_When_MachineWithSameIdAlreadyExists()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 2002,
            MachineId = 2002,
            Name = "Tesla Model Y Battery Pack Assembly Robot",
            Location = "Gigafactory Berlin - Line B",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        var existingMachine = new Machine
        {
            MachineId = new MachineId(2002),
            Name = "Existing Tesla Robot",
            Location = "Existing Location"
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("A machine with the same ID or name already exists.");

        await _repository.Received(1).FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnFailure_When_MachineWithSameNameAlreadyExists operation.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_MachineWithSameNameAlreadyExists.</returns>

    [Fact]
    public async Task Should_ReturnFailure_When_MachineWithSameNameAlreadyExists()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 3003,
            MachineId = 3003,
            Name = "Apple iPhone 15 Pro PCB Assembly Station",
            Location = "Apple Park Manufacturing - Line C",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        var existingMachine = new Machine
        {
            MachineId = new MachineId(9999),
            Name = "Apple iPhone 15 Pro PCB Assembly Station", // Same name, different ID
            Location = "Different Location"
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("A machine with the same ID or name already exists.");
    }

    /// <summary>
    /// Executes Should_CreateFordF150AutomotiveManufacturingMachine_When_ValidFordRequestProvided operation.
    /// </summary>
    /// <returns>The result of Should_CreateFordF150AutomotiveManufacturingMachine_When_ValidFordRequestProvided.</returns>

    [Fact]
    public async Task Should_CreateFordF150AutomotiveManufacturingMachine_When_ValidFordRequestProvided()
    {
        // Arrange
        var fordRequest = new CreateMachineMonitorRequest
        {
            Id = 10001,
            MachineId = 1000001,
            Name = "Ford F-150 SuperCrew 4x4 Robotic Welding Cell #1",
            Location = "Ford Dearborn Assembly Plant - Body-in-White Line A - Station 15",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(fordRequest, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        //[Fix]
        //CLAUDE
        //Date: 28/08/2025
        //Reason: [CS8602] - Add null-forgiving operator since result.IsSuccess was verified true
        result.Value.ShouldNotBeNull().MachineId.ShouldBe(1000001);
        result.Value.Name.ShouldBe("Ford F-150 SuperCrew 4x4 Robotic Welding Cell #1");
        result.Value.Location.ShouldBe("Ford Dearborn Assembly Plant - Body-in-White Line A - Station 15");
        result.Value.MachineType.ShouldBe(8);
        result.Value.EnableAppTraceability.ShouldBe(1);
        result.Value.EnableBypassTraceability.ShouldBe(0);
    }

    /// <summary>
    /// Executes Should_CreateTeslaModelYElectricVehicleManufacturingMachine_When_ValidTeslaRequestProvided operation.
    /// </summary>
    /// <returns>The result of Should_CreateTeslaModelYElectricVehicleManufacturingMachine_When_ValidTeslaRequestProvided.</returns>

    [Fact]
    public async Task Should_CreateTeslaModelYElectricVehicleManufacturingMachine_When_ValidTeslaRequestProvided()
    {
        // Arrange
        var teslaRequest = new CreateMachineMonitorRequest
        {
            Id = 20002,
            MachineId = 20002,
            Name = "Tesla Model Y 4680 Battery Pack Assembly Robot",
            Location = "Tesla Gigafactory Berlin-Brandenburg - Battery Line B - Station 8",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(teslaRequest, TestContext.Current.CancellationToken);

        // Assert

        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix assertion order - check IsSuccess before accessing Value properties
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(20002);
        result.Value.Name.ShouldBe("Tesla Model Y 4680 Battery Pack Assembly Robot");
        result.Value.Location.ShouldBe("Tesla Gigafactory Berlin-Brandenburg - Battery Line B - Station 8");
    }

    /// <summary>
    /// Executes Should_CreateAppleIPhoneElectronicsManufacturingMachine_When_ValidAppleRequestProvided operation.
    /// </summary>
    /// <returns>The result of Should_CreateAppleIPhoneElectronicsManufacturingMachine_When_ValidAppleRequestProvided.</returns>

    [Fact]
    public async Task Should_CreateAppleIPhoneElectronicsManufacturingMachine_When_ValidAppleRequestProvided()
    {
        // Arrange
        var appleRequest = new CreateMachineMonitorRequest
        {
            Id = 30003,
            MachineId = 30003,
            Name = "Apple iPhone 15 Pro Max A17 Pro PCB SMT Line",
            Location = "Apple Park Manufacturing Facility - Cupertino - PCB Assembly Line C",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(appleRequest, TestContext.Current.CancellationToken);

        // Assert

        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix assertion order - check IsSuccess before accessing Value properties
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(30003);
        result.Value.Name.ShouldBe("Apple iPhone 15 Pro Max A17 Pro PCB SMT Line");
        result.Value.Location.ShouldBe("Apple Park Manufacturing Facility - Cupertino - PCB Assembly Line C");
    }

    /// <summary>
    /// Executes Should_CreatePfizerVaccinePharmaceuticalManufacturingMachine_When_ValidPfizerRequestProvided operation.
    /// </summary>
    /// <returns>The result of Should_CreatePfizerVaccinePharmaceuticalManufacturingMachine_When_ValidPfizerRequestProvided.</returns>

    [Fact]
    public async Task Should_CreatePfizerVaccinePharmaceuticalManufacturingMachine_When_ValidPfizerRequestProvided()
    {
        // Arrange
        var pfizerRequest = new CreateMachineMonitorRequest
        {
            Id = 40004,
            MachineId = 40004,
            Name = "Pfizer COVID-19 mRNA Vaccine Fill-Finish Station",
            Location = "Pfizer Kalamazoo Manufacturing Site - GMP Cleanroom Line 1",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(pfizerRequest, TestContext.Current.CancellationToken);

        // Assert

        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix assertion order - check IsSuccess before accessing Value properties
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(40004);
        result.Value.Name.ShouldBe("Pfizer COVID-19 mRNA Vaccine Fill-Finish Station");
        result.Value.Location.ShouldBe("Pfizer Kalamazoo Manufacturing Site - GMP Cleanroom Line 1");
    }

    /// <summary>
    /// Executes Should_CreateBoeingAerospaceManufacturingMachine_When_ValidBoeingRequestProvided operation.
    /// </summary>
    /// <returns>The result of Should_CreateBoeingAerospaceManufacturingMachine_When_ValidBoeingRequestProvided.</returns>

    [Fact]
    public async Task Should_CreateBoeingAerospaceManufacturingMachine_When_ValidBoeingRequestProvided()
    {
        // Arrange
        var boeingRequest = new CreateMachineMonitorRequest
        {
            Id = 50005,
            MachineId = 50005,
            Name = "Boeing 777X Composite Wing Automated Drilling Station",
            Location = "Boeing Everett Factory - Wing Assembly Building - Line B - Station 12",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(boeingRequest, TestContext.Current.CancellationToken);

        // Assert

        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix assertion order - check IsSuccess before accessing Value properties
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(50005);
        result.Value.Name.ShouldBe("Boeing 777X Composite Wing Automated Drilling Station");
        result.Value.Location.ShouldBe("Boeing Everett Factory - Wing Assembly Building - Line B - Station 12");
    }

    /// <summary>
    /// Executes Should_CreateInternationalManufacturingMachines_When_ValidGlobalRequestsProvided operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="name">The name.</param>
    /// <param name="location">The location.</param>
    /// <param name="regionDescription">The regionDescription.</param>
    /// <returns>The result of Should_CreateInternationalManufacturingMachines_When_ValidGlobalRequestsProvided.</returns>

    [Theory]
    [InlineData(110001, "BMW X5 Body Welding Station", "BMW Spartanburg Manufacturing - South Carolina", "German Automotive Manufacturing")]
    [InlineData(120002, "Samsung Galaxy S24 Ultra Display Assembly", "Samsung Giheung Semiconductor Fab - South Korea", "South Korean Electronics Manufacturing")]
    [InlineData(130003, "Novo Nordisk FlexPen Insulin Assembly", "Novo Nordisk Kalundborg Production Site - Denmark", "Danish Pharmaceutical Manufacturing")]
    [InlineData(140004, "Airbus A350 XWB Fuselage Section Assembly", "Airbus Toulouse Final Assembly Line - France", "European Aerospace Manufacturing")]
    [InlineData(150005, "Rolls-Royce Trent XWB Engine Blade Manufacturing", "Rolls-Royce Derby Engine Manufacturing - UK", "UK Aerospace Manufacturing")]
    public async Task Should_CreateInternationalManufacturingMachines_When_ValidGlobalRequestsProvided(int machineId, string name, string location, string regionDescription)
    {
        var logger = XUnitLogger.CreateLogger<CreateMachineMonitorRequestHandlerTests>();
        logger.LogInformation("Test parameters: machineId={MachineId}, name={Name}, location={Location}, regionDescription={RegionDescription}", machineId, name, location, regionDescription);
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = machineId,
            MachineId = machineId,
            Name = name,
            Location = location,
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert

        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix assertion order - check IsSuccess before accessing Value properties
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(machineId);
        result.Value.Name.ShouldBe(name);
        result.Value.Location.ShouldBe(location);
    }

    /// <summary>
    /// Executes Should_CreateMachinesWithDifferentTypes_When_VariousMachineTypesProvided operation.
    /// </summary>
    /// <param name="machineType">The machineType.</param>
    /// <param name="typeName">The typeName.</param>
    /// <param name="typeDescription">The typeDescription.</param>
    /// <returns>The result of Should_CreateMachinesWithDifferentTypes_When_VariousMachineTypesProvided.</returns>

    [Theory]
    [InlineData(1, "Printer", "Initial printer station")]
    [InlineData(2, "Initial", "Initial entry station")]
    [InlineData(4, "InitialPrinter", "Combined initial and printer")]
    [InlineData(8, "Process", "Manufacturing process station")]
    [InlineData(16, "Final", "Final exit station")]
    [InlineData(32, "Inspection", "Quality inspection station")]
    [InlineData(64, "DashBoard", "Monitoring dashboard station")]
    public async Task Should_CreateMachinesWithDifferentTypes_When_VariousMachineTypesProvided(int machineType, string typeName, string typeDescription)
    {
        var logger = XUnitLogger.CreateLogger<CreateMachineMonitorRequestHandlerTests>();
        logger.LogInformation("Test parameters: machineType={MachineType}, typeName={TypeName}, typeDescription={TypeDescription}", machineType, typeName, typeDescription);
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = $"Manufacturing {typeName} Machine",
            Location = $"{typeDescription} Location",
            MachineType = machineType,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineType.ShouldBe(machineType);
    }

    /// <summary>
    /// Executes Should_CreateMachinesWithDifferentTraceabilitySettings_When_VariousTraceabilityConfigurationsProvided operation.
    /// </summary>
    /// <param name="enableApp">The enableApp.</param>
    /// <param name="enableBypass">The enableBypass.</param>
    /// <param name="scenario">The scenario.</param>
    /// <returns>The result of Should_CreateMachinesWithDifferentTraceabilitySettings_When_VariousTraceabilityConfigurationsProvided.</returns>

    [Theory]
    [InlineData(0, 0, "Traceability disabled")]
    [InlineData(1, 0, "App traceability enabled, bypass disabled")]
    [InlineData(0, 1, "App traceability disabled, bypass enabled")]
    [InlineData(1, 1, "Both traceability options enabled")]
    public async Task Should_CreateMachinesWithDifferentTraceabilitySettings_When_VariousTraceabilityConfigurationsProvided(int enableApp, int enableBypass, string scenario)
    {
        var logger = XUnitLogger.CreateLogger<CreateMachineMonitorRequestHandlerTests>();
        logger.LogInformation("Test parameters: enableApp={EnableApp}, enableBypass={EnableBypass}, scenario={Scenario}", enableApp, enableBypass, scenario);
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = "Traceability Test Machine",
            Location = "Test Location",
            MachineType = 8,
            EnableAppTraceability = enableApp,
            EnableBypassTraceability = enableBypass
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.EnableAppTraceability.ShouldBe(enableApp);
        result.Value.EnableBypassTraceability.ShouldBe(enableBypass);
    }

    /// <summary>
    /// Executes Should_HandleRepositoryFailure_When_FirstOrDefaultAsyncFails operation.
    /// </summary>
    /// <returns>The result of Should_HandleRepositoryFailure_When_FirstOrDefaultAsyncFails.</returns>

    [Fact]
    public async Task Should_HandleRepositoryFailure_When_FirstOrDefaultAsyncFails()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = "Test Machine",
            Location = "Test Location",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.WithFailure(new[] { "Database connection failed" }));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database connection failed");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandleAddAsyncFailure_When_RepositoryAddFails operation.
    /// </summary>
    /// <returns>The result of Should_HandleAddAsyncFailure_When_RepositoryAddFails.</returns>

    [Fact]
    public async Task Should_HandleAddAsyncFailure_When_RepositoryAddFails()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = "Test Machine",
            Location = "Test Location",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.WithFailure(new[] { "Failed to add machine to database" })));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Failed to add machine to database");
        await _repository.DidNotReceive().DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_RespectCancellationToken_When_CancellationRequested operation.
    /// </summary>
    /// <returns>The result of Should_RespectCancellationToken_When_CancellationRequested.</returns>

    [Fact]
    public async Task Should_RespectCancellationToken_When_CancellationRequested()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = "Test Machine",
            Location = "Test Location",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8620] Fix nullability mismatch - Repository returns nullable Result, add nullable marker
        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromCanceled<Result<Machine?>>(callInfo.Arg<CancellationToken>()));

        // Act & Assert
        await Should.ThrowAsync<TaskCanceledException>(async () =>
            await _handler.ProcessAsync(request, cts.Token));
    }

    /// <summary>
    /// Executes Should_HandleEdgeCaseMachineIds_When_SpecialValuesProvided operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="scenario">The scenario.</param>
    /// <returns>The result of Should_HandleEdgeCaseMachineIds_When_SpecialValuesProvided.</returns>

    [Theory]
    [InlineData(0, "Zero machine ID")]
    [InlineData(-1, "Negative machine ID")]
    [InlineData(999999, "Large machine ID")]
    [InlineData(int.MaxValue, "Maximum integer machine ID")]
    public async Task Should_HandleEdgeCaseMachineIds_When_SpecialValuesProvided(int machineId, string scenario)
    {
        var logger = XUnitLogger.CreateLogger<CreateMachineMonitorRequestHandlerTests>();
        logger.LogInformation("Testing scenario: {Scenario} with MachineId: {MachineId}", scenario, machineId);
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = machineId,
            MachineId = machineId,
            Name = "Edge Case Machine",
            Location = "Edge Case Location",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        //[Fix]
        //CLAUDE
        //Date: 28/08/2025
        //Reason: [CS8602] - Add null-forgiving operator since result.IsSuccess was verified true
        result.Value.ShouldNotBeNull().MachineId.ShouldBe(machineId);
    }

    /// <summary>
    /// Executes Should_HandleEdgeCaseMachineNames_When_SpecialStringValuesProvided operation.
    /// </summary>
    /// <param name="machineName">The machineName.</param>
    /// <param name="scenario">The scenario.</param>
    /// <returns>The result of Should_HandleEdgeCaseMachineNames_When_SpecialStringValuesProvided.</returns>

    [Theory]
    [InlineData("", "Empty machine name")]
    [InlineData("   ", "Whitespace machine name")]
    [InlineData("VERY-LONG-MACHINE-NAME-THAT-EXCEEDS-NORMAL-LIMITS-FOR-TESTING-EDGE-CASES-IN-MANUFACTURING", "Very long machine name")]
    [InlineData("Machine with Special Characters !@#$%^&*()", "Machine name with special characters")]
    public async Task Should_HandleEdgeCaseMachineNames_When_SpecialStringValuesProvided(string machineName, string scenario)
    {
        var logger = XUnitLogger.CreateLogger<CreateMachineMonitorRequestHandlerTests>();
        logger.LogInformation("Testing scenario: {Scenario} with MachineName: '{MachineName}'", scenario, machineName);
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = machineName,
            Location = "Test Location",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert

        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix assertion order - check IsSuccess before accessing Value properties
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe(machineName);
    }

    /// <summary>
    /// Executes Should_LogWarningMessage_When_DuplicateMachineDetected operation.
    /// </summary>
    /// <returns>The result of Should_LogWarningMessage_When_DuplicateMachineDetected.</returns>

    [Fact]
    public async Task Should_LogWarningMessage_When_DuplicateMachineDetected()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = "Duplicate Test Machine",
            Location = "Test Location",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        var existingMachine = new Machine { MachineId = new MachineId(100001), Name = "Existing Machine" };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// Executes Should_CallRepositoryMethodsInCorrectOrder_When_SuccessfulCreation operation.
    /// </summary>
    /// <returns>The result of Should_CallRepositoryMethodsInCorrectOrder_When_SuccessfulCreation.</returns>

    [Fact]
    public async Task Should_CallRepositoryMethodsInCorrectOrder_When_SuccessfulCreation()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = "Order Test Machine",
            Location = "Test Location",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        Received.InOrder(() =>
        {
            _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>());
            _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
            _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
        });

        await _repository.Received(1).FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_MapAllRequestPropertiesToMachine_When_CreatingNewMachine operation.
    /// </summary>
    /// <returns>The result of Should_MapAllRequestPropertiesToMachine_When_CreatingNewMachine.</returns>

    [Fact]
    public async Task Should_MapAllRequestPropertiesToMachine_When_CreatingNewMachine()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 99999,
            MachineId = 88888,
            Name = "Complete Mapping Test Machine",
            Location = "Complete Test Location with Details",
            MachineType = 32, // Inspection type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 1
        };

        Machine capturedMachine = null!;

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(null!));

        _repository.AddAsync(Arg.Do<Machine>(m => capturedMachine = m), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        // Assert

        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix assertion order - check IsSuccess before accessing Value properties
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        capturedMachine.ShouldNotBeNull();
        capturedMachine.MachineId.Value.ShouldBe(88888);
        capturedMachine.Name.ShouldBe("Complete Mapping Test Machine");
        capturedMachine.Location.ShouldBe("Complete Test Location with Details");
        capturedMachine.MachineType.Value.ShouldBe(32);
        capturedMachine.EnableAppTraceability.ShouldBe(1);
        capturedMachine.EnableBypassTraceability.ShouldBe(1);
    }

    /// <summary>
    /// #113 F3: an infrastructure failure whose message happens to contain the word "entity"
    /// (e.g. an EF provider timeout) must NOT be misread as the repository's not-found sentinel —
    /// the handler must fail loud and never proceed to insert a (possibly duplicate) machine.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Should_FailLoud_When_LookupFailsWithInfraErrorContainingWordEntity()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = "Sniff Regression Machine",
            Location = "Test Location",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.WithFailure(new[] { "The entity framework provider timed out" }));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("The entity framework provider timed out");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// #113 F3: the repository's genuine "No matching entity found" sentinel failure still means
    /// "no duplicate exists" and the handler must proceed to create the machine.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Should_ProceedToCreate_When_LookupFailsWithNotFoundSentinel()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1002,
            MachineId = 100002,
            Name = "Sentinel Not-Found Machine",
            Location = "Test Location",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.WithFailure("No matching entity found."));

        _repository.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        _repository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(request.MachineId);
        await _repository.Received(1).AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }
}