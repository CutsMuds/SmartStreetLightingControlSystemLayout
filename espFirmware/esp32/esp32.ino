#include <WiFi.h>
#include "Adafruit_NeoPixel.h"

#define SSID "4real"
#define PWD "81855872"

#define STATUS_LED_PIN 4
#define POLES_VCC_PIN 16
#define POLES_PIN 13
#define SHIFTDATA_PIN 17
#define SHIFTCLOCK_PIN 19
#define SHIFTLATCH_PIN 18

#define POLES_COUNT 10
#define BLINK_DELAY 1000
#define SHIFT_DELAY 50
#define POLES_DELAY 6

IPAddress serverIp = {192, 168, 1, 100};
WiFiClient server;
TaskHandle_t serverHandlerTask;
Adafruit_NeoPixel poles(POLES_COUNT, POLES_PIN, NEO_BRG + NEO_KHZ800);

unsigned long shiftTimer = 0;
unsigned long blinkTimer = 0;
unsigned long polesUpdateTimer = 0;
bool blinkState = false;

byte halls[10] = { 0 };
struct pole {
  int8_t previous;
  int8_t next;
  byte bright;
  unsigned long wasHereTimer;
};
pole polesStruct[10] = { {-1, 1, 25, 0},  //0
                         {0, 2, 25, 0},   //1
                         {1, 3, 25, 0},   //2
                         {2, -1, 25, 0},  //3
                         {-1, 5, 25, 0},  //4
                         {4, 6, 25, 0},   //5
                         {5, -1, 25, 0},  //6
                         {-1, 8, 25, 0},  //7
                         {7, 9, 25, 0},   //8
                         {8, -1, 25, 0} };//9

bool isRain = false;
bool isLight = false;
byte lowestBrightness = 25;

byte hallsCounter = 0;
byte light = 0;
int rise = 1;

byte negativeBrightnessForRain = 0;

void setup(){
  Serial.begin(115200);
  pinMode(STATUS_LED_PIN, OUTPUT);
  pinMode(POLES_VCC_PIN, OUTPUT);

  pinMode(SHIFTDATA_PIN, INPUT);
  pinMode(SHIFTCLOCK_PIN, OUTPUT);
  pinMode(SHIFTLATCH_PIN, OUTPUT);

  digitalWrite(STATUS_LED_PIN, LOW);

  WiFi.begin(SSID, PWD);
  while (WiFi.status() != WL_CONNECTED) {
    if(light + rise > 255 || light + rise < 0) rise * -1;
    light += rise;
    FillPoles(0, light, 0);
  }
  FillPoles(0, 0, 0);
  
  xTaskCreatePinnedToCore(ServerHandler, "serverHandler", 10000, NULL, 1, &serverHandlerTask, 0);
}

void ServerHandler(void * pvParameters) {
  byte received[32];
  byte receivedCounter = 0;
  while(true) {
    if(!server.connected()) {
      Connect();
    }
    
    receivedCounter = 0;
    
    while(server.available()) {
      received[receivedCounter] = server.read();
      if(received[receivedCounter] == 100) {
        server.flush();
        break;
      }
      receivedCounter++;
    }

    if(receivedCounter > 0) {
      if(received[0] == 1) {
        isLight = received[1];
        isRain = received[2];
      }
      else if(received[0] == 2) {
        byte temp[10];
        for(int i = 0; i < 10; i++) {
          temp[i] = polesStruct[i].bright;
        }
        server.write((uint8_t*)temp, 10);
        server.flush();
      }
    }
    delay(1);
  }
}

void ApplyPoles() {
  digitalWrite(POLES_VCC_PIN, LOW);
  poles.show();
  digitalWrite(POLES_VCC_PIN, HIGH);
}

void FillPoles(uint8_t r, uint8_t g, uint8_t b) {
  for(uint16_t i = 0; i < POLES_COUNT; i++) {
    poles.setPixelColor(i, r, g, b);
  }
  ApplyPoles();
}

void Connect() {
  while(!server.connected()) {
    server.connect(serverIp, 1953);
  }
}

void ReadShift() {
  byte temp[10];
  hallsCounter = 0;
  
  digitalWrite(SHIFTLATCH_PIN, LOW);
  digitalWrite(SHIFTLATCH_PIN, HIGH);

  for (int i = 0; i < 16; i++) {
    if(i != 0 && i != 1 && i != 4 && i != 5 && i != 6 && i != 7) {
      int bit = digitalRead(SHIFTDATA_PIN);
      if (bit == LOW) {
        temp[hallsCounter] = 1;
      } 
      else {
        temp[hallsCounter] = 0;
      }
      hallsCounter++;
    }
    digitalWrite(SHIFTCLOCK_PIN, HIGH);
    digitalWrite(SHIFTCLOCK_PIN, LOW);
  }
  halls[0] = temp[6];
  halls[1] = temp[3];
  halls[2] = temp[4];
  halls[3] = temp[1];
  halls[4] = temp[8];
  halls[5] = temp[5];
  halls[6] = temp[7];
  halls[7] = temp[9];
  halls[8] = temp[2];
  halls[9] = temp[0];

  for(int i = 0; i < 10; i++) {
    if(halls[i] == HIGH && !isLight) { 
      polesStruct[i].wasHereTimer = millis();
      polesStruct[i].bright = 255;
      if(polesStruct[i].previous != -1) {
        polesStruct[polesStruct[i].previous].wasHereTimer = millis();
        polesStruct[polesStruct[i].previous].bright = 255;
      }
      if(polesStruct[i].next != -1) {
        polesStruct[polesStruct[i].next].wasHereTimer = millis();
        polesStruct[polesStruct[i].next].bright = 255;
      }
    }
  }
}

void updatePoles() {
  if(!isLight) { if(lowestBrightness < 25) lowestBrightness++; }
  else if(lowestBrightness > 0) lowestBrightness--; 
  if(!isRain) { if(negativeBrightnessForRain > 0) negativeBrightnessForRain--; }
  else if(negativeBrightnessForRain <= 254) negativeBrightnessForRain++;
  for(int i = 0; i < 10; i++) {
    if(millis() - polesStruct[i].wasHereTimer > 3000 && polesStruct[i].bright > lowestBrightness) {
      polesStruct[i].bright--;
    }
    else if(polesStruct[i].bright < lowestBrightness) polesStruct[i].bright++;
    if(isLight) polesStruct[i].bright = lowestBrightness;
    int toBlue = constrain(polesStruct[i].bright - negativeBrightnessForRain, 0, 255);
    poles.setPixelColor(i, polesStruct[i].bright, polesStruct[i].bright, toBlue);
  }
  ApplyPoles();
}

void loop() {
  if((millis() - blinkTimer >= BLINK_DELAY / 5 && server.connected()) || (millis() - blinkTimer >= BLINK_DELAY && !server.connected())) {
    blinkTimer = millis();
    digitalWrite(STATUS_LED_PIN, blinkState);
    blinkState = !blinkState;
  }
  if(millis() - shiftTimer >= SHIFT_DELAY) {
    shiftTimer = millis();
    ReadShift();
  }
  if(millis() - polesUpdateTimer >= POLES_DELAY) {
    polesUpdateTimer = millis();
    updatePoles();
  }
}
