# My Home App
### Control garden lights &amp; garage door with a Particle Photon/Spark Core + Relay Shield, with IFTTT applets.

To program your Particle Photon/Spark Core, go to https://build.particle.io/

[Read the blog post](http://michaeldimoudis.com/blog/iot-garden-lights-and-garage-door-with-mobile-app) on how to wire up the Particle Photon/Spark Core to your low voltage 12V garden lights, garage door remote, and garage door sensor.

**Particle and IFTTT integration**
* On sunrise, turn garden lights on (and set `isNighttime` to true)
    * IFTTT `On sunrise`, then call function name `onSunrise`.
* On sunset, set `isNighttime` to false, and turn garden lights off (in case they were still on).
    * IFTTT `On sunrise`, then call function name `onSunrise`.
* When approaching home at night, turn front garden lights on
    * IFTTT `When you enter an area`, then call function name `onApproaHome`.
    * **Or use the native iOS app** with geo fence location services
* If you leave home and forget to close the garage door, send a notification
    * IFTTT `When you exit an area`, then call function name `notifyGarage`. This function will then publish an event to trigger the next IFTTT applet.
    * IFTTT `When new event published with event name notify-garage-open`, send a notification from the IFTTT app `Did you just leave home and forgot to close the garage door?`
    * **Or use the native iOS app** with geo fence location services

**App features**
* Front Garden Lights
    * Enable/disable if lights should turn on at sunset
    * Toggle lights on/off
    * See the time the lights will automatically turn off (currently hard coded to midnight 12am plus the minute the lights turned on, ie: sunset was at 7:34pm, light will turn off at 12:34am)
* Garage Door
    * See if the garage door is currently open/closed
    * Toggle the garage door button
* iOS Settings for My Home App
    * Toggle location debugging (ie: always send a notification when you leave/approach home)

