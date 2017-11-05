// This #include statement was automatically added by the Particle IDE.
#include "RelayShield.h"

int onSunset(String command);
int onSunrise(String command);
int onApproachingHome(String command);
int toggleSunsetLightsSetting(String command);
int frontGardenLightsOn(String command);
int toggleFrontGardenLights(String command);
int pressGarageDoorButton(String command);
int notifyGarageState(String command);

int turnRelayOn(String command);
int turnRelayOff(String command);
int isRelayOn(String command);

//Create an instance of the RelayShield library, so we have something to talk to
RelayShield myRelays;

int reed = D4; // variable for reed switch
int doorstatus = digitalRead(reed); //variable for door status

int frontGardenLights = 1;
int garageDoorButton = 2;
bool sunsetModeActive = true;
bool isNighttime = false;
String eventNameCloudFunction = "Cloud function triggered";
String currentState = "0|0|0";

int eventArray[10][9]; // Store events (up to 10) for time based triggers.
int numberOfEvents = 0;

void setup() {
    
    // register the cloud functions
    Particle.function("onSunset", onSunset);
    Particle.function("onSunrise", onSunrise);
    Particle.function("onApproaHome", onApproachingHome);
    Particle.function("toggleSunset", toggleSunsetLightsSetting);
    Particle.function("frontOn", frontGardenLightsOn);
    Particle.function("toggleFront", toggleFrontGardenLights);
    Particle.function("garageButton", pressGarageDoorButton);
    Particle.function("notifyGarage", notifyGarageState);
    Particle.variable("currentState", currentState);
    
    Particle.function("relayOn", turnRelayOn);
    Particle.function("relayOff", turnRelayOff);
    Particle.function("isRelayOn", isRelayOn);

    Time.zone(10);
    Time.beginDST();

    //.begin() sets up a couple of things and is necessary to use the rest of the functions
    myRelays.begin();

    pinMode(reed, INPUT_PULLDOWN);
    
    createDefaultEvent();
    
    Particle.publish("Time test", Time.format(Time.now(), TIME_FORMAT_ISO8601_FULL), 60, PRIVATE);
}

void loop(){
    evalTime(Time.year(), Time.month(), Time.day(), Time.weekday(), Time.hour(), Time.minute(), Time.second());
    createCurrentState();
    
    delay(1000);
}

// Cloud functions: these functions automagically get called upon a matching POST request

int onSunset(String command) {
    Particle.publish(eventNameCloudFunction, "onSunset", 60, PRIVATE);

    isNighttime = true;
    
    if (sunsetModeActive) {
        turnOnFrontGardenLights();
        
        createOnSunsetEvent();
        return 1;
    }
    return 0;
}

int onSunrise(String command) {
    Particle.publish(eventNameCloudFunction, "onSunrise", 60, PRIVATE);

    isNighttime = false;
    turnOffFrontGardenLights(); // just in case lights were still on

    return 1;
}

int onApproachingHome(String command) {
    Particle.publish(eventNameCloudFunction, "onApproachingHome", 60, PRIVATE);

    if (isNighttime && !areFrontGardenLightsOn()) {
        turnOnFrontGardenLights();
        
        Particle.publish("Garden lights turned on in function", "onApproachingHome", 60, PRIVATE);
        
        createOnApproachingEvent();

        return 1;
    }

    return 0;
}

int toggleSunsetLightsSetting(String command) {
    Particle.publish(eventNameCloudFunction, "toggleSunsetLightsSetting", 60, PRIVATE);

    if (sunsetModeActive)
        sunsetModeActive = false;
    else
        sunsetModeActive = true;
}

int frontGardenLightsOn(String command) {
    Particle.publish(eventNameCloudFunction, "frontGardenLightsOn", 60, PRIVATE);

    if (myRelays.isOn(frontGardenLights))
        return 1;
    
    return 0;
}

int toggleFrontGardenLights(String command) {
    Particle.publish(eventNameCloudFunction, "toggleFrontGardenLights", 60, PRIVATE);

    if (myRelays.isOn(frontGardenLights))
        myRelays.off(frontGardenLights);
    else
        myRelays.on(frontGardenLights);

    return 1;
}

int pressGarageDoorButton(String command) {
    Particle.publish(eventNameCloudFunction, "pressGarageDoorButton", 60, PRIVATE);

    myRelays.on(garageDoorButton);
    delay(1000);
    myRelays.off(garageDoorButton);

    return 1;
}

int notifyGarageState(String command) {
    Particle.publish(eventNameCloudFunction, "garageState", 60, PRIVATE);

    if (garageDoorStatusCheck() == "OPEN") {
        bool success;
        success = Particle.publish("notify-garage-open");
        if (!success) {
          return 0;
        }
    }
    
    return 1;
}

int turnRelayOn(String command) {
    Particle.publish(eventNameCloudFunction, "turnRelayOn", 60, PRIVATE);

    myRelays.on(command.toInt());
    return 1;
}

int turnRelayOff(String command) {
    Particle.publish(eventNameCloudFunction, "turnRelayOff", 60, PRIVATE);

    myRelays.off(command.toInt());
    return 1;
}

