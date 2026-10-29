#include <Arduino.h>

int dataPin = 2;
int clockPin = 3;
int latchPin = 4;

const int numStrands = 16;  // 16 outputs
char ledsset[] = "1111111111111111"; // active low for all 16

void setup() {
  Serial.begin(230400);
  pinMode(dataPin, OUTPUT);
  pinMode(clockPin, OUTPUT);
  pinMode(latchPin, OUTPUT);
}

char frame[numStrands + 1];  // +1 for null

void loop() {
  if (Serial.available()) {
    int len = Serial.readBytesUntil('\n', frame, numStrands + 1);

    if (len == numStrands) {
      for (int i = 0; i < numStrands; i++) {
        if (frame[i] == '1') ledOn(i);
        else ledOff(i);
      }
    }
    // else: discard bad frame automatically
  }
}

void updateShiftRegisters() {
  digitalWrite(latchPin, LOW);
  // Send the second register first (high bits), then the first (low bits)
  uint16_t value = binaryToHex(ledsset);
  shiftOut16(value);
  digitalWrite(latchPin, HIGH);
}

uint16_t binaryToHex(const char *binary) {
  uint16_t value = 0;
  for (int i = 0; i < 16; i++) {
    value <<= 1;
    if (binary[i] == '1') value |= 1;
  }
  return value;
}

void shiftOut16(uint16_t dataOut) {
  for (int i = 15; i >= 0; i--) {
    digitalWrite(clockPin, LOW);
    digitalWrite(dataPin, (dataOut & (1 << i)) ? HIGH : LOW);
    digitalWrite(clockPin, HIGH);
  }
  digitalWrite(clockPin, LOW);
}

void ledOn(int strand) {
  ledsset[strand] = '0';
  updateShiftRegisters();
}

void ledOff(int strand) {
  ledsset[strand] = '1';
  updateShiftRegisters();
}
