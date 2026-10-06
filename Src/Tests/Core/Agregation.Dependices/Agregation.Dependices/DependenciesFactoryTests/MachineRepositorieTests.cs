// <copyright file="MachineRepositorieTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Agregation.Dependices.DependenciesFactoryTests
{
    public class MachineRepositorieTests(ITestOutputHelper output) : DependenciesFactory(output)
    {
        [Fact]
        public async Task ShouldGetMachineForAGivenID_RetrieveCorrectsMAchineType()
        {
            // Arrange & Act - Factory initialization

            var machineId = 500;

            var specificMachineSpec1 = new Specification<Machine>(c => c.MachineId == new MachineId(machineId));

            // Act

            var logger = XUnitLogger.CreateLogger<MachineRepositorieTests>();

            // Check if specific rwMachine  exists
            var rwRepoMachineResult = await DpMachineRepository.FirstOrDefaultAsync(specificMachineSpec1, TestContext.Current.CancellationToken);
            logger.LogInformation("Specific Machine {Machine} exists: {Exists} and is type {Type} ", machineId, rwRepoMachineResult.IsSuccess && rwRepoMachineResult.Value != null, rwRepoMachineResult.Value?.MachineType);

            rwRepoMachineResult.IsSuccess.ShouldBeTrue();

            rwRepoMachineResult.Value.ShouldNotBeNull();

            var rwMachine = rwRepoMachineResult.Value;

            logger.LogInformation("Machine Founde was Machie {machine}", rwMachine);

            rwMachine.MachineType.Name.ShouldBe("Final");

            var specificMachineSpec = new Specification<Machine>(c => c.MachineId == new MachineId(machineId));
            // Check if specific roMachine  exists
            var roRepoMachineResult1 = await DpRoMachineRepository.FirstOrDefaultAsync(specificMachineSpec, TestContext.Current.CancellationToken);
            var roRepoMachineResult2 = await DpRoMachineRepository.FirstOrDefaultAsync(specificMachineSpec, TestContext.Current.CancellationToken);
            var roRepoMachineResult3 = await DpRoMachineRepository.FirstOrDefaultAsync(specificMachineSpec, TestContext.Current.CancellationToken);
            logger.LogInformation("Specific Machine {Machine} exists: {Exists} and is type {Type} ", machineId, roRepoMachineResult1.IsSuccess && roRepoMachineResult1.Value != null, roRepoMachineResult1.Value?.MachineType);

            // Check if specific roMachine  exists
            var roRepoMachineResult = await DpRoMachineRepository.FirstOrDefaultAsync(specificMachineSpec, TestContext.Current.CancellationToken);
            logger.LogInformation("Specific Machine {Machine} exists: {Exists} and is type {Type} ", machineId, roRepoMachineResult.IsSuccess && roRepoMachineResult.Value != null, roRepoMachineResult.Value?.MachineType);

            roRepoMachineResult.IsSuccess.ShouldBeTrue();

            roRepoMachineResult.Value.ShouldNotBeNull();

            var roMachine = roRepoMachineResult.Value;

            logger.LogInformation("Machine Founde was Machie {machine}", roMachine);

            roMachine.MachineType.Name.ShouldBe("Final");
        }
    }
}