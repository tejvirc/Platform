namespace Aristocrat.Monaco.Hardware.Contracts.Cabinet
{
    using System.Collections.Generic;
    using Aristocrat.Cabinet.Contracts;

    /// <summary>
    ///     Provides a mechanism to get information on the Cabinet
    /// </summary>
    public interface ICabinetDetectionService
    {
        /// <summary>
        ///     Type of the cabinet
        /// </summary>
        CabinetType Type { get; }

        /// <summary>
        ///     Returns Hardware Family
        /// </summary>
        HardwareFamily Family { get; }

        /// <summary>
        ///     Id of the cabinet
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     Returns number of screens connected during platform initialization
        /// </summary>
        int NumberOfDisplaysConnectedDuringInitialization { get; }

        /// <summary>
        ///     Returns number of Displays connected
        /// </summary>
        int NumberOfDisplaysConnected { get; }

        /// <summary>
        ///     Returns ButtonDeck Type connected to Cabinet
        /// </summary>
        string ButtonDeckType { get; }

        /// <summary>
        ///     Returns cabinet's all expected devices.
        /// </summary>
        IReadOnlyCollection<IDevice> CabinetExpectedDevices { get; }

        /// <summary>
        ///     Returns cabinet's expected display devices.
        /// </summary>
        IEnumerable<IDisplayDevice> ExpectedDisplayDevices { get; }

        /// <summary>
        ///     Returns cabinet's expected display devices with serial touch.
        /// </summary>
        IEnumerable<IDisplayDevice> ExpectedDisplayDevicesWithSerialTouch { get; }

        /// <summary>
        ///     Returns cabinet's expected serial touch devices.
        /// </summary>
        IEnumerable<ITouchDevice> ExpectedSerialTouchDevices { get; }

        /// <summary>
        ///     Returns cabinet's expected touch devices.
        /// </summary>
        IEnumerable<ITouchDevice> ExpectedTouchDevices { get; }

        /// <summary>
        ///     Returns true if touchscreen mapping exists.
        /// </summary>
        bool TouchscreensMapped { get; }

        /// <summary>
        ///     Gets the <see cref="IDisplayDevice"/> mapped to the given <see cref="ITouchDevice"/>.
        /// </summary>
        /// <param name="touchDevice">The touch device.</param>
        /// <returns>The mapped <see cref="IDisplayDevice"/>, or <c>null</c> if unmapped.</returns>
        IDisplayDevice GetDisplayMappedToTouchDevice(ITouchDevice touchDevice);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="touchDevice"></param>
        /// <returns></returns>
        DisplayRole? GetDisplayRoleMappedToTouchDevice(ITouchDevice touchDevice);

        /// <summary>
        ///     Gets the <see cref="ITouchDevice"/> mapped to the given <see cref="IDisplayDevice"/>.
        /// </summary>
        /// <param name="displayDevice">The display device.</param>
        /// <returns>The mapped <see cref="ITouchDevice"/>, or <c>null</c> if unmapped.</returns>
        ITouchDevice GetTouchDeviceMappedToDisplay(IDisplayDevice displayDevice);

        /// <summary>
        ///     Returns DisplayDevice for a given Role.
        /// </summary>
        /// <param name="role">DisplayRole for which DisplayDevice is required.</param>
        IDisplayDevice GetDisplayDeviceByItsRole(DisplayRole role);

        /// <summary>
        ///     Apply Display settings on all displays connected
        /// </summary>
        void ApplyDisplaySettings();

        /// <summary>
        ///     Refresh status of all cabinet devices
        /// </summary>
        void RefreshCabinetDeviceStatus();

        /// <summary>
        ///     Check if the identified cabinet type is the same as the given cabinet type string.
        /// </summary>
        bool IsCabinetType(string cabinetType);

        /// <summary>
        ///     Check if the VBD is touch device or not.
        /// </summary>
        bool IsTouchVbd();

        /// <summary>
        ///     Maps touchscreens to displays.
        /// </summary>
        bool MapTouchscreens(bool persistMapping = false);

        ///// <summary>
        /////     Maps touchscreens to displays. Passes back a collection of <see cref="DisplayDevice"/> to
        /////     <see cref="TouchDevice"/> mappings as represented in the OS.
        ///// </summary>
        ///// <param name="mappings"></param>
        ///// <param name="persistMappings"></param>
        ///// <returns></returns>
        //bool MapTouchscreens(
        //    out IReadOnlyCollection<(DisplayDevice Display, TouchDevice Touch)> mappings,
        //    bool persistMappings = false);

        /// <summary>
        ///     Returns the touch device by touch id.
        /// </summary>
        /// <param name="touchDeviceId"></param>
        /// <returns></returns>
        ITouchDevice TouchDeviceByCursorId(int touchDeviceId);

        /// <summary>
        ///     Returns the touch device firmware version.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        string GetFirmwareVersion(ITouchDevice device);

        /// <summary>
        ///     Api to fetch Topmost display on cabinet.
        ///     The Display order of cabinet is - Topper, Top, Main, VBD
        /// </summary>
        /// <returns>Returns topmost display on the cabinet.</returns>
        DisplayRole GetTopmostDisplay();

        /// <summary>
        ///     Is the display expected to be on the cabinet
        /// </summary>
        /// <param name="role">The display role</param>
        bool IsDisplayExpected(DisplayRole role);

        /// <summary>
        ///     Is the display connected 
        /// </summary>
        /// <param name="role">The display role</param>
        bool IsDisplayConnected(DisplayRole role);

        /// <summary>
        ///     Is the display either configured and present or not configured. Basically saying
        ///     is everything fine with this display role.
        /// </summary>
        /// <param name="role">The display role</param>
        bool IsDisplayConnectedOrNotExpected(DisplayRole role);

        /// <summary>
        ///     Is the display configured but not present. Basically saying is this display role
        ///     missing when it should be available.
        /// </summary>
        /// <param name="role">The display role</param>
        bool IsDisplayExpectedAndDisconnected(DisplayRole role);

        /// <summary>
        ///     Is the display expected and connected. Basically saying is this display not
        ///     only expected but currently present.
        /// </summary>
        /// <param name="role">The display role</param>
        /// <returns></returns>
        bool IsDisplayExpectedAndConnected(DisplayRole role);
    }
}