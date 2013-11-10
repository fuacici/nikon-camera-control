
const int focusPin =  P1_6;
const int capturePin =  P1_7;
  
// the setup routine runs once when you press reset:
void setup() {    
  pinMode(focusPin, OUTPUT); 
  pinMode(capturePin, OUTPUT);   
  Reset();
  // initialize the digital pin as an output.
  pinMode(P2_5, OUTPUT);  
  Serial.begin(9600);  
}

// the loop routine runs over and over again forever:
void loop() {
 if (Serial.available() > 0) {
    char inChar = (char)Serial.read();   
    if(inChar== '1')
      AssertFocus();
    if(inChar== '2')
      ShutterOpen();
    if(inChar== '3')
      ShutterClose();
    if(inChar== '4')
      DeassertFocus();
    if(inChar== '5'){
      AssertFocus();
      delay(2000);        
      ShutterOpen();      
      delay(200);  
      DeassertFocus();
      ShutterClose();
    }
    if(inChar== '6'){
      AssertFocus();
      delay(3000);  
      DeassertFocus();
    }
    if(inChar== '0')
      Reset();
  }
  digitalWrite(P2_5, HIGH);   // turn the LED on (HIGH is the voltage level)
  delay(1000);               // wait for a second
  digitalWrite(P2_5, LOW);    // turn the LED off by making the voltage LOW
  delay(1000);               // wait for a second
}

void AssertFocus()
{
  digitalWrite(focusPin, HIGH);
}

void DeassertFocus()
{
  digitalWrite(focusPin, LOW);
}

void ShutterOpen()
{
  digitalWrite(capturePin, HIGH);
}

void ShutterClose()
{
  digitalWrite(capturePin, LOW);
}

void Reset()
{
  digitalWrite(focusPin, LOW);
  digitalWrite(capturePin, LOW);
}
