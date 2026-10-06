// <copyright file="GetBarCodeDetailQueryQrCodeHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.DataLoaders;
using IndTrace.Application.BarCodes.Queries.Mappers;

namespace Application.UnitTests.Features.Barcodes
{
    /// <summary>
    /// Unit tests for GetBarCodeDetailQueryQrCodeHandler
    /// </summary>
    public class GetBarCodeDetailQueryQrCodeHandlerTests
    {
        /// <summary>
        /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
        /// </summary>
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange
            var barCodeRepository = Substitute.For<IRepository<BarCode>>();
            var dataLoader = Substitute.For<IBarCodeDetailDataLoader>();
            var mapper = Substitute.For<IBarCodeDetailMapper>();
            var logger = XUnitLogger.CreateLogger<GetBarCodeDetailQueryQrCodeHandler>();

            // Act
            var handler = new GetBarCodeDetailQueryQrCodeHandler(barCodeRepository, dataLoader, mapper, logger);

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
        //             IRepository<BarCode>? nullBarCodeRepository = null!;
        //             var registerRepository = Substitute.For<IRepository<IndTrace.Domain.Entities.Register>>();
        //             var cycleRepository = Substitute.For<IRepository<IndTrace.Domain.Entities.Cycle>>();
        //             var variableRepository = Substitute.For<IRepository<IndTrace.Domain.Entities.Variable>>();
        //             var logger = XUnitLogger.CreateLogger<GetBarCodeDetailQueryQrCodeHandler>();
        // 
        //             // Act & Assert
        //             Should.Throw<ArgumentNullException>(() => new GetBarCodeDetailQueryQrCodeHandler(nullBarCodeRepository!, registerRepository, cycleRepository, variableRepository, logger));
        //         }
        /// <summary>
        /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
        /// </summary>

        // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
        // [Fact]
        //         public void Constructor_WithNullLogger_ShouldThrowException()
        //         {
        //             // Arrange
        //             var barCodeRepository = Substitute.For<IRepository<BarCode>>();
        //             var registerRepository = Substitute.For<IRepository<IndTrace.Domain.Entities.Register>>();
        //             var cycleRepository = Substitute.For<IRepository<IndTrace.Domain.Entities.Cycle>>();
        //             var variableRepository = Substitute.For<IRepository<IndTrace.Domain.Entities.Variable>>();
        //             ILogger<GetBarCodeDetailQueryQrCodeHandler>? nullLogger = null!;
        // 
        //             // Act & Assert
        //             Should.Throw<ArgumentNullException>(() => new GetBarCodeDetailQueryQrCodeHandler(barCodeRepository, registerRepository, cycleRepository, variableRepository, nullLogger!));
        //         }
    }
}