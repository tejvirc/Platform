namespace Vgt.Client12.Hardware.IO
{
    public class Gen8PCI
    {
        private const int HWO_LEGACY_DEVICE = 0;
        private const int HWO_FPGA_REGISTERS = 0x00000200;		        // FPGA registers, (0x00000200 - 0x000003ff)
        private const int HWO_INTERRUPT_CONTROLER = 0x00000400;		    // Interrup controler, (0x00000400 - 0x000005ff)
        private const int HWO_UARTS = 0x00000600;		                // UARTS (0x00000600 - 0x000007ff)
        private const int HWO_SMART_CARD = 0x00000800;		            // Smart Card (0x00000800 - 0x000009ff)
        private const int HWO_I2C = 0x00000A00;		                    // I2C master controller
        private const int HWO_ONEWIRE = 0x00000C00;		                // One wire controller
        private const int HWO_BAR2_RESERVED = 0x00000E00;		        // Reserved (0x00000e00 - 0x00000fff)


        public enum Legacy : int
        {
            HWO_SWSEC = HWO_LEGACY_DEVICE + 0x00,                // Mechanical door Switches (R) (8) bit(8 - 15), Latch seal (R) bit(1-7) bit 0 (W) RESDOOR 
            HWO_OPSEC = HWO_LEGACY_DEVICE + 0x08,                // Optic switch (R) bit(0-3) enable optical emitters (w) bit0
            HWO_HOPPER = HWO_LEGACY_DEVICE + 0x10,                // Hopper (R/W)
            HWO_COINVAL = HWO_LEGACY_DEVICE + 0x18,                // coin validator I/O (R/W)
            HWO_COINDIV = HWO_LEGACY_DEVICE + 0x20,               // Coin Diverter I/O (R/W) 
            HWO_VFM4 = HWO_LEGACY_DEVICE + 0x28,                // VFM4 (R/W)
            HWO_MPIO = HWO_LEGACY_DEVICE + 0x30,                // MPIO General purpose input/output
            HWO_AUDIT = HWO_LEGACY_DEVICE + 0x38,                // Key Switches bit (0-2), meter light control (w) bit 0
            HWO_JPBELL = HWO_LEGACY_DEVICE + 0x40,                // Jackpot Bell (w) bit 0
            HWO_FEXPINT = HWO_LEGACY_DEVICE + 0x48,                // FEXPINT (R/W)
            HWO_FEXPILVL = HWO_LEGACY_DEVICE + 0x50,                // FEXPICTL register FEXPILVL bit1, FEXPIOE    bit0
            HWO_FEXPOE = HWO_LEGACY_DEVICE + 0x58,                // FEXPOE (R/W)
            HWO_FEXPIO = HWO_LEGACY_DEVICE + 0x60,                // FEXPIO
            HWO_MIKOHN = HWO_LEGACY_DEVICE + 0x68,                // Mikohn pulse output bit 0 (w)
            HWO_LOWPWR = HWO_LEGACY_DEVICE + 0x70,                // Low power mode (w)
            HWO_HANDLE = HWO_LEGACY_DEVICE + 0x78,                // Mikohn communication I/O signal (w)
            HWO_CFCTL = HWO_LEGACY_DEVICE + 0x80,                // Compat flash control I/O
            HWO_USBCTL = HWO_LEGACY_DEVICE + 0x88,                // USB voltage enable bit(0-6) (R/W)
            HWO_HWID = HWO_LEGACY_DEVICE + 0x90,                // Hardware id audio AM id bit(4-6) backplane id bit(0-2)
            HWO_SPIDATA = HWO_LEGACY_DEVICE + 0x98,                // SPIDATA bit(0-7) (R/W)
            HWO_SPICTL = HWO_LEGACY_DEVICE + 0xA0,                // SPICTL bit(0-7)
            HWO_SPIRST = HWO_LEGACY_DEVICE + 0xA8,                // SPIRST bit(0,1) (w)
            HWO_FSPIDATA = HWO_LEGACY_DEVICE + 0xB0,                // FSPIDATA
            HWO_FSPICTL = HWO_LEGACY_DEVICE + 0xB8,                // FSPICTL
            HWO_FSPIRST = HWO_LEGACY_DEVICE + 0xC0,                // FSPIRST
            HWO_BTEST = HWO_LEGACY_DEVICE + 0xC8,                // battery test bit (0, 1) 
            HWO_FANSPEED = HWO_LEGACY_DEVICE + 0xD0,                // FAN speed (bit0-7) (R)
            HWO_FANPWM = HWO_LEGACY_DEVICE + 0xD8,                // PWW output duty cycle value bit(0-7) R/W
            HWO_DBGDISP = HWO_LEGACY_DEVICE + 0xE0,                // 7-Segment debug display
            HWO_SRAMEN = HWO_LEGACY_DEVICE + 0xE8,                // SRAM    Write enable
            HWO_RNG = HWO_LEGACY_DEVICE + 0xF0,                // Random number generator (R/W)
            HWO_SYSSTAT = HWO_LEGACY_DEVICE + 0xF8,                // System Status
            HWO_ITEST = HWO_LEGACY_DEVICE + 0x100,                // ITEST 
            HWO_SRAM_BC_START = HWO_LEGACY_DEVICE + 0x108,       // SRAM Block Check Start Address
            HWO_SRAM_BC_END = HWO_LEGACY_DEVICE + 0x110,       // SRAM Block Check End Address
            HWO_SRAM_BC_CTRL = HWO_LEGACY_DEVICE + 0x118,       // SRAM Block Check Control/Status Register

                        // FPGA Register Area (HWO_FPGA_REGISTERS+ )
            HWO_FPGA_VERSION = HWO_FPGA_REGISTERS + 0x0000,            // FPGA Version Register FPGA_VERSION (R/W)
            HWO_BUS_CLK = HWO_FPGA_REGISTERS + 0x0004,            // Bus clock resgister (old WB clcok)
            HWO_SECCTL = HWO_FPGA_REGISTERS + 0x0008,            // Security control resgister
            HWO_FPGA_USN_L = HWO_FPGA_REGISTERS + 0x0018,            // FPGA Version Register FPGA_VERSION (R/W)
            HWO_FPGA_USN_U = HWO_FPGA_REGISTERS + 0x001C,            // FPGA Version Register FPGA_VERSION (R/W)

                        // Interrupt Controller Area (HWO_INTERRUPT_CONTROLER + )
            HWO_FPGA_ISR = HWO_INTERRUPT_CONTROLER + 0x00,        // FPGA Interrupt Status Register (R)
            HWO_FPGA_IMR = HWO_INTERRUPT_CONTROLER + 0x10,        // FPGA Interrupt Mask Register (R)
            HWO_FPGA_IER = HWO_INTERRUPT_CONTROLER + 0x14,        // FPGA Interrupt Enable Register FPIDIS (W)
            HWO_FPGA_IDIS = HWO_INTERRUPT_CONTROLER + 0x18        // FPGA Interrupt Disable Register FPIDIS (W)
        }

        public enum BatteryTest : int
        {
            // battery test register mask
            // read
            BT_NFAILBATT = 0x02,
            BT_BMONO = 0x01,
                        // write
            BT_RESET = 0x00,
            BT_TEST_2 = 0x01,
            BT_TEST_1 = 0x02,
            BT_NOTEST = 0x03,

        }
    }
}
