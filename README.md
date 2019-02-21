# GyroStepDetector

## Basically, i am trying to build algorithm that filters out the "rebound" from gyroscopes, and detects steps, the direction of those, and jumps and crouches, for my obnidirectional tredmil.

hardware:

* One gyro+arduino on waist, which also sends direction through cereal.
* Another two gyro-arduino pairs on each foot, that send position info through cereal(COM3 and COM2).
* A laptop :).

### So far, i have one gyro-arduino pair connected to COM3, i will try and decipher individual steps from the raw data using simple if() statments and bools.

![](/screenshot.PNG)



PS if you come across this and becomes offended by my horrific code(woudnt blame ya), feel free to correct/educate me, im willing to learns
