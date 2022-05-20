Command Line Arguments
======================

<table>
  <colgroup>
    <col style="width: 29%" />
    <col style="width: 12%" />
    <col style="width: 57%" />
  </colgroup>
  <thead>
    <tr class="header">
      <th><strong>Key</strong></th>
      <th><strong>Value</strong></th>
      <th><strong>Description</strong></th>
    </tr>
  </thead>
  <tbody>
    <tr class="odd">
      <td>display</td>
      <td>windowed</td>
      <td>
        <p>Sets the platform to windowed mode.</p>
        <p>Example: Bootstrap display=windowed</p>
      </td>
    </tr>
    <tr class="even">
      <td>simulateLcdButtonDeck</td>
      <td>true</td>
      <td>
        <p>
          Shows the button deck simulator window in the platform to
          debug/develop without button deck hardware. This is optional if you do
          not need/want it to reduce clutter on the screen.
        </p>
        <p>
          <strong>NOTE:</strong>
          <em
            >This flag is mutually exclusive with "showVirtualButtonDeckAlways"
            as a game will not support rendering to both displays at the same
            time.</em
          >
        </p>
        <p>Example: Bootstrap simulateLcdButtonDeck=true</p>
      </td>
    </tr>
    <tr class="odd">
      <td>simulateVirtualButtonDeck</td>
      <td>true</td>
      <td>
        <p>
          Shows the virtual button deck window even if there is not a 3rd
          monitor to debug/develop outside of a rig with virtual button deck
          hardware. This is optional if you do not need/want it to reduce
          clutter on the screen.
        </p>
        <p>
          <strong>NOTE:</strong>
          <em
            >This flag is mutually exclusive with "showButtonDeckSimulator" as a
            game will not support rendering to both displays at the same
            time.</em
          >
        </p>
        <p>Example: Bootstrap simulateVirtualButtonDeck=true</p>
      </td>
    </tr>
    <tr class="even">
      <td>showTestTool</td>
      <td>true</td>
      <td>
        <p>Shows a utility test tool for internal development or testing.</p>
        <p>Example: Bootstrap showTestTool=true</p>
      </td>
    </tr>
    <tr class="odd">
      <td>maxmeters</td>
      <td>true</td>
      <td>
        <p>
          This should set the platform meters to their maximum possible value.
          This is useful for roll-over testing.
        </p>
        <p>
          <strong>NOTE:</strong>
          <em
            >To use this parameter the meters must be at 0. It is recommended
            you perform a RAM clear or run this parameter on a fresh
            installation for it to work properly.</em
          >
        </p>
        <p>Example: Bootstrap maxmeters=true</p>
      </td>
    </tr>
    <tr class="even">
      <td>showMouseCursor</td>
      <td>true</td>
      <td>
        <p>
          Shows the windows cursor (and touch cross hairs) in full-screen.
          Cursor is always shown in windowed mode. During normal operation, the
          player should not observe the cross hairs tracking on the touchscreen.
          <a href="http://jerry.ali.local/browse/VLT-3051">VLT-3051</a>.
        </p>
        <p>Example: Bootstrap showMouseCursor=true</p>
      </td>
    </tr>
    <tr class="odd">
      <td>runtimeArgs</td>
      <td>string</td>
      <td>
        <p>
          A list of command line args that will be passed to runtime when
          launching a game.
        </p>
        <p>Example: Bootstrap runtimeArgs="--plugin=perfplugin.dll"</p>
      </td>
    </tr>
    <tr class="even">
      <td>AutoConfigFile</td>
      <td>string</td>
      <td>
        <p>A file path of an auto-config file, relative to Bootstrap.exe.</p>
        <p>Example: Bootstrap.exe AutoConfigFile="ALC FakeID.xml"</p>
        <p>Example: Bootstrap.exe AutoConfigFile="../../Some File.xml"</p>
        <p>
          See <a
            href="file:///C:\display\MON\Auto+Configuration+-+Aristocrat+VLT+Platform"
            >Auto Configuration - Aristocrat VLT Platform</a
          >
          for more information
        </p>
      </td>
    </tr>
    <tr class="odd">
      <td>SystemKey</td>
      <td>string</td>
      <td>
        Path to the public key from the public/private key pair that was used to
        generate the signature line in the manifest files for the platform and
        runtime ISOs
      </td>
    </tr>
    <tr class="even">
      <td>GameKey</td>
      <td>string</td>
      <td>
        Path to the public key from the public/private key pair that was used to
        generate the signature line in the manifest files for the game ISOs
      </td>
    </tr>
    <tr class="odd">
      <td>SmartCardKey</td>
      <td>string</td>
      <td>
        Path to the public key from the public/private key pair that was used to
        generate Smart Card(s). This key is used for both the onboard Smart Card
        and the eKey.
      </td>
    </tr>
    <tr class="even">
      <td>DisplayFakePrinterTickets</td>
      <td>true</td>
      <td>
        Displays printed tickets in a MessageBox when using the FakePrinter
      </td>
    </tr>
    <tr class="odd">
      <td>ignoreTouchCalibration</td>
      <td>none</td>
      <td>
        <p>
          Skips the check for proper touch screen calibration on initial config.
        </p>
        <p>
          <strong>NOTE:</strong>
          <em
            >The touch calibration screens will still appear during Initial
            Configuration. This flag allows bypassing the calibration by
            pressing ENTER on the keyboard to skip through each screen.</em
          >
        </p>
        <p>Example: Bootstrap.exe ignoreTouchCalibration</p>
      </td>
    </tr>
    <tr class="even">
      <td>readonlymediaoptional</td>
      <td>true</td>
      <td>
        <p>
          Allows you to run without having read only media as the second drive. 
        </p>
        <p>Example: Bootstrap.exe readonlymediaoptional=true</p>
      </td>
    </tr>
    <tr class="odd">
      <td>audioDeviceOptional</td>
      <td>true</td>
      <td>
        <p>
          Allows you to run without having an audio device attached if required
          by jurisdiction.
        </p>
        <p>Example: Bootstrap.exe audioDeviceOptional=true </p>
      </td>
    </tr>
    <tr class="even">
      <td>horseAnimationOff</td>
      <td>true</td>
      <td>
        Will not show the running horses animation. This applies only to the
        Kentucky HHR jurisdiction.<br />
        Example: Bootsrap.exe horseAnimationOff=true
      </td>
    </tr>
    <tr class="odd">
      <td>maxFailedPollCount</td>
      <td>int</td>
      <td>
        Optional value to use for maximum failed poll count before disconnecting
        a serial device.  Default is 3.  Intended for use with Device Simulator
        when running on same system, recommend using maxPollFailedCount=100 to
        avoid disconnects caused by system lag. 
      </td>
    </tr>
  </tbody>
</table>
