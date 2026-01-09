# ESP32 Piano 

## Project Description

This project combines an ESP32-based electronic piano with a web-based Angular interface.
The ESP32 reads physical buttons (direct GPIO and PCF8574 I2C expander) and a potentiometer, generates corresponding tones on a buzzer, and updates an LCD display.
The Angular frontend displays pressed notes in real time and allows users to play predefined melodies or use free play mode via Server-Sent Events (SSE).

## Features

- 12-button piano (4 GPIO + 8 I2C PCF8574)

- Buzzer for sound output with variable pitch controlled by a potentiometer

- LCD display showing current note and frequency+offset(pitch)

- Angular web interface showing pressed notes in real-time

- SSE communication from ESP32 to Angular frontend

- Melody mode with step-by-step guidance

- Free play mode for manual playing

## Hardware Requirements
Below is the complete list of components and all wiring details, including power, resistors, and connections:

- ESP32 development board

- Buzzer connected to GPIO25
(optional 100–220 Ω resistor in series for protection)

- Potentiometer connected to ADC1_CH7 (GPIO35)

- 4 GPIO buttons: GPIO12, GPIO13, GPIO14, GPIO27
(active-low with internal pull-ups, no external resistors needed)

- PCF8574 I2C expander: 8 additional buttons

  - SDA → GPIO32

  - SCL → GPIO33

  - Address: 0x24  

  - Requires 4.7kΩ pull-up resistors on SDA/SCL
(most modules already include them; if you see “472”, do NOT add external resistors)

  - PCF8574 Addressing

    - A2 = 1 (3.3V)
    - A1 = 0 (GND)
    - A0 = 0 (GND)
        → Address = 0x24


- 8 Buttons on PCF8574
    - One leg → GND
      
    - Other leg → P0–P7
      
      ```
      P7 → Button 5
      P6 → Button 6
      …
      P0 → Button 12
      ```
- 16x2 LCD in 4-bit mode
  ```
  | LCD Pin  | ESP32 Pin                | Notes                   |
  | -------- | ------------------------ | ----------------------- |
  | VSS      | GND                      | Ground                  |
  | VDD      | 5V or 3.3V               | Most LCDs prefer **5V** |
  | VO       | Middle of 10k pot        | Controls contrast       |
  | RS       | GPIO4                    | Register select         |
  | RW       | GND                      | Always write mode       |
  | E        | GPIO5                    | Enable                  |
  | D4       | GPIO18                   | Data bit 4              |
  | D5       | GPIO19                   | Data bit 5              |
  | D6       | GPIO21                   | Data bit 6              |
  | D7       | GPIO22                   | Data bit 7              |
  | A (LED+) | 5V via **220Ω resistor** | Backlight               |
  | K (LED–) | GND                      | Backlight ground        |
  
  ```

## Software Structure

### ESP32 (VS Code + ESP-IDF)

#### Components

- buttons.c/h – Initialize GPIO and I2C buttons, read states

- buzzer.c/h – Initialize buzzer and play variable frequency tones

- lcd.c/h – Control 16x2 LCD

- potentiometer.c/h – Read potentiometer value and map to frequency offset

- main.c – Core application

#### FreeRTOS tasks:

- Buttons_task – Reads buttons and sends SSE events

- Pot_task – Reads potentiometer and updates frequency

- Buzzer_task – Plays buzzer tones based on current note and pot

- LCD_task – Updates LCD with current note and frequency

- SSE server for Angular frontend

- WiFi STA mode (connects to configured SSID)

#### Key Functions

- buttons_init(), buttons_read(), button_pressed()

- buzzer_init(), buzzer_play(freq), buzzer_stop()

- lcd_init(), lcd_print(text), lcd_clear(), lcd_set_cursor(col,row)

- pot_init(), pot_read_raw(), pot_read_mapped()

- sse_send_all(msg) – Sends note updates to Angular via SSE

### Angular Frontend

- Component: Piano

- Displays 12-key piano

- Supports Free mode and Melody mode

- Highlights pressed notes (yellow) and next melody note (green)

- Connects to ESP32 SSE server at  ```http://<ESP32_IP>/sse```

- Updates UI in real-time using EventSource

#### Key Methods

- handleNote() – Handles note_on events from ESP32

- startMelody(notes) – Starts a predefined melody

- resetMelody() – Resets melody progress

- switchToFreeMode() – Switches to free play

- isActive() / isHighlighted() – UI highlighting logic

## Build & Run Instructions

### ESP32 (VS Code + ESP-IDF)

1. Open the ESP-IDF project in VS Code

2. Install ESP-IDf library 

3. Set WiFi SSID and password in main.c
```main.c
void wifi_init_sta(void)
{
    esp_netif_init();          // init TCP/IP stack
    esp_event_loop_create_default();
    esp_netif_create_default_wifi_sta();

    wifi_init_config_t cfg = WIFI_INIT_CONFIG_DEFAULT();
    esp_wifi_init(&cfg);
    esp_wifi_set_mode(WIFI_MODE_STA);

    wifi_config_t wifi_config = 
    {
        .sta = 
        {
            .ssid = "GalaxyA71", // your wifi
            .password = "qwerty12",  //password
            .threshold.authmode = WIFI_AUTH_WPA2_PSK,
        },
    };
    esp_wifi_set_config(WIFI_IF_STA, &wifi_config);
    esp_wifi_start();
    esp_wifi_connect();
}
```

Make sure that the laptop is conected to that internet!

4. Build and flash
   
Make sure that the board, the flash method and the port are set corectly

5. Wait for ESP32 to connect to WiFi

6. Note the IP address printed in the console

### Angular Frontend

1. Navigate to the Angular project folder:
```
cd angular-piano
```

2. Install dependencies:
```
npm install
```

3. Ensure SSE connection points to your ESP32 IP in piano.component.ts:
```piano.ts
const evtSource = new EventSource('http://<ESP32_IP>/sse');// you will see it in thw ESP-IDF after you run the project and the server
```

## Notes

- Frequency Mapping

- Potentiometer offsets tone ±200 units (mapped to note_lower → note_upper)

- Buzzer frequency limited: 262–694 Hz to avoid distortion

- Button Mapping

  - GPIO: buttons 1–4

  - PCF8574: buttons 5–12 (P7→button5, P0→button12)

- Concurrency

  - FreeRTOS tasks synchronize using synth_mutex

- SSE clients managed with sse_mutex

## Technologies Used

- ESP32 with ESP-IDF

- FreeRTOS multitasking

- I2C communication for PCF8574

- LEDC PWM for buzzer tone generation

- ADC for potentiometer input

- 16x2 LCD in 4-bit mode

- Server-Sent Events (SSE) for real-time frontend updates
  
- Angular (16+)

- HTML/CSS/TypeScript
