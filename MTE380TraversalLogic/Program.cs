using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MTE380TraversalLogic
{
    class Program
    {
        #region Layout Map
        // just to map out problem
        // 1 is starting tile
        // ideally should never be on 0 or -1
        readonly int[,] pathPlan = new int[6,6] 
        { 
            { 9, 10, 11, 12, 13, 14 },
            { 8, 25, 26, 27, 28, 15 },
            { 7, 24, 33, 34, 29, 16 },
            { 6, 23, 32, 31, 30, 17 },
            { 5, 22, 21, 20, 19, 18 },
            { 4, 3, 2, 1, 0, -1 },
        };
        #endregion

        #region Sensor/Motor Mocks
        // motors mock
        enum MotorSpeed
        {
            NormalBackward = -2,
            SlowBackward = -1,
            Stop = 0,
            SlowForward = 1,
            NormalForward = 2,
        };
        static MotorSpeed motorLeftSpeed = MotorSpeed.Stop;
        static MotorSpeed motorRightSpeed = MotorSpeed.Stop;

        // sensors mock
        static double angleSensor = 0;
        static readonly double ANGLE_TOLERANCE_VALUE = 0.05;
        static double getAngleSensor()
        {
            return angleSensor;
        }
        static double leftSensorDistance = 0;
        static double frontSensorDistance = 0;
        static readonly double DISTANCE_TOLERANCE_VALUE = 0.25;
        static double getLeftSensorDistance()
        {
            return leftSensorDistance;
        }
        static double getFrontSensorDistance()
        {
            return frontSensorDistance;
        }
        #endregion

        #region Supporting Variables / Functions
        // variable to set angle sensor to zero
        static double angleZero = 0;

        // variable to save tile distance for knowing when to turn
        static double tileDistance = 0;

        // count of turns through course
        static int turnCount = 0;

        // function to know when approaching drop (possibly not needed)
        static bool approachingDrop()
        {
            return false;
        }

        static void driveForward(bool slowDown = false) {
            MotorSpeed speed = slowDown ? MotorSpeed.SlowForward : MotorSpeed.NormalForward;
            motorLeftSpeed = speed;
            motorRightSpeed = speed;
        }

        static void stopRobot()
        {
            motorLeftSpeed = MotorSpeed.Stop;
            motorRightSpeed = MotorSpeed.Stop;
        }

        static void rightTurnRobot() {
            stopRobot();

            motorLeftSpeed = MotorSpeed.NormalForward;
            motorRightSpeed = MotorSpeed.NormalBackward;

            double angle = getAngleSensor();
            // turn until angle matches expected one after turn
            while (
                Math.Abs(angle - angleZero - (((turnCount * 90) % 360) + 90)) >
                ANGLE_TOLERANCE_VALUE
            )
            {
                angle = getAngleSensor();

                // simulate turning when testing by updating angle
                if (isTesting)
                {
                    setAngleSensor(Math.Round(angle+ 0.01, 2)); 
                }
            }

            stopRobot();
            driveForward();

            // simulate turning right when testing by updating distance sensors
            if (isTesting)
            {
                simulationTurnRight();
            }

            // increment turn count
            turnCount++;
        }

        static void selfAlign() {
            double startingAngle = getAngleSensor();
            bool shouldTurnLeft = startingAngle > (turnCount * 90) % 360;
            if (shouldTurnLeft) {
                // turn left if needed
                motorLeftSpeed = MotorSpeed.SlowForward;
                motorRightSpeed = MotorSpeed.SlowBackward;
            } else {
                // turn right if needed
                motorLeftSpeed = MotorSpeed.SlowBackward;
                motorRightSpeed = MotorSpeed.SlowForward;
            }

            double angle = startingAngle;
            while (
                Math.Abs(angle - angleZero - ((turnCount* 90) % 360)) >
                ANGLE_TOLERANCE_VALUE
            ) {
                angle = getAngleSensor();

                if (isTesting)
                {
                    setAngleSensor(angle + (shouldTurnLeft ? -0.005 : 0.005));
                }
            }

            stopRobot();
            driveForward();
        }

        #endregion

        #region Setup and Loop for Arduino
        static void setup()
        {
            // simulate known start position when testing
            if (isTesting)
            {
                setLeftSensorDistance(1);
                setFrontSensorDistance(7);
            }

            // default variables to start course
            tileDistance = 2 * getLeftSensorDistance();
            angleZero = getAngleSensor();
            turnCount = 0;
        }
        
        static void loop()
        {
            if (isTesting)
            {
                simulationStart();
            }

            while (true)
            {
                // slow down if approaching drop
                driveForward(approachingDrop());
                
                // self align if angle is too far off desired angle
                if (
                    Math.Abs(getAngleSensor() - angleZero - ((turnCount * 90) % 360)) >
                    ANGLE_TOLERANCE_VALUE
                )
                {
                    selfAlign();
                    continue;
                }
                
                // keep going if robot hasn't reached turning point
                if (
                    Math.Abs(
                        getFrontSensorDistance() -
                        (tileDistance * ((turnCount + 1) / 4) + tileDistance / 2)
                    ) > DISTANCE_TOLERANCE_VALUE
                )
                {
                    continue;
                }

                // stop if got to center of course
                if (turnCount == 10)
                {
                    stopRobot();
                    if (isTesting)
                    {
                        simulationRunning = false;
                    }
                    break;
                }
                // turn if reached known turning point
                else
                {
                    rightTurnRobot();
                }
            }
        }
        #endregion

        #region TESTING STUFF
        static void expect(object expectedValue, object actualValue, string description = "")
        {
            // Show description of test if exists
            if (description != "")
            {
                Console.Write($"{description.ToUpper()}: ");
            }
            // Show test result
            bool success = expectedValue.Equals(actualValue);
            failCount += !success ? 1 : 0;
            Console.ForegroundColor = success ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
            Console.WriteLine((success ? "success" : "fail").ToUpper());
            Console.ResetColor();
        }
        delegate void Tests();
        static void section(string description, Tests tests)
        {
            Console.WriteLine(description.ToUpper());
            tests();
            Console.WriteLine();
        }

        static void setAngleSensor(double value)
        {
            angleSensor = value;
        }

        static void setLeftSensorDistance(double value)
        {
            leftSensorDistance = Math.Round(value, 2);
        }

        static void setFrontSensorDistance(double value)
        {
            frontSensorDistance = Math.Round(value, 2);
        }

        static int failCount = 0;
        static bool isTesting = false;
        static bool simulationRunning = false;

        static void simulationStart()
        {
            setLeftSensorDistance(tileDistance / 2);
            setFrontSensorDistance(3.5*tileDistance);
            simulationRunning = true;

            // simulate motor movement
            Task.Run(() =>
            {
                while (simulationRunning)
                {
                    // simulation for movement of motors when both are set to move forward
                    if ((motorLeftSpeed == MotorSpeed.NormalForward && motorRightSpeed == MotorSpeed.NormalForward) ||
                    (motorLeftSpeed == MotorSpeed.SlowForward && motorRightSpeed == MotorSpeed.SlowForward))
                    {
                        setFrontSensorDistance(getFrontSensorDistance() - (motorLeftSpeed == MotorSpeed.NormalForward ? 0.01 : 0.005));
                        // slow down a bit to make realistic and avoid multithread weirdness
                        Thread.Sleep(1);
                    }
                }
            });
        }
        static void simulationTurnRight()
        {
            // update left sensor to simulate turning
            if (turnCount == 3 || turnCount == 7)
            {
                setLeftSensorDistance(getLeftSensorDistance() + tileDistance);
            }

            // update front sensor to simulate turning in actual course
            if (turnCount < 4)
            {
                setFrontSensorDistance(Math.Round(5.5 * tileDistance));
            } else if (turnCount < 8)
            {
                setFrontSensorDistance(Math.Round(4.5 * tileDistance));
            } else
            {
                setFrontSensorDistance(Math.Round(3.5 * tileDistance));
            }

            if (simulationRunning)
            {
                // output progress of simulation every turn
                string turnCountDisplay = turnCount == 0 ? "1st" :
                    (turnCount == 1 ? "2nd" :
                    (turnCount == 2 ? "3rd" : $"{turnCount + 1}th"));
                Console.WriteLine($"{turnCountDisplay} turn!! left distance after: {getLeftSensorDistance()}, front distance after: {getFrontSensorDistance()}".ToUpper());
            }
        }
        #endregion

        #region Main code for unit tests and simulation
        static void Main(string[] args)
        {
            isTesting = true;
            section("unit tests", () => 
            {
                // check defaults
                section("defaults", () =>
                {
                    expect(getAngleSensor(), (double)0, "angle sensor should be 0");
                    expect(getLeftSensorDistance(), (double)0, "left distance should be 0");
                    expect(getFrontSensorDistance(), (double)0, "front distance should be 0");
                });

                // check setup
                section("setup", () =>
                {
                    setAngleSensor(0.1);
                    setLeftSensorDistance(1);
                    expect(getAngleSensor(), 0.1, "angle sensor should be as expected");
                    expect(getLeftSensorDistance(), (double)1, "left distance should be as expected");
                    setup();
                    expect(angleZero, getAngleSensor(), "angle zero should be same as initial sensor");
                    expect(tileDistance, (double)2, "tile distance should be double of left sensor distance");
                    expect(turnCount, 0, "turn count should be 0");
                });

                // check drive forward
                section("drive forward", () =>
                {
                    expect(motorLeftSpeed, MotorSpeed.Stop, "motor left should be initally stopped");
                    expect(motorRightSpeed, MotorSpeed.Stop, "motor right should be initially stopped");
                    driveForward();
                    expect(motorLeftSpeed, MotorSpeed.NormalForward, "motor left speed should be normal forward");
                    expect(motorRightSpeed, MotorSpeed.NormalForward, "motor right speed should be normal forward");
                    driveForward(true);
                    expect(motorLeftSpeed, MotorSpeed.SlowForward, "motor left speed should be slow forward if specified");
                    expect(motorRightSpeed, MotorSpeed.SlowForward, "motor right speed should be slow forward if specified");
                });

                // check stop
                section("stop robot", () =>
                {
                    stopRobot();
                    expect(motorLeftSpeed, MotorSpeed.Stop, "motor left should be stopped");
                    expect(motorRightSpeed, MotorSpeed.Stop, "motor right should be stopped");
                });

                // turn right
                section("turn right", () =>
                {
                    setAngleSensor(0.2);
                    rightTurnRobot();
                    expect(Math.Abs(getAngleSensor() - angleZero - 90) <= ANGLE_TOLERANCE_VALUE, true, "angle should be 90 after 1st turn");
                    expect(Math.Abs(getLeftSensorDistance() - (tileDistance / 2)) <= DISTANCE_TOLERANCE_VALUE, true, "left sensor should be tile distance / 2 after 1st turn");
                    expect(Math.Abs(getFrontSensorDistance() - (5.5 * tileDistance)) <= DISTANCE_TOLERANCE_VALUE, true, "front sensor should be 5.5 * tile distance after 1st turn");
                    expect(turnCount, 1, "turn count should be 1");

                    rightTurnRobot();
                    expect(Math.Abs(getAngleSensor() - angleZero - 180) <= ANGLE_TOLERANCE_VALUE, true, "angle should be 180 after 2nd turn");
                    expect(turnCount, 2, "turn count should be 2");

                    rightTurnRobot();
                    expect(Math.Abs(getAngleSensor() - angleZero - 270) <= ANGLE_TOLERANCE_VALUE, true, "angle should be 270 after 3rd turn");
                    expect(turnCount, 3, "turn count should be 3");

                    rightTurnRobot();
                    expect(Math.Abs(getAngleSensor() - angleZero - 360) <= ANGLE_TOLERANCE_VALUE, true, "angle should be 360 after 4th turn");
                    expect(turnCount, 4, "turn count should be 4");
                });

                // self align
                section("self align", () =>
                {
                    turnCount = 0;
                    setAngleSensor(2);
                    selfAlign();
                    expect(Math.Abs(getAngleSensor() - angleZero) <= ANGLE_TOLERANCE_VALUE, true, "angle sensor should be aligned to 0 intially");
                    turnCount = 1;
                    setAngleSensor(89);
                    selfAlign();
                    expect(Math.Abs(getAngleSensor() - angleZero - 90) <= ANGLE_TOLERANCE_VALUE, true, "angle sensor should be aligned to 90 after 1 turn");
                    turnCount = 2;
                    setAngleSensor(182);
                    selfAlign();
                    expect(Math.Abs(getAngleSensor() - angleZero - 180) <= ANGLE_TOLERANCE_VALUE, true, "angle sensor should be aligned to 180 after 2 turns");
                    turnCount = 3;
                    setAngleSensor(267.8);
                    selfAlign();
                    expect(Math.Abs(getAngleSensor() - angleZero - 270) <= ANGLE_TOLERANCE_VALUE, true, "angle sensor should be aligned to 270 after 3 turns");
                });
            });

            // simulation
            section("starting simulation", () =>
            {
                setAngleSensor(0.1);
                setup();
                Console.WriteLine($"tile distance for simulation: {tileDistance}".ToUpper());
                loop();
                expect(turnCount, 10, "turning count should be 10 after end");
                expect(Math.Abs(getFrontSensorDistance() - (2.5 * tileDistance)) <= DISTANCE_TOLERANCE_VALUE, true, "front sensor should be 2.5 * tile distance");
                expect(Math.Abs(getLeftSensorDistance() - (2.5 * tileDistance)) <= DISTANCE_TOLERANCE_VALUE, true, "left sensor should be 2.5 * tile distance");
                expect(motorLeftSpeed, MotorSpeed.Stop, "motor left should be stopped");
                expect(motorRightSpeed, MotorSpeed.Stop, "motor right should be stopped");
                Console.WriteLine("Simulation Complete");
            });

            // results
            section("test results", () =>
            {
                Console.ForegroundColor = failCount > 0 ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen;
                Console.WriteLine(failCount > 0 ? $"{failCount} Tests Failed" : "All Tests Passed");
                Console.ResetColor();
            });

            // keep console open
            Console.Write("Tests and Simulation Complete. Press Enter to close");
            Console.Read();
        }
        #endregion
    }
}
