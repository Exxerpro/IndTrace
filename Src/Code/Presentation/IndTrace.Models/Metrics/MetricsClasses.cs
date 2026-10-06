// <copyright file="MetricsClasses.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Metrics;

using IndTrace.UI.Models.Performance;

/// <summary>
/// Provides a collection of manufacturing metrics and performance indicators with calculation implementations.
/// Contains predefined indicator instances and factory methods for metric calculations.
/// </summary>
public class MetricsClasses
{
    /// <summary>
    /// Gets a dictionary of indicator instances keyed by their display names.
    /// Contains pre-configured implementations of common manufacturing metrics.
    /// </summary>
    public static Dictionary<string, IIndicator> IndicatorInstances = new()
    {
        { "Cycle Time", new CycleTime() },
        { "Throughput", new Throughput() },
        { "Defect Rate", new DefectRate() },
        { "Yield", new Yield() },
        { "Work in Progress (WIP)", new WorkInProgress() },
        { "Average Cycle Time", new AverageCycleTime() },
        { "Lead Time", new LeadTime() },
        { "Utilization Rate", new UtilizationRate() },
        { "Takt Time", new TaktTime() },
        { "Downtime", new Downtime() },
        { "ProcessAsync Efficiency", new ProcessEfficiency() },
        { "Scrap Rate", new ScrapRate() },
        { "On-time Delivery Rate", new OnTimeDeliveryRate() },
        { "First Pass Yield (FPY)", new FirstPassYield() },
        { "Overall Equipment Effectiveness (OEE)", new OverallEquipmentEffectiveness() },
        { "Production Rate", new ProductionRate() },
        { "Rework Rate", new ReworkRate() },
        { "Customer Rejects", new CustomerRejects() },
        { "Setup Time", new SetupTime() },
        { "Changeover Time", new ChangeoverTime() },
        { "Inventory Turnover", new InventoryTurnover() },
        { "Order Fulfillment Cycle Time", new OrderFulfillmentCycleTime() },
        { "Mean Time Between Failures (MTBF)", new MeanTimeBetweenFailures() },
        { "Mean Time to Repair (MTTR)", new MeanTimeToRepair() },
        { "Labor Efficiency", new LaborEfficiency() },
        { "Schedule Adherence", new ScheduleAdherence() },
    };

    /// <summary>
    /// Gets a list of available indicator names from the predefined indicator instances.
    /// </summary>
    public static List<string> Indicators => IndicatorInstances.Keys.ToList();

