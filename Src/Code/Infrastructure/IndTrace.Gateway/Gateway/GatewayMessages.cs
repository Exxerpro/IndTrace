// <copyright file="GatewayMessages.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Gateway.Gateway;

public static class GatewayMessages
{
    // Error messages
    public const string ErrorCreateBarcode = "Error while sending create barcode Request";

    public const string FailedCreateBarcode = "Failed to create barcode";
    public const string ErrorReadBarcode = "Error while sending get barcode detail query";
    public const string FailedReadBarcode = "Failed to read barcode";
    public const string ErrorCreateCycle = "Error while creating a cycle Request";
    public const string FailedCreateCycle = "Failed to create cycle";
    public const string ErrorUpdateCycle = "Error while updating a cycle Request";
    public const string FailedUpdateCycle = "Failed to update cycle";
    public const string ErrorEndProcess = "Error while sending end of process Request";
    public const string FailedEndProcess = "Failed to end process workflow";

}