using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTE380TraversalLogic
{
    class Program
    {
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
        static readonly double ANGLE_TOLERANCE_VALUE = 0.5;
        static double getAngleSensor()
        {
            return 0;
        }
        static readonly double DISTANCE_TOLERANCE_VALUE = 0.5;
        static double getLeftSensorDistance()
        {
            return 0;
        }
        static double getFrontSensorDistance()
        {
            return 0;
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
            stopRobot();

            motorLeftSpeed = MotorSpeed.NormalForward;
            motorRightSpeed = MotorSpeed.NormalBackward;

            double angle = getAngleSensor();
            while (angle % 360 < (startingAngle % 360) + 90) {
                angle = getAngleSensor();
            }

            stopRobot();
            driveForward();
        }

        static void selfAlign(double startingAngle) {
            if (startingAngle > (turnCount* 90) % 360) {
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
            if (description != "")
            {
                Console.WriteLine(description);
            }
            Console.WriteLine(expectedValue.Equals(actualValue) ? "SUCCESS" : "FAIL");
        }
        #endregion

        static void Main(string[] args)
        {
            expect(getLeftSensorDistance(), (double)0, "LEFT DISTANCE SHOULD BE 0");
            expect(getFrontSensorDistance(), (double)0, "FRONT DISTANCE SHOULD BE 0");
            Console.Read();
        }
    }
}
