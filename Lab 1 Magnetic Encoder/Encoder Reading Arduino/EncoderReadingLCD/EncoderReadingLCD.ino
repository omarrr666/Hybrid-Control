/****************************************************************************************
 *  MCT432 Hybrid Control - Prof. Farid A. Tolbah
 *  Code to demonstrate angle acquisition from Hall-Effect Encoder (2 Channels)
 *  To be read from Simulink / LabVIEW
 *  Encoder library will not be used
 *  
 ****************************************************************************************/

/*
		 |----|    |----| 
ENCA ----|    |----|    |
		
	 --|    |----|    |-- 
ENCB   |----|    |----| 
*/

#include <Encoder.h>
#include <LiquidCrystal.h>

const int encaPin=2;
const int encbPin=3;

int oldPosition  = -999;
char buffer[6];

LiquidCrystal lcd(12, 11, 7, 6, 5, 4);
Encoder myEnc(encbPin, encaPin); // for high performance


void setup() 
{
  
	 // Serial start
	 Serial.begin(9600);

	 // Initialize LCD
	 lcd.begin(16, 2);
	 lcd.setCursor(0,0);
	 lcd.print("Encoder Count:");
}

void loop() 
{ 
   int newPosition = myEnc.read(); // Reading encoder position
   if (newPosition>231) // Encoder count for one revolution
	  myEnc.write(0); // Reset counter
   if (newPosition<-231)
	  myEnc.write(0); 
   if (newPosition != oldPosition) { // Update LCD / Serial new position
	  oldPosition = newPosition;
	  sprintf(buffer,"%04d", newPosition);
	  Serial.println(buffer);
	  lcd.setCursor(0,1);
	  lcd.print("             ");
	  lcd.setCursor(0,1);
	  lcd.print(buffer);
	  delay(50);
   }
}
