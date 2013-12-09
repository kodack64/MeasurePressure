// Wire Master Reader
// by Nicholas Zambetti <http://www.zambetti.com>

// Demonstrates use of the Wire library
// Reads data from an I2C/TWI slave device
// Refer to the "Wire Slave Sender" example for use with this

// Created 29 March 2006

// This example code is in the public domain.


#include <Wire.h>

void setup()
{
  int data=0;
  unsigned char msb,lsb;
  Serial.begin(9600);  // start serial for output
  Wire.begin();        // join i2c bus (address optional for master)
  Wire.beginTransmission(0x77);
  Wire.write(0xAA);
  Wire.endTransmission();
  Wire.requestFrom(0x77,2);
  while(true){
    data=Wire.available();
    Serial.print(data);
    if(data>=2){
      Serial.print("ok");
      msb = Wire.read();
      lsb=Wire.read();
      Serial.print(msb<<8 | lsb);
      break;
    }else{
      Serial.println("wait");
      delay(1000);
    }
  }
}

void loop()
{
  Serial.println("loop");
  delay(500);
}
