
#define LED_PIN 9
#define LED_FEED 6

int time;
double feedVal;
double driveVal;
double fixVal;
double feedSpeed;
int lock;
double feeding;

void setup(){
  Serial.begin(9600);
  pinMode(LED_PIN,OUTPUT);
  pinMode(LED_FEED,OUTPUT);
  time=0;
  driveVal=0;
  feedVal=0;
  fixVal=350;
  lock=1;
  feedSpeed=30;
}

void loop(){
  int val = analogRead(A0);

  time++;
  if(time>=10000)time=0;
  
  driveVal = sin(time/3000.0*2*PI)*100+100;
  
  if(lock==1){
    feeding = abs(val-fixVal)/feedSpeed;
    if(val>fixVal) { if(val>0) feedVal-=feeding;}
    else {if(val<256) feedVal+=feeding;}
  }
  
  
  driveVal = driveVal<0?0:driveVal;
  driveVal = driveVal>254?254:driveVal;
  feedVal = feedVal<0?0:feedVal;
  feedVal = feedVal>254?254:feedVal;
  
  analogWrite(LED_PIN,255-driveVal);
  if(lock==0){
    analogWrite(LED_FEED,255);
  }else{
    analogWrite(LED_FEED,255-feedVal);
  }

  if(time%1==0){
    Serial.print("Time:");
    Serial.println(time);
    Serial.print("Current:");
    Serial.println(val);
    Serial.print("Drive:");
    Serial.println(driveVal);
    Serial.print("Feed:");
    Serial.println(feedVal);
  }
  if(Serial.available()>0){
    char ch = Serial.read();
    if(ch=='l'){
      lock=1;
    }
    if(ch=='u'){
      lock=0;
      feedVal=0;
    }
  }
  
  delay(50);
}
