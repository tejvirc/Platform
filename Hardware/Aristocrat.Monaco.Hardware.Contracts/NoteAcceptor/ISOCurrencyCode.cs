// ReSharper disable CommentTypo
// ReSharper disable UnusedMember.Global
namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System.ComponentModel;

    /// <summary>ISO 4217 enumerations.</summary>
    public enum ISOCurrencyCode
    {
        /// <summary>United Arab Emirates dirham United Arab Emirates</summary>
        AED,

        /// <summary>Afghani Afghanistan.</summary>
        AFN,

        /// <summary>Lek Albania.</summary>
        ALL,

        /// <summary>Armenian dram Armenia.</summary>
        AMD,

        /// <summary>Netherlands Antillean guilder Netherlands Antilles.</summary>
        ANG,

        /// <summary>Kwanza Angola.</summary>
        AOA,

        /// <summary>Argentine peso Argentina.</summary>
        ARS,

        /// <summary>
        ///     Australian dollar Australia, Australian Antarctic Territory, Christmas Island, Cocos (Keeling) Islands, Heard
        ///     and McDonald Islands, Kiribati, Nauru, Norfolk Island, Tuvalu.
        /// </summary>
        AUD,

        /// <summary>Aruban guilder Aruba.</summary>
        AWG,

        /// <summary>Azerbaijanian manat Azerbaijan.</summary>
        AZN,

        /// <summary>Convertible marks Bosnia and Herzegovina.</summary>
        BAM,

        /// <summary>Barbados dollar Barbados.</summary>
        BBD,

        /// <summary>Bangladeshi taka Bangladesh.</summary>
        BDT,

        /// <summary>Bulgarian lev Bulgaria.</summary>
        BGN,

        /// <summary>Bahraini dinar Bahrain.</summary>
        BHD,

        /// <summary>Burundian franc Burundi.</summary>
        BIF,

        /// <summary>Bermudian dollar (customarily known as Bermuda dollar) Bermuda.</summary>
        BMD,

        /// <summary>Brunei dollar Brunei, Singapore</summary>
        BND,

        /// <summary>Boliviano Bolivia.</summary>
        BOB,

        /// <summary>Bolivian Mvdol (funds code) Bolivia.</summary>
        BOV,

        /// <summary>Brazilian real Brazil.</summary>
        BRL,

        /// <summary>Bahamian dollar Bahamas.</summary>
        BSD,

        /// <summary>Ngultrum Bhutan.</summary>
        BTN,

        /// <summary>Pula Botswana.</summary>
        BWP,

        /// <summary>Belarussian ruble Belarus.</summary>
        BYR,

        /// <summary>Belize dollar Belize.</summary>
        BZD,

        /// <summary>Canadian dollar Canada.</summary>
        [Description("Canadian Dollar ($)")] CAD,

        /// <summary>Franc Congolais Democratic Republic of Congo.</summary>
        CDF,

        /// <summary>WIR euro (complementary currency) Switzerland.</summary>
        CHE,

        /// <summary>Swiss franc Switzerland, Liechtenstein.</summary>
        CHF,

        /// <summary>WIR franc (complementary currency) Switzerland.</summary>
        CHW,

        /// <summary>Unidad de Fomento (funds code) Chile.</summary>
        CLF,

        /// <summary>Chilean peso Chile.</summary>
        CLP,

        /// <summary>Renminbi Mainland China.</summary>
        CNY,

        /// <summary>Colombian peso Colombia.</summary>
        COP,

        /// <summary>Unidad de Valor Real Colombia.</summary>
        COU,

        /// <summary>Costa Rican colon Costa Rica.</summary>
        CRC,

        /// <summary>Cuban peso Cuba.</summary>
        CUP,

        /// <summary>Cape Verde escudo Cape Verde.</summary>
        CVE,

        /// <summary>Czech koruna Czech Republic.</summary>
        CZK,

        /// <summary>Djibouti franc Djibouti.</summary>
        DJF,

        /// <summary>Danish krone Denmark, Faroe Islands, Greenland.</summary>
        DKK,

        /// <summary>Dominican peso Dominican Republic.</summary>
        DOP,

        /// <summary>Algerian dinar Algeria.</summary>
        DZD,

        /// <summary>Kroon Estonia.</summary>
        EEK,

        /// <summary>Egyptian pound Egypt.</summary>
        EGP,

        /// <summary>Nakfa Eritrea.</summary>
        ERN,

        /// <summary>Ethiopian birr Ethiopia.</summary>
        ETB,

        /// <summary>Euro 15 European Union countries, Andorra, Kosovo, Monaco, Montenegro, San Marino, Vatican; see eurozone.</summary>
        EUR,

        /// <summary>Fiji dollar Fiji.</summary>
        FJD,

        /// <summary>Falkland Islands pound Falkland Islands.</summary>
        FKP,

        /// <summary>
        ///     Pound sterling United Kingdom, Crown Dependencies (the Isle of Man and the Channel Islands), certain British
        ///     Overseas Territories (South Georgia and the South Sandwich Islands, British Antarctic Territory and British Indian
        ///     Ocean Territory).
        /// </summary>
        GBP,

        /// <summary>Lari Georgia.</summary>
        GEL,

        /// <summary>Cedi Ghana.</summary>
        GHS,

        /// <summary>Gibraltar pound Gibraltar.</summary>
        GIP,

        /// <summary>Dalasi Gambia.</summary>
        GMD,

        /// <summary>Guinea franc Guinea.</summary>
        GNF,

        /// <summary>Quetzal Guatemala.</summary>
        GTQ,

        /// <summary>Guyana dollar Guyana.</summary>
        GYD,

        /// <summary>Hong Kong dollar Hong Kong Special Administrative Region.</summary>
        HKD,

        /// <summary>Lempira Honduras.</summary>
        HNL,

        /// <summary>Croatian kuna Croatia.</summary>
        HRK,

        /// <summary>Haiti gourde Haiti.</summary>
        HTG,

        /// <summary>Forint Hungary.</summary>
        HUF,

        /// <summary>Rupiah Indonesia.</summary>
        IDR,

        /// <summary>Israeli new sheqel Israel.</summary>
        ILS,

        /// <summary>Indian rupee Bhutan, India.</summary>
        INR,

        /// <summary>Iraqi dinar Iraq.</summary>
        IQD,

        /// <summary>Iranian rial Iran.</summary>
        IRR,

        /// <summary>Iceland krona Iceland.</summary>
        ISK,

        /// <summary>Jamaican dollar Jamaica.</summary>
        JMD,

        /// <summary>Jordanian dinar Jordan.</summary>
        JOD,

        /// <summary>Japanese yen Japan.</summary>
        JPY,

        /// <summary>Kenyan shilling Kenya.</summary>
        KES,

        /// <summary>Som Kyrgyzstan.</summary>
        KGS,

        /// <summary>Riel Cambodia.</summary>
        KHR,

        /// <summary>Comoro franc Comoros.</summary>
        KMF,

        /// <summary>North Korean won North Korea.</summary>
        KPW,

        /// <summary>South Korean won South Korea.</summary>
        KRW,

        /// <summary>Kuwaiti dinar Kuwait.</summary>
        KWD,

        /// <summary>Cayman Islands dollar Cayman Islands.</summary>
        KYD,

        /// <summary>Tenge Kazakhstan.</summary>
        KZT,

        /// <summary>The country Kip Laos.</summary>
        LAK,

        /// <summary>Lebanese pound Lebanon.</summary>
        LBP,

        /// <summary>Sri Lanka rupee Sri Lanka.</summary>
        LKR,

        /// <summary>Liberian dollar Liberia.</summary>
        LRD,

        /// <summary>Loti Lesotho.</summary>
        LSL,

        /// <summary>Lithuanian litas Lithuania.</summary>
        LTL,

        /// <summary>Latvian lats Latvia.</summary>
        LVL,

        /// <summary>Libyan dinar Libya.</summary>
        LYD,

        /// <summary>Moroccan dirham Morocco, Western Sahara.</summary>
        MAD,

        /// <summary>Moldovan leu Moldova.</summary>
        MDL,

        /// <summary>Malagasy ariary Madagascar.</summary>
        MGA,

        /// <summary>Denar The former Yugoslav Republic of Macedonia.</summary>
        MKD,

        /// <summary>Kyat Myanmar.</summary>
        MMK,

        /// <summary>Tugrik Mongolia.</summary>
        MNT,

        /// <summary>Pataca Macau Special Administrative Region.</summary>
        MOP,

        /// <summary>Ouguiya Mauritania.</summary>
        MRU,

        /// <summary>Mauritius rupee Mauritius.</summary>
        MUR,

        /// <summary>Rufiyaa Maldives.</summary>
        MVR,

        /// <summary>Kwacha Malawi.</summary>
        MWK,

        /// <summary>Mexican peso Mexico.</summary>
        MXN,

        /// <summary>Mexican Unidad de Inversion (UDI) (funds code) Mexico.</summary>
        MXV,

        /// <summary>Malaysian ringgit Malaysia.</summary>
        MYR,

        /// <summary>Metical Mozambique.</summary>
        MZN,

        /// <summary>Namibian dollar Namibia.</summary>
        NAD,

        /// <summary>Naira Nigeria.</summary>
        NGN,

        /// <summary>Cordoba oro Nicaragua.</summary>
        NIO,

        /// <summary>Norwegian krone Norway.</summary>
        NOK,

        /// <summary>Nepalese rupee Nepal.</summary>
        NPR,

        /// <summary>New Zealand dollar Cook Islands, New Zealand, Niue, Pitcairn, Tokelau.</summary>
        NZD,

        /// <summary>Rial Omani Oman.</summary>
        OMR,

        /// <summary>Balboa Panama.</summary>
        PAB,

        /// <summary>Nuevo sol Peru.</summary>
        PEN,

        /// <summary>Kina Papua New Guinea.</summary>
        PGK,

        /// <summary>Philippine peso Philippines.</summary>
        PHP,

        /// <summary>Pakistan rupee Pakistan.</summary>
        PKR,

        /// <summary>Zloty Poland.</summary>
        PLN,

        /// <summary>Guarani Paraguay.</summary>
        PYG,

        /// <summary>Qatari rial Qatar.</summary>
        QAR,

        /// <summary>Romanian new leu Romania.</summary>
        RON,

        /// <summary>Serbian dinar Serbia.</summary>
        RSD,

        /// <summary>Russian rouble Russia, Abkhazia, South Ossetia.</summary>
        RUB,

        /// <summary>Rwanda franc Rwanda.</summary>
        RWF,

        /// <summary>Saudi riyal Saudi Arabia.</summary>
        SAR,

        /// <summary>Solomon Islands dollar Solomon Islands.</summary>
        SBD,

        /// <summary>Seychelles rupee Seychelles.</summary>
        SCR,

        /// <summary>Sudanese pound Sudan.</summary>
        SDG,

        /// <summary>Swedish krona Sweden.</summary>
        SEK,

        /// <summary>Singapore dollar Singapore, Brunei.</summary>
        SGD,

        /// <summary>Saint Helena pound Saint Helena.</summary>
        SHP,

        /// <summary>Slovak koruna Slovakia.</summary>
        SKK,

        /// <summary>Leone Sierra Leone.</summary>
        SLL,

        /// <summary>Somali shilling Somalia.</summary>
        SOS,

        /// <summary>Surinam dollar Suriname.</summary>
        SRD,

        /// <summary>Dobra São Tomé and Príncipe.</summary>
        STN,

        /// <summary>Syrian pound Syria.</summary>
        SYP,

        /// <summary>Lilangeni Swaziland.</summary>
        SZL,

        /// <summary>Baht Thailand.</summary>
        THB,

        /// <summary>Somoni Tajikistan.</summary>
        TJS,

        /// <summary>Manat Turkmenistan.</summary>
        TMM,

        /// <summary>Tunisian dinar Tunisia.</summary>
        TND,

        /// <summary>Pa'anga Tonga.</summary>
        TOP,

        /// <summary>New Turkish lira Turkey.</summary>
        TRY,

        /// <summary>Trinidad and Tobago dollar Trinidad and Tobago.</summary>
        TTD,

        /// <summary>
        ///     New Taiwan dollar Taiwan and other islands that are under the effective control of the Republic of China
        ///     (ROC).
        /// </summary>
        TWD,

        /// <summary>Tanzanian shilling Tanzania.</summary>
        TZS,

        /// <summary>Hryvnia Ukraine.</summary>
        UAH,

        /// <summary>Uganda shilling Uganda.</summary>
        UGX,

        /// <summary>
        ///     US dollar American Samoa, British Indian Ocean Territory, Ecuador, El Salvador, Guam, Haiti, Marshall Islands,
        ///     Micronesia, Northern Mariana Islands, Palau, Panama, Puerto Rico, Timor-Leste, Turks and Caicos Islands, United
        ///     States, Virgin Islands.
        /// </summary>
        [Description("US Dollar ($)")] USD,

        /// <summary>United States dollar (next day) (funds code) United States.</summary>
        USN,

        /// <summary>
        ///     United States dollar (same day) (funds code) (one source claims it is no longer used, but it is still on the
        ///     ISO 4217-MA list) United States.
        /// </summary>
        USS,

        /// <summary>Peso Uruguayo Uruguay.</summary>
        UYU,

        /// <summary>Uzbekistan som Uzbekistan.</summary>
        UZS,

        /// <summary>Venezuelan bolívar fuerte Venezuela.</summary>
        VEF,

        /// <summary>Vietnamese d?ng Vietnam.</summary>
        VND,

        /// <summary>Vatu Vanuatu.</summary>
        VUV,

        /// <summary>Samoan tala Samoa.</summary>
        WST,

        /// <summary>CFA franc BEAC Cameroon, Central African Republic, Congo, Chad, Equatorial Guinea, Gabon.</summary>
        XAF,

        /// <summary>Silver (one troy ounce).</summary>
        XAG,

        /// <summary>Gold (one troy ounce).</summary>
        XAU,

        /// <summary>European Composite Unit (EURCO) (bond market unit).</summary>
        XBA,

        /// <summary>European Monetary Unit (E.M.U.-6) (bond market unit).</summary>
        XBB,

        /// <summary>European Unit of Account 9 (E.U.A.-9) (bond market unit).</summary>
        XBC,

        /// <summary>European Unit of Account 17 (E.U.A.-17) (bond market unit).</summary>
        XBD,

        /// <summary>
        ///     East Caribbean dollar Anguilla, Antigua and Barbuda, Dominica, Grenada, Montserrat, Saint Kitts and Nevis,
        ///     Saint Lucia, Saint Vincent and the Grenadines.
        /// </summary>
        XCD,

        /// <summary>Special Drawing Rights International Monetary Fund.</summary>
        XDR,

        /// <summary>UIC franc (special settlement currency) International Union of Railways.</summary>
        XFU,

        /// <summary>CFA Franc BCEAO Benin, Burkina Faso, Côte d'Ivoire, Guinea-Bissau, Mali, Niger, Senegal, Togo.</summary>
        XOF,

        /// <summary>Palladium (one troy ounce).</summary>
        XPD,

        /// <summary>CFP franc French Polynesia, New Caledonia, Wallis and Futuna.</summary>
        XPF,

        /// <summary>Platinum (one troy ounce).</summary>
        XPT,

        /// <summary>Code reserved for testing purposes.</summary>
        XTS,

        /// <summary>No currency.</summary>
        XXX,

        /// <summary>Yemeni rial Yemen.</summary>
        YER,

        /// <summary>South African rand South Africa.</summary>
        ZAR,

        /// <summary>Kwacha Zambia.</summary>
        ZMW,

        /// <summary>Zimbabwe dollar Zimbabwe.</summary>
        ZWD
    }
}