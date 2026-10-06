// <copyright file="ControllerMonitor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UI.Models
{
    using System.Timers;
    using Timer = System.Timers.Timer;

    /// <summary>
    /// Represents a controller monitor for tracking PLC controller status and communication.
    /// </summary>
    public class ControllerMonitor : IMonitorFilter, IComparable<ControllerMonitor>, IEquatable<ControllerMonitor>, IDisposable
    {
        private readonly IDateTimeMachine dateTimeMachine;
        private readonly object @lock = new();
        private DateTime timeStamp;
        private bool isConnected;
        private bool disposed;
        private const int TimeOut = 12; // Change this value to set the timeout from the database configuration
        private TimeSpan heartBeatInterval = TimeSpan.FromSeconds(TimeOut);

        /// <summary>
        /// Gets or sets the PlcId.
        /// </summary>
        public int PlcId { get; set; }

        /// <summary>
        /// Gets or sets the MachineId.
        /// </summary>
        public int MachineId { get; set; }

        /// <summary>
        /// Gets or sets the PartNumber.
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Label.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HeartBeat.
        /// </summary>
        public int HeartBeat { get; set; }

        /// <summary>
        /// Gets or sets the CyclesOk.
        /// </summary>
        public int CyclesOk { get; set; }

        /// <summary>
        /// Gets or sets the IP address for network communication with the controller.
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the descriptive name of the controller.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public event EventHandler? ConnectionLost;

        public event EventHandler? ConnectionRecovered;

        public event EventHandler? UpdateReceived;

        /// <summary>
        /// Gets or sets the Parameters.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = [];

        /// <inheritdoc/>
        public DateTime TimeStamp
        {
            get => this.timeStamp;
            set
            {
                bool wasConnected;
                lock (this.@lock)
                {
                    // #120 F5: recovery means dead -> alive, i.e. the connection was NOT alive
                    // before this heartbeat. Capture the previous state BEFORE mutating the
                    // timestamp (IsConnectionAlive reads this.timeStamp).
                    wasConnected = this.isConnected && this.IsConnectionAlive;

                    this.timeStamp = value;
                    this.RestartHeartBeatTimer();
                    this.isConnected = true;
                }

                // #120 F5: events are raised OUTSIDE the lock so subscribers (Blazor circuit
                // threads) can safely read back into this instance without lock-ordering hazards.
                if (!wasConnected)
                {
                    this.OnConnectionRecovered(EventArgs.Empty);
                }

                this.OnUpdateReceived(EventArgs.Empty);  // Raise the UpdateReceived event
            }
        }

        public bool IsConnected
        {
            get
            {
                lock (this.@lock)
                {
                    return this.isConnected && this.IsConnectionAlive;  // Reflect actual desired state
                }
            }
        }

        /// <summary>
        /// Gets or sets the heartbeat timer. Always constructed by the root constructor (every other constructor
        /// chains to it), so it is non-null for the lifetime of the instance without a <c>null!</c> placeholder.
        /// </summary>
        public Timer HeartBeatTimer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerMonitor"/> class with optional date time machine dependency.
        /// </summary>
        /// <param name="dateTimeMachine">The date time machine to use for timestamp operations. Defaults to new DateTimeMachine() if null.</param>
        public ControllerMonitor(IDateTimeMachine? dateTimeMachine = null)
        {
            this.dateTimeMachine = dateTimeMachine ?? new DateTimeMachine();

            // Initialize with default date 2020-01-01 when no dependency provided
            this.timeStamp = this.dateTimeMachine.Now;

            // Assign the timer directly from the factory here (the root constructor) so the compiler's definite-
            // assignment analysis sees HeartBeatTimer set without a null! placeholder; all other constructors chain
            // to this one via `: this(...)`.
            this.HeartBeatTimer = this.CreateHeartBeatTimer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerMonitor"/> class with a name and optional date time machine dependency.
        /// </summary>
        /// <param name="name">The name of the controller.</param>
        /// <param name="dateTimeMachine">The date time machine to use for timestamp operations. Defaults to new DateTimeMachine() if null.</param>
        public ControllerMonitor(string name, IDateTimeMachine? dateTimeMachine = null)
            : this(dateTimeMachine)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerMonitor"/> class with PLC and machine IDs and optional date time machine dependency.
        /// </summary>
        /// <param name="plcId">The PLC identifier.</param>
        /// <param name="machineId">The machine identifier.</param>
        /// <param name="dateTimeMachine">The date time machine to use for timestamp operations. Defaults to new DateTimeMachine() if null.</param>
        public ControllerMonitor(int plcId, int machineId, IDateTimeMachine? dateTimeMachine = null)
            : this(dateTimeMachine)
        {
            this.PlcId = plcId;
            this.MachineId = machineId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerMonitor"/> class with a name, timestamp, and optional date time machine dependency.
        /// </summary>
        /// <param name="name">The name of the controller.</param>
        /// <param name="timeStamp">The initial timestamp.</param>
        /// <param name="dateTimeMachine">The date time machine to use for timestamp operations. Defaults to new DateTimeMachine() if null.</param>
        public ControllerMonitor(string name, DateTime timeStamp, IDateTimeMachine? dateTimeMachine = null)
            : this(dateTimeMachine)
        {
            this.Name = name;
            this.timeStamp = timeStamp;
        }

        // Override GetHashCode to ensure that equal objects have the same hash code

        /// <summary>
        /// Executes GetHashCode operation.
        /// </summary>
        /// <returns>The result of GetHashCode.</returns>
        public override int GetHashCode()
        {
            return this.PlcId.GetHashCode();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerMonitor"/> class.
        /// Initializes a new instance of the class.
        /// </summary>
        public ControllerMonitor()
            : this(null)
        {
        }

        private Timer CreateHeartBeatTimer()
        {
            var timer = new Timer(this.heartBeatInterval.TotalMilliseconds);
            timer.Elapsed += this.OnHeartBeatTimerElapsed;
            timer.AutoReset = false; // Timer should not restart automatically
            return timer;
        }

        private void RestartHeartBeatTimer()
        {
            this.HeartBeatTimer.Stop();
            this.HeartBeatTimer.Start();
        }

        private void OnHeartBeatTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            lock (this.@lock)
            {
                this.isConnected = false;
            }

            this.HeartBeatTimer.Stop();
            this.OnConnectionLost(EventArgs.Empty);
        }

        protected virtual void OnConnectionLost(EventArgs e)
        {
            this.ConnectionLost?.Invoke(this, e);
        }

        protected virtual void OnConnectionRecovered(EventArgs e)
        {
            this.ConnectionRecovered?.Invoke(this, e);
        }

        protected virtual void OnUpdateReceived(EventArgs e)
        {
            this.UpdateReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Releases the heartbeat timer and clears event subscriptions.
        /// #120 F2: a replaced monitor must never keep an armed <see cref="System.Timers.Timer"/>
        /// (stale ConnectionLost events + timer accumulation). Idempotent and null-safe; callable
        /// from the events-service replacement path on a hub-callback thread while Blazor circuit
        /// threads may still hold a stale reference.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases managed resources held by this monitor.
        /// </summary>
        /// <param name="disposing">True when called from <see cref="Dispose()"/>; false from a finalizer path.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                var timer = this.HeartBeatTimer;
                if (timer is not null)
                {
                    timer.Stop();
                    timer.Elapsed -= this.OnHeartBeatTimerElapsed;
                    timer.Dispose();
                }

                // Drop all subscriptions so a stale reference cannot receive (or root) anything further.
                this.ConnectionLost = null;
                this.ConnectionRecovered = null;
                this.UpdateReceived = null;
            }

            this.disposed = true;
        }

        /// <inheritdoc/>
        int IComparable<ControllerMonitor>.CompareTo(ControllerMonitor? other)
        {
            if (other is null)
            {
                return 1;
            }

            return this.PlcId.CompareTo(other.PlcId);
        }

        // Override the base Equals method

        /// <summary>
        /// Executes Equals operation.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The result of Equals.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is ControllerMonitor other)
            {
                // Call the explicit interface implementation directly to avoid recursion
                return this.PlcId == other.PlcId;
            }

            return false;
        }

        /// <inheritdoc/>
        bool IEquatable<ControllerMonitor>.Equals(ControllerMonitor? other)
        {
            return this.PlcId == other?.PlcId;
        }

        /// <summary>
        /// Copies the monitored state of <paramref name="source"/> onto this instance IN PLACE (#126 C11).
        /// The events service keeps ONE canonical <see cref="ControllerMonitor"/> per PLC id for the process
        /// lifetime because Blazor circuits hold direct references to the instances served by the read view;
        /// the former replace-and-dispose per heartbeat made every stale reference throw
        /// <see cref="ObjectDisposedException"/> from <see cref="TimeStamp"/>/<see cref="RefreshConnection"/>
        /// (both restart the disposed heartbeat <see cref="System.Timers.Timer"/>). <see cref="TimeStamp"/>
        /// is copied LAST: its setter re-arms the timer and raises ConnectionRecovered/UpdateReceived, so
        /// every other field must already carry the new state when subscribers observe those events.
        /// </summary>
        /// <param name="source">The incoming (unpublished) monitor carrying the new state.</param>
        public void UpdateFrom(ControllerMonitor? source)
        {
            if (source is null || ReferenceEquals(this, source))
            {
                return;
            }

            this.PlcId = source.PlcId;
            this.MachineId = source.MachineId;
            this.PartNumber = source.PartNumber;
            this.Description = source.Description;
            this.Label = source.Label;
            this.HeartBeat = source.HeartBeat;
            this.CyclesOk = source.CyclesOk;
            this.IpAddress = source.IpAddress;
            this.Name = source.Name;
            this.Parameters = source.Parameters;
            this.TimeStamp = source.TimeStamp; // last — see remarks above
        }

        /// <summary>
        /// Executes RefreshConnection operation.
        /// </summary>
        public void RefreshConnection()
        {
            lock (this.@lock)
            {
                this.TimeStamp = this.dateTimeMachine.Now.AddSeconds(-TimeOut - 1);
                this.RestartHeartBeatTimer();
            }
        }

        public bool IsConnectionAlive => (this.dateTimeMachine.Now - this.timeStamp) < this.heartBeatInterval;

        /// <summary>
        /// Executes MapTo operation.
        /// </summary>
        /// <param name="src">The src.</param>
        /// <returns>The result of MapTo.</returns>
        public static IndQuestResults.Result<ControllerMonitor> MapTo(PlcDto src)
        {
            if (src == null)
            {
                return IndQuestResults.Result<ControllerMonitor>.WithFailure("PlcDto source cannot be null");
            }

            return IndQuestResults.Result<ControllerMonitor>.Success(new ControllerMonitor
            {
                PlcId = src.PlcId,
                MachineId = src.MachineId,
                Name = src.Name,
                IpAddress = src.IpAddress,
                Description = src.PlcType, // or src.Description if available

                // Map other properties as needed
            });
        }

        /// <summary>
        /// Executes ToEntity operation.
        /// </summary>
        /// <param name="src">The src.</param>
        /// <returns>The result of ToEntity.</returns>
        public static IndQuestResults.Result<PlcDto> ToEntity(ControllerMonitor src)
        {
            if (src == null)
            {
                return IndQuestResults.Result<PlcDto>.WithFailure("ControllerMonitor source cannot be null");
            }

            return IndQuestResults.Result<PlcDto>.Success(new PlcDto
            {
                PlcId = src.PlcId,
                MachineId = src.MachineId,
                Name = src.Name,
                IpAddress = src.IpAddress,
                PlcType = src.Description, // or src.PlcType if available

                // Map other properties as needed
            });
        }

        /// <summary>
        /// Executes ToString operation.
        /// </summary>
        /// <returns>The result of ToString.</returns>
        public override string ToString()
        {
            return $"PlcId: {this.PlcId}, MachineId: {this.MachineId}, IpAddress: {this.IpAddress}, Name: {this.Name}";
        }

    }
}