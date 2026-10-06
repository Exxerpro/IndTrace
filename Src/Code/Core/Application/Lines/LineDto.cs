// <copyright file="LineDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Lines
{
    /// <summary>
    /// Data transfer object for production line information used in application layer operations.
    /// </summary>
    /// <remarks>
    /// This DTO facilitates the transfer of production line data between layers while providing
    /// mapping functionality to convert between domain entities and DTOs.
    /// </remarks>
    public class LineDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineDto"/> class.
        /// Initializes a new instance of the class.
        /// </summary>
        public LineDto()
        {
            this.Name = string.Empty;
            this.Description = string.Empty;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the production line.
        /// </summary>
        public int LineId { get; set; }

        /// <summary>
        /// Gets or sets the name of the production line.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the production line.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the status of the production line.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Converts a Line domain entity to a LineDto.
        /// </summary>
        /// <param name="src">The line domain entity to convert.</param>
        /// <returns>A LineDto containing the line data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source line is null.</exception>
        /// <summary>
        /// Executes ToDto operation.
        /// </summary>
        /// <param name="src">The src.</param>
        /// <returns>The result of ToDto.</returns>
        public static IndQuestResults.Result<LineDto> ToDto(Line src)
        {
            if (src == null)
            {
                return IndQuestResults.Result<LineDto>.WithFailure("Line source cannot be null");
            }

            return IndQuestResults.Result<LineDto>.Success(new LineDto
            {
                LineId = src.LineId,
                Name = src.Name,
                Description = src.Description,
                Status = src.Status,
            });
        }

        /// <summary>
        /// Converts a LineDto to a Line domain entity.
        /// </summary>
        /// <param name="src">The line DTO to convert.</param>
        /// <returns>A Line domain entity containing the DTO data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source DTO is null.</exception>
        public static IndQuestResults.Result<Line> ToEntity(LineDto src)
        {
            if (src == null)
            {
                return IndQuestResults.Result<Line>.WithFailure("LineDto source cannot be null");
            }

            return IndQuestResults.Result<Line>.Success(new Line
            {
                LineId = src.LineId,
                Name = src.Name,
                Description = src.Description,
                Status = src.Status,
            });
        }
    }
}
