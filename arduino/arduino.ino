#include <MsTimer2.h>
#include "DHT.h"
#define DHTPIN 4 // номер пина, к которому подсоединен датчик

// Раскомментируйте в соответствии с используемым датчиком
// Инициируем датчик
DHT dht(DHTPIN, DHT22);
//DHT dht(DHTPIN, DHT11);

void setup() {
  Serial.begin(9600);
  dht.begin();
  flash();
  MsTimer2::set(30 * 1000, flash); // 500ms period
  MsTimer2::start();
}

void flash()
{
  //Считываем влажность
  float humidity = dht.readHumidity();
  // Считываем температуру
  float temperature = dht.readTemperature();
  // Проверка удачно прошло ли считывание.
  if (isnan(humidity) || isnan(temperature)) {
    Serial.println("Не удается считать показания");
    return;
  }
  Serial.print("t:");
  Serial.print(temperature);
  Serial.print(";");
  Serial.print("h:");
  Serial.print(humidity);
  Serial.println(";");
}

void loop() {
}
