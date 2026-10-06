// <copyright file="RegisterInformationServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Registers
{
    /// <summary>
    /// Unit tests for RegisterInformationService
    /// </summary>
    public class RegisterInformationServiceTests
    {
        /// <summary>
        /// Executes Constructor_ShouldCreateInstance operation.
        /// </summary>
        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Arrange
            var dateTime = Substitute.For<IDateTimeMachine>();
            var distinctRegisterService = Substitute.For<IDistinctRegisterService>();
            var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
            var logger = XUnitLogger.CreateLogger<RegisterInformationService>();

            // Act
            var registerRepository = Substitute.For<IReadOnlyRepository<Register>>();
            var service = new RegisterInformationService(dateTime, distinctRegisterService, registerRepository, logger);

            // Assert
            service.ShouldNotBeNull();
        }

        /// <summary>
        /// Executes GetRegisterInformation_ShouldGetListOfAvailableRegisters operation.
        /// </summary>

        [Fact]
        public async Task GetRegisterInformation_ShouldGetListOfAvailableRegisters()
        {
            // Arrange
            var dateTime = Substitute.For<IDateTimeMachine>();
            var distinctRegisterService = Substitute.For<IDistinctRegisterService>();
            var logger = XUnitLogger.CreateLogger<RegisterInformationService>();

            dateTime.Now.Returns(DateTime.UtcNow);

            // #119 (F4): the catalog now travels on the Result railway.
            distinctRegisterService.GetDistinctRegistersAsync(Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<DistinctRegister>>.Success(new List<DistinctRegister>
                {
                    new() { Name = "PartStatusPlc", VariableId = 1, MachineId = 100 },
                }));

            var registerRepository = Substitute.For<IReadOnlyRepository<Register>>();
            var service = new RegisterInformationService(dateTime, distinctRegisterService, registerRepository, logger);

            // Act
            var result = await service.GetListOfAvailableRegisters(TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Count().ShouldBe(1);
        }

        /// <summary>
        /// Issue #119 (F4): a catalog failure propagates to the Metrics page as a failure Result
        /// instead of masquerading as an empty register list.
        /// </summary>
        /// <returns>The result of the assertion task.</returns>
        [Fact]
        public async Task GetListOfAvailableRegisters_WhenCatalogFails_PropagatesFailure()
        {
            // Arrange
            var dateTime = Substitute.For<IDateTimeMachine>();
            var distinctRegisterService = Substitute.For<IDistinctRegisterService>();
            var logger = XUnitLogger.CreateLogger<RegisterInformationService>();

            distinctRegisterService.GetDistinctRegistersAsync(Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<DistinctRegister>>.WithFailure(["Registers store unreachable."]));

            var registerRepository = Substitute.For<IReadOnlyRepository<Register>>();
            var service = new RegisterInformationService(dateTime, distinctRegisterService, registerRepository, logger);

            // Act
            var result = await service.GetListOfAvailableRegisters(TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Registers store unreachable.");
        }

        /// <summary>
        /// Executes ProcessRegisterData_ShouldProcessData operation.
        /// </summary>
        /// <returns>The result of ProcessRegisterData_ShouldProcessData.</returns>

        [Fact]
        public async Task ProcessRegisterData_ShouldProcessData()
        {
            // Arrange
            var dateTime = Substitute.For<IDateTimeMachine>();
            var distinctRegisterService = Substitute.For<IDistinctRegisterService>();
            var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
            var logger = XUnitLogger.CreateLogger<RegisterInformationService>();

            //[Fix]
            //CLAUDE
            //Date: 25/08/2025
            //Reason: [NULL REFERENCE FIX] - monitorRequestDispatcher.QueryAsync() was returning null, need to mock proper response

            // Mock DateTime service
            dateTime.Now.Returns(DateTime.UtcNow);

            //[Fix]
            //CLAUDE
            //Date: 25/08/2025
            //Reason: [MISSING MOCK SETUP] - Setup monitorRequestDispatcher.QueryAsync to return success with mock data instead of null
            var mockRegisterData = new List<RegisterDto>
            {
                new RegisterDto { MachineId = 100, Name = "TestRegister", Value = "42.5", DataType = "float", TimeStamp = DateTime.Now }
            };

            var registerRepository = Substitute.For<IReadOnlyRepository<Register>>();

            // Mock register repository to return success with Register entities
            var mockRegisterEntities = new List<Register>
            {
                Register.CreateFixture(name: "TestRegister", machineId: 100, variableId: 1, value: "42.5", dataType: "float", timeStamp: DateTime.Now)
            };
            registerRepository.ListAsync(Arg.Any<Specification<Register>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Register>>.Success(mockRegisterEntities));

            var service = new RegisterInformationService(dateTime, distinctRegisterService, registerRepository, logger);

            // Create test data with proper structure
            var variables = new List<RegistersRecords>
            {
                new RegistersRecords { MachineId = 100, Name = "TestRegister", VariableId = 1 }
            };

            // Act
            var result = await service.GetListRegisterTrends(variables, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.ShouldNotBeNull();

            result.Value.ShouldNotBeNull();
        }

        /// <summary>
        /// Regression for issue #73 (P0-13): on sparse/empty data the day-by-day walk-back
        /// must terminate within the bounded look-back window instead of storming the
        /// repository back to <see cref="DateTime.MinValue"/>.
        /// </summary>
        /// <returns>The result of the assertion task.</returns>
        [Fact]
        public async Task GetListRegisterTrends_WhenDataIsSparse_TerminatesWithinLookBackBound()
        {
            // Arrange
            var dateTime = Substitute.For<IDateTimeMachine>();
            var distinctRegisterService = Substitute.For<IDistinctRegisterService>();
            var logger = XUnitLogger.CreateLogger<RegisterInformationService>();

            // Fixed deterministic clock so the look-back window is stable.
            dateTime.Now.Returns(new DateTime(2026, 7, 7, 12, 0, 0, DateTimeKind.Local));

            var registerRepository = Substitute.For<IReadOnlyRepository<Register>>();

            // Empty result on every window: the loop can never reach maxItems and, before the
            // fix, would walk back forever. It must now stop at the look-back bound.
            registerRepository.ListAsync(Arg.Any<Specification<Register>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Register>>.Success(new List<Register>()));

            var service = new RegisterInformationService(dateTime, distinctRegisterService, registerRepository, logger);

            var variables = new List<RegistersRecords>
            {
                new RegistersRecords { MachineId = 100, Name = "TestRegister", VariableId = 1 },
            };

            // Act
            var result = await service.GetListRegisterTrends(variables, maxItems: 100, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();

            // Bounded: at most one repository round-trip per day within the look-back window
            // (plus the final clamped window). Proves the loop cannot run away.
            var calls = registerRepository.ReceivedCalls().Count(c => c.GetMethodInfo().Name == nameof(IReadOnlyRepository<Register>.ListAsync));
            calls.ShouldBeLessThanOrEqualTo(366);
            calls.ShouldBeGreaterThan(0);
        }

        /// <summary>
        /// Regression for issue #73 (P0-13): on dense data the walk-back must exit early on the
        /// first window (a single repository round-trip) and return the most recent
        /// <c>maxItems</c> points per key, preserving pre-fix observable behaviour.
        /// </summary>
        /// <returns>The result of the assertion task.</returns>
        [Fact]
        public async Task GetListRegisterTrends_WhenDataIsDense_ExitsEarlyAndReturnsMaxItems()
        {
            // Arrange
            var dateTime = Substitute.For<IDateTimeMachine>();
            var distinctRegisterService = Substitute.For<IDistinctRegisterService>();
            var logger = XUnitLogger.CreateLogger<RegisterInformationService>();

            var now = new DateTime(2026, 7, 7, 12, 0, 0, DateTimeKind.Local);
            dateTime.Now.Returns(now);

            // 150 distinct-timestamp registers for a single machine — more than maxItems (100).
            var denseRegisters = Enumerable.Range(0, 150)
                .Select(j => Register.CreateFixture(
                    name: "TestRegister",
                    machineId: 100,
                    variableId: 1,
                    value: j.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    dataType: "float",
                    timeStamp: now.AddMinutes(-j)))
                .ToList();

            var registerRepository = Substitute.For<IReadOnlyRepository<Register>>();
            registerRepository.ListAsync(Arg.Any<Specification<Register>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Register>>.Success(denseRegisters));

            var service = new RegisterInformationService(dateTime, distinctRegisterService, registerRepository, logger);

            var variables = new List<RegistersRecords>
            {
                new RegistersRecords { MachineId = 100, Name = "TestRegister", VariableId = 1 },
            };

            // Act
            var result = await service.GetListRegisterTrends(variables, maxItems: 100, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();

            var key = (MachineId: 100, Name: "TestRegister");
            result.Value.ShouldContainKey(key);
            result.Value[key].Count().ShouldBe(100);

            // Early exit: dense first window satisfies maxItems in a single round-trip.
            var calls = registerRepository.ReceivedCalls().Count(c => c.GetMethodInfo().Name == nameof(IReadOnlyRepository<Register>.ListAsync));
            calls.ShouldBe(1);
        }

        /// <summary>
        /// Regression for issue #119 (F3): consecutive one-day windows shared the boundary instant
        /// (<c>&gt;= start &amp;&amp; &lt;= end</c> with <c>end == next start</c>), so a row whose
        /// <c>TimeStamp</c> falls exactly on a window boundary was fetched by BOTH windows and
        /// double-counted in the trend series. The single-range-query rewrite must count it ONCE.
        /// </summary>
        /// <returns>The result of the assertion task.</returns>
        [Fact]
        public async Task GetListRegisterTrends_RowExactlyOnWindowBoundary_IsCountedOnce()
        {
            // Arrange
            var dateTime = Substitute.For<IDateTimeMachine>();
            var distinctRegisterService = Substitute.For<IDistinctRegisterService>();
            var logger = XUnitLogger.CreateLogger<RegisterInformationService>();

            var now = new DateTime(2026, 7, 7, 12, 0, 0, DateTimeKind.Local);
            dateTime.Now.Returns(now);

            // Pre-fix windows: [now-1d, now], [now-2d, now-1d], ... — the instant now-1d belongs to BOTH.
            var boundary = now.AddDays(-1);
            var backingStore = new List<Register>
            {
                Register.CreateFixture(name: "TestRegister", machineId: 100, variableId: 1, value: "1", dataType: "float", timeStamp: boundary),
                Register.CreateFixture(name: "TestRegister", machineId: 100, variableId: 1, value: "2", dataType: "float", timeStamp: boundary.AddHours(-2)),
            };

            // The mock materializes each specification against the backing store (criteria, ordering,
            // paging), so overlapping windows genuinely re-fetch the boundary row exactly like a database.
            var registerRepository = Substitute.For<IReadOnlyRepository<Register>>();
            registerRepository.ListAsync(Arg.Any<ISpecification<Register>>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => MaterializeAgainst(backingStore, callInfo.Arg<ISpecification<Register>>()));

            var service = new RegisterInformationService(dateTime, distinctRegisterService, registerRepository, logger);

            var variables = new List<RegistersRecords>
            {
                new RegistersRecords { MachineId = 100, Name = "TestRegister", VariableId = 1 },
            };

            // Act
            var result = await service.GetListRegisterTrends(variables, maxItems: 100, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();

            var key = (MachineId: 100, Name: "TestRegister");
            result.Value.ShouldContainKey(key);

            // The boundary-instant row must appear exactly once (pre-fix it appeared twice).
            result.Value[key].Count(point => point.TimeStamp == boundary).ShouldBe(1);
            result.Value[key].Count().ShouldBe(2);
        }

        /// <summary>
        /// #126 adversarial review C9: the <c>maxItems</c> cap is PER REGISTER, not a single global
        /// server-side Take. Before the fix one chatty register whose rows were all newer than a
        /// co-selected quiet register's rows consumed the whole global cap and starved the quiet
        /// register's trend to zero points.
        /// </summary>
        /// <returns>The result of the assertion task.</returns>
        [Fact]
        public async Task GetListRegisterTrends_ChattyRegisterMustNotStarveCoSelectedRegisters()
        {
            // Arrange
            var dateTime = Substitute.For<IDateTimeMachine>();
            var distinctRegisterService = Substitute.For<IDistinctRegisterService>();
            var logger = XUnitLogger.CreateLogger<RegisterInformationService>();

            var now = new DateTime(2026, 7, 7, 12, 0, 0, DateTimeKind.Local);
            dateTime.Now.Returns(now);

            // Chatty register: 150 recent rows. Quiet register: 5 rows, ALL older than every chatty row,
            // so a single global newest-first Take(100) is fully consumed by the chatty register.
            var backingStore = new List<Register>();
            backingStore.AddRange(Enumerable.Range(0, 150).Select(j => Register.CreateFixture(
                name: "Chatty",
                machineId: 100,
                variableId: 1,
                value: j.ToString(System.Globalization.CultureInfo.InvariantCulture),
                dataType: "float",
                timeStamp: now.AddMinutes(-j))));
            backingStore.AddRange(Enumerable.Range(0, 5).Select(j => Register.CreateFixture(
                name: "Quiet",
                machineId: 200,
                variableId: 1,
                value: j.ToString(System.Globalization.CultureInfo.InvariantCulture),
                dataType: "float",
                timeStamp: now.AddDays(-1).AddMinutes(-j))));

            var registerRepository = Substitute.For<IReadOnlyRepository<Register>>();
            registerRepository.ListAsync(Arg.Any<ISpecification<Register>>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => MaterializeAgainst(backingStore, callInfo.Arg<ISpecification<Register>>()));

            var service = new RegisterInformationService(dateTime, distinctRegisterService, registerRepository, logger);

            var variables = new List<RegistersRecords>
            {
                new RegistersRecords { MachineId = 100, Name = "Chatty", VariableId = 1 },
                new RegistersRecords { MachineId = 200, Name = "Quiet", VariableId = 1 },
            };

            // Act
            var result = await service.GetListRegisterTrends(variables, maxItems: 100, cancellationToken: TestContext.Current.CancellationToken);

            // Assert - BOTH selected registers return points: the quiet one keeps all 5, the chatty one
            // is capped at maxItems.
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.ShouldContainKey((MachineId: 200, Name: "Quiet"));
            result.Value[(MachineId: 200, Name: "Quiet")].Count().ShouldBe(5);
            result.Value.ShouldContainKey((MachineId: 100, Name: "Chatty"));
            result.Value[(MachineId: 100, Name: "Chatty")].Count().ShouldBe(100);
        }

        /// <summary>
        /// Materializes a specification against an in-memory backing store the way a real repository
        /// would: criteria filter, then descending ordering, then paging. This keeps the mock honest so
        /// overlapping-window duplication (and the single-query fix) are observable.
        /// </summary>
        /// <param name="backingStore">The in-memory rows standing in for the Registers table.</param>
        /// <param name="spec">The specification the system under test issued.</param>
        /// <returns>The rows the specification selects.</returns>
        private static Result<IEnumerable<Register>> MaterializeAgainst(IEnumerable<Register> backingStore, ISpecification<Register> spec)
        {
            var filtered = backingStore.Where(spec.Criteria.Compile());

            if (spec.OrderByDescending is not null)
            {
                filtered = filtered.OrderByDescending(spec.OrderByDescending.Compile());
            }

            if (spec.Skip.HasValue)
            {
                filtered = filtered.Skip(spec.Skip.Value);
            }

            if (spec.Take.HasValue)
            {
                filtered = filtered.Take(spec.Take.Value);
            }

            return Result<IEnumerable<Register>>.Success(filtered.ToList());
        }

        /// <summary>
        /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
        /// </summary>

        [Fact]
        public void Properties_WhenSet_ShouldReturnCorrectValues()
        {
            // Arrange
            var dateTime = Substitute.For<IDateTimeMachine>();
            var distinctRegisterService = Substitute.For<IDistinctRegisterService>();
            var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
            var logger = XUnitLogger.CreateLogger<RegisterInformationService>();

            // Act
            var registerRepository = Substitute.For<IReadOnlyRepository<Register>>();
            var service = new RegisterInformationService(dateTime, distinctRegisterService, registerRepository, logger);

            // Act & Assert
            // TODO: Test property setters and getters
        }

        /// <summary>
        /// Executes Methods_WhenCalled_ShouldReturnExpectedResults operation.
        /// </summary>

        [Fact]
        public void Methods_WhenCalled_ShouldReturnExpectedResults()
        {
            // Arrange
            var dateTime = Substitute.For<IDateTimeMachine>();
            var distinctRegisterService = Substitute.For<IDistinctRegisterService>();
            var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
            var logger = XUnitLogger.CreateLogger<RegisterInformationService>();

            // Act
            var registerRepository = Substitute.For<IReadOnlyRepository<Register>>();
            var service = new RegisterInformationService(dateTime, distinctRegisterService, registerRepository, logger);

            // Act
            // TODO: Call methods

            // Assert
            // TODO: Verify results
        }
    }
}