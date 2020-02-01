using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTE380TraversalLogic
{
    class Program
    {
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
        static readonly double ANGLE_TOLERANCE_VALUE = 0.5;
        static double getAngleSensor()
        {
            return angleSensor;
        }
        static double leftSensorDistance = 0;
        static double frontSensorDistance = 0;
        static readonly double DISTANCE_TOLERANCE_VALUE = 0.5;
        static double getLeftSensorDistance()
        {
            return leftSensorDistance;
        }
        static double getFrontSensorDistance()
        {
            return frontSensorDistance;
        }

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

        static void rightTurnRobot(double startingAngle) {
            Console.WriteLine("RIGHT TURN ROBOT");
            stopRobot();

            motorLeftSpeed = MotorSpeed.NormalForward;
            motorRightSpeed = MotorSpeed.NormalBackward;

            double angle = getAngleSensor();
            while (angle < (startingAngle % 360) + 90) {
                angle = getAngleSensor();
                var temp = angle % 360;
                var temp2 = (startingAngle % 360) + 90;
                var temp3 = angle % 360 < (startingAngle % 360) + 90;

                if (isTesting)
                {
                    setAngleSensor(Math.Round(angle+ 0.01, 2)); 
                }
            }

            stopRobot();
            driveForward();

            if (isTesting & simulationRunning)
            {
                simulationTurnRight();
            }
        }

        static void selfAlign(double startingAngle) {
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

            double angle = getAngleSensor();
            while (
                Math.Abs(angle - angleZero - ((turnCount* 90) % 360)) >
                ANGLE_TOLERANCE_VALUE
            ) {
                angle = getAngleSensor();

                if (isTesting & simulationRunning)
                {
                    setAngleSensor(angle + (shouldTurnLeft ? -0.005 : 0.005));
                }
            }

            stopRobot();
            driveForward();
        }
        
        static void setup()
        {
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
                driveForward(approachingDrop());

                double angle = getAngleSensor();
                if (
                    Math.Abs(angle - angleZero - ((turnCount * 90) % 360)) >
                    ANGLE_TOLERANCE_VALUE
                )
                {
                    selfAlign(angle);
                    continue;
                }

                double frontDistance = getFrontSensorDistance();
                if (
                    Math.Abs(
                        frontDistance -
                        tileDistance * ((turnCount + 1) / 4) + tileDistance / 2
                    ) > DISTANCE_TOLERANCE_VALUE
                )
                {
                    continue;
                }

                if (turnCount == 10)
                {
                    stopRobot();
                    if (isTesting)
                    {
                        simulationRunning = false;
                    }
                    break;
                }
                else
                {
                    rightTurnRobot(angle);
                    turnCount++;
                }
            }
        }

        #region TESTING STUFF
        static void expect(object expectedValue, object actualValue, string description = "")
        {
            // Show description of test if exists
            if (description != "")
            {
                Console.Write($"{description}: ");
            }
            // Show test result
            Console.WriteLine(expectedValue.Equals(actualValue) ? "SUCCESS" : "FAIL");
        }

        static void setAngleSensor(double value)
        {
            angleSensor = value;
        }

        static void setLeftDistanceSensor(double value)
        {
            leftSensorDistance = value;
        }

        static void setFrontDistanceSensor(double value)
        {
            frontSensorDistance = value;
        }

        static bool isTesting = false;
        static bool simulationRunning = false;

        static void simulationStart()
        {
            setLeftDistanceSensor(1);
            setFrontDistanceSensor(6);
            simulationRunning = true;

            Task.Run(() =>
            {
                while (simulationRunning)
                {
                    if ((motorLeftSpeed == MotorSpeed.NormalForward && motorRightSpeed == MotorSpeed.NormalForward) ||
                    (motorLeftSpeed == MotorSpeed.SlowForward && motorRightSpeed == MotorSpeed.SlowForward))
                    {
                        // Console.WriteLine("DRIVING FORWARD");
                        setFrontDistanceSensor(getFrontSensorDistance() - (motorLeftSpeed == MotorSpeed.NormalForward ? 0.01 : 0.005));
                    } else
                    {
                        var temp = motorLeftSpeed;
                    }
                    Console.WriteLine($"TURN COUNT: {turnCount}");
                }
            });
            Console.WriteLine("MADE IT PAST TASK");
        }
        static void simulationTurnRight()
        {
            var temp = getAngleSensor();
            Console.WriteLine($"ANGLE SENSOR: {temp}");
            // update left sensor
            if (turnCount == 2 || turnCount == 6 || turnCount == 9)
            {
                leftSensorDistance += tileDistance;
            }

            // update front sensor
            if (turnCount < 2)
            {
                setFrontDistanceSensor(5.5 * tileDistance);
            } else if (turnCount < 6)
            {
                setFrontDistanceSensor(4.5 * tileDistance);
            } else
            {
                setFrontDistanceSensor(3.5 * tileDistance);
            }
            Console.WriteLine($"TURN RIGHT!! NEW LEFT DISTANCE AFTER TURN: {leftSensorDistance}");
        }
        #endregion

        static void Main(string[] args)
        {
            isTesting = true;
            Console.WriteLine("UNIT TESTS");
            Console.WriteLine();

            // check defaults
            Console.WriteLine("DEFAULTS");
            Console.WriteLine();

            expect(getAngleSensor(), (double)0, "ANGLE SENSOR SHOULD BE 0");
            expect(getLeftSensorDistance(), (double)0, "LEFT DISTANCE SHOULD BE 0");
            expect(getFrontSensorDistance(), (double)0, "FRONT DISTANCE SHOULD BE 0");
            Console.WriteLine();

            // check setup
            Console.WriteLine("SETUP");
            Console.WriteLine();

            setAngleSensor(0.1);
            setLeftDistanceSensor(1);
            expect(getAngleSensor(), 0.1, "ANGLE SENSOR SHOULD BE AS EXPECTED");
            expect(getLeftSensorDistance(), (double)1, "LEFT DISTANCE SHOULD BE AS EXPECTED");
            setup();
            expect(angleZero, getAngleSensor(), "ANGLE ZERO SHOULD BE SAME AS INITIAL SENSOR");
            expect(tileDistance, (double)2, "TILE DISTANCE SHOULD BE DOUBLE OF LEFT SENSOR DISTANCE");
            expect(turnCount, 0, "TURN COUNT SHOULD BE 0");
            Console.WriteLine();

            // check drive forward
            Console.WriteLine("DRIVE FORWARD");
            Console.WriteLine();
            expect(motorLeftSpeed, MotorSpeed.Stop, "MOTOR LEFT SHOULD BE INITIALLY STOPPED");
            expect(motorRightSpeed, MotorSpeed.Stop, "MOTOR RIGHT SHOULD BE INITIALLY STOPPED");
            driveForward();
            expect(motorLeftSpeed, MotorSpeed.NormalForward, "MOTOR LEFT SPEED SHOULD BE NORMAL FORWARD");
            expect(motorRightSpeed, MotorSpeed.NormalForward, "MOTOR RIGHT SPEED SHOULD BE NORMAL FORWARD");

            // check stop
            Console.WriteLine("STOP ROBOT");
            Console.WriteLine();
            stopRobot();
            expect(motorLeftSpeed, MotorSpeed.Stop, "MOTOR LEFT SHOULD BE STOPPED");
            expect(motorRightSpeed, MotorSpeed.Stop, "MOTOR RIGHT SHOULD BE STOPPED");

            // loop
            Console.WriteLine("STARTING SIMULATION");
            Console.WriteLine();
            loop();
            expect(turnCount, 10, "TURNING COUNT SHOULD BE 10 AFTER END");
            expect(getLeftSensorDistance(), 3.5*tileDistance, "LEFT SENSOR SHOULD BE 3.5 * TILE DISTANCE");
            expect(motorLeftSpeed, MotorSpeed.Stop, "MOTOR LEFT SHOULD BE STOPPED");
            expect(motorRightSpeed, MotorSpeed.Stop, "MOTOR RIGHT SHOULD BE STOPPED");


            Console.Read();
        }
    }
}
