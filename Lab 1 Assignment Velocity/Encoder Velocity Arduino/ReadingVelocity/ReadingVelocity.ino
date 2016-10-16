#include <Encoder.h>
#include <LiquidCrystal.h>
#include <SimpleTimer.h> //Timer to update encoder count

const int encaPin=2; // Encoder Channel 1
const int encbPin=3; // Encoder Channel 2

int oldVelocityPPS  = -999; // Encoder old position
int newVelocityPPS = 0; //encoder reading

String oldDirection = "CW";  // old direction
String newDirection = "CCW"; // new direction

int speedRPM = 0;               //calculated speed in RPM
char speedRPMSerial[6];   // RPM with fixed character length  
char directionSerial[6];  // direction with fixed length


/*********************************************************/
const double encoderPulsesPerRevolution = 400.0; // 200 PPR (both edges)
const long timerInterval = 500L; // Timer is triggered every 500ms
/********************************************************/


LiquidCrystal lcd(12, 11, 7, 6, 5, 4); // LCD object
Encoder myEnc(encbPin, encaPin);  // Encoder object
SimpleTimer timer; //Timer object


// get the sign of velocity
static inline int sgn(int val) {
	if (val < 0) return -1;
	if (val == 0) return 0;
	return 1;
}


void timerRoutine()
{
	 newVelocityPPS= myEnc.read(); // read count after 100 ms;
	 speedRPM = (newVelocityPPS/ encoderPulsesPerRevolution)*(1000.0/ timerInterval)*60.0;
	 myEnc.write(0); // reset encoder counter to start counting again from zero.

	 switch (sgn(speedRPM)) // check motor direction
	 {
	 case 1:
		 newDirection = "CW";
		 break;
	 case -1:
		 newDirection = "CCW";
		 break;
	 case 0:
		 newDirection = "Idle";
		 break;
	 default:
		 break;
	 }

	 speedRPM = abs(speedRPM);
	 if (newVelocityPPS != oldVelocityPPS) {
		 oldVelocityPPS = newVelocityPPS;
		 sprintf(speedRPMSerial, "%06d", speedRPM);
		 lcd.setCursor(0, 1);
		 lcd.print("               ");
		 lcd.setCursor(0, 1);
		 lcd.print(speedRPMSerial);


		 if (oldDirection!=newDirection){
			 oldDirection = newDirection;
			 newDirection.toCharArray(directionSerial,6,0); // cast string to 4 character array
			 lcd.setCursor(0,3);
			 lcd.print("               ");
			 lcd.setCursor(0, 3);
			 lcd.print(directionSerial);
		 }
	 }
}	

void setup() 
{
	 lcd.begin(20, 4);
	 lcd.setCursor(0,0);
	 lcd.print("Speed:"); //Speed header
	 lcd.setCursor(0, 2);
	 lcd.print("Direction:"); //Direction header

	 timer.setInterval(timerInterval, timerRoutine); //Read velocity every 100 msec
}

void loop() 
{ 
   timer.run(); //polling technique
}