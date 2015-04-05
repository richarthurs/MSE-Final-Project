using System;
using MonoBrick.EV3;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;

enum Color {Yellow, Orange, Green, Black, Blue, Red, LightBrown, DarkBrown};


// START PROCEDURE:
// FACE OUTSIDE, BUTTON PRESS - DRIVE TO LINE, STOP, TURN AROUND, 
public static class Program
{
    const bool left_side = false;

    enum direction { North, East, South, West };
    enum sensor { Left, Center, Right };
    static sensor sensorposition = sensor.Center;
    static direction RobotDirection = direction.North;

    static int XPos = 0;
    static int YPos = 0; 

    static bool OnWhite(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3) // [3] returns true if the line sensor value is over a thershold
    {
        Debug.WriteLine("OnWhite()");
        int THRESHOLD = 600;
        // white < 600 < black

        int color = ((ColorSensor)ev3.Sensor3).Read();
        Debug.WriteLine(color);
        return color == 6;
    }

    static bool GetReflectedColor(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {
        Debug.WriteLine("ReflectedColor()");

        int reflection = ((ColorSensor)ev3.Sensor3).Read();
        return true;
    }

    static void StartingRoutine(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {
        PartOne(ev3);
        ConsoleKeyInfo cki = Console.ReadKey(true);

        while (cki.Key != ConsoleKey.S)
        {
            Thread.Sleep(100);
        }

        PartTwo(ev3);
    }

    private static bool EndOfTrack(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {
        
        return GetReflectedColor(ev3);
    }

    private static void PartTwo(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {
        TurnWithGyroscope(ev3);
        int distance_from_side_wall = 15;
        // Bolt back to starting position.
        while (!EndOfTrack())
        {
            if (((UltrasonicSensor)ev3.Sensor1).Read() > distance_from_side_wall)
            {
                ev3.MotorA.On(100);
                ev3.MotorB.On(90);
            }
            else
            {
                ev3.MotorA.On(90);
                ev3.MotorB.On(100);
            }
        }

        ev3.MotorA.Brake();
        ev3.MotorB.Brake();
    }

    private static void TurnWithGyroscope(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {
        var total_angle = 0.0;
        if (left_side)
        {
            ev3.MotorA.On(25);
            ev3.MotorB.On(-25);
            while (total_angle < 80)
            {
                var iteration_time = 100;
                Console.WriteLine(((GyroSensor)(ev3.Sensor2)).Read());
                total_angle += ((GyroSensor)(ev3.Sensor2)).Read() * iteration_time / 900;
                Thread.Sleep(iteration_time);
            }
        }
        else
        {
            ev3.MotorA.On(-25);
            ev3.MotorB.On(25);
            while (total_angle > -80)
            {
                var iteration_time = 100;
                Console.WriteLine(((GyroSensor)(ev3.Sensor2)).Read());
                total_angle += ((GyroSensor)(ev3.Sensor2)).Read() * iteration_time / 900;
                Thread.Sleep(iteration_time);
            }
        }
        
        ev3.MotorB.Brake();
        ev3.MotorA.Brake();
    }

    private static void turn90CW(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {
        Console.WriteLine("Robot Turning ClockWise");

        int starting_tacho = ev3.MotorB.GetTachoCount();
        Console.WriteLine(starting_tacho);
        int turnthresh = 162;
        int current = ev3.MotorB.GetTachoCount();
        while(current < starting_tacho+turnthresh){
            ev3.MotorA.On(-25);
            ev3.MotorB.On(25);
         

            current = ev3.MotorB.GetTachoCount();

            Console.WriteLine(current);
        }
        ev3.MotorA.Brake();
        ev3.MotorB.Brake();

        // Change Direction (Since we are using Enums, this is the Long Way but more readable)
        switch (RobotDirection)
        {
            case direction.North: RobotDirection = direction.East; break;
            case direction.East: RobotDirection = direction.South; break;
            case direction.South: RobotDirection = direction.West; break;
            case direction.West: RobotDirection = direction.North; break;
            default: break;
        }// Switch
        Console.WriteLine("Robot Now Facing {0} ", RobotDirection);
    }

    private static void turn90CCW(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {
        Console.WriteLine("Robot Turning CounterClockWise");
        int starting_tacho = ev3.MotorB.GetTachoCount();
        Console.WriteLine(starting_tacho);
        int turnthresh = 162;
        int current = ev3.MotorB.GetTachoCount();
        while (current > starting_tacho - turnthresh)
        {
            ev3.MotorA.On(25);
            ev3.MotorB.On(-25);
            current = ev3.MotorB.GetTachoCount();

            Console.WriteLine(current);
        }
        ev3.MotorA.Brake();
        ev3.MotorB.Brake();

        // Change Direction (Since we are using Enums, this is the Long Way but more readable)
        switch (RobotDirection)
        {
            case direction.North: RobotDirection = direction.West; break;
            case direction.East: RobotDirection = direction.North; break;
            case direction.South: RobotDirection = direction.East; break;
            case direction.West: RobotDirection = direction.South; break;
            default: break;
        }// Switch
        Console.WriteLine("Robot Now Facing {0} ", RobotDirection);
    }
    static void LineLeft(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {
        switch (sensorposition)
        {
            case sensor.Center:
                ev3.MotorC.On(50);
                Thread.Sleep(500);
                ev3.MotorC.Off();

                break;
            case sensor.Right:
                ev3.MotorC.On(50);
                Thread.Sleep(1000);
                ev3.MotorC.Off();
                break;
        }
        sensorposition = sensor.Left;
    }
    static void LineRight(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {
        switch (sensorposition)
        {
            case sensor.Center:
                ev3.MotorC.On(-50);
                Thread.Sleep(500);
                ev3.MotorC.Off();

                break;
            case sensor.Left:
                ev3.MotorC.On(-50);
                Thread.Sleep(1000);
                ev3.MotorC.Off();
                break;
        }
        sensorposition = sensor.Right;
    }
    static void LineCenter(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {

        switch (sensorposition)
        {
            case sensor.Left:
                ev3.MotorC.On(-50);
                Thread.Sleep(300);
                ev3.MotorC.Off();

                break;
            case sensor.Right:
                ev3.MotorC.On(50);
                Thread.Sleep(500);
                ev3.MotorC.Off();
                break;
        }
        
        sensorposition = sensor.Center;

    }
   

    static void MoveFwd(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {
        Console.WriteLine("Robot Moving Forward");
        LineRight(ev3);
        int starting_tacho = ev3.MotorB.GetTachoCount();
        Console.WriteLine(starting_tacho);
        int fwdthresh = 300;
        int current = ev3.MotorB.GetTachoCount();
        while (current < starting_tacho + fwdthresh)
        {
            ev3.MotorA.On(25);
            ev3.MotorB.On(25);
            current = ev3.MotorB.GetTachoCount();
        }
        ev3.MotorA.Off();
        ev3.MotorB.Off();
        int sensorThresh = 30;
        int sensorVal = ((ColorSensor)ev3.Sensor3).Read();
        while (sensorVal > sensorThresh)
        {
            ev3.MotorA.On(25);
            ev3.MotorB.On(25);
            sensorVal = ((ColorSensor)ev3.Sensor3).Read();
        }
        ev3.MotorA.Brake();
        ev3.MotorB.Brake();

        switch (RobotDirection)
        {
            case direction.North: YPos++; break;
            case direction.East: XPos++; break;
            case direction.South: YPos--; break;
            case direction.West: XPos--; break;
            default: break;
        }// Switch
        Console.WriteLine("Robot Position X= {0} Y={1}", XPos, YPos);
    }


    private static bool EndOfTrack()
    {
        throw new NotImplementedException();
    }

    private static void PartOne(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {
        var FEEDBACK_TO_SPEED_RATIO = 0.8;
        sbyte max_speed = 80;
        int desired_dist_from_side = 18; // Distance to maintain from side wall in centimeters.

        List<int> tach_counts = new List<int>();
        List<int> light_sensor_values = new List<int>();

        // Dark Brown: 30
        // Light Brown: 60
        // Red: 50
        
        int starting_tacho = ev3.MotorB.GetTachoCount();
        Console.WriteLine("Starting tacho: " + starting_tacho);
        var accumulated_error = 0.0;
        var last_e = 0.0;
        bool first_run = true;
        bool quit_loop = false;
        int in_light_brown = 0;
        int j = 5;
        int sample_divider = 5;
        while (!quit_loop) // At light brown
        {
            if (j == sample_divider)
            {
                j = 1;
                if (ev3.MotorB.GetTachoCount() - starting_tacho > 2500 && ev3.MotorB.GetTachoCount() - starting_tacho < 6000)
                {
                    //FEEDBACK_TO_SPEED_RATIO = 0.3;
                    //max_speed = 40;
                    FEEDBACK_TO_SPEED_RATIO = 0.3;
                    max_speed = 10;
                }
                else if (ev3.MotorB.GetTachoCount() - starting_tacho > 6000)
                {
                    
                    //else if (((ColorSensor)ev3.Sensor3).Read() < 57 && in_light_brown > 5)
                    if (in_light_brown > 7)
                    { // if sees red
                        quit_loop = true;
                    }
                    else if (((ColorSensor)ev3.Sensor3).Read() > 55)
                    {
                        FEEDBACK_TO_SPEED_RATIO = .07;
                        max_speed = 10;
                        in_light_brown += 1;
                        Console.WriteLine(in_light_brown);
                    }
                    else
                    {
                        FEEDBACK_TO_SPEED_RATIO = .8;
                        max_speed = 80;
                    }
                }

                Console.WriteLine("Colour:" + ((ColorSensor)ev3.Sensor3).Read());


                ev3.MotorA.On(max_speed);
                ev3.MotorB.On(max_speed);

                // Maintain distance from the side wall.
                var error = ((UltrasonicSensor)ev3.Sensor1).Read() - desired_dist_from_side;
                accumulated_error += error;
                var derivative_e = error - last_e;
                if (first_run)
                {
                    derivative_e = 0;
                    first_run = false;
                }
                last_e = error;

                //var speed_diff = Math.Abs(error * 3);
                //speed_diff = Math.Min(speed_diff, 15);
                //sbyte fast_speed = (sbyte)(speed + speed_diff);

                //Console.WriteLine(speed_diff);


                var Kp = 2.55;
                var Ki = .005; // 2.55,.005,.01
                var Kd = 0.05;
                var raw_feedback = (Kp * error + Ki * accumulated_error + Kd * derivative_e) * FEEDBACK_TO_SPEED_RATIO;
                var feedback = Math.Min(Math.Abs(raw_feedback), max_speed / 2);
                var slow_speed = (sbyte)(max_speed - (sbyte)(feedback));

                if ((raw_feedback < 0) != left_side)
                { // just a XOR statement
                    // Turn left
                    ev3.MotorB.On(max_speed);
                    ev3.MotorA.On(slow_speed);
                    //    Console.WriteLine("left: {0}; right: {3}; p: {1}; i: {2}; d: {4}", slow_speed, error, (int)accumulated_error, max_speed, derivative_e);
                }
                else
                {
                    ev3.MotorA.On(max_speed);
                    ev3.MotorB.On(slow_speed);
                    // Console.WriteLine("left: {3}; right: {0}; p: {1}; i: {2}; d: {4}", slow_speed, error, (int)accumulated_error, max_speed, derivative_e);
                }
            }
         
            // Scan floor
            tach_counts.Add(ev3.MotorB.GetTachoCount());
            light_sensor_values.Add(((ColorSensor)ev3.Sensor3).Read());

            Thread.Sleep(50/sample_divider);
            j++;
        }

        Console.WriteLine("Done!");

        ev3.MotorA.Off();
        ev3.MotorB.Off();


        ev3.MotorC.Off();
        if (left_side){
            ev3.MotorC.On(10, 91, false);
        }
        else
        {
            var angle = (UInt32)(int)91;
            ev3.MotorC.On(-10, angle, false);
        }

        End(ev3);

        ev3.MotorC.Off();
        if (left_side)
        {
            ev3.MotorC.On(10, 91, false);
        }
        else
        {
            var angle = (UInt32)(int)100;
            ev3.MotorC.On(-10, angle, false);
        }

        Thread.Sleep(1000);
        
        bool quit_loop2 = false;
        starting_tacho = ev3.MotorB.GetTachoCount();
        max_speed = 20;
        FEEDBACK_TO_SPEED_RATIO = 0.3;
        accumulated_error = 0;
        while (!quit_loop2)
        {
            if (ev3.MotorB.GetTachoCount() - starting_tacho > 2000 && ev3.MotorB.GetTachoCount() - starting_tacho < 9000)
            {
                FEEDBACK_TO_SPEED_RATIO = 0.8;
                max_speed = 80;
            }
            else if (ev3.MotorB.GetTachoCount() - starting_tacho > 9000)
            {
                FEEDBACK_TO_SPEED_RATIO = 0.3;
                max_speed = 40;

                if (((ColorSensor)ev3.Sensor3).Read() < 20)
                {
                    quit_loop2 = true;
                }
            }
            //Console.WriteLine("tacho: {0}", (ev3.MotorB.GetTachoCount() - starting_tacho));


            ev3.MotorA.On(max_speed);
            ev3.MotorB.On(max_speed);

            // Maintain distance from the side wall.
            var error = ((UltrasonicSensor)ev3.Sensor1).Read() - desired_dist_from_side;
            accumulated_error += error;
            var derivative_e = error - last_e;
            if (first_run)
            {
                derivative_e = 0;
                first_run = false;
            }
            last_e = error;


            var Kp = 2.55;
            var Ki = .006; // 2.55,.005,.01
            var Kd = 0.05;
            var raw_feedback = (Kp * error + Ki * accumulated_error + Kd * derivative_e) * FEEDBACK_TO_SPEED_RATIO;
            var feedback = Math.Min(Math.Abs(raw_feedback), max_speed / 2);
            var slow_speed = (sbyte)(max_speed - (sbyte)(feedback));

            if ((raw_feedback < 0) != !left_side)
            { // just a XOR statement
                // Turn left
                ev3.MotorB.On(max_speed);
                ev3.MotorA.On(slow_speed);
                Console.WriteLine("left: {0}; right: {3}; p: {1}; i: {2}; d: {4}", slow_speed, error, (int)accumulated_error, max_speed, derivative_e);
            }
            else
            {
                ev3.MotorA.On(max_speed);
                ev3.MotorB.On(slow_speed);
                Console.WriteLine("left: {3}; right: {0}; p: {1}; i: {2}; d: {4}", slow_speed, error, (int)accumulated_error, max_speed, derivative_e);
            }

            Thread.Sleep(50);
        }

        ev3.MotorA.Brake();
        ev3.MotorB.Brake();

    }


   

    static void End(EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3)
    {
        sbyte end_speed;
        while (((UltrasonicSensor)ev3.Sensor1).Read() > 4)
        {
            end_speed = (sbyte)(((UltrasonicSensor)ev3.Sensor1).Read() - 3);
            ev3.MotorA.On(end_speed);
            ev3.MotorB.On(end_speed);
            Thread.Sleep(100);
        }

        Console.WriteLine("End of forward");

        ev3.MotorA.Brake();
        ev3.MotorB.Brake();

        ev3.PlayTone(100, 5, 100);

        Thread.Sleep(30000);
                    
        ev3.MotorA.On(-25);
        ev3.MotorB.On(-25);
        Thread.Sleep(200);


        TurnWithGyroscope(ev3);
    }

    static void Main(string[] args)
    {
        var ev3 = new EV3Plus<Sensor, Sensor, Sensor, Sensor>("COM3");
        //var ev3 = new Brick<Sensor, Sensor, Sensor, Sensor>("COM3");
        //EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3plus = ev3;
        ((Brick<Sensor, Sensor, Sensor, Sensor>)ev3).Connection.Close();

        ((Brick<Sensor, Sensor, Sensor, Sensor>)ev3).Connection.Open();
        ev3.Sensor1 = new UltrasonicSensor();
        ev3.Sensor3 = new ColorSensor(ColorMode.Reflection);
        ev3.Sensor2 = new GyroSensor(GyroMode.AngularVelocity);



        ConsoleKeyInfo cki;
        Console.WriteLine("Press Q to quit");

        bool quit = false;
        while (!quit)
        {
            cki = Console.ReadKey(true); // press a key    
            switch (cki.Key)
            {
                case ConsoleKey.Spacebar:
                    PartOne(ev3);
                    break;

                case ConsoleKey.T:
                    Console.WriteLine(((UltrasonicSensor)ev3.Sensor1).Read());
                    break;

                case ConsoleKey.Y:  // prints the value from the colour sensor
                    Console.WriteLine(((ColorSensor)ev3.Sensor3).Read());
                    break;
               

                case ConsoleKey.G:
                    turn90CW(ev3);
                    break;

                case ConsoleKey.U:
                    ev3.MotorC.On(10);
                    Thread.Sleep(50);
                    ev3.MotorC.Off();
                    break;

                //case ConsoleKey.G:
                //    for (int i = 0; i < 500; i++)
                //    {
                //        ev3.Forward(100);
                //    }
                //    ev3.MotorA.Brake();
                //    ev3.MotorB.Brake();
                //    break;

              

                case ConsoleKey.S:
                    LineCenter(ev3);
                    ev3.Stop();
                    break;

                case ConsoleKey.Q:
                    LineCenter(ev3);
                    ev3.Stop();
                    quit = true;
                    break;

                case ConsoleKey.E:
                    End(ev3);
                    break;
                case ConsoleKey.A:
                    turn90CCW(ev3);
                    break;
                
                case ConsoleKey.F:
                    MoveFwd(ev3);
                    break;
                case ConsoleKey.O:
                    LineCenter(ev3);
                    break;
                case ConsoleKey.I:
                    LineLeft(ev3);
                    break;
                case ConsoleKey.P:
                    LineRight(ev3);
                    break;

            }
        }
        ev3.Connection.Close();

    }
}

namespace MonoBrick.EV3
{
    public class EV3Plus<TSensor1, TSensor2, TSensor3, TSensor4> : Brick<TSensor1, TSensor2, TSensor3, TSensor4>
        where TSensor1 : Sensor, new()
        where TSensor2 : Sensor, new()
        where TSensor3 : Sensor, new()
        where TSensor4 : Sensor, new()
    {
        public EV3Plus(string connection) : base(connection) {
        }

        public void Rotate(Int32 degrees)
        {
            float TACHO_DISTANCE_PER_DEGREES = 410/360;
            int initial_tacho_count = this.MotorA.GetTachoCount();
            this.MotorA.On(100);
            this.MotorD.On(-100);

            var max_tacho_distance = TACHO_DISTANCE_PER_DEGREES * degrees;
            LockUntilTachoGoal(initial_tacho_count, max_tacho_distance, this.MotorA);

            this.MotorA.Brake();
            this.MotorD.Brake();
        }

        public void LockUntilTachoGoal(Int32 initial_tacho_count, float max_tacho_distance, Motor motor_to_check)
        {
            var new_tacho_count = motor_to_check.GetTachoCount();
            while (new_tacho_count - initial_tacho_count < max_tacho_distance && new_tacho_count - initial_tacho_count > -1 * max_tacho_distance)
            {
                new_tacho_count = motor_to_check.GetTachoCount();
                Debug.WriteLine(new_tacho_count);
                Debug.WriteLine(new_tacho_count - initial_tacho_count);
                System.Threading.Thread.Sleep(10);
            }
        }

        public void ReverseAnd180()
        {
            int initial_tacho_count = this.MotorA.GetTachoCount();
            Forward(-50);
            LockUntilTachoGoal(initial_tacho_count, 280, this.MotorA);
            Rotate(180);
        }

        public void Forward(sbyte speed)
        {
            this.MotorA.On(speed);
            this.MotorB.On(speed);
        }

       

        public void Stop()
        {
            Debug.WriteLine("EV3Plus.Stop()");
            this.MotorA.Brake();
            this.MotorB.Brake();
            this.MotorC.Brake();
            this.MotorA.Off();
            this.MotorB.Off();
            this.MotorC.Off();
        }
    }
}