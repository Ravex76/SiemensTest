# MqttPublisher
Содержит основной код программы. **App.config** содержит раздел _appSettings_, в котором можно задать адрес и порт брокера. 
В случае отсутствия значений приложение будет пытаться подключиться к брокеру, развернутому локально на порт 1883.

# MqttPublisher.Tests
Содержит unit-тесты

# Проверка работоспособности
Для проверки работоспособности использовался брокер [Mosquitto](https://mosquitto.org/files/binary/win64/mosquitto-2.0.15-install-windows-x64.exe)
и Mqtt клиент [MQTT Explorer](https://github.com/thomasnordquist/MQTT-Explorer/releases/download/0.0.0-0.4.0-beta1/MQTT-Explorer-0.4.0-beta1.exe).
1. Запустить брокер
2. Запустить клиент и подключить его к запущенному брокеру
3. Запустить скомпилированное приложение Publisher.exe
4. На клиенте должны появляться полученные сообщения в топике "test/data"
5. Останов посылки сообщений брокеру осуществляется путем нажатия Ctrl-C в консоли приложения Publisher.exe