    /// <summary>
    /// Calculates the average cycle time from production data.
    /// </summary>
    public class AverageCycleTime : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AverageCycleTime"/> class.
        /// </summary>
        public AverageCycleTime()
        {
            this.CalculateValue = () =>
            {
                // Assuming Value contains cycle times in minutes
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                double sum = 0;

                for (var i = 0; i < this.Data.GetLength(0); i++)
                {
                    sum += this.Data[i, 0];
                }

                this.Value = sum / this.Data.GetLength(0);
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the changeover time between production runs.
    /// </summary>
    public class ChangeoverTime : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeoverTime"/> class.
        /// </summary>
        public ChangeoverTime()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains changeover start and end times
                this.Value = this.Data[0, 1] - this.Data[0, 0];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the number of customer rejects.
    /// </summary>
    public class CustomerRejects : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerRejects"/> class.
        /// </summary>
        public CustomerRejects()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains customer rejects count
                this.Value = this.Data[0, 0];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the cycle time for production processes.
    /// </summary>
    public class CycleTime : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CycleTime"/> class.
        /// </summary>
        public CycleTime()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains StartTime and EndTime in minutes
                this.Value = this.Data[0, 1] - this.Data[0, 0];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the defect rate as a percentage of defective items.
    /// </summary>
    public class DefectRate : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefectRate"/> class.
        /// </summary>
        public DefectRate()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                var defectiveCount = 0;
                var totalProduced = this.Data.GetLength(0);

                for (var i = 0; i < totalProduced; i++)
                {
                    if (this.Data[i, 0] == 1) // Assuming 1 means defective
                    {
                        defectiveCount++;
                    }
                }

                this.Value = ((double)defectiveCount / totalProduced) * 100;
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the total downtime from downtime periods.
    /// </summary>
    public class Downtime : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Downtime"/> class.
        /// </summary>
        public Downtime()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains start and end times of downtime periods
                this.Value = 0;
                for (var i = 0; i < this.Data.GetLength(0); i++)
                {
                    this.Value += this.Data[i, 1] - this.Data[i, 0];
                }
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the first pass yield as a percentage of parts passing inspection on first attempt.
    /// </summary>
    public class FirstPassYield : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstPassYield"/> class.
        /// </summary>
        public FirstPassYield()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains parts passing inspection on first attempt and total produced parts
                this.Value = (this.Data[0, 0] / this.Data[0, 1]) * 100;
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the inventory turnover ratio based on cost of goods sold and average inventory.
    /// </summary>
    public class InventoryTurnover : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryTurnover"/> class.
        /// </summary>
        public InventoryTurnover()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains cost of goods sold and average inventory
                this.Value = this.Data[0, 0] / this.Data[0, 1];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the labor efficiency ratio based on total output and total labor hours.
    /// </summary>
    public class LaborEfficiency : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LaborEfficiency"/> class.
        /// </summary>
        public LaborEfficiency()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains total output and total labor hours
                this.Value = this.Data[0, 0] / this.Data[0, 1];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the lead time from start to end of production process.
    /// </summary>
    public class LeadTime : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeadTime"/> class.
        /// </summary>
        public LeadTime()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains start and end times for each batch
                this.Value = this.Data[0, 1] - this.Data[0, 0];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the mean time between failures based on total operating time and number of failures.
    /// </summary>
    public class MeanTimeBetweenFailures : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeanTimeBetweenFailures"/> class.
        /// </summary>
        public MeanTimeBetweenFailures()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains total operating time and number of failures
                this.Value = this.Data[0, 0] / this.Data[0, 1];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the mean time to repair based on total repair time and number of repairs.
    /// </summary>
    public class MeanTimeToRepair : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeanTimeToRepair"/> class.
        /// </summary>
        public MeanTimeToRepair()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains total repair time and number of repairs
                this.Value = this.Data[0, 0] / this.Data[0, 1];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the on-time delivery rate as a percentage of deliveries made on time.
    /// </summary>
    public class OnTimeDeliveryRate : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnTimeDeliveryRate"/> class.
        /// </summary>
        public OnTimeDeliveryRate()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains on-time deliveries and total deliveries
                this.Value = (this.Data[0, 0] / this.Data[0, 1]) * 100;
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the order fulfillment cycle time from order receipt to delivery.
    /// </summary>
    public class OrderFulfillmentCycleTime : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderFulfillmentCycleTime"/> class.
        /// </summary>
        public OrderFulfillmentCycleTime()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains order receipt and delivery times
                this.Value = this.Data[0, 1] - this.Data[0, 0];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the Overall Equipment Effectiveness (OEE) based on availability, performance, and quality rates.
    /// </summary>
    public class OverallEquipmentEffectiveness : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OverallEquipmentEffectiveness"/> class.
        /// </summary>
        public OverallEquipmentEffectiveness()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains availability rate, performance rate, and quality rate
                this.Value = this.Data[0, 0] * this.Data[0, 1] * this.Data[0, 2];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the process efficiency as a percentage of cycle time versus total time.
    /// </summary>
    public class ProcessEfficiency : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessEfficiency"/> class.
        /// </summary>
        public ProcessEfficiency()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains sum of cycle times and total time
                this.Value = (this.Data[0, 0] / this.Data[0, 1]) * 100;
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the production rate based on total produced parts and total production time.
    /// </summary>
    public class ProductionRate : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductionRate"/> class.
        /// </summary>
        public ProductionRate()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains total produced parts and total production time
                this.Value = this.Data[0, 0] / this.Data[0, 1];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the rework rate as a percentage of total produced parts that required rework.
    /// </summary>
    public class ReworkRate : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReworkRate"/> class.
        /// </summary>
        public ReworkRate()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains total produced parts and reworked parts
                this.Value = (this.Data[0, 1] / this.Data[0, 0]) * 100;
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the schedule adherence as a percentage of on-schedule production versus total scheduled production.
    /// </summary>
    public class ScheduleAdherence : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleAdherence"/> class.
        /// </summary>
        public ScheduleAdherence()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains on-schedule production and total scheduled production
                this.Value = (this.Data[0, 0] / this.Data[0, 1]) * 100;
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the scrap rate as a percentage of total produced parts that were scrapped.
    /// </summary>
    public class ScrapRate : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScrapRate"/> class.
        /// </summary>
        public ScrapRate()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains total produced parts and scrapped parts
                this.Value = (this.Data[0, 1] / this.Data[0, 0]) * 100;
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the setup time based on setup start and end times.
    /// </summary>
    public class SetupTime : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupTime"/> class.
        /// </summary>
        public SetupTime()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains setup start and end times
                this.Value = this.Data[0, 1] - this.Data[0, 0];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the takt time based on available production time and customer demand.
    /// </summary>
    public class TaktTime : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaktTime"/> class.
        /// </summary>
        public TaktTime()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains total available production time and customer demand
                this.Value = this.Data[0, 0] / this.Data[0, 1];
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the throughput by counting completed and defective items.
    /// </summary>
    public class Throughput : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Throughput"/> class.
        /// </summary>
        public Throughput()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains status where 1 is completed and 2 is defective
                this.Value = 0;
                for (var i = 0; i < this.Data.GetLength(0); i++)
                {
                    if (this.Data[i, 0] == 1 || this.Data[i, 0] == 2)
                    {
                        this.Value++;
                    }
                }
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the utilization rate as a percentage of total production time versus total available time.
    /// </summary>
    public class UtilizationRate : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UtilizationRate"/> class.
        /// </summary>
        public UtilizationRate()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                // Assuming Value contains total production time and total available time
                this.Value = (this.Data[0, 0] / this.Data[0, 1]) * 100;
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the work in progress by counting items currently in production.
    /// </summary>
    public class WorkInProgress : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkInProgress"/> class.
        /// </summary>
        public WorkInProgress()
        {
            this.CalculateValue = () =>
            {
                // Assuming Value contains status where 1 means In Progress
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                this.Value = 0;
                for (var i = 0; i < this.Data.GetLength(0); i++)
                {
                    if (this.Data[i, 0] == 1)
                    {
                        this.Value++;
                    }
                }
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Calculates the yield as a percentage of completed items.
    /// </summary>
    public class Yield : IIndicator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Yield"/> class.
        /// </summary>
        public Yield()
        {
            this.CalculateValue = () =>
            {
                if (this.Data is null || this.Data.GetLength(0) == 0)
                {
                    return;
                }

                var completedCount = 0;
                var totalProduced = this.Data.GetLength(0);

                for (var i = 0; i < totalProduced; i++)
                {
                    if (this.Data[i, 0] == 1) // Assuming 1 means completed
                    {
                        completedCount++;
                    }
                }

                this.Value = ((double)completedCount / totalProduced) * 100;
            };
        }

        /// <summary>
        /// Gets or sets the action to calculate the indicator value.
        /// </summary>
        public Action CalculateValue { get; set; }

        /// <summary>
        /// Gets or sets the data array used for calculations.
        /// </summary>
        public double[,] Data { get; set; } = new double[0, 0];

        /// <summary>
        /// Gets or sets the calculated indicator value.
        /// </summary>
        public double Value { get; set; }
    }
}
