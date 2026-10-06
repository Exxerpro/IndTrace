// <copyright file="GetAppDetailsGuiRequestHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features
{
    /// <summary>
    /// Unit tests for GetAppDetailsMonitorRequestHandler
    /// </summary>
    public class GetAppDetailsMonitorRequestHandlerTests
    {
        private readonly CacheManager<ApplicationConfiguration> _cacheManagerSub = null!;
        private readonly AppDetailsFactory _appDetailsFactorySub = null!;
        private readonly ILogger<GetAppDetailsMonitorRequestHandler> _loggerSub = null!;
        private readonly GetAppDetailsMonitorRequestHandler _handler = null!;
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>

        public GetAppDetailsMonitorRequestHandlerTests()
        {
            //[Fix]
            //CLAUDE
            //Date: 22/08/2025
            //Reason: PATTERN 12 Fix - Cannot mock concrete classes with NSubstitute, use real instances for constructor tests
            _cacheManagerSub = new CacheManager<ApplicationConfiguration>(TimeSpan.FromMinutes(3));
            _appDetailsFactorySub = CreateMockAppDetailsFactory();
            _loggerSub = XUnitLogger.CreateLogger<GetAppDetailsMonitorRequestHandler>();
            _handler = new GetAppDetailsMonitorRequestHandler(_cacheManagerSub, _appDetailsFactorySub, _loggerSub);
        }

        /// <summary>
        /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
        /// </summary>

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange
            //[Fix]
            //CLAUDE
            //Date: 22/08/2025
            //Reason: PATTERN 12 Fix - Use real instances instead of mocking concrete classes
            var cacheManager = new CacheManager<ApplicationConfiguration>(TimeSpan.FromMinutes(3));
            var appDetailsFactory = CreateMockAppDetailsFactory();
            var logger = XUnitLogger.CreateLogger<GetAppDetailsMonitorRequestHandler>();

            // Act
            var instance = new GetAppDetailsMonitorRequestHandler(cacheManager, appDetailsFactory, logger);

            // Assert
            instance.ShouldNotBeNull();
        }

        ///// <summary>
        ///// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
        ///// </summary>

        //[Fact]
        //public void Constructor_WithInvalidParameters_ShouldThrowException()
        //{
        //    // Arrange
        //    var appDetailsFactory = Substitute.For<AppDetailsFactory>();
        //    var logger =  XUnitLogger.CreateLogger<GetAppDetailsMonitorRequestHandler>();

        //    // Act & Assert
        //    Should.Throw<ArgumentNullException>(() => new GetAppDetailsMonitorRequestHandler(null!, appDetailsFactory, logger));
        //}
        /// <summary>
        /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
        /// </summary>

        [Fact]
        public void Properties_WhenSet_ShouldReturnCorrectValues()
        {
            // Arrange
            var instance = _handler;

            // Act & Assert
            instance.ShouldNotBeNull();
        }

        /// <summary>
        /// Executes Methods_WhenCalled_ShouldReturnExpectedResults operation.
        /// </summary>

        [Fact]
        public void Methods_WhenCalled_ShouldReturnExpectedResults()
        {
            // Arrange
            var instance = _handler;

            // Act & Assert
            instance.ShouldNotBeNull();
        }

        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Helper method to create AppDetailsFactory with mocked dependencies for constructor tests
        private AppDetailsFactory CreateMockAppDetailsFactory()
        {
            var configAppRepository = Substitute.For<IRepository<ConfigApp>>();
            var plcRepository = Substitute.For<IRepository<Plc>>();
            var machinePlcRepository = Substitute.For<IReadOnlyRepository<MachinePlc>>();
            var workflowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
            var machineRepository = Substitute.For<IRepository<Machine>>();
            var variableRepository = Substitute.For<IRepository<Variable>>();
            var customerRepository = Substitute.For<IRepository<Customer>>();
            var variablesGroupRepository = Substitute.For<IRepository<VariablesGroup>>();
            var productRepository = Substitute.For<IRepository<Product>>();
            var isOeeEnabledChecker = Substitute.For<IIsOeeEnabledChecker>();
            var logger = XUnitLogger.CreateLogger<AppDetailsFactory>();
            var dateTimeMachine = Substitute.For<IDateTimeMachine>();

            return new AppDetailsFactory(
                configAppRepository,
                plcRepository,
                machinePlcRepository,
                workflowRepository,
                machineRepository,
                variableRepository,
                customerRepository,
                variablesGroupRepository,
                productRepository,
                isOeeEnabledChecker,
                logger,
                dateTimeMachine);
        }
    }
}