int isRelayOn(String command) {
    Particle.publish(eventNameCloudFunction, "isRelayOn", 60, PRIVATE);

    if (myRelays.isOn(command.toInt()))
        return 1;
    
    return 0;
}

// Local functions

void evalTime(int year, int month, int dayOfMonth, int dayOfWeek, int hour, int minute, int second) {
    
    //Cycle through stored events and compare to current date/time.
    for(int i = 0; i < numberOfEvents; i++){
        bool runCommand = true;
        
        //Check Year
        if(eventArray[i][0] != year){
            //Year does not match and is required
            if(eventArray[i][0] != 0){
                runCommand = false;
            }
            
        }
        //Check Month
        if(eventArray[i][1] != month){
            //Month does not match and is required
            if(eventArray[i][1] != 0){
                runCommand = false;
            }
        }
        //Check Day of Month
        if(eventArray[i][2] != dayOfMonth){
            //Day of Month does not match and is required
            if(eventArray[i][2] != 0){
                runCommand = false;
            }
        }
        //Check Day of Week
        if(eventArray[i][3] != dayOfWeek){
            //Day of Week does not match and is required
            if(eventArray[i][3] != 0){
                runCommand = false;
            }
        }
        //Check Hour
        if(eventArray[i][4] != hour){
            //Hour does not match and is required
            if(eventArray[i][4] != 24){
                runCommand = false;
            }
        }
        //Check Minute
        if(eventArray[i][5] != minute){
            //Minute does not match and is required
            if(eventArray[i][5] != 60){
                runCommand = false;
            }
        }
        //Check Second
        if(eventArray[i][6] != second){
            //Second does not match and is required
            if(eventArray[i][6] != 60){
                runCommand = false;
            }
        }
        
        if(runCommand){
            Particle.publish("Running command", String(eventArray[i][7]), 60, PRIVATE);
            executeCommand(eventArray[i][7], eventArray[i][8]);
        }
    }
}

void turnOnFrontGardenLights() {
	myRelays.on(frontGardenLights);
    Particle.publish("Front garden lights turned on", "", 60, PRIVATE);
}

void turnOffFrontGardenLights() {
	myRelays.off(frontGardenLights);
    Particle.publish("Front garden lights turned off", "", 60, PRIVATE);
}

bool areFrontGardenLightsOn() {
    return myRelays.isOn(frontGardenLights);
}

void executeCommand(int commandID, int commandData){
    switch(commandID){
        case 0:
            // Sync time once a day with the cloud
            if (Particle.syncTimeDone())
                Particle.syncTime();

            Particle.publish("Sync time command triggered", Time.format(Time.now(), TIME_FORMAT_ISO8601_FULL), 60, PRIVATE);
            break;

        case 1:
            // Turn front garden lights off
            turnOffFrontGardenLights();
            clearOnSunsetEvent();
            break;

        case 2:
            // Turn front garden lights off after approached home event
            turnOffFrontGardenLights();
            clearOnApproachingEvent();
            break;
    }
}

void createDefaultEvent() {
    eventArray[0][5] = 1;  // Event 0 is all zero's to sync time at 12:01am (can't be midnight, as then all will be zero and not get called)
    
    numberOfEvents++;
}

void createOnSunsetEvent() {
    // Create event to turn front garden lights off at 12:(minute) of when turned on. Ie: 7:45pm sunset called, 12:45am turn lights off
    if (Time.minute() == 0)
        eventArray[1][5] = 1;
    else
        eventArray[1][5] = Time.minute();
    eventArray[1][7] = 1; // commandID = 1
    
    numberOfEvents++;
}

void clearOnSunsetEvent() {
    eventArray[1][5] = 0;
    eventArray[1][7] = 0;
    
    numberOfEvents--;
}

void createOnApproachingEvent() {
    // Create event to turn front garden lights off 15 minutes when arrived home Ie: 12:15am arrived home, 12:30am turn lights off
    eventArray[2][4] = Time.hour();
    eventArray[2][5] = Time.minute() + 15;
    eventArray[2][7] = 2; // commandID = 2
    
    numberOfEvents++;
}

void clearOnApproachingEvent() {
    eventArray[2][4] = 0;
    eventArray[2][5] = 0;
    eventArray[2][7] = 0;
    
    numberOfEvents--;
}

// void testRelays() {
//     //Test relays
//     for(int i = 1; i <= 4; i++) {
//         myRelays.on(i);
//         delay(500);
//     }
//     for(int i = 1; i <= 4; i++) {
//         myRelays.off(i);
//         delay(500);
//     }
//     myRelays.allOn();
//     delay(500);
//     myRelays.allOff();
    
//     Particle.publish("Relay test", "PASSED", 60, PRIVATE);
// }

void createCurrentState() {

    currentState =  String(String(sunsetModeActive) + "|" +  String(areFrontGardenLightsOn()) + "|" + String(eventArray[1][4]) + ":" + String(eventArray[1][5]) + "|" + garageDoorStatusCheck());

}

String garageDoorStatusCheck() {
    int status = digitalRead(reed);
    if(status == HIGH) {
        return "OPEN";
    }
    else if(status == LOW) {
        return "CLOSED";
    }
    else {
        return "OFFLINE";
    }
}

