#include <ESP8266WiFi.h>

#define SSID "4real"
#define PWD "81855872"

IPAddress serverIp = {192, 168, 1, 100};
WiFiClient server;

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(13, INPUT);
  bool flag = false;
  Serial.begin(111200);
  WiFi.begin(SSID, PWD);
  while (WiFi.status() != WL_CONNECTED) {
    digitalWrite(LED_BUILTIN, flag);
    flag = !flag;
    delay(100);
  }
  digitalWrite(LED_BUILTIN, HIGH);
}

void Connect() {
  while(!server.connected()) {
    Serial.println("connection...");
    server.connect(serverIp, 1924);
  }
  Serial.println("connected");
}

byte received[32];
byte receivedCounter = 0;

byte toSend[2] = { 0 };

void loop() {
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
  if(receivedCounter > 0 && received[0] == 2) {
    digitalWrite(LED_BUILTIN, LOW);
    if(analogRead(A0) > 500) toSend[0] = 1;
    else toSend[0] = 0;
    toSend[1] = !digitalRead(13);
    server.write((uint8_t*)toSend, 2);
    digitalWrite(LED_BUILTIN, HIGH);
  }
}
