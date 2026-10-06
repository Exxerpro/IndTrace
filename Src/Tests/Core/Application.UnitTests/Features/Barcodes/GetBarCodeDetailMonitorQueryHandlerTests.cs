// <copyright file="GetBarCodeDetailMonitorQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.DataLoaders;
using IndTrace.Application.BarCodes.Queries.Mappers;

namespace Application.UnitTests.Features.Barcodes
{
    /// <summary>
    /// Unit tests for GetBarCodeDetailMonitorQueryHandler
    /// </summary>
    public class GetBarCodeDetailMonitorQueryHandlerTests
    {
        private readonly IRepository<BarCode> _barCodeRepository = Substitute.For<IRepository<BarCode>>();
        private readonly IBarCodeDetailDataLoader _dataLoaderSub = Substitute.For<IBarCodeDetailDataLoader>();
        private readonly IBarCodeDetailMapper _mapperSub = Substitute.For<IBarCodeDetailMapper>();
        private readonly ILogger<GetBarCodeDetailMonitorQueryHandler> _logger = XUnitLogger.CreateLogger<GetBarCodeDetailMonitorQueryHandler>();
        /// <summary>
        /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
        /// </summary>

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange & Act
            var handler = new GetBarCodeDetailMonitorQueryHandler(
                _barCodeRepository,
                _dataLoaderSub,
                _mapperSub,
                _logger);

            // Assert
            handler.ShouldNotBeNull();
        }

        /// <summary>
        /// Executes Constructor_WithNullBarCodeRepository_ShouldThrowException operation.
        /// </summary>

        // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
        // [Fact]
        //         public void Constructor_WithNullBarCodeRepository_ShouldThrowException()
        //         {
        //             // Arrange
        //             IRepository<BarCode>? nullRepository = null!;
        //
        //             // Act & Assert
        //             Should.Throw<ArgumentNullException>(() => new GetBarCodeDetailMonitorQueryHandler(
        //                 nullRepository!,
        //                 _registerRepository,
        //                 _cycleRepository,
        //                 _variableRepository,
        //                 _logger));
        //         }
        /// <summary>
        /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
        /// </summary>

        // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
        // [Fact]
        //         public void Constructor_WithNullLogger_ShouldThrowException()
        //         {
        //             // Arrange
        //             ILogger<GetBarCodeDetailMonitorQueryHandler>? nullLogger = null!;
        //
        //             // Act & Assert
        //             Should.Throw<ArgumentNullException>(() => new GetBarCodeDetailMonitorQueryHandler(
        //                 _barCodeRepository,
        //                 _registerRepository,
        //                 _cycleRepository,
        //                 _variableRepository,
        //                 nullLogger!));
        //         }

        /// <summary>
        /// Executes Process_WhenRepositoryFails_ShouldReturnFailure operation.
        /// </summary>
        /// <returns>The result of Process_WhenRepositoryFails_ShouldReturnFailure.</returns>

        [Fact]
        public async Task Process_WhenRepositoryFails_ShouldReturnFailure()
        {
            // Arrange
            var handler = new GetBarCodeDetailMonitorQueryHandler(
                _barCodeRepository,
                _dataLoaderSub,
                _mapperSub,
                _logger);

            var query = new GetBarCodeDetailMonitorQuery { BarCode = "TEST123" };
            
            // Mock the repository method - use FirstOrDefaultAsync as GetBarCodeByLabelAsync may be extension method
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.WithFailure("Repository error"));

            // Act
            var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain("Repository error");
        }
    }
}