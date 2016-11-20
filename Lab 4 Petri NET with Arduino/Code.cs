//C#
//using System.IO.Ports
//using System.Threading
/***********************************
	PETRI NET STAMPING MACHINE
			AUTHORS
		FARID A. TOLBAH 
		WALEED EL-BADRY
/***********************************/

/***********************************************************
|             INPUTS                                       |
| pSTART        |   START Button                           |
| pLS1          |   Limit Switch on Start of Cylinder A    |
| pLS2          |   Limit Switch on End of Cylinder A      |
| pLS3          |   Limit Switch on Start of Cylinder B    |
| pLS4          |   Limit Switch on End of Cylinder B      |
***********************************************************/

/*****************************************
             OUTPUTS
| pEXTA          |   Extend Cylinder A   |
| pEXTB          |   Extend Cylinder B   |
| pRETB          |   Retract Cylinder B  |
| pRETA          |   Retract Cylinder A  |
*****************************************/
/*   --------
    /         \  "Cylinder A"
   /           \
---             ---
         -
        /  \     "Cylinder B"
       /    \     
-------      --------
*/

////////////////////////////////////
// Triggers are discussed in lab
////////////////////////////////////

ArduinoUno ObjArduinoUno=null; // Arduino Uno Firmata Object

///////////////////////////////////
//Arduino Pin Assignment        //
/////////////////////////////////
int swSTART=10,swLS1=6,swLS2=7,swLS3=8,swLS4=9;   
int solEXTA=2,solEXTB=5,solRETB=4,solRETA=3;

////////////////////////////////////////////////
//Create place object for each switch/solenoid//
///////////////////////////////////////////////
Place pSTART=new Place();
Place pLS1=new Place();
Place pLS2=new Place(); 
Place pLS3=new Place();
Place pLS4=new Place();

Place pEXTA=new Place();
Place pEXTB=new Place();
Place pRETB=new Place();
Place pRETA=new Place();

public void Step(int k)
{
	//First time initialization
	if (ObjArduinoUno==null)
	{
		 ObjArduinoUno=new ArduinoUno("COM2",9600);
		 ObjArduinoUno.Open(); //Open Serial port
         
         ObjArduinoUno.pinMode(6,ArduinoUno.INPUT);  //LS1
         ObjArduinoUno.pinMode(7,ArduinoUno.INPUT);  //LS2
         ObjArduinoUno.pinMode(8,ArduinoUno.INPUT);  //LS3
         ObjArduinoUno.pinMode(9,ArduinoUno.INPUT);  //LS4
         ObjArduinoUno.pinMode(10,ArduinoUno.INPUT); //START
         
         
         ObjArduinoUno.pinMode(2, ArduinoUno.OUTPUT); //EXTA
         ObjArduinoUno.pinMode(3, ArduinoUno.OUTPUT); //RETA
         ObjArduinoUno.pinMode(4, ArduinoUno.OUTPUT); //RETB
         ObjArduinoUno.pinMode(5, ArduinoUno.OUTPUT); //EXTB
         
         // Get places by NameID
		 pSTART=FindPlace("START");  // Link Object to place by NameID
		 pLS1=FindPlace("LS1");
		 pLS2=FindPlace("LS2");
		 pLS3=FindPlace("LS3");
   		 pLS4=FindPlace("LS4");
   		 
   		 pEXTA=FindPlace("EXTA");
   		 pEXTB=FindPlace("EXTB");
   		 pRETB=FindPlace("RETB");
   		 pRETA=FindPlace("RETA");
	}
	
	//Update places token based on switches connected to Arduino
	pSTART.Tokens=ObjArduinoUno.digitalRead(swSTART);
	pLS1.Tokens=ObjArduinoUno.digitalRead(swLS1);
	pLS3.Tokens=ObjArduinoUno.digitalRead(swLS3);
	pLS2.Tokens=ObjArduinoUno.digitalRead(swLS2);
	pLS4.Tokens=ObjArduinoUno.digitalRead(swLS4);
	
	//Prevent tokens of Solenoids to be more than one
	pEXTA.Tokens=(pEXTA.Tokens>=1) ? 1 : pEXTA.Tokens;
	pEXTB.Tokens=(pEXTB.Tokens>=1) ? 1 : pEXTB.Tokens;
	pRETB.Tokens=(pRETB.Tokens>=1) ? 1 : pRETB.Tokens;
	pRETA.Tokens=(pRETA.Tokens>=1) ? 1 : pRETA.Tokens;
	
	ObjArduinoUno.digitalWrite(solEXTA,pEXTA.Tokens);
   	ObjArduinoUno.digitalWrite(solEXTB,pEXTB.Tokens);
   	ObjArduinoUno.digitalWrite(solRETB,pRETB.Tokens);
   	ObjArduinoUno.digitalWrite(solRETA,pRETA.Tokens);
   	
   	Print("k"+k+"\n");
}

public void Reset()
{
	ObjArduinoUno.Close();
}

//////////////////////////////////////////////////////////
//   Arduino Firmata Class : Don't Modify It           //
////////////////////////////////////////////////////////
		
