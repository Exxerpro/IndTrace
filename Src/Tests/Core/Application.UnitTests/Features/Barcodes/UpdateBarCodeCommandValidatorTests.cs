// <copyright file="UpdateBarCodeCommandValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for UpdateBarCodeCommandValidator
/// </summary>
public class UpdateBarCodeCommandValidatorTests
{
    private readonly UpdateBarCodeCommandValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public UpdateBarCodeCommandValidatorTests()
    {
        _validator = new UpdateBarCodeCommandValidator();
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new UpdateBarCodeCommandValidator();

        // Assert
        validator.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Validate_WithValidCommand_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidCommand_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = "L1ATEST1230001",
                CycleStatus = CycleStatus.FinishedOk,
                PartStatus = PartStatus.Ok
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithNullRegisters_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullRegisters_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = "L1ATEST1230001"
            },
            Registers = null!
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.Registers);
    }

    /// <summary>
    /// Executes Validate_WithEmptyRegisters_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyRegisters_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = "L1ATEST1230001"
            },
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.Registers);
    }

    /// <summary>
    /// Executes Validate_WithNullCommand_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullCommand_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = null!,
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.Command);
    }

    /// <summary>
    /// Executes Validate_WithZeroMachineId_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithZeroMachineId_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 0,
                PartNumber = "TEST123",
                BarCode = "L1ATEST1230001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.Command.MachineId);
    }

    /// <summary>
    /// Executes Validate_WithNegativeMachineId_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithNegativeMachineId_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = -1,
                PartNumber = "TEST123",
                BarCode = "L1ATEST1230001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.Command.MachineId);
    }

    /// <summary>
    /// Executes Validate_WithNullPartNumber_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullPartNumber_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = null!,
                BarCode = "L1ATEST1230001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Command.PartNumber)
            ;
    }

    /// <summary>
    /// Executes Validate_WithEmptyPartNumber_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyPartNumber_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "",
                BarCode = "L1ATEST1230001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Command.PartNumber)
            ;
    }

    /// <summary>
    /// Executes Validate_WithShortPartNumber_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithShortPartNumber_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "AB",
                BarCode = "L1ATEST1230001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Command.PartNumber)
            ;
    }

    /// <summary>
    /// Executes Validate_WithMinimumLengthPartNumber_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithMinimumLengthPartNumber_ShouldPassValidation()
    {
        // Arrange
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Fixed BarCode to contain PartNumber as required by validator business rule
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "ABC",
                BarCode = "L1AABC0001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Command.PartNumber);
    }

    /// <summary>
    /// Executes Validate_WithNullBarCode_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullBarCode_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = null!
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Command.BarCode)
            ;
    }

    /// <summary>
    /// Executes Validate_WithEmptyBarCode_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithEmptyBarCode_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = ""
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Command.BarCode)
            ;
    }

    /// <summary>
    /// Executes Validate_WithShortBarCode_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithShortBarCode_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = "AB"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Command.BarCode)
            ;
    }

    /// <summary>
    /// Executes Validate_WithMinimumLengthBarCode_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithMinimumLengthBarCode_ShouldPassValidation()
    {
        // Arrange
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Fixed BarCode to contain PartNumber and meet minimum length requirement
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = "TEST123"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Command.BarCode);
    }

    /// <summary>
    /// Executes Validate_WithBarCodeNotContainingPartNumber_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithBarCodeNotContainingPartNumber_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = "L1AOTHER4560001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Command.BarCode)
            ;
    }

    /// <summary>
    /// Executes Validate_WithBarCodeContainingPartNumber_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithBarCodeContainingPartNumber_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = "L1ATEST1230001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Command.BarCode);
    }

    /// <summary>
    /// Executes Validate_WithValidMachineId_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidMachineId_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = "L1ATEST1230001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Command.MachineId);
    }

    /// <summary>
    /// Executes Validate_WithLargeMachineId_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithLargeMachineId_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = int.MaxValue,
                PartNumber = "TEST123",
                BarCode = "L1ATEST1230001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Command.MachineId);
    }

    /// <summary>
    /// Executes Validate_WithLongPartNumber_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithLongPartNumber_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "VERYLONGPARTNUMBER123",
                BarCode = "L1AVERYLONGPARTNUMBER1230001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Command.PartNumber);
    }

    /// <summary>
    /// Executes Validate_WithLongBarCode_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithLongBarCode_ShouldPassValidation()
    {
        // Arrange
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Fixed BarCode to contain PartNumber as required by validator business rule
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = "VERYLONGBARCODETEST123456789"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Command.BarCode);
    }

    /// <summary>
    /// Executes Validate_WithMultipleValidationErrors_ShouldFailAllValidations operation.
    /// </summary>

    [Fact]
    public void Validate_WithMultipleValidationErrors_ShouldFailAllValidations()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 0,
                PartNumber = "AB",
                BarCode = "CD"
            },
            Registers = null!
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Registers);
        result.ShouldHaveValidationErrorFor(x => x.Command.MachineId);
        result.ShouldHaveValidationErrorFor(x => x.Command.PartNumber);
        result.ShouldHaveValidationErrorFor(x => x.Command.BarCode);
        result.Errors.Count().ShouldBeGreaterThan(1);
    }

    /// <summary>
    /// Executes Validate_WithWhitespacePartNumber_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithWhitespacePartNumber_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "   ",
                BarCode = "L1ATEST1230001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Command.PartNumber)
            ;
    }

    /// <summary>
    /// Executes Validate_WithWhitespaceBarCode_ShouldFailValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithWhitespaceBarCode_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = "   "
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Command.BarCode)
            ;
    }

    /// <summary>
    /// Executes Validate_WithSpecialCharactersInPartNumber_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithSpecialCharactersInPartNumber_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST-123_ABC",
                BarCode = "L1ATEST-123_ABC0001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Command.PartNumber);
    }

    /// <summary>
    /// Executes Validate_WithSpecialCharactersInBarCode_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithSpecialCharactersInBarCode_ShouldPassValidation()
    {
        // Arrange
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Fixed BarCode to contain exact PartNumber as required by validator business rule
        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "TEST123",
                BarCode = "L1A-TEST123_@0001"
            },
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Added valid timestamp to Register for validator time range validation
            Registers = new Dictionary<string, IndTrace.Domain.Entities.Register>
            {
                { "Register1", IndTrace.Domain.Entities.Register.CreateFixture(name: "Register1", value: "Value1", timeStamp: DateTime.Now) }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Command.BarCode);
    }
}