public class ArduinoUno
    {
        public static int INPUT = 0;
        public static int OUTPUT = 1;
        public static int LOW = 0;
        public static int HIGH = 1;

        private const int MAX_DATA_BYTES = 32;

        private const int DIGITAL_MESSAGE = 0x90; // send data for a digital port
        private const int ANALOG_MESSAGE = 0xE0; // send data for an analog pin (or PWM)
        private const int REPORT_ANALOG = 0xC0; // enable analog input by pin #
        private const int REPORT_DIGITAL = 0xD0; // enable digital input by port
        private const int SET_PIN_MODE = 0xF4; // set a pin to INPUT/OUTPUT/PWM/etc
        private const int REPORT_VERSION = 0xF9; // report firmware version
        private const int SYSTEM_RESET = 0xFF; // reset from MIDI
        private const int START_SYSEX = 0xF0; // start a MIDI SysEx message
        private const int END_SYSEX = 0xF7; // end a MIDI SysEx message

        private SerialPort _serialPort;
        private int delay;

        private int waitForData = 0;
        private int executeMultiByteCommand = 0;
        private int multiByteChannel = 0;
        private int[] storedInputData = new int[MAX_DATA_BYTES];
        private bool parsingSysex;
        private int sysexBytesRead;

        private volatile int[] digitalOutputData = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private volatile int[] digitalInputData = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private volatile int[] analogInputData = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private int majorVersion = 0;
        private int minorVersion = 0;
        private Thread readThread = null;
        private object locker = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPortName">String specifying the name of the serial port. eg COM4</param>
        /// <param name="baudRate">The baud rate of the communication. Default 115200</param>
        /// <param name="autoStart">Determines whether the serial port should be opened automatically.
        ///                     use the Open() method to open the connection manually.</param>
        /// <param name="delay">Time delay that may be required to allow some arduino models
        ///                     to reboot after opening a serial connection. The delay will only activate
        ///                     when autoStart is true.</param>
        public ArduinoUno(string serialPortName, Int32 baudRate, bool autoStart, int delay)
        {
            _serialPort = new SerialPort(serialPortName, baudRate);
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            if (autoStart)
            {
                this.delay = delay;
                this.Open();
            }
        }

        /// <summary>
        /// Creates an instance of the ArduinoUno object, based on a user-specified serial port.
        /// Assumes default values for baud rate (115200) and reboot delay (8 seconds)
        /// and automatically opens the specified serial connection.
        /// </summary>
        /// <param name="serialPortName">String specifying the name of the serial port. eg COM4</param>
        public ArduinoUno(string serialPortName) : this(serialPortName, 115200, false, 8000) { }

        /// <summary>
        /// Creates an instance of the ArduinoUno object, based on user-specified serial port and baud rate.
        /// Assumes default value for reboot delay (8 seconds).
        /// and automatically opens the specified serial connection.
        /// </summary>
        /// <param name="serialPortName">String specifying the name of the serial port. eg COM4</param>
        /// <param name="baudRate">Baud rate.</param>
        /// <param name="autostart">Start automatically?.</param>
        public ArduinoUno(string serialPortName, Int32 baudRate, bool autostart) : this(serialPortName, baudRate, autostart, 8000) { }

        /// <summary>
        /// Creates an instance of the ArduinoUno object, based on user-specified serial port and baud rate.
        /// Assumes default value for reboot delay (8 seconds).
        /// and automatically opens the specified serial connection.
        /// </summary>
        /// <param name="serialPortName">String specifying the name of the serial port. eg COM4</param>
        /// <param name="baudRate">Baud rate.</param>
        public ArduinoUno(string serialPortName, Int32 baudRate) : this(serialPortName, baudRate, false, 8000) { }

        /// <summary>
        /// Creates an instance of the ArduinoUno object using default arguments.
        /// Assumes the arduino is connected as the HIGHEST serial port on the machine,
        /// default baud rate (115200), and a reboot delay (8 seconds).
        /// and automatically opens the specified serial connection.
        /// </summary>

        /// <summary>
        /// Opens the serial port connection, should it be required. By default the port is
        /// opened when the object is first created.
        /// </summary>
        public void Open()
        {
            _serialPort.Open();

            Thread.Sleep(delay);

            byte[] command = new byte[2];

            for (int i = 0; i < 6; i++)
            {
                command[0] = (byte)(REPORT_ANALOG | i);
                command[1] = (byte)1;
                _serialPort.Write(command, 0, 2);
            }


            for (int i = 0; i < 2; i++)
            {
                command[0] = (byte)(REPORT_DIGITAL | i);
                command[1] = (byte)1;
                _serialPort.Write(command, 0, 2);
            }
            command = null;


            if (readThread == null)
            {
                readThread = new Thread(processInput);
                readThread.Start();
            }

        }

        /// <summary>
        /// Closes the serial port.
        /// </summary>
        public void Close()
        {
            readThread.Join(500);
            readThread = null;
            _serialPort.Close();
        }

        /// <summary>
        /// Lists all available serial ports on current system.
        /// </summary>
        /// <returns>An array of strings containing all available serial ports.</returns>
        public static string[] list()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// Returns the last known state of the digital pin.
        /// </summary>
        /// <param name="pin">The arduino digital input pin.</param>
        /// <returns>ArduinoUno.HIGH or ArduinoUno.LOW</returns>
        public int digitalRead(int pin)
        {
            return (digitalInputData[pin >> 3] >> (pin & 0x07)) & 0x01;
        }

        /// <summary>
        /// Returns the last known state of the analog pin.
        /// </summary>
        /// <param name="pin">The arduino analog input pin.</param>
        /// <returns>A value representing the analog value between 0 (0V) and 1023 (5V).</returns>
        public int analogRead(int pin)
        {
            return analogInputData[pin];
        }

        /// <summary>
        /// Sets the mode of the specified pin (INPUT or OUTPUT).
        /// </summary>
        /// <param name="pin">The arduino pin.</param>
        /// <param name="mode">Mode ArduinoUno.INPUT or ArduinoUno.OUTPUT.</param>
        public void pinMode(int pin, int mode)
        {

            byte[] message = new byte[3];
            message[0] = (byte)(SET_PIN_MODE);
            message[1] = (byte)(pin);
            message[2] = (byte)(mode);
            _serialPort.Write(message, 0, 3);
            message = null;
        }

        /// <summary>
        /// Write to a digital pin that has been toggled to output mode with pinMode() method.
        /// </summary>
        /// <param name="pin">The digital pin to write to.</param>
        /// <param name="value">Value either ArduinoUno.LOW or ArduinoUno.HIGH.</param>
        public void digitalWrite(int pin, int value)
        {

            int portNumber = (pin >> 3) & 0x0F;
            byte[] message = new byte[3];

            if (value == 0)
                digitalOutputData[portNumber] &= ~(1 << (pin & 0x07));
            else
                digitalOutputData[portNumber] |= (1 << (pin & 0x07));

            message[0] = (byte)(DIGITAL_MESSAGE | portNumber);
            message[1] = (byte)(digitalOutputData[portNumber] & 0x7F);
            message[2] = (byte)(digitalOutputData[portNumber] >> 7);
            _serialPort.Write(message, 0, 3);
        }

        /// <summary>
        /// Write to an analog pin using Pulse-width modulation (PWM).
        /// </summary>
        /// <param name="pin">Analog output pin.</param>
        /// <param name="value">PWM frequency from 0 (always off) to 255 (always on).</param>
        public void analogWrite(int pin, int value)
        {

            byte[] message = new byte[3];
            message[0] = (byte)(ANALOG_MESSAGE | (pin & 0x0F));
            message[1] = (byte)(value & 0x7F);
            message[2] = (byte)(value >> 7);
            _serialPort.Write(message, 0, 3);

        }

        private void setDigitalInputs(int portNumber, int portData)
        {
            digitalInputData[portNumber] = portData;
        }

        private void setAnalogInput(int pin, int value)
        {
            analogInputData[pin] = value;
        }

        private void setVersion(int majorVersion, int minorVersion)
        {
            this.majorVersion = majorVersion;
            this.minorVersion = minorVersion;
        }

        private int available()
        {
            return _serialPort.BytesToRead;
        }

        public void processInput()
        {
            while (_serialPort.IsOpen)
            {
                lock (this)
                {
                    try
                    {
                        int inputData = _serialPort.ReadByte();
                        int command;

                        if (parsingSysex)
                        {
                            if (inputData == END_SYSEX)
                            {
                                parsingSysex = false;
                                //processSysexMessage();
                            }
                            else
                            {
                                storedInputData[sysexBytesRead] = inputData;
                                sysexBytesRead++;
                            }
                        }
                        else if (waitForData > 0 && inputData < 128)
                        {
                            waitForData--;
                            storedInputData[waitForData] = inputData;

                            if (executeMultiByteCommand != 0 && waitForData == 0)
                            {
                                //we got everything
                                switch (executeMultiByteCommand)
                                {
                                    case DIGITAL_MESSAGE:
                                        setDigitalInputs(multiByteChannel, (storedInputData[0] << 7) + storedInputData[1]);
                                        break;
                                    case ANALOG_MESSAGE:
                                        setAnalogInput(multiByteChannel, (storedInputData[0] << 7) + storedInputData[1]);
                                        break;
                                    case REPORT_VERSION:
                                        setVersion(storedInputData[1], storedInputData[0]);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            if (inputData < 0xF0)
                            {
                                command = inputData & 0xF0;
                                multiByteChannel = inputData & 0x0F;
                            }
                            else
                            {
                                command = inputData;
                                // commands in the 0xF* range don't use channel data
                            }
                            switch (command)
                            {
                                case DIGITAL_MESSAGE:

                                case ANALOG_MESSAGE:
                                case REPORT_VERSION:
                                    waitForData = 2;
                                    executeMultiByteCommand = command;
                                    break;
                            }
                        }

                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }

                }
            }
        }

    }